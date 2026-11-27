using MineReports;

var builder = WebApplication.CreateBuilder(args);
var maxTitles = builder.Configuration.GetValue("Mining:MaxTitles", 20000);
builder.Services.AddSingleton(sp =>
{
    var environment = sp.GetRequiredService<IWebHostEnvironment>();
    return new BackgroundMinerSession(FindData(environment, "keto-cta-quant-and-semi-quant.csv"), FindData(environment, "keto-cta-qangio.csv"), maxTitles);
});
builder.Services.AddHostedService<MinerHost>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/presets", (BackgroundMinerSession miner) => Results.Ok(new { presets = miner.PresetSummaries() }));

app.MapGet("/api/presets/{name}", (string name, BackgroundMinerSession miner) =>
{
    try
    {
        var titles = miner.PresetTitles(name);
        return Results.Ok(new { name, count = titles.Length, titles });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/titles", (TitleBody body, BackgroundMinerSession miner) =>
{
    try
    {
        var accepted = miner.Enqueue(body.Titles ?? []);
        return Results.Ok(new { accepted, status = miner.Status() });
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.MapPost("/api/miner/start", (BackgroundMinerSession miner) =>
{
    var already = miner.IsRunning;
    miner.Start();
    return Results.Ok(new { alreadyRunning = already, status = miner.Status() });
});

app.MapPost("/api/miner/stop", (BackgroundMinerSession miner) =>
{
    miner.Stop();
    return Results.Ok(miner.Status());
});

app.MapPost("/api/miner/clear", (BackgroundMinerSession miner) =>
{
    miner.Clear();
    return Results.Ok(miner.Status());
});

app.MapGet("/api/miner/status", (BackgroundMinerSession miner) => Results.Ok(miner.Status()));

app.MapPost("/api/dusts", (DustQueryBody body, BackgroundMinerSession miner) =>
    Results.Ok(miner.Query(body.Search, body.Limit, body.MaxP, body.Sets)));

app.Run();

static string FindData(IWebHostEnvironment environment, string fileName)
{
    var candidates = new[]
    {
        Path.Combine(AppContext.BaseDirectory, "Data", fileName),
        Path.GetFullPath(Path.Combine(environment.ContentRootPath, "..", "KetoCtaRegressions", "TestData", fileName))
    };
    return candidates.FirstOrDefault(File.Exists)
        ?? throw new FileNotFoundException($"Dataset file '{fileName}' was not next to the app or in KetoCtaRegressions/TestData.");
}

public sealed class MinerHost(BackgroundMinerSession session) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        session.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        session.Stop();
        return Task.CompletedTask;
    }
}

public sealed record TitleBody(string[]? Titles);

public sealed record DustQueryBody(string? Search, int Limit, double? MaxP, string[]? Sets);
