var builder = AiorosApplication.CreateBuilder(args);
builder.Services.AddScoped<Gallery.Models.CodeBlockStreaming>().AddScoped<Gallery.Models.CodeBlockMarkdownExample>().AddScoped<Gallery.Models.TraceThinkingExample>()
    .AddScoped<Gallery.Models.TraceToolCallsExample>().AddScoped<Gallery.Models.TraceDemoExample>()
    .AddScoped<Gallery.Models.CodeBlockPlayback>().AddScoped<Gallery.Models.TracePlayback>();
var app = builder.Build();
app.MapAioros();
app.MapGet("/reference/{name}.png", (string name) =>
{
    var file = Path.Combine(AppContext.BaseDirectory, "Reference", "png", Path.GetFileName(name) + ".png");
    return File.Exists(file) ? Results.File(file, "image/png") : Results.NotFound();
});
return app.RunAioros();
