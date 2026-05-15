using AIJobHelper.Application.Interfaces;
using AIJobHelper.Application.Services;
using AIJobHelper.Infrastructure.AI;
using AIJobHelper.Infrastructure.Documents;
using AIJobHelper.Infrastructure.FileStorage;
using AIJobHelper.Infrastructure.Pdf;
using AIJobHelper.Infrastructure.Persistence;
using AIJobHelper.Infrastructure.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace AIJobHelper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.AddHttpClient<IGeminiClient, GeminiClient>(client =>
        {
            var baseUrl = configuration[$"{GeminiOptions.SectionName}:BaseUrl"]
                ?? "https://generativelanguage.googleapis.com";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddSingleton<IFileStore, LocalFileStore>();

        services.AddSingleton<IDocumentParser, PdfDocumentParser>();
        services.AddSingleton<IDocumentParser, DocxDocumentParser>();

        services.AddScoped<IResumeService, ResumeService>();

        services.AddHttpClient<IUrlContentFetcher, UrlContentFetcher>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "Mozilla/5.0 (compatible; AIJobHelper/1.0; +https://github.com/AIJobHelper)");
        });

        services.AddScoped<IJobDescriptionService, JobDescriptionService>();
        services.AddScoped<IAtsService, AtsService>();

        services.AddSingleton<IPdfGenerator, QuestPdfGenerator>();
        services.AddScoped<ICoverLetterService, CoverLetterService>();

        return services;
    }
}
