using ContactsApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ContactsApi; // ADD THIS USING DIRECTIVE

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddSingleton<Utilities>(); // Register Utilities as a singleton service

// Configure logging
builder.Logging.ClearProviders(); // Clear default providers
builder.Logging.AddConsole(); // Add console logger
builder.Logging.AddDebug(); // Add debug logger

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<ContactsGrpcService>();
app.MapGrpcService<LookupsGrpcService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

// Initialize the database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var utilities = services.GetRequiredService<Utilities>();
    await utilities.InitializeDatabaseAsync();
}

app.Run();
