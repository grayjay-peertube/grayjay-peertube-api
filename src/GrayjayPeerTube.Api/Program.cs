using System.Threading.RateLimiting;
using GrayjayPeerTube.Api.Middleware;
using GrayjayPeerTube.Application;
using GrayjayPeerTube.Infrastructure;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure forwarded headers for reverse proxy (Dokku/nginx)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    // Only process one hop (the nginx proxy) - safer than clearing all networks
    options.ForwardLimit = 1;
});

// Add services from layers
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// Rate limiting - 100 requests per minute per IP
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

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
app.UseForwardedHeaders(); // Must be first to set correct RemoteIpAddress
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors();
app.UseRateLimiter();

app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
