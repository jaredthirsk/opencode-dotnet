# LionFire.OpenCode.Blazor Package Structure

## Overview

This document describes the complete structure of the LionFire.OpenCode.Blazor package, which provides Blazor UI components for the OpenCode IDE interface.

## Package Location

```
/src/opencode-dotnet/src/LionFire.OpenCode.Blazor/
```

## Complete File Structure

```
LionFire.OpenCode.Blazor/
├── LionFire.OpenCode.Blazor.csproj          # Project file (.NET 8.0 & 9.0)
├── README.md                                 # Package documentation and usage guide
├── CHANGELOG.md                              # Version history
├── PACKAGE_STRUCTURE.md                      # This file
├── _Imports.razor                            # Common using statements
│
├── Components/                               # Razor components
│   ├── Layout/
│   │   ├── OpenCodeLayout.razor             # Main IDE container (MAIN_LAYOUT)
│   │   ├── DesktopHeader.razor              # Top bar with controls
│   │   └── Sidebar.razor                    # Collapsible file tree
│   │
│   ├── Session/
│   │   ├── SessionTurn.razor                # Conversation turn display
│   │   ├── MessagePart.razor                # Individual message blocks
│   │   ├── SessionList.razor                # Session history
│   │   └── SessionSelector.razor            # Session dropdown
│   │
│   ├── Files/
│   │   ├── FileTree.razor                   # Hierarchical file explorer
│   │   ├── FileTabs.razor                   # Multi-file tab bar
│   │   ├── FileViewer.razor                 # Code editor display
│   │   └── FileIcon.razor                   # File type icons
│   │
│   ├── Input/
│   │   ├── PromptInput.razor                # User input area with autocomplete
│   │   ├── Autocomplete.razor               # @ and / suggestions
│   │   └── FileAttachment.razor             # Drag-drop file dialog
│   │
│   ├── Diffs/
│   │   ├── DiffViewer.razor                 # Diff display (unified/side-by-side)
│   │   ├── DiffSummary.razor                # Change statistics
│   │   └── DiffLine.razor                   # Individual diff line
│   │
│   ├── Terminal/
│   │   ├── PtyTerminal.razor                # xterm.js PTY terminal
│   │   └── PtyTabs.razor                    # Multiple terminal tabs
│   │
│   └── Shared/
│       ├── ModelSelector.razor              # Provider/model picker
│       ├── ProviderIcon.razor               # Provider brand icons
│       ├── CostTracker.razor                # Token/cost display
│       └── ThemeToggle.razor                # Light/dark theme toggle
│
├── Services/
│   ├── OpenCodeSessionManager.cs            # Session lifecycle management
│   ├── DiffService.cs                       # Diff parsing and formatting
│   ├── FileTreeBuilder.cs                   # File tree construction
│   └── PtyManager.cs                        # PTY terminal management
│
└── wwwroot/                                  # Static assets
    ├── styles/
    │   ├── opencode-theme.css               # Theme variables and base styles
    │   └── components.css                   # Component-specific styles
    ├── js/
    │   └── xterm-interop.js                 # xterm.js initialization
    └── icons/
        └── README.md                         # Icon asset guide
```

## Component Details

### Layout Components (3)
- **OpenCodeLayout**: Main container component combining all UI sections
- **DesktopHeader**: Header bar with session selector, model selector, cost tracker
- **Sidebar**: Collapsible left sidebar with file tree

### Session Components (4)
- **SessionTurn**: Displays individual conversation turns from session
- **MessagePart**: Renders different message types (text, code, markdown, diff, error)
- **SessionList**: Lists all available sessions with metadata
- **SessionSelector**: Dropdown for switching between sessions

### File Components (4)
- **FileTree**: Hierarchical file/directory tree with expand/collapse
- **FileTabs**: Tab bar for open files with drag-drop reordering
- **FileViewer**: Main code viewer with syntax highlighting support
- **FileIcon**: Returns appropriate icon for file types

### Input Components (3)
- **PromptInput**: Main input area with autocomplete triggers (@file, /command)
- **Autocomplete**: Dropdown list for completion suggestions
- **FileAttachment**: Dialog for drag-drop or browse file attachment

### Diff Components (3)
- **DiffViewer**: Displays diffs in unified or side-by-side format
- **DiffSummary**: Shows file-level change statistics
- **DiffLine**: Renders individual diff lines with type styling

### Terminal Components (2)
- **PtyTerminal**: Single PTY terminal with xterm.js integration
- **PtyTabs**: Manages multiple terminal tabs

### Shared Components (4)
- **ModelSelector**: Dropdown to select AI provider and model
- **ProviderIcon**: Displays provider branding icons
- **CostTracker**: Shows token usage and cost metrics
- **ThemeToggle**: Switches between light and dark themes

## Service Layer Details

### OpenCodeSessionManager
**Purpose**: Manage session lifecycle and state
**Key Methods**:
- LoadSessionsAsync() - Load available sessions
- CreateSessionAsync(name) - Create new session
- OpenSessionAsync(sessionId) - Load specific session
- SaveSessionAsync() - Persist session state
**Events**: SessionChanged

### DiffService
**Purpose**: Parse and format diff content
**Key Methods**:
- ParseDiffAsync(content) - Parse diff string
- GetDiffLinesAsync(content) - Extract individual lines
- GetDiffStatisticsAsync(content) - Calculate stats
- FormatDiffForDisplay(line) - Apply formatting
**Supports**: Unified diff, Git diff, Patch format

