# OpenCode .NET SDK DevContainer

Production-ready development container for the OpencodeAI .NET SDK project.

## Overview

This DevContainer provides a complete .NET 8 development environment optimized for:
- Building and testing .NET 8 SDK libraries
- NuGet package development and publishing
- xUnit testing with FluentAssertions and Moq
- Source generator development (System.Text.Json)
- Integration with VS Code C# Dev Kit

## Quick Start

### Prerequisites

- Docker Desktop or Docker in WSL2
- VS Code with Dev Containers extension
- WSL2 Ubuntu LTS (for Windows users)

### Important: WSL Native Filesystem Required

**CRITICAL:** This project is currently on `/mnt/c/src/opencode-dotnet` (Windows filesystem). For optimal performance and reliability, you should clone the project to WSL native filesystem:

```bash
# Clone to WSL native filesystem
mkdir -p /src
cd /src
git clone <repository-url> opencode-dotnet
cd opencode-dotnet

# Then open in VS Code
code .
```

**Why?** Docker volume mounts from `/mnt/c` paths can be unreliable and slow. WSL native paths (`/src/`, `/home/user/`) provide:
- Better performance (10-100x faster file I/O)
- Reliable volume mounts
- Fewer permission issues
- Consistent line endings

If you must use the Windows filesystem location, the devcontainer will still work but may have degraded performance.

### Opening the DevContainer

1. Open the project in VS Code
2. When prompted, click "Reopen in Container"
3. Or use Command Palette: `Dev Containers: Reopen in Container`
4. Wait for the container to build (first time only, ~5-10 minutes)

## What's Included

### Development Tools

- **.NET 8 SDK** - Latest .NET 8 SDK for building libraries
- **Node.js 22.17.1** - For web-based tooling and test runners
- **neovim** - Terminal-based editor
- **tmux** - Terminal multiplexer for session management
- **Git** - Version control
- **dos2unix** - Line ending conversion utilities

### .NET Tools

- `dotnet-ef` - Entity Framework Core tools (if needed later)
- `dotnet-format` - Code formatting
- `dotnet-watch` - File watcher for hot reload

### VS Code Extensions

- **C# Dev Kit** - Modern C# development experience
- **C# Extension** - IntelliSense, debugging, and refactoring
- **.NET Test Explorer** - Test runner integration
- **NuGet Package Manager** - Package management UI
- **EditorConfig** - Consistent code style
- **GitLens** - Git integration
- **Docker** - Docker file support
- **Code Spell Checker** - Spelling validation

### Persistent Volumes

The following data persists across container rebuilds:
- **vscode-server** - VS Code server installation
- **bash-history** - Command history
- **tmux-sessions** - Tmux session state
- **nuget-cache** - NuGet package cache

## Configuration

### Environment Variables

Set in `docker-compose.yml`:
- `DOTNET_CLI_TELEMETRY_OPTOUT=1` - Disable telemetry
- `ASPNETCORE_ENVIRONMENT=Development` - Development mode
- `DOTNET_USE_POLLING_FILE_WATCHER=true` - File watcher for hot reload
- `NUGET_XMLDOC_MODE=skip` - Skip XML documentation processing

### Volume Mounts

**Read-only volumes:**
- `/etc/hostname:/etc/host_hostname:ro` - Host hostname detection
- Dotfiles from host (tmux, git config, SSH keys)

**Read-write volumes:**
- `/src/opencode-dotnet` - Project source code
- `/mnt/c/build/packages` - NuGet package output (Windows C:)
- `/mnt/g/build/packages` - NuGet package output (Windows G:)
- Claude Code installation and data
- VS Code server data

### Customization

To customize the environment:

1. **Hostname**: Set `PROJECT_NAME` environment variable in `docker-compose.yml`
2. **User ID**: Set `USER_UID` and `USER_GID` if needed (default: 1000)
3. **Additional tools**: Add packages to `Dockerfile` apt-get install section
4. **VS Code settings**: Edit `devcontainer.json` customizations section

## Common Tasks

### Building the Project

```bash
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Publishing NuGet Package

```bash
# Build in release mode
dotnet build -c Release

# Pack the NuGet package
dotnet pack -c Release -o /mnt/c/build/packages

