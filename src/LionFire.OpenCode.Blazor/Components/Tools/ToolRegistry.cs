namespace LionFire.OpenCode.Blazor.Components.Tools;

/// <summary>
/// Registry for tool renderers.
/// </summary>
public class ToolRegistry
{
    private readonly List<IToolRenderer> _renderers = new();
    private readonly GenericToolRenderer _fallback = new();

    public ToolRegistry()
    {
        // Register default renderers in order of priority
        _renderers.Add(new ReadToolRenderer());
        _renderers.Add(new WriteToolRenderer());
        _renderers.Add(new EditToolRenderer());
        _renderers.Add(new BashToolRenderer());
        _renderers.Add(new GlobToolRenderer());
        _renderers.Add(new GrepToolRenderer());
        _renderers.Add(new WebFetchToolRenderer());
        _renderers.Add(new TodoWriteToolRenderer());
        _renderers.Add(new TaskToolRenderer());
    }

    /// <summary>
    /// Get the renderer for a specific tool.
    /// </summary>
    public IToolRenderer GetRenderer(string toolName)
    {
        foreach (var renderer in _renderers)
        {
            if (renderer.CanHandle(toolName))
            {
                return renderer;
            }
        }
        return _fallback;
    }

    /// <summary>
    /// Register a custom tool renderer.
    /// </summary>
    public void RegisterRenderer(IToolRenderer renderer)
    {
        _renderers.Insert(0, renderer); // Custom renderers take priority
    }

    /// <summary>
    /// Get the icon for a tool.
    /// </summary>
    public string GetIcon(string toolName)
    {
        return GetRenderer(toolName).GetIcon(toolName);
    }

    /// <summary>
    /// Get a summary for a tool call.
    /// </summary>
    public string GetSummary(string toolName, object? input)
    {
        return GetRenderer(toolName).GetSummary(toolName, input);
    }
}

/// <summary>
/// Base class for tool renderers.
/// </summary>
public abstract class BaseToolRenderer : IToolRenderer
{
    public abstract IEnumerable<string> SupportedTools { get; }
    public abstract string DefaultIcon { get; }

    public virtual bool CanHandle(string toolName)
    {
        return SupportedTools.Contains(toolName, StringComparer.OrdinalIgnoreCase);
    }

    public virtual string GetIcon(string toolName) => DefaultIcon;

    public abstract string GetSummary(string toolName, object? input);

    protected string? GetStringProperty(object? input, string propertyName)
    {
        if (input == null) return null;

        if (input is System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.TryGetProperty(propertyName, out var prop))
            {
                return prop.GetString();
            }
            // Try snake_case
            var snakeCase = ToSnakeCase(propertyName);
            if (jsonElement.TryGetProperty(snakeCase, out prop))
            {
                return prop.GetString();
            }
        }
        else if (input is IDictionary<string, object> dict)
        {
            if (dict.TryGetValue(propertyName, out var val))
            {
                return val?.ToString();
            }
        }

        // Try reflection
        var propInfo = input.GetType().GetProperty(propertyName);
        return propInfo?.GetValue(input)?.ToString();
    }

    private static string ToSnakeCase(string str)
    {
        return string.Concat(str.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString())).ToLower();
    }
}

// Tool-specific renderers

public class ReadToolRenderer : BaseToolRenderer
{
    public override IEnumerable<string> SupportedTools => new[] { "read", "Read" };
    public override string DefaultIcon => "visibility"; // Material icon name

    public override string GetSummary(string toolName, object? input)
    {
        var path = GetStringProperty(input, "file_path") ?? GetStringProperty(input, "path");
        if (!string.IsNullOrEmpty(path))
        {
            var fileName = Path.GetFileName(path);
            return $"Read {fileName}";
        }
        return "Read file";
    }
}

public class WriteToolRenderer : BaseToolRenderer
{
    public override IEnumerable<string> SupportedTools => new[] { "write", "Write" };
    public override string DefaultIcon => "create";