### FileTreeBuilder
**Purpose**: Build and manage file tree structures
**Key Methods**:
- BuildTreeAsync(path) - Build tree from directory
- BuildTreeAsync(path, options) - Build with custom options
- GetChildrenAsync(path) - Lazy-load children
- GetFileIcon(path) - Get icon for file
**Features**: .gitignore support, filtering, lazy-loading

### PtyManager
**Purpose**: Manage PTY terminal connections
**Key Methods**:
- CreateSessionAsync(directory) - Create PTY session
- SendInputAsync(sessionId, input) - Send terminal input
- ExecuteCommandAsync(sessionId, command) - Run command
- ResizeAsync(sessionId, cols, rows) - Resize terminal
- TerminateAsync(sessionId, force) - Terminate session
**Events**: Output, Terminated

## Styling

### Theme System
**File**: `wwwroot/styles/opencode-theme.css`
**Features**:
- CSS custom properties for all colors
- Light and dark mode variables
- Spacing scales
- Typography scales
- Shadow definitions
- Z-index management
- Transition timing

### Component Styles
**File**: `wwwroot/styles/components.css`
**Contains**:
- Layout component styling
- File tree styles
- Tab bar styles
- Editor styles
- Diff viewer styles
- Terminal styles
- Input styles
- Button styles
- Dialog/modal styles

## JavaScript Integration

### xterm.js Interop
**File**: `wwwroot/js/xterm-interop.js`
**Features**:
- Terminal initialization
- WebSocket/SignalR communication
- Theme switching
- Terminal resizing
- Input/output handling

## Dependencies

### NuGet Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.0" />
<PackageReference Include="MudBlazor" Version="7.0.0" />
<ProjectReference Include="../LionFire.OpenCode.Serve/LionFire.OpenCode.Serve.csproj" />
```

### Conditional References
- **LionFire.AgUi.Blazor**: Project reference if local repo exists, otherwise NuGet fallback

### NPM/CDN
- `xterm` v5.0.0+ - Terminal emulator
- Font Awesome (optional) - Icon library

## Project Configuration

### Target Frameworks
- .NET 8.0
- .NET 9.0

### Project Settings
- SDK: Microsoft.NET.Sdk.Razor
- Nullable: enabled
- Implicit using statements: enabled
- Documentation file generation: enabled
- Platform: Browser (web only)

### Package Metadata
- PackageId: LionFire.OpenCode.Blazor
- Version: 0.1.0
- Title: LionFire OpenCode Blazor
- License: MIT
- Tags: opencode, blazor, ui, components, ide

## Import Statements

The `_Imports.razor` file includes:
```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using MudBlazor
@using LionFire.OpenCode.Blazor.Components
@using LionFire.OpenCode.Blazor.Services
@using LionFire.OpenCode.Serve
```

## File Statistics

### Component Count
- Razor Components: 24
- C# Services: 4
- Total Components & Services: 28

### CSS Files
- Theme file: 1
- Component styles: 1

### JavaScript Files
- Interop files: 1

### Documentation
- README.md: Main documentation
- CHANGELOG.md: Version history
- PACKAGE_STRUCTURE.md: This file
- Icon README: Asset guide

## Implementation Status

### Completed
- Project structure and scaffolding
- Component and service templates
- Theme and styling system
- Documentation
- Service interfaces and models

### TODO (Marked with TODO Comments)
- Component implementations
- Service method implementations
- Monaco/CodeMirror integration
- xterm.js PTY terminal integration
- Diff parsing algorithm
- File operations (create, delete, rename)
- Autocomplete functionality
- Keyboard shortcuts and accessibility
- Unit and integration tests
- SignalR integration for real-time sync

## Usage Example

```razor
@page "/opencode"
@using LionFire.OpenCode.Blazor.Components.Layout

<OpenCodeLayout />
```

In Program.cs:
```csharp
builder.Services.AddOpenCodeServices();
```

## Architecture Notes

### Component Hierarchy
```
OpenCodeLayout
├── DesktopHeader
│   ├── SessionSelector
│   ├── ModelSelector
│   ├── CostTracker
│   └── ThemeToggle
├── Sidebar
│   └── FileTree
├── Layout Center
│   ├── FileTabs
│   └── FileViewer
├── Layout Right
│   ├── SessionTurn
│   │   └── MessagePart
│   │       ├── DiffViewer
│   │       └── Code Display
│   └── SessionList
└── Layout Bottom
    └── PtyTabs
        └── PtyTerminal
```

### State Management
- Session state: OpenCodeSessionManager
- Component-level state: Component @code blocks
- Preferences: localStorage
- Persistence: Backend API

### Real-time Updates
- SignalR for session synchronization
- WebSocket for terminal I/O
- Event-based notifications

## Next Steps for Implementation

1. Complete component @code implementations
2. Integrate code editor (Monaco or CodeMirror)
3. Implement xterm.js PTY integration
4. Add diff parsing algorithm
5. Implement file operations
6. Add keyboard shortcuts
7. Add unit tests
8. Add accessibility features
9. Implement SignalR integration
10. Create example/demo application

## Related Resources

- OpenCode Web: `/dv/opencode`
- Backend Services: `../LionFire.OpenCode.Serve/`
- UI Components: `../../../ag-ui-blazor/`
- Documentation: `/docs` directory
