using AIJobHelper.Infrastructure;
using AIJobHelper.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, services, config) =>
    {
        var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app-.log");
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(logPath, rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
    });

    builder.Services.AddControllers();
    builder.Services.AddRazorPages();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "AI Job Helper API",
            Version = "v1",
            Description = """
                AI-powered resume analysis, ATS scoring, and cover letter generation.
                Powered by Google Gemini AI.
                """
        });
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    builder.Services.AddHealthChecks()
        .AddDbContextCheck<AppDbContext>("database");

    builder.Services.AddInfrastructure(builder.Configuration);

    builder.Services.AddProblemDetails();
    builder.Services.AddCors(o =>
        o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

    var app = builder.Build();

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler(errApp =>
    {
        errApp.Run(async ctx =>
        {
            var feature = ctx.Features.Get<IExceptionHandlerFeature>();
            var ex = feature?.Error;

            var (status, title) = ex switch
            {
                KeyNotFoundException => (404, "Not Found"),
                InvalidOperationException => (400, "Bad Request"),
                _ => (500, "Internal Server Error")
            };

            ctx.Response.StatusCode = status;
            ctx.Response.ContentType = "application/problem+json";

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = ex?.Message,
                Instance = ctx.Request.Path
            };

            await ctx.Response.WriteAsJsonAsync(problem);
        });
    });

    app.UseCors();
    app.UseStaticFiles();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Job Helper v1");
        options.RoutePrefix = "swagger";
        options.DocumentTitle = "AI Job Helper API";
        options.DisplayRequestDuration();
    });

    app.MapControllers();
    app.MapRazorPages();
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");

    // Auto-apply migrations on startup (safe for dev; gate behind env check for production)
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
