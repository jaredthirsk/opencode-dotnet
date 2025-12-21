namespace AgUi.IDE.BlazorServer.Services;

/// <summary>
/// Represents a file diff.
/// </summary>
public class FileDiff
{
    public string FileName { get; set; } = "";
    public string FilePath { get; set; } = "";
    public string OldContent { get; set; } = "";
    public string NewContent { get; set; } = "";
    public List<DiffHunk> Hunks { get; set; } = new();
    public int Additions { get; set; }
    public int Deletions { get; set; }
}

/// <summary>
/// Represents a diff hunk.
/// </summary>
public class DiffHunk
{
    public int OldStart { get; set; }
    public int OldCount { get; set; }
    public int NewStart { get; set; }
    public int NewCount { get; set; }
    public List<DiffLine> Lines { get; set; } = new();
}

/// <summary>
/// Represents a single line in a diff.
/// </summary>
public class DiffLine
{
    public DiffLineType Type { get; set; }
    public string Content { get; set; } = "";
    public int? OldLineNumber { get; set; }
    public int? NewLineNumber { get; set; }
}

public enum DiffLineType
{
    Context,
    Added,
    Removed
}

/// <summary>
/// Service for managing diff data.
/// </summary>
public class DiffService
{
    private readonly List<FileDiff> _diffs = new();

    public event Action? DiffsChanged;

    public IReadOnlyList<FileDiff> Diffs => _diffs.AsReadOnly();

    public FileDiff? SelectedDiff { get; private set; }

    public void SetDiffs(IEnumerable<FileDiff> diffs)
    {
        _diffs.Clear();
        _diffs.AddRange(diffs);
        SelectedDiff = _diffs.FirstOrDefault();
        DiffsChanged?.Invoke();
    }

    public void SelectDiff(FileDiff diff)
    {
        SelectedDiff = diff;
        DiffsChanged?.Invoke();
    }

    public void Clear()
    {
        _diffs.Clear();
        SelectedDiff = null;
        DiffsChanged?.Invoke();
    }

    /// <summary>
    /// Generate a sample diff for demonstration.
    /// </summary>
    public FileDiff GenerateSampleDiff()
    {
        return new FileDiff
        {
            FileName = "Example.cs",
            FilePath = "src/Example.cs",
            Additions = 5,
            Deletions = 2,
            Hunks = new List<DiffHunk>
            {
                new DiffHunk
                {
                    OldStart = 1,
                    OldCount = 10,
                    NewStart = 1,
                    NewCount = 13,
                    Lines = new List<DiffLine>
                    {
                        new DiffLine { Type = DiffLineType.Context, Content = "namespace Example;", OldLineNumber = 1, NewLineNumber = 1 },
                        new DiffLine { Type = DiffLineType.Context, Content = "", OldLineNumber = 2, NewLineNumber = 2 },
                        new DiffLine { Type = DiffLineType.Context, Content = "public class Sample", OldLineNumber = 3, NewLineNumber = 3 },
                        new DiffLine { Type = DiffLineType.Context, Content = "{", OldLineNumber = 4, NewLineNumber = 4 },
                        new DiffLine { Type = DiffLineType.Removed, Content = "    public void OldMethod()", OldLineNumber = 5 },
                        new DiffLine { Type = DiffLineType.Removed, Content = "    {", OldLineNumber = 6 },
                        new DiffLine { Type = DiffLineType.Added, Content = "    public async Task NewMethodAsync()", NewLineNumber = 5 },
                        new DiffLine { Type = DiffLineType.Added, Content = "    {", NewLineNumber = 6 },
                        new DiffLine { Type = DiffLineType.Added, Content = "        await Task.Delay(100);", NewLineNumber = 7 },
                        new DiffLine { Type = DiffLineType.Context, Content = "        Console.WriteLine(\"Hello!\");", OldLineNumber = 7, NewLineNumber = 8 },
                        new DiffLine { Type = DiffLineType.Context, Content = "    }", OldLineNumber = 8, NewLineNumber = 9 },
                        new DiffLine { Type = DiffLineType.Added, Content = "", NewLineNumber = 10 },
                        new DiffLine { Type = DiffLineType.Added, Content = "    public int Value { get; set; }", NewLineNumber = 11 },
                        new DiffLine { Type = DiffLineType.Context, Content = "}", OldLineNumber = 9, NewLineNumber = 12 },
                    }
                }
            }
        };
    }
}
