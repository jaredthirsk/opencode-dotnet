# AgUi.IDE.BlazorServer

A full-featured Blazor Server application providing an integrated development environment using OpenCode.

## Overview

This is a comprehensive Blazor Server sample application that demonstrates:
- Complete IDE experience with multiple panels
- File explorer with project structure
- Code editor and diff viewer
- AI chat assistant for code generation
- Terminal emulator for command execution
- Integration with OpenCode Serve API
- Uses shared UI components from `LionFire.OpenCode.Blazor`

## Architecture

- **Framework**: Blazor Server (.NET 9.0)
- **Rendering**: Interactive Server (using Blazor's SignalR connection)
- **Components**: Pre-built components from `LionFire.OpenCode.Blazor` package
- **API Integration**: OpenCode Serve API via LionFire.OpenCode.Serve
- **Real-time**: Blazor SignalR for real-time updates

## Getting Started

1. Restore NuGet packages:
   ```bash
   dotnet restore
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

3. Navigate to `https://localhost:5001` in your browser

## Project Structure

- `Program.cs` - Application entry point and service configuration
- `Components/App.razor` - Root application layout
- `Components/Routes.razor` - Router configuration
- `Components/Pages/IDE.razor` - Main IDE page with multi-panel layout

## Features

- File explorer with project navigation
- Code editor with syntax highlighting
- Diff viewer for comparing files
- Chat interface with AI agents
- Terminal emulator
- Real-time collaboration via Blazor SignalR
- VS Code-like keyboard shortcuts

## Components Used

This application uses OpenCode-specific components from `LionFire.OpenCode.Blazor`:
- `IDEView` - Main IDE layout with resizable panels
- `FilesPanel` - Project file explorer
- `DiffPanel` - File diff viewer
- `ChatPanel` - AI chat interface
- `TerminalPanel` - Terminal emulator

See TODO.md for implementation details.
