using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ContactsApi.Services;
using ContactsApi; // ADD THIS USING for Utilities

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddGrpc();

// ADD Utilities as a singleton service
builder.Services.AddSingleton<Utilities>();

var app = builder.Build();

// Initialize the database on startup
// Get the Utilities instance from the service provider and call InitializeDatabase
using (var scope = app.Services.CreateScope())
{
    var utilities = scope.ServiceProvider.GetRequiredService<Utilities>();
    utilities.InitializeDatabase();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<ContactsGrpcService>();
app.MapGrpcService<LookupsGrpcService>();

app.Run();
