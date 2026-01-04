using GrayjayPeerTube.Api.Middleware;
using GrayjayPeerTube.Application;
using GrayjayPeerTube.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services from layers
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// CORS - Allow any origin, GET method only (matching Node.js behavior)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .WithMethods("GET")
              .AllowAnyHeader();
    });
});

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors();

// Static files (serves from wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

// Fallback to index.html for SPA-style routing
app.MapFallbackToFile("index.html");

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
