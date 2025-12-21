# Changelog

All notable changes to LionFire.OpenCode.Blazor will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project structure and component scaffold
- Layout components (OpenCodeLayout, DesktopHeader, Sidebar)
- Session components (SessionTurn, MessagePart, SessionList, SessionSelector)
- File components (FileTree, FileTabs, FileViewer, FileIcon)
- Input components (PromptInput, Autocomplete, FileAttachment)
- Diff components (DiffViewer, DiffSummary, DiffLine)
- Terminal components (PtyTerminal, PtyTabs)
- Shared components (ModelSelector, ProviderIcon, CostTracker, ThemeToggle)
- Service layer (OpenCodeSessionManager, DiffService, FileTreeBuilder, PtyManager)
- Theme CSS with OpenCode color variables
- Component styles (components.css)
- xterm.js JavaScript interop
- Comprehensive README with installation and usage guide
- MIT License

### TODO
- Complete component implementations (currently have TODO placeholders)
- Integrate Monaco Editor or CodeMirror for code editing
- Complete xterm.js PTY terminal integration
- Implement diff parsing algorithm
- Add file operation handlers
- Implement autocomplete functionality
- Add keyboard shortcuts and accessibility
- Add unit and integration tests
- Complete code highlighting for diff viewer
- Session persistence and real-time synchronization via SignalR
- Cost tracking and billing integration

## [0.1.0] - TBD

Initial release placeholder.

---

## Version History Notes

### Pre-release Development
This library was created as a faithful replication of the OpenCode web interface (from /dv/opencode) for .NET/Blazor environments.

### Target Frameworks
- .NET 8.0
- .NET 9.0

### Key Dependencies
- MudBlazor 7.0.0+ - Component library
- xterm.js - Terminal emulation
- LionFire.OpenCode.Serve - Backend services

### Known Limitations
- Many components are scaffolded with TODO items for implementation
- Terminal integration depends on xterm.js (must be added via CDN)
- Code editor integration is planned but not yet implemented
- Diff viewer parsing is not yet implemented
- File operations are not yet implemented

### Migration Guide

When transitioning from this version to newer releases, please check the CHANGELOG for breaking changes.

### Contributing

See CONTRIBUTING.md for guidelines on how to contribute to this project.
