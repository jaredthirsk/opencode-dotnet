using System.Text;
using LionFire.OpenCode.Serve;
using LionFire.OpenCode.Serve.Models;
using LionFire.OpenCode.Serve.Models.Events;

namespace AgUi.IDE.BlazorServer.Services;

/// <summary>
/// Chat message model.
/// </summary>
public class ChatMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsUser { get; set; }
    public bool IsStreaming { get; set; }
    public List<ChatMessagePart> Parts { get; set; } = new();
}

/// <summary>
/// Part of a chat message (text, code, tool call, etc.).
/// </summary>
public class ChatMessagePart
{
    public ChatMessagePartType Type { get; set; }
    public string Content { get; set; } = "";
    public string? Language { get; set; }

    // Tool-specific properties
    public string? ToolName { get; set; }
    public string? ToolCallId { get; set; }
    public string? ToolState { get; set; } // pending, running, completed, error
    public object? ToolInput { get; set; }
    public object? ToolOutput { get; set; }
    public bool IsCollapsed { get; set; } = true;
}

public enum ChatMessagePartType
{
    Text,
    Code,
    CodeBlock,
    InlineCode,
    ToolCall,
    ToolResult,
    Thinking,
    Error
}

/// <summary>
/// Represents a tool execution in progress.
/// </summary>
public class ToolExecution
{
    public string Id { get; set; } = "";
    public string CallId { get; set; } = "";
    public string ToolName { get; set; } = "";
    public string State { get; set; } = "pending"; // pending, running, completed, error
    public object? Input { get; set; }
    public object? Output { get; set; }
    public bool HasPendingPermission { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// Service for managing chat conversations.
/// Supports both mock mode and real OpenCode backend via SSE.
/// </summary>
public class ChatService
{
    private readonly IOpenCodeClient? _client;
    private readonly PermissionService? _permissionService;
    private readonly IdeStateService? _ideState;
    private readonly ILogger<ChatService> _logger;
    private readonly List<ChatMessage> _messages = new();
    private readonly Dictionary<string, ToolExecution> _toolExecutions = new();

    private string? _sessionId;
    private string? _workspacePath;

    public ChatService(IOpenCodeClient? client = null, PermissionService? permissionService = null, IdeStateService? ideState = null, ILogger<ChatService>? logger = null)
    {
        _client = client;
        _permissionService = permissionService;
        _ideState = ideState;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ChatService>.Instance;
    }

    public event Action? MessagesChanged;
    public event Action<ChatMessage>? MessageStreaming;
    public event Action? ToolsChanged;

    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();
    public IReadOnlyDictionary<string, ToolExecution> ToolExecutions => _toolExecutions;

    public bool IsStreaming { get; private set; }

    /// <summary>
    /// Initialize with session information for real backend mode.
    /// </summary>
    public void Initialize(string? sessionId, string? workspacePath)
    {
        _sessionId = sessionId;
        _workspacePath = workspacePath;
    }

    public void AddUserMessage(string content)
    {
        var message = new ChatMessage
        {
            Content = content,
            IsUser = true,
            Parts = new List<ChatMessagePart>
            {
                new ChatMessagePart { Type = ChatMessagePartType.Text, Content = content }
            }
        };
        _messages.Add(message);
        MessagesChanged?.Invoke();
    }

    public async Task StreamAssistantResponseAsync(string userMessage, CancellationToken cancellationToken = default)
    {
        // Use real backend if available and initialized
        if (_client != null && _sessionId != null)
        {
            await StreamFromBackendAsync(userMessage, cancellationToken);
        }
        else
        {
            await StreamMockResponseAsync(userMessage, cancellationToken);
        }
    }

    private async Task StreamFromBackendAsync(string userMessage, CancellationToken cancellationToken)
    {
        IsStreaming = true;

        var assistantMessage = new ChatMessage
        {
            Content = "",
            IsUser = false,
            IsStreaming = true
        };
        _messages.Add(assistantMessage);
        MessagesChanged?.Invoke();

        // Track all parts by their ID to handle multiple responses
        var textParts = new Dictionary<string, string>();
        var reasoningParts = new Dictionary<string, string>();
        var toolParts = new Dictionary<string, ChatMessagePart>();

        try
        {
            // Create message request with selected model
            var request = new SendMessageRequest
            {
                Parts = new List<PartInput>
                {
                    PartInput.TextInput(userMessage)
                },
                Model = GetSelectedModel(),
                Agent = _ideState?.CurrentAgentId
            };

            _logger.LogInformation("Sending message with model: {ProviderId}/{ModelId}, agent: {Agent}",
                request.Model?.ProviderId ?? "(default)", request.Model?.ModelId ?? "(default)", request.Agent ?? "(default)");

            // Start SSE subscription for streaming updates
            var eventTask = Task.Run(async () =>
            {
                try
                {
                    await foreach (var ev in _client!.SubscribeToEventsAsync(_workspacePath, cancellationToken))
                    {
                        if (ev is MessagePartUpdatedEvent partEvent)
                        {
                            var part = partEvent.Properties?.Part;
                            var delta = partEvent.Properties?.Delta;
                            if (part?.SessionId != _sessionId) continue;

                            // Handle text parts (assistant response) - only assistant parts have Time.Start
                            if (part?.Type == "text" && part?.Time?.Start != null)
                            {
                                // Accumulate delta if available, otherwise use text
                                if (!string.IsNullOrEmpty(delta))
                                {
                                    textParts.TryGetValue(part.Id, out var existing);
                                    textParts[part.Id] = (existing ?? "") + delta;
                                }
                                else if (!string.IsNullOrEmpty(part.Text))
                                {
                                    textParts[part.Id] = part.Text;
                                }
                                _logger.LogDebug("Text part {Id}: {Length} chars (delta: {Delta})",
                                    part.Id, textParts.GetValueOrDefault(part.Id)?.Length ?? 0, delta?.Length ?? 0);
                            }
                            // Handle reasoning/thinking parts
                            else if (part?.Type == "reasoning")
                            {
                                // Accumulate delta if available, otherwise use text
                                if (!string.IsNullOrEmpty(delta))
                                {
                                    reasoningParts.TryGetValue(part.Id, out var existing);
                                    reasoningParts[part.Id] = (existing ?? "") + delta;
                                }
                                else if (!string.IsNullOrEmpty(part.Text))
                                {
                                    reasoningParts[part.Id] = part.Text;
                                }
                                _logger.LogDebug("Reasoning part {Id}: {Length} chars (delta: {Delta})",
                                    part.Id, reasoningParts.GetValueOrDefault(part.Id)?.Length ?? 0, delta?.Length ?? 0);
                            }
                            // Handle tool parts
                            else if (part?.Type == "tool" && part?.CallId != null)
                            {
                                var stateStatus = part.StateStatus ?? "pending";
                                var toolExec = new ToolExecution
                                {
                                    Id = part.Id,
                                    CallId = part.CallId,
                                    ToolName = part.Tool ?? "unknown",
                                    State = stateStatus,
                                    Input = part.Input,
                                    Output = part.Output,
                                    HasPendingPermission = _permissionService?.HasPendingPermission(_sessionId!, part.CallId) ?? false
                                };

                                if (stateStatus == "completed" || stateStatus == "error")
                                {
                                    toolExec.EndTime = DateTime.UtcNow;
                                }

                                _toolExecutions[part.CallId] = toolExec;

                                // Track tool part for message display
                                var toolMessagePart = new ChatMessagePart
                                {
                                    Type = ChatMessagePartType.ToolCall,
                                    ToolName = part.Tool ?? "unknown",
                                    ToolCallId = part.CallId,
                                    ToolState = stateStatus,
                                    ToolInput = part.Input,
                                    ToolOutput = part.Output,
                                    Content = part.Tool ?? "unknown"
                                };
                                toolParts[part.CallId] = toolMessagePart;

                                ToolsChanged?.Invoke();
                                _logger.LogDebug("Tool {Tool} state: {State}", part.Tool, stateStatus);
                            }
                        }
                        else if (ev is PermissionUpdatedEvent permEvent)
                        {
                            var permission = permEvent.Properties?.Permission;
                            if (permission != null && permission.SessionId == _sessionId)
                            {
                                _permissionService?.AddPermission(permission);
                                _logger.LogInformation("Permission requested: {Title}", permission.Title);

                                // Update tool execution to show it has pending permission
                                if (permission.CallId != null && _toolExecutions.TryGetValue(permission.CallId, out var tool))
                                {
                                    tool.HasPendingPermission = true;
                                    ToolsChanged?.Invoke();
                                }
                            }
                        }
                        else if (ev is PermissionRepliedEvent repliedEvent)
                        {
                            var props = repliedEvent.Properties;
                            if (props?.SessionId == _sessionId && props?.PermissionId != null)
                            {
                                _permissionService?.RemovePermission(_sessionId!, props.PermissionId);

                                // Update tool execution
                                foreach (var tool in _toolExecutions.Values.Where(t => t.HasPendingPermission))
                                {
                                    tool.HasPendingPermission = _permissionService?.HasPendingPermission(_sessionId!, tool.CallId) ?? false;
                                }
                                ToolsChanged?.Invoke();
                            }
                        }
                        else if (ev is SessionIdleEvent idleEvent && idleEvent.Properties?.SessionId == _sessionId)
                        {
                            _logger.LogInformation("Session idle - response complete");
                            break;
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in SSE subscription");
                }
            }, cancellationToken);

            // Send message (non-blocking)
            await _client!.PromptAsyncNonBlocking(_sessionId!, request, _workspacePath, cancellationToken);

            // Stream updates to UI
            var lastContent = "";
            while (!eventTask.IsCompleted && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50, cancellationToken);

                // Combine all text parts (there may be multiple text responses)
                var currentContent = string.Join("\n\n", textParts.Values.Where(t => !string.IsNullOrEmpty(t)));

                if (currentContent != lastContent)
                {
                    assistantMessage.Content = currentContent;
                    lastContent = currentContent;
                    MessageStreaming?.Invoke(assistantMessage);
                }
            }

            // Final update - build message parts in order: thinking, tools, text
            var finalParts = new List<ChatMessagePart>();

            // Add reasoning/thinking parts first
            if (reasoningParts.Count > 0)
            {
                var reasoning = string.Join("\n\n", reasoningParts.Values);
                finalParts.Add(new ChatMessagePart
                {
                    Type = ChatMessagePartType.Thinking,
                    Content = reasoning
                });
            }

            // Add tool parts
            foreach (var toolPart in toolParts.Values)
            {
                finalParts.Add(toolPart);
            }

            // Add text parts
            var finalContent = string.Join("\n\n", textParts.Values.Where(t => !string.IsNullOrEmpty(t)));
            if (!string.IsNullOrEmpty(finalContent))
            {
                var textMessageParts = ParseMessageParts(finalContent);
                finalParts.AddRange(textMessageParts);
            }

            assistantMessage.Content = finalContent;
            assistantMessage.IsStreaming = false;
            assistantMessage.Parts = finalParts;

            // If no parts at all, show informative message
            if (finalParts.Count == 0)
            {
                // Check if there are pending permissions
                var pendingPerms = _permissionService?.GetPendingPermissions(_sessionId!) ?? Array.Empty<Permission>();
                if (pendingPerms.Count > 0)
                {
                    assistantMessage.Content = "(Waiting for permission to continue...)";
                }
                else if (_toolExecutions.Values.Any(t => t.State == "running"))
                {
                    assistantMessage.Content = "(Processing tools...)";
                }
                else
                {
                    assistantMessage.Content = "(Response processing...)";
                }
                assistantMessage.Parts = new List<ChatMessagePart>
                {
                    new ChatMessagePart { Type = ChatMessagePartType.Text, Content = assistantMessage.Content }
                };
            }

            _logger.LogInformation("Response complete: {TextParts} text parts, {ReasoningParts} reasoning parts",
                textParts.Count, reasoningParts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error streaming from backend");
            assistantMessage.Content = $"Error: {ex.Message}";
            assistantMessage.IsStreaming = false;
            assistantMessage.Parts = new List<ChatMessagePart>
            {
                new ChatMessagePart { Type = ChatMessagePartType.Text, Content = assistantMessage.Content }
            };
        }
        finally
        {
            IsStreaming = false;
            MessagesChanged?.Invoke();
        }
    }

    private async Task StreamMockResponseAsync(string userMessage, CancellationToken cancellationToken)
    {
        IsStreaming = true;

        var assistantMessage = new ChatMessage
        {
            Content = "",
            IsUser = false,
            IsStreaming = true
        };
        _messages.Add(assistantMessage);
        MessagesChanged?.Invoke();

        // Simulate streaming response for demo
        var responses = new[]
        {
            "I understand you're asking about ",
            $"\"{userMessage}\". ",
            "Let me help you with that.\n\n",
            "Here's what I found:\n\n",
            "```csharp\n",
            "// Example code\n",
            "public class Example\n",
            "{\n",
            "    public void DoSomething()\n",
            "    {\n",
            "        Console.WriteLine(\"Hello!\");\n",
            "    }\n",
            "}\n",
            "```\n\n",
            "This should help you get started. ",
            "Let me know if you need anything else!"
        };

        foreach (var chunk in responses)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await Task.Delay(50, cancellationToken);
            assistantMessage.Content += chunk;
            MessageStreaming?.Invoke(assistantMessage);
        }

        assistantMessage.IsStreaming = false;
        assistantMessage.Parts = ParseMessageParts(assistantMessage.Content);
        IsStreaming = false;
        MessagesChanged?.Invoke();
    }

    private List<ChatMessagePart> ParseMessageParts(string content)
    {
        var parts = new List<ChatMessagePart>();
        var lines = content.Split('\n');
        var inCodeBlock = false;
        var codeContent = "";
        var codeLanguage = "";
        var textContent = "";

        foreach (var line in lines)
        {
            if (line.StartsWith("```"))
            {
                if (inCodeBlock)
                {
                    // End of code block
                    parts.Add(new ChatMessagePart
                    {
                        Type = ChatMessagePartType.CodeBlock,
                        Content = codeContent.TrimEnd('\n'),
                        Language = codeLanguage
                    });
                    codeContent = "";
                    codeLanguage = "";
                    inCodeBlock = false;
                }
                else
                {
                    // Start of code block - save any pending text
                    if (!string.IsNullOrWhiteSpace(textContent))
                    {
                        parts.Add(new ChatMessagePart
                        {
                            Type = ChatMessagePartType.Text,
                            Content = textContent.TrimEnd('\n')
                        });
                        textContent = "";
                    }
                    inCodeBlock = true;
                    codeLanguage = line.Length > 3 ? line.Substring(3).Trim() : "";
                }
            }
            else if (inCodeBlock)
            {
                codeContent += line + "\n";
            }
            else
            {
                textContent += line + "\n";
            }
        }

        // Add any remaining text
        if (!string.IsNullOrWhiteSpace(textContent))
        {
            parts.Add(new ChatMessagePart
            {
                Type = ChatMessagePartType.Text,
                Content = textContent.TrimEnd('\n')
            });
        }

        return parts;
    }

    public void Clear()
    {
        _messages.Clear();
        MessagesChanged?.Invoke();
    }

    /// <summary>
    /// Gets the currently selected model from IdeStateService.
    /// </summary>
    private ModelReference? GetSelectedModel()
    {
        if (_ideState == null) return null;
        if (string.IsNullOrEmpty(_ideState.CurrentProviderId) || string.IsNullOrEmpty(_ideState.CurrentModelId))
            return null;

        return new ModelReference
        {
            ProviderId = _ideState.CurrentProviderId,
            ModelId = _ideState.CurrentModelId
        };
    }
}
