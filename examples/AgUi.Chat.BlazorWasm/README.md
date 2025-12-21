# AgUi.Chat.BlazorWasm

A minimal Blazor WebAssembly application with Server backend providing an AI chat interface using OpenCode.

## Overview

This is a lightweight Blazor WASM sample application that demonstrates:
- Client-side rendering with Blazor WebAssembly
- Server-side API backend for OpenCode integration
- Client/Server communication patterns
- Chat UI with AI agent interaction
- Separation of concerns with shared project

## Architecture

The project is split into three parts:

1. **Client** (`AgUi.Chat.BlazorWasm.Client.csproj`)
   - WebAssembly client application
   - Razor components rendered on client
   - HttpClient calls to server API

2. **Server** (`AgUi.Chat.BlazorWasm.Server.csproj`)
   - ASP.NET Core hosting application
   - API endpoints for OpenCode integration
   - Serves static files for WASM app
   - Handles backend communication

3. **Shared** (`AgUi.Chat.BlazorWasm.Shared.csproj`)
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
- `Controllers/` - API endpoints for chat functionality

### Client
- `Program.cs` - WebAssembly entry point
- `App.razor` - Root client layout
- `Routes.razor` - Router configuration
- `Pages/Home.razor` - Home page with chat interface

### Shared
- Models and DTOs for Client/Server communication

## Features

- Client-side rendering with Blazor WebAssembly
- Server-side API backend
- Real-time chat with AI agents
- RESTful API communication
- Responsive design

## Performance

This architecture provides:
- Fast initial page load (after WASM download)
- Reduced server load (client-side rendering)
- Rich interactive experience
- Better offline capabilities (after initial load)

See TODO.md for implementation details.
