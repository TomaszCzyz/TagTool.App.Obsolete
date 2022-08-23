using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using TagTool.App.Models;
using TagTool.Backend;

namespace TagTool.App.ViewModels;

public partial class SearchTabViewModel : ObservableObject, IDisposable
{
    private readonly ILogger<SearchTabViewModel> _logger;
    private readonly TagSearchService.TagSearchServiceClient _tagSearchServiceClient;

    public SearchTabViewModel(
        ILogger<SearchTabViewModel> logger,
        TagSearchService.TagSearchServiceClient tagSearchServiceClient)
    {
        _logger = logger;
        _tagSearchServiceClient = tagSearchServiceClient;
        _tagsSearchResults = new ObservableCollection<HighlightedMatch>();
        _exampleResults = new ObservableCollection<string>
        {
            "Tag1",
            "Tag1",
            "Tag1",
            "Tag1",
            "Tag1",
            "Tag1"
        };
    }
    
    [ObservableProperty]
    private ObservableCollection<string> _exampleResults;
    
    [ObservableProperty]
    private ObservableCollection<HighlightedMatch> _tagsSearchResults;

    [ObservableProperty]
    private string _btnText = "Click me!";

    [ObservableProperty]
    private string _searchBarText = "";

    private CancellationTokenSource? _cts;

    private int _count;

    async partial void OnSearchBarTextChanged(string value) // todo: make sure that async void won't be a problem here
    {
        // todo: throttle this method to avoid too many calls
        _cts?.Cancel();
        _cts?.Dispose();
        TagsSearchResults.Clear();
        if (string.IsNullOrEmpty(value)) return;

        _cts = new CancellationTokenSource();

        var matchTagsRequest = new MatchTagsRequest { PartialTagName = value, MaxReturn = 50 };
        var callOptions = new CallOptions().WithCancellationToken(_cts.Token);

        using var streamingCall = _tagSearchServiceClient.MatchTags(matchTagsRequest, callOptions);

        try
        {
            while (await streamingCall.ResponseStream.MoveNext(_cts.Token))
            {
                var reply = streamingCall.ResponseStream.Current;

                var highlightInfos = reply.MatchedParts
                    .Select(match => new HighlightInfo(match.StartIndex, match.Length))
                    .ToArray();

                var viewListItem = new HighlightedMatch
                {
                    HighlightedText = HighlightText(reply.MatchedTagName, highlightInfos), Score = reply.Score
                };

                TagsSearchResults.Add(viewListItem);
            }
        }
        catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
        {
            _logger.LogDebug("Streaming of tag names hints for SearchBar was cancelled");
        }
        finally
        {
            // todo: do not create new class... manage existing collection
            TagsSearchResults = new ObservableCollection<HighlightedMatch>(Enumerable.OrderByDescending<HighlightedMatch, int>(TagsSearchResults, item => item.Score));
        }
    }

    private static FormattedString HighlightText(string tagName, IReadOnlyCollection<HighlightInfo> highlightInfos)
    {
        var formattedString = new FormattedString();

        var lastIndex = 0;
        var index = 0;

        void FlushNotHighlighted()
        {
            if (lastIndex == index) return;

            formattedString.Spans.Add(new Span { Text = tagName[lastIndex..index] });
        }

        while (index < tagName.Length)
        {
            var highlightedPart = highlightInfos.FirstOrDefault(info => info.StartIndex == index);

            if (highlightedPart is null)
            {
                index++;
            }
            else
            {
                FlushNotHighlighted();

                formattedString.Spans.Add(
                    new Span
                    {
                        Text = tagName[index..(index + highlightedPart.Length)],
                        TextColor = Colors.Green,
                        FontAttributes = FontAttributes.Bold,
                        BackgroundColor = Colors.Azure
                    });
                index += highlightedPart.Length;
                lastIndex = index;
            }
        }

        FlushNotHighlighted();

        return formattedString;
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
