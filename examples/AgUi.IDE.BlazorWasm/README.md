# AgUi.IDE.BlazorWasm

A full-featured Blazor WebAssembly application with Server backend providing an integrated development environment using OpenCode.

## Overview

This is a comprehensive Blazor WASM sample application that demonstrates:
- Client-side rendering with Blazor WebAssembly
- Server-side API backend for OpenCode integration
- Complete IDE experience with multiple panels
- File explorer with project structure
- Code editor and diff viewer
- AI chat assistant for code generation
- Terminal emulator for command execution
- Uses shared UI components from `LionFire.OpenCode.Blazor`

## Architecture

The project is split into three parts:

1. **Client** (`AgUi.IDE.BlazorWasm.Client.csproj`)
   - WebAssembly client application
   - Razor components rendered on client
   - Uses pre-built components from `LionFire.OpenCode.Blazor`
   - HttpClient calls to server API

2. **Server** (`AgUi.IDE.BlazorWasm.Server.csproj`)
   - ASP.NET Core hosting application
   - API endpoints for OpenCode integration
   - Project and file management endpoints
   - Serves static files for WASM app
   - Handles backend communication

3. **Shared** (`AgUi.IDE.BlazorWasm.Shared.csproj`)
   - Shared models and utilities
   - Used by both Client and Server
   - Serialization/deserialization contracts

## Getting Started

1. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

2. Run the server (which hosts the WASM client):
   ```bash
   cd Server
   dotnet run
   ```

3. Navigate to `https://localhost:5001` in your browser

## Project Structure

### Server
- `Program.cs` - Application entry point and service configuration
- `App.razor` - Root application layout
- `Controllers/` - API endpoints for IDE functionality

### Client
- `Program.cs` - WebAssembly entry point
- `App.razor` - Root client layout
- `Routes.razor` - Router configuration
- `Pages/IDE.razor` - Main IDE page with multi-panel layout

### Shared
- Models and DTOs for Client/Server communication

## Features

- File explorer with project navigation
- Code editor with syntax highlighting
- Diff viewer for comparing files
- Chat interface with AI agents
- Terminal emulator
- Multi-panel resizable layout
- Real-time collaboration capabilities
- VS Code-like keyboard shortcuts

## Components Used

This application uses OpenCode-specific components from `LionFire.OpenCode.Blazor`:
- `IDEView` - Main IDE layout with resizable panels
- `FilesPanel` - Project file explorer
- `DiffPanel` - File diff viewer
- `ChatPanel` - AI chat interface
- `TerminalPanel` - Terminal emulator

## Benefits of WASM Architecture

- Fast initial page load (after WASM download)
- Reduced server load (client-side rendering)
- Rich interactive experience
- Better offline capabilities (after initial load)
- Scalable server infrastructure

See TODO.md for implementation details.
