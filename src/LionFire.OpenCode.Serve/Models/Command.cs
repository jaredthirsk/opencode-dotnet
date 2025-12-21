using System.Text.Json.Serialization;

namespace LionFire.OpenCode.Serve.Models;

/// <summary>
/// Represents a slash command.
/// </summary>
public record Command
{
    /// <summary>
    /// The command name.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// The command description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>
    /// The agent to use for this command.
    /// </summary>
    [JsonPropertyName("agent")]
    public string? Agent { get; init; }

    /// <summary>
    /// The model to use for this command.
    /// </summary>
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    /// <summary>
    /// The template text for the command.
    /// </summary>
    [JsonPropertyName("template")]
    public required string Template { get; init; }

    /// <summary>
    /// Whether this is a subtask command.
    /// </summary>
    [JsonPropertyName("subtask")]
    public bool? Subtask { get; init; }
}
