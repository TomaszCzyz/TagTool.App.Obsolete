using Serilog;
using Serilog.Core.Enrichers;
using Serilog.Events;
using TagTool.App.Controls.Pages;
using TagTool.App.Services;
using TagTool.Backend;

namespace TagTool.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        SetupSerilog();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Logging.AddSerilog(dispose: true);

        builder.Services.AddScoped(_ => // todo: make sure if AddSingleton would not be better here
        {
            var channel = UnixDomainSocketConnectionFactory.CreateChannel();
            return new TagToolService.TagToolServiceClient(channel);
        });
        builder.Services.AddScoped(_ => // todo: make sure if AddSingleton would not be better here
        {
            var channel = UnixDomainSocketConnectionFactory.CreateChannel();
            return new TagSearchService.TagSearchServiceClient(channel);
        });
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MainPageViewModel>();

        return builder.Build();
    }

    private static void SetupSerilog()
    {
        var logsDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TagToolBackend", "Logs",
            "applog.db");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.With(new PropertyEnricher("ApplicationName", "TagToolApp"))
            .WriteTo.SQLite(logsDbPath, storeTimestampInUtc: true, batchSize: 1)
            .CreateLogger();
    }
}
