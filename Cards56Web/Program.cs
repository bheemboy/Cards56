using Cards56Web;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHealthChecks().AddCheck<SignalRHealthCheck>("signalr_health_check");
builder.Services.AddCors();
builder.Services.AddSignalR();

var app = builder.Build();
app.UseCors(builder => builder
    .SetIsOriginAllowed(_ => true) // Allow any origin
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials());

app.UseFileServer(); // enable static files and default file serving 
app.MapHealthChecks("/health");
app.MapHub<Cards56Hub>("/Cards56Hub");

app.Run();
