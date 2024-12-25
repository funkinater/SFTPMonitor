using AutoMapper;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile)); // Add your AutoMapper profile here

// Optionally configure Application Insights if needed
// builder.Services
//     .AddApplicationInsightsTelemetryWorkerService()
//     .ConfigureFunctionsApplicationInsights();

builder.ConfigureFunctionsWebApplication(); // Keeps the existing Functions configuration

builder.Build().Run();
