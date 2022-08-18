using TagTool.App.Pages;

namespace TagTool.App.Views;

public class HighlightedLabel : Label
{
    
    public string Asdfsdfsd { get; set; } = "asdasdas";
    public IEnumerable<HighlightInfo> HighlightInfos { get; set; }

    public HighlightedLabel()
    {
        TextDecorations = TextDecorations.Underline;
        TextColor = Colors.Blue;
        // GestureRecognizers.Add(new TapGestureRecognizer
        // {
        //     // Launcher.OpenAsync is provided by Essentials.
        //     Command = new Command(async () => await Launcher.OpenAsync(Url))
        // });
    }
}
