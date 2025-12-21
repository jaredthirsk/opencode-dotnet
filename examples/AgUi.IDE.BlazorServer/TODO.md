# AgUi.IDE.BlazorServer TODO

## Project Setup & Configuration

- [x] Configure OpenCode Serve API client
  - [x] Add IOpenCodeClient registration in Program.cs
  - [x] Configure API endpoint URL
  - [ ] Setup authentication if required
- [ ] Configure project management services
  - [ ] Add IProjectManager service
  - [ ] Implement project discovery/loading
  - [ ] Setup file watching service
- [x] Configure Blazor Server options
  - [x] Set SignalR circuit options
  - [ ] Configure error boundaries
- [x] Verify package references
  - [x] Verify LionFire.OpenCode.Serve is referenced
  - [x] Verify LionFire.OpenCode.Blazor is referenced

## IDE Page Layout

- [x] Create main IDE page
  - [x] Add layout with header and main area
  - [ ] Integrate IDEView component from LionFire.OpenCode.Blazor (uses custom IdeLayout instead)
  - [ ] Responsive design for all screen sizes
- [x] Configure component properties
  - [x] Pass OpenCode client to components (via DI services)
  - [x] Setup event callbacks
  - [ ] Configure styling options

## Component Integration

- [x] Wire up FilesPanel
  - [ ] Load project structure from API (uses local filesystem currently)
  - [x] Display file tree
  - [x] Handle file selection events
- [x] Wire up DiffPanel
  - [x] Display session diffs
  - [ ] Handle apply/discard actions
- [x] Wire up ChatPanel
  - [x] Send messages to OpenCode
  - [x] Display streaming responses
  - [x] Handle message submission
- [x] Wire up TerminalPanel
  - [ ] Execute commands (display only currently)
  - [x] Display output
  - [ ] Handle interactive input

## Styling & Theme

- [x] Apply MudBlazor styling (instead of Bootstrap)
- [x] Configure dark mode (light mode pending)
- [ ] Responsive design for mobile
- [ ] Accessibility features

## Testing & Validation

- [ ] Test OpenCode API connectivity
- [ ] Test component initialization
- [ ] Test real-time updates
- [ ] Test error handling
- [ ] Performance testing

## Deployment

- [ ] Configure for production
  - [ ] SSL/TLS setup
  - [ ] API endpoint configuration
  - [ ] Performance optimization
- [ ] Create deployment instructions
- [ ] Document environment variables

## Manual review

- [ ] Make the UI more authentic to 'opencode serve'
