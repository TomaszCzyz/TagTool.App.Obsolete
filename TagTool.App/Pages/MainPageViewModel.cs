using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TagTool.App.Helpers;
using TagTool.Backend;

namespace TagTool.App.Pages;

public record HighlightInfo(int StartIndex, int Length);

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
        _tagsSearchResults = new ObservableCollection<ViewListItem>();
    }

    [ObservableProperty]
    private ObservableCollection<ViewListItem> _tagsSearchResults;

    [ObservableProperty]
    private string _btnText = "Click me!";

    [ObservableProperty]
    private string _searchBarText = "";

    private CancellationTokenSource? _cts;

    private int _count;

    async partial void OnSearchBarTextChanged(string value) // todo: make sure that async void won't be a problem here
    {
        // todo: throttle this method to avoid too many calls
        _tagsSearchResults.Clear();
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        var matchTagsRequest = new MatchTagsRequest { PartialTagName = value, MaxReturn = 50 };
        var callOptions = new CallOptions().WithCancellationToken(_cts.Token);

        using var streamingCall = _tagSearchServiceClient.MatchTags(matchTagsRequest, callOptions);

        try
        {
            while (await streamingCall.ResponseStream.MoveNext(_cts.Token))
            {
                var reply = streamingCall.ResponseStream.Current;

                var viewListItem = new ViewListItem
                {
                    Text = reply.MatchedTagName,
                    Score = reply.Score,
                    HighlightInfos = reply.MatchedParts.Select(match => new HighlightInfo(match.StartIndex, match.Length))
                };
                
                _tagsSearchResults.Add(viewListItem);

                // var index = _tagsSearchResults
                //     .ToList()
                //     .BinarySearch(viewListItem, ViewListItem.ScoreComparer); // todo: optimize
                //
                // if (index >= 0)
                // {
                //     _tagsSearchResults.Insert(index, viewListItem);
                // }
                // else
                // {
                //     _tagsSearchResults.Insert(_tagsSearchResults.Count + index, viewListItem);
                // }
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
