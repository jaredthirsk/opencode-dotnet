using System.Text.Json.Serialization;
using LionFire.OpenCode.Serve.Models;
using LionFire.OpenCode.Serve.Models.Events;

namespace LionFire.OpenCode.Serve.Internal;

/// <summary>
/// Source-generated JSON serializer context for AOT compatibility and improved performance.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    PropertyNameCaseInsensitive = true)]
// Sessions
[JsonSerializable(typeof(Session))]
[JsonSerializable(typeof(List<Session>))]
[JsonSerializable(typeof(CreateSessionRequest))]
[JsonSerializable(typeof(UpdateSessionRequest))]
[JsonSerializable(typeof(ForkSessionRequest))]
[JsonSerializable(typeof(RevertSessionRequest))]
[JsonSerializable(typeof(InitSessionRequest))]
[JsonSerializable(typeof(ExecuteCommandRequest))]
[JsonSerializable(typeof(ExecuteShellRequest))]
[JsonSerializable(typeof(SummarizeSessionRequest))]
[JsonSerializable(typeof(SessionStatus))]
[JsonSerializable(typeof(SessionStatusIdle))]
[JsonSerializable(typeof(SessionStatusRetry))]
[JsonSerializable(typeof(SessionStatusBusy))]
// Messages
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(List<Message>))]
[JsonSerializable(typeof(MessageWithParts))]
[JsonSerializable(typeof(List<MessageWithParts>))]
[JsonSerializable(typeof(SendMessageRequest))]
[JsonSerializable(typeof(ModelReference))]
[JsonSerializable(typeof(MessageTime))]
[JsonSerializable(typeof(TokenUsage))]
[JsonSerializable(typeof(CacheUsage))]
[JsonSerializable(typeof(MessagePath))]
[JsonSerializable(typeof(MessageError))]
// Parts (simplified - Part and PartInput are now concrete with all fields)
[JsonSerializable(typeof(Part))]
[JsonSerializable(typeof(List<Part>))]
[JsonSerializable(typeof(PartInput))]
[JsonSerializable(typeof(List<PartInput>))]
[JsonSerializable(typeof(PartTime))]
// Files
[JsonSerializable(typeof(FileNode))]
[JsonSerializable(typeof(List<FileNode>))]
[JsonSerializable(typeof(FileContent))]
[JsonSerializable(typeof(FileDiff))]
[JsonSerializable(typeof(List<FileDiff>))]
[JsonSerializable(typeof(FileStatus))]
[JsonSerializable(typeof(Patch))]
[JsonSerializable(typeof(PatchHunk))]
// Permissions
[JsonSerializable(typeof(Permission))]
[JsonSerializable(typeof(List<Permission>))]
[JsonSerializable(typeof(PermissionResponse))]
// PTY
[JsonSerializable(typeof(Pty))]
[JsonSerializable(typeof(List<Pty>))]
[JsonSerializable(typeof(CreatePtyRequest))]
[JsonSerializable(typeof(UpdatePtyRequest))]
// MCP
[JsonSerializable(typeof(McpStatus))]
[JsonSerializable(typeof(List<McpStatus>))]
[JsonSerializable(typeof(McpStatusConnected))]
[JsonSerializable(typeof(McpStatusDisabled))]
[JsonSerializable(typeof(McpStatusFailed))]
[JsonSerializable(typeof(McpStatusNeedsAuth))]
[JsonSerializable(typeof(McpStatusNeedsClientRegistration))]
[JsonSerializable(typeof(AddMcpServerRequest))]
[JsonSerializable(typeof(McpConfig))]
[JsonSerializable(typeof(McpLocalConfig))]
[JsonSerializable(typeof(McpRemoteConfig))]
[JsonSerializable(typeof(McpOAuthConfig))]
// Providers
[JsonSerializable(typeof(Provider))]
[JsonSerializable(typeof(List<Provider>))]
[JsonSerializable(typeof(ProviderListResponse))]
[JsonSerializable(typeof(Dictionary<string, Model>))]
[JsonSerializable(typeof(Model))]
[JsonSerializable(typeof(List<Model>))]
[JsonSerializable(typeof(ModelCost))]
[JsonSerializable(typeof(ModelCacheCost))]
[JsonSerializable(typeof(ModelLimit))]
[JsonSerializable(typeof(ProviderAuth))]
[JsonSerializable(typeof(List<ProviderAuth>))]
// Other Models
[JsonSerializable(typeof(Project))]
[JsonSerializable(typeof(List<Project>))]
[JsonSerializable(typeof(Agent))]
[JsonSerializable(typeof(List<Agent>))]
[JsonSerializable(typeof(Command))]
[JsonSerializable(typeof(List<Command>))]
[JsonSerializable(typeof(Todo))]
[JsonSerializable(typeof(List<Todo>))]
[JsonSerializable(typeof(Symbol))]
[JsonSerializable(typeof(List<Symbol>))]
[JsonSerializable(typeof(VcsInfo))]
[JsonSerializable(typeof(PathInfo))]
[JsonSerializable(typeof(LspStatus))]
[JsonSerializable(typeof(FormatterStatus))]
[JsonSerializable(typeof(LionFire.OpenCode.Serve.Models.Range))]
// Configuration
[JsonSerializable(typeof(Dictionary<string, object>))]
// Events
[JsonSerializable(typeof(Event))]
[JsonSerializable(typeof(GlobalEvent))]
[JsonSerializable(typeof(SessionCreatedEvent))]
[JsonSerializable(typeof(SessionUpdatedEvent))]
[JsonSerializable(typeof(SessionDeletedEvent))]
[JsonSerializable(typeof(MessageUpdatedEvent))]
[JsonSerializable(typeof(List<string>))]
public partial class OpenCodeSerializerContext : JsonSerializerContext
{
}