    public override string GetSummary(string toolName, object? input)
    {
        var path = GetStringProperty(input, "file_path") ?? GetStringProperty(input, "path");
        if (!string.IsNullOrEmpty(path))
        {
            var fileName = Path.GetFileName(path);
            return $"Write {fileName}";
        }
        return "Write file";
    }
}

public class EditToolRenderer : BaseToolRenderer
{
    public override IEnumerable<string> SupportedTools => new[] { "edit", "Edit" };
    public override string DefaultIcon => "edit";

    public override string GetSummary(string toolName, object? input)
    {
        var path = GetStringProperty(input, "file_path") ?? GetStringProperty(input, "path");
        if (!string.IsNullOrEmpty(path))
        {
            var fileName = Path.GetFileName(path);
            return $"Edit {fileName}";
        }
        return "Edit file";
    }
}

public class BashToolRenderer : BaseToolRenderer
{
    public override IEnumerable<string> SupportedTools => new[] { "bash", "Bash" };
    public override string DefaultIcon => "terminal";

    public override string GetSummary(string toolName, object? input)
    {
        var command = GetStringProperty(input, "command");
        var desc = GetStringProperty(input, "description");

        if (!string.IsNullOrEmpty(desc))
        {
            return desc;
        }

        if (!string.IsNullOrEmpty(command))
        {
            // Get first part of command
            var firstWord = command.Split(' ', 2)[0];
            return $"Run {firstWord}";
        }
        return "Run command";
    }
}

public class GlobToolRenderer : BaseToolRenderer
{
    public override IEnumerable<string> SupportedTools => new[] { "glob", "Glob" };
    public override string DefaultIcon => "search";

    public override string GetSummary(string toolName, object? input)
    {
        var pattern = GetStringProperty(input, "pattern");
        if (!string.IsNullOrEmpty(pattern))
        {
            return $"Find {pattern}";
        }
        return "Find files";
    }
}

public class GrepToolRenderer : BaseToolRenderer
{
    public override IEnumerable<string> SupportedTools => new[] { "grep", "Grep" };
    public override string DefaultIcon => "find_in_page";

    public override string GetSummary(string toolName, object? input)
    {
        var pattern = GetStringProperty(input, "pattern");
        if (!string.IsNullOrEmpty(pattern))
        {
            // Truncate long patterns
            var display = pattern.Length > 30 ? pattern[..27] + "..." : pattern;
            return $"Search: {display}";
        }
        return "Search in files";
    }
}

public class WebFetchToolRenderer : BaseToolRenderer
{
    public override IEnumerable<string> SupportedTools => new[] { "webfetch", "WebFetch" };
    public override string DefaultIcon => "public";

    public override string GetSummary(string toolName, object? input)
    {
        var url = GetStringProperty(input, "url");
        if (!string.IsNullOrEmpty(url))
        {
            try
            {
                var uri = new Uri(url);
                return $"Fetch {uri.Host}";
            }
            catch
            {
                return $"Fetch URL";
            }
        }
        return "Fetch web content";
    }
}

public class TodoWriteToolRenderer : BaseToolRenderer
{
    public override IEnumerable<string> SupportedTools => new[] { "todowrite", "TodoWrite" };
    public override string DefaultIcon => "checklist";

    public override string GetSummary(string toolName, object? input)
    {
        return "Update todo list";
    }
}

public class TaskToolRenderer : BaseToolRenderer
{
    public override IEnumerable<string> SupportedTools => new[] { "task", "Task" };
    public override string DefaultIcon => "assignment";

    public override string GetSummary(string toolName, object? input)
    {
        var desc = GetStringProperty(input, "description");
        if (!string.IsNullOrEmpty(desc))
        {
            return desc;
        }
        return "Launch agent";
    }
}

public class GenericToolRenderer : BaseToolRenderer
{
    public override IEnumerable<string> SupportedTools => Array.Empty<string>();
    public override string DefaultIcon => "build";

    public override bool CanHandle(string toolName) => true; // Fallback handles everything

    public override string GetSummary(string toolName, object? input)
    {
        return toolName;
    }
}
