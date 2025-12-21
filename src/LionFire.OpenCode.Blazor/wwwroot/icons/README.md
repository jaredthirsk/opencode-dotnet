# OpenCode Blazor Icons

This directory contains icon assets for the OpenCode Blazor component library.

## Icon Sets

- **File Type Icons**: Icons for different file extensions (TypeScript, JavaScript, Python, C#, etc.)
- **Provider Icons**: Icons for AI provider logos (OpenAI, Anthropic, Google, etc.)
- **UI Icons**: General UI icons (folder, file, code, terminal, etc.)

## Usage

Icons are referenced in components using the following patterns:

- File type icons: `FileIcon.razor` component
- Provider icons: `ProviderIcon.razor` component
- UI icons: Inline emoji or SVG icons

## Note

TODO: Copy file type icons from OpenCode (/dv/opencode) if available.

The current implementation uses emoji fallbacks. Replace with actual icon files as needed.

### Recommended Icon Library

Consider using one of the following for production:

- **Font Awesome**: Popular, well-maintained icon library
- **Feather Icons**: Minimalist, clean design
- **Tabler Icons**: Modern, customizable icons
- **Material Design Icons**: Comprehensive icon set
- **GitHub Octicons**: GitHub's icon set

## Adding New Icons

1. Add icon file to this directory
2. Update the corresponding component (FileIcon.razor or ProviderIcon.razor)
3. Create mapping between file type/provider and icon file
