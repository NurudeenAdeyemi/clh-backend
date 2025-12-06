using CLHCRM.Api.Middleware;
using CLHCRM.Application;
using CLHCRM.Infrastructure;
using Serilog;

// Configure Serilog from appsettings.json
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting CLHCRM API...");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithEnvironmentName()
        .Enrich.WithThreadId());

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddHttpContextAccessor();

    // Add Application Layer
    builder.Services.AddApplication();

    // Add Infrastructure Layer (Database, Repositories, etc.)
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add API documentation
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new()
        {
            Title = "CLHCRM API",
            Version = "v1",
            Description = "Training School CRM System API"
        });
    });

    // CORS configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    var app = builder.Build();

    // Global exception handler (should be first in pipeline)
    app.UseGlobalExceptionHandler();

    // Add Serilog request logging (should be early in pipeline)
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
        };
    });

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "CLHCRM API v1");
            c.RoutePrefix = "swagger";
        });
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowAll");

    app.UseAuthorization();

    app.MapControllers();

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
       .WithName("HealthCheck")
       .WithTags("Health");

    Log.Information("CLHCRM API started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "CLHCRM API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
