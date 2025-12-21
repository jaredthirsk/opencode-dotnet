# AgUi.IDE.Desktop TODO

## Project Setup & Configuration

### MAUI Configuration
- [ ] Configure MAUI project
  - [ ] Verify .NET 9 target frameworks (net9.0-windows, net9.0-maccatalyst)
  - [ ] Configure app resources
  - [ ] Setup application icon and splash screen
- [ ] Configure Blazor WebView
  - [ ] Enable Blazor WebView component
  - [ ] Configure script debugging (development)
  - [ ] Setup error handling

### OpenCode Integration
- [ ] Configure OpenCode Serve API client
  - [ ] Add IOpenCodeClient registration in MauiProgram.cs
  - [ ] Configure API endpoint URL
  - [ ] Setup authentication if required
- [ ] Configure project management services
  - [ ] Add IProjectManager service
  - [ ] Implement project discovery/loading
- [ ] Implement OpenCodeProcessManager
  - [ ] Start/stop server process
  - [ ] Monitor process health
  - [ ] Handle process restarts

## Native UI Implementation

### Main Application Shell
- [ ] Implement AppShell
  - [ ] Tab-based or navigation drawer layout
  - [ ] Menu for project operations
  - [ ] Settings access
- [ ] Implement main navigation
  - [ ] IDE page
  - [ ] Settings page
  - [ ] About page

### Main IDE Page
- [ ] Create MainPage XAML
  - [ ] Project selector
  - [ ] Load project button
  - [ ] Status bar
  - [ ] Blazor WebView container
- [ ] Implement MainPage code-behind
  - [ ] Project selection dialog
  - [ ] Initialize Blazor IDE component
  - [ ] Handle window events

## Blazor Components Integration

### IDE Component
- [ ] Create or reference IDEView from LionFire.OpenCode.Blazor
  - [ ] Embed in Blazor WebView
  - [ ] Configure BlazorWebView
  - [ ] Handle component lifecycle
- [ ] Integrate IDE features
  - [ ] FilesPanel for project navigation
  - [ ] Editor for code viewing/editing
  - [ ] DiffPanel for changes
  - [ ] ChatPanel for AI assistance
  - [ ] TerminalPanel for commands

### Routing
- [ ] Setup Blazor routing
  - [ ] Configure Routes.razor
  - [ ] Define route parameters
  - [ ] Layout components

## Process Management

### OpenCodeProcessManager
- [ ] Implement process startup
  - [ ] Locate OpenCode executable
  - [ ] Setup command-line arguments
  - [ ] Configure environment variables
  - [ ] Start process and capture output
- [ ] Implement process monitoring
  - [ ] Health checks
  - [ ] Memory and CPU monitoring
  - [ ] Handle crashes and restarts
- [ ] Implement process shutdown
  - [ ] Graceful shutdown
  - [ ] Force kill if needed
  - [ ] Cleanup resources
- [ ] Add event notifications
  - [ ] Process started/stopped
  - [ ] Health status changes
  - [ ] Error notifications

## File Management

### Platform-Specific File Access
- [ ] Implement file browser integration
  - [ ] Windows: Use Windows.Storage APIs
  - [ ] macOS: Use NSFileManager or UIDocumentPickerViewController
- [ ] Implement project folder selection
  - [ ] Browse dialog
  - [ ] Recent projects list
  - [ ] Folder validation

### Terminal Integration
- [ ] Implement terminal in TerminalPanel
  - [ ] Execute commands via OpenCode API
  - [ ] Display output with ANSI colors
  - [ ] Support interactive sessions

## Platform-Specific Features

### Windows Implementation
- [ ] Windows platform setup
  - [ ] Configure SupportedOSPlatformVersion
  - [ ] Windows-specific file paths
  - [ ] Windows-specific process handling
- [ ] Windows integration
  - [ ] Windows notification integration

### macOS Implementation
- [ ] macOS platform setup
  - [ ] Configure SupportedOSPlatformVersion
  - [ ] macOS-specific file paths
  - [ ] macOS-specific process handling
- [ ] macOS integration
  - [ ] macOS notification integration

## Styling & UX

### XAML Styling
- [ ] Create application resources
  - [ ] Color scheme
  - [ ] Typography
  - [ ] Button styles
  - [ ] Input field styles
- [ ] Apply styles throughout application
  - [ ] Consistent margins/padding
  - [ ] Responsive layout
  - [ ] Touch/mouse input support

### Blazor Component Styling
- [ ] Apply CSS to Blazor components
  - [ ] Dark/light theme support
  - [ ] Responsive design for embedded WebView
  - [ ] High DPI support

### Accessibility
- [ ] XAML accessibility
  - [ ] Automation IDs for UI elements
  - [ ] Screen reader support
- [ ] Blazor component accessibility
  - [ ] ARIA labels
  - [ ] Keyboard navigation

## Advanced Features

### Window Management
- [ ] Save/restore window state
  - [ ] Window size and position
  - [ ] Maximized state
  - [ ] Multi-monitor awareness

### Settings & Preferences
- [ ] Create settings page
  - [ ] Theme selection (dark/light)
  - [ ] Font size options
  - [ ] Keyboard shortcuts
  - [ ] OpenCode API configuration
- [ ] Persist settings

### Recent Projects
- [ ] Track recent projects
  - [ ] Store in preferences
  - [ ] Quick access menu
  - [ ] Auto-load last project option

## Performance Optimization

- [ ] Optimize startup time
  - [ ] Lazy load components
  - [ ] Optimize bundle size
  - [ ] Preload critical resources
- [ ] Memory management
  - [ ] Monitor WebView memory usage
  - [ ] Implement cleanup for large files
  - [ ] Cache optimization
- [ ] Process efficiency
  - [ ] Monitor OpenCode process resources
  - [ ] Implement resource limits
  - [ ] Optimize API calls

## Testing & Debugging

- [ ] Unit tests
  - [ ] OpenCodeProcessManager tests
  - [ ] File management tests
  - [ ] Service tests
- [ ] Integration tests
  - [ ] Process management flow
  - [ ] File loading
  - [ ] API communication
- [ ] Manual testing
  - [ ] Windows build testing
  - [ ] macOS build testing
  - [ ] File operations
  - [ ] Process management
  - [ ] Terminal operations

### Debugging
- [ ] Enable Blazor debugging
  - [ ] Development configuration
  - [ ] Browser dev tools
- [ ] Process debugging
  - [ ] Log process events
  - [ ] Monitor stdout/stderr

## Deployment & Distribution

### Build Optimization
- [ ] Configure release builds
  - [ ] IL trimming for smaller size
  - [ ] ReadyToRun compilation

### Windows Distribution
- [ ] Create MSIX package
  - [ ] Sign application
  - [ ] Configure versioning
  - [ ] Test MSIX installation

### macOS Distribution
- [ ] Create .app bundle
  - [ ] Code signing
  - [ ] Notarization for distribution
  - [ ] Create DMG installer
  - [ ] Gatekeeper compatibility

### Documentation
- [ ] Create installation guide
- [ ] Create user guide
