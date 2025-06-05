namespace FoodTruckFinder.Api.Models;

public class FoodTruck
{
    public int LocationId { get; set; }
    public string Applicant { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string FoodItems { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    // List of normalized food terms used for search matching
    public List<string> FoodTerms { get; set; } = new();
}
