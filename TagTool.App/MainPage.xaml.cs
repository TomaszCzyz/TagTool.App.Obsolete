using TagTool.App.Services;
using TagTool.Backend;

namespace TagTool.App;

public partial class MainPage : ContentPage
{
    private int _count;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        _count++;

        if (_count == 1)
            CounterBtn.Text = $"Clicked {_count} time";
        else
            CounterBtn.Text = $"Clicked {_count} times";

        SemanticScreenReader.Announce(CounterBtn.Text);
    }

    private void OnCreateTagClicked(object sender, EventArgs e)
    {
        var channel = UnixDomainSocketConnectionFactory.CreateChannel();
        var tagToolService = new TagToolService.TagToolServiceClient(channel);

        tagToolService.CreateTag(new CreateTagRequest { TagName = "TagFromMaui2" });
    }
}
