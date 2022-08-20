namespace TagTool.App.Helpers;

public class HighlightedMatch
{
    public FormattedString? HighlightedText { get; set; }

    public int Score { get; init; }

    private sealed class ScoreRelationalComparer : IComparer<HighlightedMatch>
    {
        public int Compare(HighlightedMatch? x, HighlightedMatch? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return -x.Score.CompareTo(y.Score);
        }
    }

    public static IComparer<HighlightedMatch> ScoreComparer { get; } = new ScoreRelationalComparer();
}
