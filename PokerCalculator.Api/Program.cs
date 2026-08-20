using Microsoft.Extensions.Options;
using PokerCalculator;
using PokerCalculator.Api.Contracts;
using PokerCalculator.Api.Options;
using PokerCalculator.Api.Simulation;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var configuredSimulationOptions = builder.Configuration.GetSection(SimulationOptions.SectionName).Get<SimulationOptions>() ?? new SimulationOptions();

builder.Services.Configure<SimulationOptions>(builder.Configuration.GetSection(SimulationOptions.SectionName));
builder.Services.AddMemoryCache(o =>
{
    o.SizeLimit = configuredSimulationOptions.CacheSizeLimit;
});
// Blanket defense against oversized payloads - every legitimate request to this API fits
// in a few KB even with generous ranges; MaxVillains*/MaxRangeTokensPerVillain* reject
// abusive payload *shape*, this rejects abusive payload *size* before it's even parsed.
builder.WebHost.ConfigureKestrel(o =>
{
    o.Limits.MaxRequestBodySize = configuredSimulationOptions.MaxRequestBodyBytes;
});
builder.Services.ConfigureHttpJsonOptions(o =>
{
    // So Game travels over the wire as "Holdem"/"Omaha" instead of the enum's raw int.
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddSingleton<SimulationWorkerPool>();
builder.Services.AddSingleton<EquityCache>();
builder.Services.AddScoped<EquityService>();

var app = builder.Build();

// Shared by all three routes below: computes equity and maps EquityService's failure
// modes to HTTP responses. The /holdem and /omaha routes are pure convenience - same
// request handling, just with Game pinned by the URL instead of a request field.
async Task<IResult> HandleEquity(EquityRequest request, EquityService equityService, CancellationToken cancellationToken)
{
    try
    {
        var result = await equityService.ComputeEquity(request, cancellationToken);
        return Results.Ok(result);
    }
    catch (ServiceBusyException)
    {
        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
    catch (FormatException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (UnsatisfiableRangeException ex)
    {
        return Results.BadRequest(ex.Message);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        // The client disconnected/cancelled; there is no one left to receive a response.
        return Results.StatusCode(499);
    }
}

app.MapPost("/api/equity", (EquityRequest request, EquityService equityService, CancellationToken cancellationToken) =>
        HandleEquity(request, equityService, cancellationToken))
    .WithName("ComputeEquity");

app.MapPost("/api/equity/holdem", (GameSpecificEquityRequest request, EquityService equityService, CancellationToken cancellationToken) =>
        HandleEquity(request.WithGame(GameType.Holdem), equityService, cancellationToken))
    .WithName("ComputeHoldemEquity");

app.MapPost("/api/equity/omaha", (GameSpecificEquityRequest request, EquityService equityService, CancellationToken cancellationToken) =>
        HandleEquity(request.WithGame(GameType.Omaha), equityService, cancellationToken))
    .WithName("ComputeOmahaEquity");

app.Run();
