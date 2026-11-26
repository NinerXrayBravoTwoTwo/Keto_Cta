using Keto_CTA_GoldMine;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<TitleRunService>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/presets/cool", (TitleRunService runs) =>
{
    try
    {
        return Results.Ok(new { titles = runs.CoolTitles() });
    }
    catch (Exception ex) when (ex is FileNotFoundException or IOException)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/api/runs", (RunBody body, TitleRunService runs) =>
{
    var titles = (body.Titles ?? [])
        .Select(title => title.Trim())
        .Where(title => title.Length > 0)
        .ToArray();

    try
    {
        return Results.Ok(runs.Run(titles));
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (TimeoutException ex)
    {
        return Results.Problem(ex.Message, statusCode: StatusCodes.Status504GatewayTimeout);
    }
    catch (Exception ex) when (ex is FileNotFoundException or IOException)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();

public sealed record RunBody(string[]? Titles);
