using FoodTruckFinder.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<FoodTruckService>();

var app = builder.Build();

app.MapGet("/api/foodtrucks", (double latitude, double longitude, int count, string preferred, FoodTruckService service) =>
{
    return service.Search(latitude, longitude, count, preferred);
});

app.Run();
