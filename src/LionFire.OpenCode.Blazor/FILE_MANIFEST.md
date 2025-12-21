# LionFire.OpenCode.Blazor - File Manifest

Complete listing of all files created in this package.

## Root Files

| File | Purpose |
|------|---------|
| `LionFire.OpenCode.Blazor.csproj` | Project file - targets .NET 8.0 & 9.0, defines dependencies |
| `_Imports.razor` | Global using statements for all components |
| `README.md` | Comprehensive usage guide and documentation |
| `CHANGELOG.md` | Version history and release notes |
| `PACKAGE_STRUCTURE.md` | Detailed package structure documentation |
| `FILE_MANIFEST.md` | This file - manifest of all files |

## Components/Layout (3 files)

| File | Purpose |
|------|---------|
| `OpenCodeLayout.razor` | Main IDE container - orchestrates all sub-components |
| `DesktopHeader.razor` | Header bar with session selector, model picker, cost tracker, theme toggle |
| `Sidebar.razor` | Collapsible left sidebar with file tree navigation |

## Components/Session (4 files)

| File | Purpose |
|------|---------|
| `SessionTurn.razor` | Displays individual conversation turns with parts and diffs |
| `MessagePart.razor` | Renders message content blocks (text, code, markdown, diff, error) |
| `SessionList.razor` | Lists all sessions with metadata and selection |
| `SessionSelector.razor` | Dropdown selector for switching between sessions with search |

## Components/Files (4 files)

| File | Purpose |
|------|---------|
| `FileTree.razor` | Hierarchical file/directory explorer with expand/collapse |
| `FileTabs.razor` | Tab bar for open files with drag-drop reordering |
| `FileViewer.razor` | Code editor display with syntax highlighting support |
| `FileIcon.razor` | Returns appropriate icons based on file type |

## Components/Input (3 files)

| File | Purpose |
|------|---------|
| `PromptInput.razor` | Main user input area with @ and / autocomplete triggers |
| `Autocomplete.razor` | Dropdown list for autocomplete suggestions |
| `FileAttachment.razor` | Drag-drop or browse dialog for file attachment |

## Components/Diffs (3 files)

| File | Purpose |
|------|---------|
| `DiffViewer.razor` | Displays diffs in unified or side-by-side format |
| `DiffSummary.razor` | Shows file-level change statistics and breakdown |
| `DiffLine.razor` | Renders individual diff line with type-based styling |

## Components/Terminal (2 files)

| File | Purpose |
|------|---------|
| `PtyTerminal.razor` | Single PTY terminal with xterm.js integration |
| `PtyTabs.razor` | Manages multiple terminal tabs with creation/closing |

## Components/Shared (4 files)

| File | Purpose |
|------|---------|
| `ModelSelector.razor` | Dropdown to select AI provider and model |
| `ProviderIcon.razor` | Displays provider branding icons/badges |
| `CostTracker.razor` | Shows token usage and cost metrics with breakdown |
| `ThemeToggle.razor` | Switches between light and dark themes |

## Services (4 files)

| File | Purpose |
|------|---------|
| `OpenCodeSessionManager.cs` | Manages session lifecycle: load, create, open, save, synchronize |
| `DiffService.cs` | Parses and formats diff content: unified, git, patch formats |
| `FileTreeBuilder.cs` | Builds and manages file tree structures with filtering/caching |
| `PtyManager.cs` | Manages PTY terminal sessions: create, input/output, resize, terminate |

## Styling (2 files)

| File | Purpose |
|------|---------|
| `wwwroot/styles/opencode-theme.css` | Theme variables, colors, spacing, typography, shadows, z-index |
| `wwwroot/styles/components.css` | Component-specific styles for all UI elements |

## JavaScript (1 file)

| File | Purpose |
|------|---------|
| `wwwroot/js/xterm-interop.js` | xterm.js initialization and Blazor interop wrapper |

## Assets (1 file)

| File | Purpose |
|------|---------|
| `wwwroot/icons/README.md` | Icon asset guide and recommendations |

## Summary Statistics

- **Total Files**: 36
- **Razor Components**: 24
- **C# Services**: 4
- **CSS Files**: 2
- **JavaScript Files**: 1
- **Markdown Documentation**: 5
- **Project Files**: 1 (.csproj)

## File Size Breakdown

### By Category
- Components: ~20KB
- Services: ~12KB
- Styling: ~18KB
- JavaScript: ~2KB
- Documentation: ~25KB
- Project Config: ~1KB