# Or specify version
dotnet pack -c Release -o /mnt/c/build/packages /p:Version=1.0.0
```

### Formatting Code

```bash
dotnet format
```

### Restoring Packages

```bash
dotnet restore
```

## Troubleshooting

### Container Won't Start

1. **Check Docker is running**: `docker ps`
2. **Rebuild without cache**: Use "Dev Containers: Rebuild Container Without Cache" from Command Palette
3. **Check logs**: View Docker logs in VS Code Output panel

### Permission Issues

If you encounter permission errors:
```bash
# Inside container
sudo chown -R dev:dev /src/opencode-dotnet
```

### Line Ending Issues

If shell scripts fail with "not found" or similar errors:
```bash
# Convert line endings
dos2unix /path/to/script.sh

# Check line endings
file /path/to/script.sh
# Should show "ASCII text" not "ASCII text, with CRLF line terminators"
```

### Slow Performance

If file operations are slow:
1. Verify you're using WSL native filesystem (`/src/`, not `/mnt/c/`)
2. Check Docker Desktop settings for WSL2 integration
3. Increase Docker memory allocation in Docker Desktop settings

### NuGet Cache Issues

If package restore fails with permission errors:
```bash
# Clear corrupted cache volume
docker volume rm opencode-dotnet_nuget-cache

# Rebuild container
```

## Development Workflow

### Recommended Workflow

1. Make code changes in VS Code
2. Run tests with `dotnet test` or use Test Explorer
3. Format code with `dotnet format` or save (auto-format enabled)
4. Commit changes using built-in Git integration
5. Build release package with `dotnet pack`

### Testing Strategy

- **Unit tests**: Test individual components with xUnit
- **Integration tests**: Test component interactions
- **Source generator tests**: Verify generated code correctness
- Use FluentAssertions for readable assertions
- Use Moq for mocking dependencies

### Publishing Workflow

1. Update version in `.csproj` file
2. Build and test: `dotnet build && dotnet test`
3. Pack release: `dotnet pack -c Release -o /mnt/c/build/packages`
4. Verify package contents: `unzip -l package.nupkg`
5. Publish to NuGet.org or private feed

## File Structure

```
.devcontainer/
├── devcontainer.json      # VS Code DevContainer configuration
├── docker-compose.yml     # Container orchestration and volumes
├── Dockerfile             # Container image definition
├── set-hostname.sh        # Hostname setup script
├── .tmux.conf            # Tmux configuration
├── .gitignore            # DevContainer-specific ignores
└── README.md             # This file
```

## Advanced Topics

### Debugging

The C# Dev Kit provides full debugging support:
1. Set breakpoints in code
2. Press F5 or use "Run and Debug" panel
3. Step through code, inspect variables, etc.

### Custom Scripts

Add custom scripts to `.devcontainer/scripts/` and mount them in `docker-compose.yml`.

### Multi-Container Setup

If you need databases or other services:
1. Add service definition to `docker-compose.yml`
2. Add network configuration
3. Update `depends_on` in devcontainer service

### Updating the DevContainer

To update base image or tools:
1. Edit `Dockerfile` or `docker-compose.yml`
2. Use "Dev Containers: Rebuild Container Without Cache"
3. Test thoroughly before committing changes

## Best Practices

1. **Commit .devcontainer/** - Include devcontainer configuration in version control
2. **Use .gitattributes** - Enforce LF line endings for shell scripts
3. **Keep Dockerfile minimal** - Only install what you need
4. **Document customizations** - Explain why you added specific tools
5. **Version lock base images** - Use `mcr.microsoft.com/dotnet/sdk:8.0` not `:latest`
6. **Test on clean builds** - Periodically rebuild without cache
7. **Use WSL filesystem** - Avoid `/mnt/c` paths for projects

## Resources

- [VS Code DevContainers Documentation](https://code.visualstudio.com/docs/devcontainers/containers)
- [.NET SDK Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Docker Best Practices](https://docs.docker.com/develop/dev-best-practices/)
- [WSL2 Best Practices](https://learn.microsoft.com/en-us/windows/wsl/compare-versions)

## Support

For issues or questions:
1. Check this README's troubleshooting section
2. Review Docker and VS Code logs
3. Consult the OpenCode .NET SDK documentation
4. Open an issue in the project repository

---

**Note**: This DevContainer is based on best practices from production .NET environments and includes critical lessons learned from WSL2/Docker Desktop deployments.
