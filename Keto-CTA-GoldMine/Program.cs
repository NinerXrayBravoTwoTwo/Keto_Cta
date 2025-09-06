using iTextSharp.tool.xml.html;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

//app.MapGet("/", () => "<h1>Fuckin Ehhhh!</h1>");

app.MapGet("/", () => Results.Content("<html><h1><i>Fuckin' Ehhh ...</i></h1></html><p>Cac0/Cac1 vs. TdCac</p>", "text / HTML"));

app.Run();
