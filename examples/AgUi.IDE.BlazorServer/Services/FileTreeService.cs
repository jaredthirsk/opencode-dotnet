namespace AgUi.IDE.BlazorServer.Services;

/// <summary>
/// Service for managing file tree data and navigation.
/// </summary>
public class FileTreeService
{
    private readonly IdeStateService _ideState;

    public FileTreeService(IdeStateService ideState)
    {
        _ideState = ideState;
    }

    /// <summary>
    /// Event fired when a file is selected.
    /// </summary>
    public event Action<FileTreeNode>? FileSelected;

    /// <summary>
    /// Event fired when a directory is expanded.
    /// </summary>
    public event Action<FileTreeNode>? DirectoryExpanded;

    /// <summary>
    /// Get the root nodes for a given path.
    /// </summary>
    public async Task<List<FileTreeNode>> GetRootNodesAsync(string? rootPath = null)
    {
        var basePath = rootPath ?? _ideState.WorkspacePath ?? "/";

        try
        {
            if (Directory.Exists(basePath))
            {
                return await GetDirectoryContentsAsync(basePath);
            }
        }
        catch
        {
            // Fall through to mock data
        }

        // Return mock data for demo
        return new List<FileTreeNode>
        {
            new FileTreeNode
            {
                Name = "src",
                Path = System.IO.Path.Combine(basePath, "src"),
                IsDirectory = true,
                Icon = FileTreeNode.IconFolder
            },
            new FileTreeNode
            {
                Name = "Components",
                Path = System.IO.Path.Combine(basePath, "Components"),
                IsDirectory = true,
                Icon = FileTreeNode.IconFolder,
                Children = new List<FileTreeNode>
                {
                    new FileTreeNode
                    {
                        Name = "Pages",
                        Path = System.IO.Path.Combine(basePath, "Components", "Pages"),
                        IsDirectory = true,
                        Icon = FileTreeNode.IconFolder,
                        Children = new List<FileTreeNode>
                        {
                            new FileTreeNode { Name = "Home.razor", Path = "Components/Pages/Home.razor", Icon = FileTreeNode.IconRazor },
                            new FileTreeNode { Name = "IDE.razor", Path = "Components/Pages/IDE.razor", Icon = FileTreeNode.IconRazor }
                        }
                    },
                    new FileTreeNode
                    {
                        Name = "Layout",
                        Path = System.IO.Path.Combine(basePath, "Components", "Layout"),
                        IsDirectory = true,
                        Icon = FileTreeNode.IconFolder,
                        Children = new List<FileTreeNode>
                        {
                            new FileTreeNode { Name = "MainLayout.razor", Path = "Components/Layout/MainLayout.razor", Icon = FileTreeNode.IconRazor },
                            new FileTreeNode { Name = "IdeLayout.razor", Path = "Components/Layout/IdeLayout.razor", Icon = FileTreeNode.IconRazor }
                        }
                    },
                    new FileTreeNode
                    {
                        Name = "Shared",
                        Path = System.IO.Path.Combine(basePath, "Components", "Shared"),
                        IsDirectory = true,
                        Icon = FileTreeNode.IconFolder,
                        Children = new List<FileTreeNode>
                        {
                            new FileTreeNode { Name = "FileTree", Path = "Components/Shared/FileTree", IsDirectory = true, Icon = FileTreeNode.IconFolder },
                            new FileTreeNode { Name = "Chat", Path = "Components/Shared/Chat", IsDirectory = true, Icon = FileTreeNode.IconFolder },
                            new FileTreeNode { Name = "DiffViewer", Path = "Components/Shared/DiffViewer", IsDirectory = true, Icon = FileTreeNode.IconFolder },
                            new FileTreeNode { Name = "Terminal", Path = "Components/Shared/Terminal", IsDirectory = true, Icon = FileTreeNode.IconFolder }
                        }
                    }
                }
            },
            new FileTreeNode
            {
                Name = "Services",
                Path = System.IO.Path.Combine(basePath, "Services"),
                IsDirectory = true,
                Icon = FileTreeNode.IconFolder,
                Children = new List<FileTreeNode>
                {
                    new FileTreeNode { Name = "IdeStateService.cs", Path = "Services/IdeStateService.cs", Icon = FileTreeNode.IconCSharp },
                    new FileTreeNode { Name = "FileTreeService.cs", Path = "Services/FileTreeService.cs", Icon = FileTreeNode.IconCSharp }
                }
            },
            new FileTreeNode { Name = "Program.cs", Path = System.IO.Path.Combine(basePath, "Program.cs"), Icon = FileTreeNode.IconCSharp },
            new FileTreeNode { Name = "README.md", Path = System.IO.Path.Combine(basePath, "README.md"), Icon = FileTreeNode.IconMarkdown },
            new FileTreeNode { Name = "appsettings.json", Path = System.IO.Path.Combine(basePath, "appsettings.json"), Icon = FileTreeNode.IconJson }
        };
    }

