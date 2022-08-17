using TagTool.App.Pages;

namespace TagTool.App.Helpers;

public class ViewListItem
{
    public string Text { get; set; }

    public IEnumerable<HighlightInfo>? HighlightInfos { get; set; }

    public int Score { get; init; }

    private sealed class ScoreRelationalComparer : IComparer<ViewListItem>
    {
        public int Compare(ViewListItem? x, ViewListItem? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return x.Score.CompareTo(y.Score);
        }
    }

    public static IComparer<ViewListItem> ScoreComparer { get; } = new ScoreRelationalComparer();
}
