# Developer Challenge

## Feature: Find Food Trucks Near a Location Based on Preferred Food

### Description:

Develop a solution to help the San Francisco team find food trucks near their location and based on their preferred food. The solution should return at least the closest food trucks options based on latitude, longitude, and preferred food.

### Acceptance Criteria:
* The solution should accept input for latitude, longitude, amount of results, and preferred food.
* The solution should return a configurable amount of food truck options near the given location and based on the preferred food ordered by distance.
* The food truck data should be sourced from the San Francisco's open dataset.
* The solution should be implemented using ASP.NET Core.
* Database technology is open to choose, but it is not required for this POC.

### Additional Information:
* San Francisco's food truck open dataset CSV dump of the latest data: [csv_dump_link](./Mobile_Food_Facility_Permit.csv)
* The solution should be read-only and not require any updates.


## Running the API

1. Ensure you have the .NET 9 SDK installed.
2. From the repository root, run `dotnet run --project FoodTruckFinder.Api`.
3. Call `/api/foodtrucks` with `latitude`, `longitude`, `count`, and `preferred` query parameters.
   The service performs basic stemming and synonym matching and also uses a
   Levenshtein distance check to handle minor misspellings.

Example:
```
GET https://localhost:5001/api/foodtrucks?latitude=37.7749&longitude=-122.4194&count=5&preferred=tacos
```

## Running Tests

Execute `dotnet test` to run the xUnit tests included in the solution.

## Benchmarking

The `FoodTruckFinder.Benchmarks` project uses BenchmarkDotNet to measure the
performance of the search service. Run the benchmarks with:

```bash
dotnet run -c Release --project FoodTruckFinder.Benchmarks
```
