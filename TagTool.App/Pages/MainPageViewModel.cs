using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TagTool.Backend;

namespace TagTool.App.Pages;

public partial class MainPageViewModel : ObservableObject, IDisposable
{
    private readonly ILogger<MainPageViewModel> _logger;
    private readonly TagSearchService.TagSearchServiceClient _tagSearchServiceClient;

    public MainPageViewModel(
        ILogger<MainPageViewModel> logger,
        TagSearchService.TagSearchServiceClient tagSearchServiceClient)
    {
        _logger = logger;
        _tagSearchServiceClient = tagSearchServiceClient;
        _tagsSearchResults = new ObservableCollection<string>();
    }

    [ObservableProperty]
    private ObservableCollection<string> _tagsSearchResults;

    [ObservableProperty]
    private string _btnText = "Click me!";

    [ObservableProperty]
    private string _searchBarText = "";

    private CancellationTokenSource? _cts;

    private int _count;

    async partial void OnSearchBarTextChanged(string value)
    {
        _tagsSearchResults.Clear();
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var findTagsRequest = new FindTagsRequest { PartialTagName = value, MaxReturn = 10 };
        var callOptions = new CallOptions(cancellationToken: _cts.Token);

        using var streamingCall = _tagSearchServiceClient.FindTags(findTagsRequest, callOptions);

        try
        {
            while (await streamingCall.ResponseStream.MoveNext(_cts.Token))
            {
                var findTagsReply = streamingCall.ResponseStream.Current;
                _tagsSearchResults.Add(findTagsReply.TagName);
            }
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
        {
            _logger.LogDebug("Streaming of tag names hints for SearchBar was cancelled");
        }
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

    public void Dispose()
    {
        _cts?.Dispose();
        GC.SuppressFinalize(this);
    }
}
