var builder = AiorosApplication.CreateBuilder(args);

var app = builder.Build();
app.MapAioros();
return app.RunAioros();
