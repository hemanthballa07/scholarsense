using System.Text.Json;
using System.Text.Json.Serialization;
using ScholarSense.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
});

var app = builder.Build();

app.UseStatusCodePages();

app.MapGet("/api/v1/health", () => Results.Ok(new { status = "ok" }));

app.MapGet("/api/v1/scholarships/sample", (ILogger<Program> logger) =>
{
    logger.LogInformation("Returning sample synthetic scholarship");
    var sample = new Scholarship(
        Id: "synth-sample-001",
        Title: "Synthetic Sample Scholarship (Development Fixture)",
        Description: "Synthetic placeholder scholarship used for local development. Not a real award.",
        EligibilityText: "Open to enrolled undergraduate students in the United States with a minimum 3.0 GPA, pursuing degrees in Computer Science or related disciplines.",
        Sponsor: "Synthetic Sponsor (placeholder)",
        AmountUsd: 5000.0,
        Deadline: new DateOnly(2026, 8, 15),
        MinGpa: 3.0,
        EligibleCountries: ["US"],
        EligibleStates: [],
        EligibleMajors: ["Computer Science", "Computer Engineering"],
        DemographicTags: [],
        SourceUrl: "https://example.invalid/synthetic-fixture");
    return Results.Ok(sample);
});

app.Run();

public partial class Program { }
