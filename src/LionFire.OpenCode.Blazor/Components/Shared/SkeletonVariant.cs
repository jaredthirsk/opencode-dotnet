namespace LionFire.OpenCode.Blazor.Components.Shared;

/// <summary>
/// Variants for skeleton loading placeholders.
/// </summary>
public enum SkeletonVariant
{
    /// <summary>Single line of text</summary>
    Text,
    /// <summary>Multiple lines of text</summary>
    TextMultiline,
    /// <summary>Avatar placeholder</summary>
    Avatar,
    /// <summary>Card placeholder</summary>
    Card,
    /// <summary>Full message with avatar and text</summary>
    Message,
    /// <summary>List of items</summary>
    List,
    /// <summary>Custom content using ChildContent</summary>
    Custom
}