## Component Hierarchy

```
OpenCodeLayout (main entry point)
├── DesktopHeader
│   ├── SessionSelector
│   ├── ModelSelector
│   ├── CostTracker
│   └── ThemeToggle
├── Sidebar
│   └── FileTree
├── FileTabs
├── FileViewer
├── SessionTurn
│   └── MessagePart (x N)
│       ├── Text content
│       ├── Code blocks
│       ├── Markdown
│       └── DiffViewer
│           ├── DiffSummary
│           └── DiffLine (x N)
└── PtyTabs
    └── PtyTerminal
```

## Cross-Component Dependencies

### PromptInput
- Uses: Autocomplete, FileAttachment, FileIcon
- Events: OnPromptSubmitted

### SessionTurn
- Uses: MessagePart
- Displays: Code, Markdown, Diffs

### MessagePart
- Uses: DiffViewer (for diff content)
- Conditional rendering based on type

### FileTree
- Uses: FileIcon
- Events: OnFileSelected

### FileTabs
- Uses: FileIcon
- Events: OnFileSelected, OnFileClosed

### DiffViewer
- Uses: DiffSummary, DiffLine
- Displays: Unified or side-by-side diffs

## Service Integration Points

| Service | Used By | Purpose |
|---------|---------|---------|
| OpenCodeSessionManager | OpenCodeLayout, SessionSelector, SessionList | Session management |
| DiffService | DiffViewer, MessagePart | Diff processing |
| FileTreeBuilder | FileTree | Tree construction |
| PtyManager | PtyTerminal, PtyTabs | Terminal management |

## Asset References

### CSS
- opencode-theme.css: Imported in parent application
- components.css: Imported in parent application

### JavaScript
- xterm-interop.js: For terminal functionality
- xterm.js: External CDN dependency
- Font Awesome: Optional icon library

## Import Dependencies

### Global (_Imports.razor)
- System.Net.Http
- System.Net.Http.Json
- Microsoft.AspNetCore.Components.*
- MudBlazor
- LionFire.OpenCode.Blazor.* (namespace)
- LionFire.OpenCode.Serve

### Project References
- Microsoft.AspNetCore.Components.Web
- MudBlazor
- LionFire.OpenCode.Serve
- LionFire.AgUi.Blazor (conditional)

## Implementation Status

### Completed
- File structure and scaffolding ✓
- Component templates ✓
- Service interfaces ✓
- Theme system ✓
- Documentation ✓
- Project configuration ✓

### Pending
- Component logic implementation (marked with TODO)
- Service method implementations (marked with TODO)
- Unit and integration tests
- Example/demo application
- Full integration testing

## File Access Paths

All files relative to: `/src/opencode-dotnet/src/LionFire.OpenCode.Blazor/`

### Component Access
```csharp
using LionFire.OpenCode.Blazor.Components.Layout;
using LionFire.OpenCode.Blazor.Components.Session;
using LionFire.OpenCode.Blazor.Components.Files;
using LionFire.OpenCode.Blazor.Components.Input;
using LionFire.OpenCode.Blazor.Components.Diffs;
using LionFire.OpenCode.Blazor.Components.Terminal;
using LionFire.OpenCode.Blazor.Components.Shared;
```

### Service Access
```csharp
using LionFire.OpenCode.Blazor.Services;
```

## Special Files

### _Imports.razor
- **Purpose**: Global using statements
- **Loaded**: Automatically by Blazor for all .razor files
- **Content**: Common namespace imports

### OpenCodeLayout.razor
- **Purpose**: Entry point component
- **Root component**: Main IDE interface
- **Implements**: IAsyncDisposable for cleanup

### opencode-theme.css
- **Purpose**: Theme foundation
- **Content**: CSS custom properties (variables)
- **Usage**: All components inherit from root variables

### xterm-interop.js
- **Purpose**: JavaScript bridge to xterm.js
- **Exports**: OpenCodeTerminal class for Blazor

## Next Steps for Developers

1. Read `README.md` for package overview
2. Review `PACKAGE_STRUCTURE.md` for architecture details
3. Implement TODO items in components
4. Run `dotnet build` to verify compilation
5. Create test components to verify functionality
6. Integrate with OpenCode.Serve backend
7. Add unit tests for services
8. Create example application

## Version Information

- **Package Version**: 0.1.0
- **Target Frameworks**: .NET 8.0, .NET 9.0
- **License**: MIT
- **Repository**: https://github.com/lionfire/opencode-dotnet
