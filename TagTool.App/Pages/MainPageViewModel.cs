using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagTool.Backend;

namespace TagTool.App.Pages;

public partial class MainPageViewModel : ObservableObject
{
    private readonly TagToolService.TagToolServiceClient _tagToolServiceClient;

    public MainPageViewModel(TagToolService.TagToolServiceClient tagToolServiceClient)
    {
        _tagToolServiceClient = tagToolServiceClient;
        _tagsSearchResults = new ObservableCollection<string>();
    }

    [ObservableProperty]
    private ObservableCollection<string> _tagsSearchResults;

    [ObservableProperty]
    private string _btnText = "Click me!";
    
    [ObservableProperty] 
    private object _searchBarText = "";

    private int _count;

    partial void OnSearchBarTextChanged(object value)
    {
        _tagToolServiceClient.CreateTag(new CreateTagRequest { TagName = (string)value });
    }

    [RelayCommand]
    private void CounterClicked()
    {
        _count++;

        BtnText = $"Clicked {_count} time{(_count == 1 ? "" : "s")}";
    }

    [RelayCommand]
    private void PerformSearch()
    {
        BtnText = "Performed search";
    }

    // private void OnCreateTagClicked(object sender, EventArgs e)
    // {
    //     var channel = UnixDomainSocketConnectionFactory.CreateChannel();
    //     var tagToolService = new TagToolService.TagToolServiceClient(channel);
    //
    //     tagToolService.CreateTag(new CreateTagRequest { TagName = "TagFromMaui2" });
    // }
}
