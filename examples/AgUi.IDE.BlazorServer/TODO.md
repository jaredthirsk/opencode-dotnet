# AgUi.IDE.BlazorServer TODO

## Project Setup & Configuration

- [ ] Configure OpenCode Serve API client
  - [ ] Add IOpenCodeClient registration in Program.cs
  - [ ] Configure API endpoint URL
  - [ ] Setup authentication if required
- [ ] Configure project management services
  - [ ] Add IProjectManager service
  - [ ] Implement project discovery/loading
  - [ ] Setup file watching service
- [ ] Configure Blazor Server options
  - [ ] Set SignalR circuit options
  - [ ] Configure error boundaries
- [ ] Verify package references
  - [ ] Verify LionFire.OpenCode.Serve is referenced
  - [ ] Verify LionFire.OpenCode.Blazor is referenced

## IDE Page Layout

- [ ] Create main IDE page
  - [ ] Add layout with header and main area
  - [ ] Integrate IDEView component from LionFire.OpenCode.Blazor
  - [ ] Responsive design for all screen sizes
- [ ] Configure component properties
  - [ ] Pass OpenCode client to components
  - [ ] Setup event callbacks
  - [ ] Configure styling options

## Component Integration

- [ ] Wire up FilesPanel
  - [ ] Load project structure from API
  - [ ] Display file tree
  - [ ] Handle file selection events
- [ ] Wire up DiffPanel
  - [ ] Display session diffs
  - [ ] Handle apply/discard actions
- [ ] Wire up ChatPanel
  - [ ] Send messages to OpenCode
  - [ ] Display streaming responses
  - [ ] Handle message submission
- [ ] Wire up TerminalPanel
  - [ ] Execute commands
  - [ ] Display output
  - [ ] Handle interactive input

## Styling & Theme

- [ ] Apply Bootstrap styling
- [ ] Configure dark/light mode
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
