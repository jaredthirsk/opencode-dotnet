# LionFire.OpenCode.Blazor

Blazor UI components for OpenCode - a faithful replication of the OpenCode web interface for .NET applications.

## Overview

LionFire.OpenCode.Blazor provides a complete set of Blazor components that replicate the OpenCode IDE interface, enabling developers to build web-based AI-assisted development environments using .NET and Blazor.

## Features

- **IDE Layout Components**: Complete OpenCode desktop UI replication
- **Session Management**: Create, load, and manage coding sessions
- **File Tree Navigation**: Hierarchical file explorer with drag-drop support
- **File Tabs**: Multi-file editing with tab management
- **Code Viewer**: Syntax-highlighted code editing (Monaco/Editor support)
- **Diff Viewer**: Compare files with diff visualization
- **Terminal Integration**: PTY terminal with xterm.js support
- **Chat Interface**: Session-based conversation with AI models
- **Model Selector**: Provider and model selection dropdown
- **Cost Tracking**: Token usage and cost monitoring
- **Theme Support**: Light and dark theme toggle
- **Responsive Design**: Works on desktop and tablet

## Installation

Add the NuGet package:

```bash
dotnet add package LionFire.OpenCode.Blazor
```

Or modify your `.csproj`:

```xml
<ItemGroup>
    <PackageReference Include="LionFire.OpenCode.Blazor" Version="0.1.0" />
</ItemGroup>
```

## Quick Start

### 1. Add to Blazor App

In `Program.cs`, register the services:

```csharp
using LionFire.OpenCode.Blazor;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add OpenCode services
builder.Services.AddOpenCodeServices();

await builder.Build().RunAsync();
```

### 2. Add Styles and Scripts

In `wwwroot/index.html`:

```html
<link href="_framework/LionFire.OpenCode.Blazor/styles/opencode-theme.css" rel="stylesheet" />
<link href="_framework/LionFire.OpenCode.Blazor/styles/components.css" rel="stylesheet" />

<!-- For xterm.js terminal support -->
<script src="https://cdn.jsdelivr.net/npm/xterm@5.0.0/lib/xterm.min.js"></script>
<link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/xterm@5.0.0/css/xterm.min.css" />
```

### 3. Use the Main Layout Component

In your page:

```razor
@page "/opencode"
@using LionFire.OpenCode.Blazor.Components.Layout

<OpenCodeLayout />
```

## Component Structure

### Layout Components
- `OpenCodeLayout`: Main IDE container
- `DesktopHeader`: Top bar with session selector and controls
- `Sidebar`: Collapsible file tree sidebar

### Session Components
- `SessionTurn`: Displays conversation turns
- `MessagePart`: Individual message content blocks
- `SessionList`: Session history list
- `SessionSelector`: Dropdown session selector

### File Components
- `FileTree`: Hierarchical file structure
- `FileTabs`: Multi-file tab bar
- `FileViewer`: Code editor viewer
- `FileIcon`: File type icon renderer

### Input Components
- `PromptInput`: Main user input area with @ and / autocomplete
- `Autocomplete`: Dropdown autocomplete suggestions
- `FileAttachment`: Drag-drop file attachment dialog

### Diff Components
- `DiffViewer`: Unified and side-by-side diff viewer
- `DiffSummary`: Diff statistics display
- `DiffLine`: Individual diff line renderer

### Terminal Components
- `PtyTerminal`: xterm.js PTY terminal
- `PtyTabs`: Multiple terminal tab management

### Shared Components
- `ModelSelector`: Provider/model picker
- `ProviderIcon`: Provider brand icons
- `CostTracker`: Token usage and cost display
- `ThemeToggle`: Light/dark theme toggle

## Services

### OpenCodeSessionManager
Manages session lifecycle and state:
- Load sessions from backend
- Create new sessions
- Switch between sessions
- Save session state
- Handle real-time synchronization

### DiffService
Handles diff parsing and formatting:
- Parse unified diff format
- Generate diff statistics
- Format for display with syntax highlighting
- Support side-by-side and unified views

### FileTreeBuilder
Builds and manages file tree structure:
- Build tree from directory paths
- Handle large directory structures
- Support .gitignore patterns
- Lazy-load subdirectories
- Cache for performance

### PtyManager
Manages PTY sessions and connections:
- Create PTY sessions
- Send/receive terminal input/output
- Manage multiple terminals
- Handle terminal resizing
- Support signal sending

## Configuration

TODO: Add configuration options for:
- Default theme preference
- Terminal settings
- Code editor configuration
- Session persistence
- Cost tracking options

## Development

### Building from Source

```bash
cd /src/opencode-dotnet/src/LionFire.OpenCode.Blazor
dotnet build
```

### Running Tests

```bash
dotnet test
```

## Integration with OpenCode Backend

This component library is designed to work with the OpenCode.Serve backend:

```csharp
builder.Services.AddOpenCodeServices(options =>
{
    options.ApiUrl = "https://api.opencode.local";
    options.WebSocketUrl = "wss://ws.opencode.local";
});
```

## Architecture Notes

### Real-time Updates

Components use SignalR for real-time synchronization:
- Session updates
- File changes
- Terminal output
- Cost tracking

### Performance

- Lazy-loading for file trees
- Virtual scrolling for large lists
- Diff content pagination
- Terminal output buffering

### State Management

Session state is managed through:
- `OpenCodeSessionManager` for session lifecycle
- Component-level state for UI
- localStorage for preferences
- Backend API for persistence

## TODO Items

The following features are marked for implementation:

1. **Code Editor**
   - Integrate Monaco Editor or CodeMirror
   - Syntax highlighting for all languages
   - Code completion and IntelliSense

2. **Terminal**
   - Complete xterm.js integration
   - WebSocket/SignalR communication
   - Multiple PTY session support

3. **Diff Viewer**
   - Complete diff parsing algorithm
   - Syntax highlighting in diffs
   - Side-by-side view implementation

4. **File Operations**
   - Create/delete/rename files
   - Upload file support
   - Directory operations

5. **Autocomplete**
   - @ file/path autocomplete
   - / command autocomplete
   - Context-aware suggestions

6. **Session Features**
   - Session export/import
   - Session collaboration
   - Session branching

## Contributing

TODO: Add contribution guidelines

## License

MIT License - See LICENSE file

## Dependencies

### NuGet Packages
- `MudBlazor` 7.0.0+ - Material Design component library
- `Microsoft.AspNetCore.Components.Web` 9.0.0+
- `LionFire.OpenCode.Serve` - Backend service library

### NPM Packages
- `xterm` - Terminal emulator
- `xterm-addon-*` - xterm add-ons for enhanced functionality

### External
- Bootstrap (via MudBlazor)
- Font Awesome icons (optional)

## Related Projects

- [OpenCode](https://github.com/your-org/opencode) - Main web application
- [LionFire.OpenCode.Serve](../LionFire.OpenCode.Serve/) - Backend services
- [LionFire.AgUi.Blazor](../../../ag-ui-blazor/) - Shared UI component library

## Support

For issues and questions:
- GitHub Issues: https://github.com/your-org/opencode-dotnet/issues
- Documentation: See `/docs` directory
- Examples: See `/samples` directory
