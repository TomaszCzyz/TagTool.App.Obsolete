namespace TagTool.App.Controls.Pages;

public partial class MainPage : ContentPage
{
    public MainPage(ViewModels.SearchTabViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
