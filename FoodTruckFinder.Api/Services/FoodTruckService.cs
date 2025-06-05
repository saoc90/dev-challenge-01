using System.Globalization;
using System.Collections.Generic;
using CsvHelper;
using CsvHelper.Configuration;
using FoodTruckFinder.Api.Models;
using System.Linq;

namespace FoodTruckFinder.Api.Services;

public class FoodTruckService
{
    private readonly List<FoodTruck> _trucks;
    private readonly Dictionary<string, List<FoodTruck>> _termIndex = new();
    private readonly Dictionary<string, double> _idf = new();
    private static readonly Dictionary<string, string[]> _synonyms = new()
    {
        ["hotdog"] = new[] { "hotdog", "hotdogs", "hot dog", "hot dogs", "frankfurter", "wiener", "sausage" },
        ["frankfurter"] = new[] { "hotdog" },
        ["wiener"] = new[] { "hotdog" },
        ["sausage"] = new[] { "hotdog" },
        ["taco"] = new[] { "taco", "tacos" },
        ["burrito"] = new[] { "burrito", "burritos" },
        ["noodle"] = new[] { "noodle", "noodles", "ramen" }
    };

    public FoodTruckService(IWebHostEnvironment env)
    {
        var filePath = Path.Combine(env.ContentRootPath, "..", "Mobile_Food_Facility_Permit.csv");
        _trucks = LoadFoodTrucks(filePath);
        BuildIndex();
    }

    private static List<FoodTruck> LoadFoodTrucks(string path)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        };
        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, config);
        csv.Read();
        csv.ReadHeader();
        var records = new List<FoodTruck>();
        while (csv.Read())
        {
            var record = new FoodTruck
            {
                LocationId = csv.GetField<int>("locationid"),
                Applicant = csv.GetField("Applicant"),
                Address = csv.GetField("Address"),
                FoodItems = csv.GetField("FoodItems"),
                Latitude = csv.GetField<double>("Latitude"),
                Longitude = csv.GetField<double>("Longitude")
            };
            record.FoodTerms = ParseTerms(record.FoodItems);
            records.Add(record);
        }
        return records;
    }

    private void BuildIndex()
    {
        foreach (var truck in _trucks)
        {
            foreach (var term in truck.FoodTerms)
            {
                if (!_termIndex.TryGetValue(term, out var list))
                {
                    list = new List<FoodTruck>();
                    _termIndex[term] = list;
                }
                list.Add(truck);
            }
        }

        var totalDocs = (double)_trucks.Count;
        foreach (var kvp in _termIndex)
        {
            _idf[kvp.Key] = Math.Log(totalDocs / kvp.Value.Count);
        }
    }

    public IEnumerable<FoodTruckResult> Search(double latitude, double longitude, int count, string preferred)
    {
        var queryTerms = PrepareSearchTerms(preferred).ToList();

        HashSet<FoodTruck> candidates = new();
        if (queryTerms.Count == 0)
        {
            candidates.UnionWith(_trucks);
        }
        else
        {
            foreach (var qt in queryTerms)
            {
                if (_termIndex.TryGetValue(qt, out var list))
                    candidates.UnionWith(list);
                else
                {
                    foreach (var key in _termIndex.Keys)
                    {
                        if (LevenshteinDistance(key, qt) <= 2)
                            candidates.UnionWith(_termIndex[key]);
                    }
                }
            }
        }

        return candidates
            .Select(t => new FoodTruckResult(t, Distance(latitude, longitude, t.Latitude, t.Longitude)))
            .OrderBy(r => r.Distance)
            .Take(count);
    }

    private static double Distance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371e3; // metres
        var phi1 = lat1 * Math.PI / 180;
        var phi2 = lat2 * Math.PI / 180;
        var dphi = (lat2 - lat1) * Math.PI / 180;
        var dlambda = (lon2 - lon1) * Math.PI / 180;

        var a = Math.Sin(dphi / 2) * Math.Sin(dphi / 2) +
                Math.Cos(phi1) * Math.Cos(phi2) *
                Math.Sin(dlambda / 2) * Math.Sin(dlambda / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private static List<string> ParseTerms(string items)
    {
        var result = new HashSet<string>();
        var phrases = items.Split([':', ',', ';'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var phrase in phrases)
        {
            var words = phrase.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var joined = string.Concat(words);
            result.Add(NormalizeTerm(joined));
            for (int i = 0; i < words.Length; i++)
            {
                result.Add(NormalizeTerm(words[i]));
                if (i < words.Length - 1)
                {
                    result.Add(NormalizeTerm(words[i] + words[i + 1]));
                }
            }
        }
        return result.Where(t => t.Length > 0).ToList();
    }

    private static string NormalizeTerm(string term)
    {
        var t = new string(term.ToLowerInvariant().Where(char.IsLetter).ToArray());
        if (t.EndsWith("ing")) t = t[..^3];
        if (t.EndsWith("es")) t = t[..^2];
        if (t.EndsWith("s")) t = t[..^1];
        if (t.EndsWith("ed")) t = t[..^2];
        return t;
    }

    private static IEnumerable<string> PrepareSearchTerms(string preferred)
    {
        if (string.IsNullOrWhiteSpace(preferred))
            return Array.Empty<string>();
        var norm = NormalizeTerm(preferred);
        var result = new HashSet<string> { norm };
        if (_synonyms.TryGetValue(norm, out var syns))
        {
            foreach (var s in syns)
                result.Add(NormalizeTerm(s));
        }
        return result;
    }

    private static int LevenshteinDistance(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b.Length;
        if (string.IsNullOrEmpty(b)) return a.Length;
        var d = new int[a.Length + 1, b.Length + 1];
        for (int i = 0; i <= a.Length; i++) d[i, 0] = i;
        for (int j = 0; j <= b.Length; j++) d[0, j] = j;
        for (int i = 1; i <= a.Length; i++)
        for (int j = 1; j <= b.Length; j++)
        {
            var cost = a[i - 1] == b[j - 1] ? 0 : 1;
            d[i, j] = new[]
            {
                d[i - 1, j] + 1,
                d[i, j - 1] + 1,
                d[i - 1, j - 1] + cost
            }.Min();
        }
        return d[a.Length, b.Length];
    }
}

public record FoodTruckResult(FoodTruck Truck, double Distance);
