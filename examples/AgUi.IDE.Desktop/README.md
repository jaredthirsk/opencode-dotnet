# AgUi.IDE.Desktop

A MAUI Blazor Hybrid desktop application providing a native OpenCode IDE experience for Windows and macOS.

## Overview

This is a desktop sample application that demonstrates:
- MAUI Blazor Hybrid architecture
- Native desktop integration
- IDE experience with shared Blazor components
- Cross-platform support (Windows, macOS)
- OpenCode process management
- System integration (file browser, terminal, etc.)

## Architecture

This project uses MAUI Blazor Hybrid, which allows:
- Blazor components rendered in a WebView
- Native XAML UI for shell and platform-specific features
- Shared C# business logic between web and native
- Full access to platform APIs via MAUI

## Project Structure

- `MauiProgram.cs` - Application entry point and service registration
- `App.xaml/cs` - Application definition and resources
- `AppShell.xaml/cs` - Shell navigation structure
- `MainPage.xaml/cs` - Main IDE page
- `Components/Routes.razor` - Blazor routing configuration
- `Services/OpenCodeProcessManager.cs` - OpenCode process lifecycle management
- `Platforms/Windows/` - Windows-specific code
- `Platforms/MacCatalyst/` - macOS-specific code

## Getting Started

### Prerequisites

- .NET 9 SDK
- Visual Studio 2022 or Visual Studio Code with MAUI extension
- Windows 10+ (for Windows builds)
- macOS 13.1+ (for macOS builds)

### Building for Windows

```bash
dotnet build -f net9.0-windows
```

### Building for macOS

```bash
dotnet build -f net9.0-maccatalyst
```

### Running

```bash
dotnet run -f net9.0-windows
# or
dotnet run -f net9.0-maccatalyst
```

## Features

- Full IDE interface with Blazor components
- File explorer and editor
- AI chat assistant
- Terminal emulator
- Diff viewer
- Native desktop window management
- OpenCode process management
- Cross-platform file access
- System integration

## Components Used

This application uses:
- MAUI Blazor WebView for rendering Blazor components
- OpenCode-specific components from `LionFire.OpenCode.Blazor`
- OpenCode Serve API for backend operations
- MVVM Community Toolkit for data binding

## Technology Stack

- **Framework**: MAUI (.NET 9)
- **UI Rendering**: Blazor (WebView-based)
- **UI Components**: Razor components + XAML
- **Data Binding**: MVVM Community Toolkit
- **Cross-platform**: Windows 10+ and macOS 13.1+

## Build Targets

- `net9.0-windows` - Windows 10 and newer
- `net9.0-maccatalyst` - macOS 13.1 and newer

## Platform-Specific Considerations

### Windows
- Full access to Windows APIs via WinRT
- Native window chrome and Aero effects
- Integration with Windows file explorer and terminal

### macOS
- Native macOS window chrome and effects
- Integration with macOS Finder and Terminal.app
- Support for Mac trackpad gestures

## Notes

This is a hybrid application combining native MAUI UI with Blazor components. The architecture provides:
- Fast native UI response for shell and navigation
- Rich interactive experience with Blazor components
- Full platform capabilities through MAUI APIs
- Shared business logic between web and desktop

See TODO.md for implementation details.
