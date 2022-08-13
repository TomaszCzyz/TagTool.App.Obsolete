using TagTool.App.Services;
using TagTool.Backend;

namespace TagTool.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddScoped(_ => // todo: make sure if AddSingleton would not be better here 
        {
            var channel = UnixDomainSocketConnectionFactory.CreateChannel();
            return new TagToolService.TagToolServiceClient(channel);
        });

        return builder.Build();
    }
}