    /// <summary>
    /// Get children of a directory.
    /// </summary>
    public async Task<List<FileTreeNode>> GetChildrenAsync(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                return await GetDirectoryContentsAsync(directoryPath);
            }
        }
        catch
        {
            // Return empty on error
        }

        return new List<FileTreeNode>();
    }

    private Task<List<FileTreeNode>> GetDirectoryContentsAsync(string path)
    {
        var nodes = new List<FileTreeNode>();

        // Add directories first
        foreach (var dir in Directory.GetDirectories(path).OrderBy(d => System.IO.Path.GetFileName(d)))
        {
            var name = System.IO.Path.GetFileName(dir);
            if (!name.StartsWith(".") && name != "bin" && name != "obj" && name != "node_modules")
            {
                nodes.Add(new FileTreeNode
                {
                    Name = name,
                    Path = dir,
                    IsDirectory = true,
                    Icon = FileTreeNode.IconFolder
                });
            }
        }

        // Add files
        foreach (var file in Directory.GetFiles(path).OrderBy(f => System.IO.Path.GetFileName(f)))
        {
            var name = System.IO.Path.GetFileName(file);
            if (!name.StartsWith("."))
            {
                nodes.Add(new FileTreeNode
                {
                    Name = name,
                    Path = file,
                    IsDirectory = false,
                    Icon = GetFileIcon(name)
                });
            }
        }

        return Task.FromResult(nodes);
    }

    private static string GetFileIcon(string fileName)
    {
        var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".cs" => FileTreeNode.IconCSharp,
            ".razor" => FileTreeNode.IconRazor,
            ".cshtml" => FileTreeNode.IconRazor,
            ".json" => FileTreeNode.IconJson,
            ".md" => FileTreeNode.IconMarkdown,
            ".js" => FileTreeNode.IconJavaScript,
            ".ts" => FileTreeNode.IconTypeScript,
            ".css" => FileTreeNode.IconCss,
            ".html" => FileTreeNode.IconHtml,
            ".xml" => FileTreeNode.IconXml,
            ".csproj" => FileTreeNode.IconProject,
            ".sln" => FileTreeNode.IconSolution,
            _ => FileTreeNode.IconFile
        };
    }

    /// <summary>
    /// Notify that a file was selected.
    /// </summary>
    public void SelectFile(FileTreeNode node)
    {
        if (!node.IsDirectory)
        {
            FileSelected?.Invoke(node);
        }
    }

    /// <summary>
    /// Notify that a directory was expanded.
    /// </summary>
    public void ExpandDirectory(FileTreeNode node)
    {
        if (node.IsDirectory)
        {
            DirectoryExpanded?.Invoke(node);
        }
    }
}

/// <summary>
/// Represents a node in the file tree.
/// </summary>
public class FileTreeNode
{
    // Icon constants for MudBlazor
    public const string IconFolder = "folder";
    public const string IconFile = "description";
    public const string IconCSharp = "code";
    public const string IconRazor = "web";
    public const string IconJson = "data_object";
    public const string IconMarkdown = "article";
    public const string IconJavaScript = "javascript";
    public const string IconTypeScript = "code";
    public const string IconCss = "style";
    public const string IconHtml = "html";
    public const string IconXml = "code";
    public const string IconProject = "folder_special";
    public const string IconSolution = "workspaces";

    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public bool IsDirectory { get; set; }
    public bool IsExpanded { get; set; }
    public bool IsLoading { get; set; }
    public bool IsSelected { get; set; }
    public string Icon { get; set; } = IconFile;
    public List<FileTreeNode> Children { get; set; } = new();

    /// <summary>
    /// Gets the appropriate MudBlazor icon string.
    /// </summary>
    public string GetMudIcon()
    {
        return Icon switch
        {
            IconFolder => IsExpanded ? MudBlazor.Icons.Material.Filled.FolderOpen : MudBlazor.Icons.Material.Filled.Folder,
            IconCSharp => MudBlazor.Icons.Custom.FileFormats.FileCode,
            IconRazor => MudBlazor.Icons.Custom.FileFormats.FileCode,
            IconJson => MudBlazor.Icons.Material.Filled.DataObject,
            IconMarkdown => MudBlazor.Icons.Material.Filled.Article,
            IconProject => MudBlazor.Icons.Material.Filled.FolderSpecial,
            IconSolution => MudBlazor.Icons.Material.Filled.Workspaces,
            _ => MudBlazor.Icons.Material.Filled.Description
        };
    }
}
