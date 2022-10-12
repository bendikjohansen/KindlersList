using Cli;
using Core;
using Core.Extractor;
using Core.Importer;
using Core.Translator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(config => config.AddUserSecrets<Program>())
    .ConfigureServices((context, services) =>
    {
        services
            .AddDbContextFactory<ApplicationContext>(options => options.UseSqlite("Data Source=Database.db"))
            .AddScoped<ISourceExtractor, KindleExtractor>()
            .AddScoped<ITranslator, DeeplTranslator>()
            .AddScoped<IImporter, AnkiImporter>()
            .AddScoped<IApplicationOrchestrator, ApplicationOrchestrator>()
            .AddScoped<IExtractionRepository, ExtractionRepository>()
            .AddSingleton(context.Configuration.GetSection(AnkiOptions.Anki).Get<AnkiOptions>())
            .AddSingleton(context.Configuration.GetSection(AmazonOptions.Amazon).Get<AmazonOptions>())
            .AddSingleton<IPlaywrightFactory, PlaywrightFactory>()
            .AddHostedService<ApplicationCliService>();
    });

await host.RunConsoleAsync();
