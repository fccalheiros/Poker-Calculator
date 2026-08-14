using Microsoft.Extensions.Options;
using PokerCalculator.Api.Contracts;
using PokerCalculator.Api.Options;
using PokerCalculator.Api.Simulation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SimulationOptions>(builder.Configuration.GetSection(SimulationOptions.SectionName));
builder.Services.AddMemoryCache(o =>
{
    var simOptions = builder.Configuration.GetSection(SimulationOptions.SectionName).Get<SimulationOptions>() ?? new SimulationOptions();
    o.SizeLimit = simOptions.CacheSizeLimit;
});

builder.Services.AddSingleton<SimulationWorkerPool>();
builder.Services.AddSingleton<EquityCache>();
builder.Services.AddScoped<EquityService>();

var app = builder.Build();

app.MapPost("/api/equity", async (EquityRequest request, EquityService equityService, CancellationToken cancellationToken) =>
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
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        // The client disconnected/cancelled; there is no one left to receive a response.
        return Results.StatusCode(499);
    }
})
.WithName("ComputeEquity");

app.Run();
