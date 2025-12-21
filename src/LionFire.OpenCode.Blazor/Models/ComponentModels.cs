namespace LionFire.OpenCode.Blazor.Models;

/// <summary>
/// Represents a part of a message in a session turn.
/// </summary>
public class MessagePartData
{
    public string? Type { get; set; }
    public string? Content { get; set; }
    public string? Language { get; set; }
    public string? FileName { get; set; }
    public string? FilePath { get; set; }
    public object? Metadata { get; set; }
}

/// <summary>
/// Represents an item in the autocomplete dropdown.
/// </summary>
public class AutocompleteItem
{
    public string? Label { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// Represents a file attached to a prompt.
/// </summary>
public class AttachedFile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
    public string? Path { get; set; }
    public byte[]? Content { get; set; }
    public string? MimeType { get; set; }
    public long Size { get; set; }
}

/// <summary>
/// Represents a prompt submission from the user.
/// </summary>
public class PromptSubmission
{
    public string? Text { get; set; }
    public List<AttachedFile> AttachedFiles { get; set; } = new();
}

/// <summary>
/// Represents a line in a diff view.
/// </summary>
public class DiffLineData
{
    public string? Type { get; set; } // "context", "added", "removed"
    public string? Content { get; set; }
    public int? LeftLineNum { get; set; }
    public int? RightLineNum { get; set; }
}

/// <summary>
/// Represents a file node in the file tree.
/// </summary>
public class FileTreeNode
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Path { get; set; }
    public bool IsDirectory { get; set; }
    public bool IsExpanded { get; set; }
    public List<FileTreeNode> Children { get; set; } = new();
}

/// <summary>
/// Represents an open file tab.
/// </summary>
public class FileTab
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public bool IsModified { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Represents a chat session.
/// </summary>
public class ChatSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Represents a session turn (user message + assistant response).
/// </summary>
public class SessionTurnData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? UserMessage { get; set; }
    public List<MessagePartData> AssistantParts { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsStreaming { get; set; }
}

/// <summary>
/// Represents an AI model provider.
/// </summary>
public class ModelProvider
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public List<AIModel> Models { get; set; } = new();
}

/// <summary>
/// Represents an AI model.
/// </summary>
public class AIModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? ProviderId { get; set; }
    public decimal? InputCostPer1k { get; set; }
    public decimal? OutputCostPer1k { get; set; }
}

/// <summary>
/// Represents a terminal/PTY tab.
/// </summary>
public class TerminalTab
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Title { get; set; }
    public bool IsActive { get; set; }
    public string? WorkingDirectory { get; set; }
}
