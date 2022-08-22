using TagTool.App.Controls.Pages;

namespace TagTool.App;

public partial class App : Application
{
    public App(MainPage mainPage)
    {
        InitializeComponent();

        MainPage = mainPage;
    }
}
