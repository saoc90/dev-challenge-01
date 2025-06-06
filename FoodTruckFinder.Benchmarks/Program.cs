using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FoodTruckFinder.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

BenchmarkRunner.Run<SearchBenchmark>();

public class SearchBenchmark
{
    private readonly FoodTruckService _service;

    public SearchBenchmark()
    {
        _service = new FoodTruckService(new TestEnvironment());
    }

    [Benchmark]
    public void SearchHotDogs()
    {
        _service.Search(37.7601, -122.4190, 5, "hot dog").ToList();
    }

    private class TestEnvironment : IWebHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "FoodTruckFinder.Api";
        public string WebRootPath { get; set; } = string.Empty;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();
        public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider(Directory.GetCurrentDirectory());
    }
}
