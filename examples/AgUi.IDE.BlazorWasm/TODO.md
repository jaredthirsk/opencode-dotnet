# AgUi.IDE.BlazorWasm TODO

## Project Setup & Configuration

### Server Setup
- [ ] Configure OpenCode Serve API integration
  - [ ] Add IOpenCodeClient registration in Program.cs
  - [ ] Configure API endpoint URL
  - [ ] Setup authentication if required
- [ ] Configure project management services
  - [ ] Add IProjectManager service
  - [ ] Implement project discovery/loading
  - [ ] Setup file watching service
- [ ] Configure CORS for client communication
  - [ ] Allow requests from client app
- [ ] Configure response compression
  - [ ] Enable gzip compression
  - [ ] Optimize for WASM delivery

### Client Setup
- [ ] Configure HttpClient for API calls
  - [ ] Set base address to server
  - [ ] Add request/response interceptors
- [ ] Configure error handling
  - [ ] Handle network errors
  - [ ] Handle API errors

## Shared Models

### File and Project Models
- [ ] Define ProjectMetadata DTO
- [ ] Define FileEntry DTO
- [ ] Define FileContent DTO

### API Models
- [ ] Define API response wrapper

## Server API Implementation

### Project Endpoints
- [ ] GET `/api/projects` - List available projects
- [ ] GET `/api/projects/{projectId}` - Get project details

### File Endpoints
- [ ] GET `/api/projects/{projectId}/files` - Get project file structure
- [ ] GET `/api/projects/{projectId}/files/{filePath}` - Get file content

### Chat/Code Generation
- [ ] POST `/api/chat/send-message` - Send messages with streaming

## Client Implementation

### IDE Layout
- [ ] Create IDE main page
  - [ ] Layout with resizable panels
  - [ ] Integrate IDEView component from LionFire.OpenCode.Blazor
  - [ ] Responsive design

### Component Integration
- [ ] Wire up FilesPanel
  - [ ] Load project structure
  - [ ] Display file tree
  - [ ] Handle file selection
- [ ] Wire up DiffPanel
  - [ ] Display file differences
- [ ] Wire up ChatPanel
  - [ ] Send messages with file context
  - [ ] Display streaming responses
- [ ] Wire up TerminalPanel
  - [ ] Execute commands
  - [ ] Display output

## Performance Optimization

- [ ] WASM bundle optimization
  - [ ] Enable IL trimming
  - [ ] Analyze bundle size
- [ ] Caching strategy
  - [ ] Cache project structure
  - [ ] Cache file content
- [ ] API optimization
  - [ ] Pagination for large directories
  - [ ] Lazy load file content

## Styling & UX

- [ ] Apply Bootstrap styling
- [ ] Custom CSS for IDE
- [ ] Dark/light mode support
- [ ] Responsive design for mobile

## Testing & Validation

- [ ] Test OpenCode API connectivity
- [ ] Test client-server communication
- [ ] Test component initialization
- [ ] Test error handling
- [ ] Browser compatibility testing (Chrome/Firefox/Safari)

## Deployment

- [ ] Configure for production
  - [ ] API endpoint configuration
  - [ ] CORS settings
  - [ ] Security headers
- [ ] Performance tuning
  - [ ] Compression settings
  - [ ] Caching headers
- [ ] Create deployment instructions
