using FoodTruckFinder.Api.Services;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using System.Linq;

namespace FoodTruckFinder.Tests;

public class FoodTruckServiceTests
{
    [Fact]
    public void Search_Returns_Trucks_Filtered_By_Food_And_Distance()
    {
        var env = new TestEnvironment();
        var service = new FoodTruckService(env);

        var results = service.Search(37.7601, -122.4190, 3, "hot dogs").ToList();

        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Truck.Applicant.Contains("Leo's Hot Dogs"));
    }

    [Fact]
    public void Search_Matches_Synonyms()
    {
        var env = new TestEnvironment();
        var service = new FoodTruckService(env);

        var results = service.Search(37.7601, -122.4190, 3, "frankfurter").ToList();

        Assert.Contains(results, r => r.Truck.Applicant.Contains("Leo's Hot Dogs"));
    }

    [Fact]
    public void Search_Matches_With_Typo()
    {
        var env = new TestEnvironment();
        var service = new FoodTruckService(env);

        var results = service.Search(37.7601, -122.4190, 3, "hotdgo").ToList();

        Assert.Contains(results, r => r.Truck.Applicant.Contains("Leo's Hot Dogs"));
    }

    private class TestEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "FoodTruckFinder.Api";
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        private static readonly string Root = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "..", "FoodTruckFinder.Api"));
        public string ContentRootPath { get; set; } = Root;
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(Root);
    }
}

