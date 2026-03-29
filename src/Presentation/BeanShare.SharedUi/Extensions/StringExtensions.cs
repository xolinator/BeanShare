namespace BeanShare.SharedUi.Extensions;

/// <summary>
/// Extension methods for string operations shared across UI components.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Extracts up to two initials from a person's name.
    /// Returns "?" for null/empty input.
    /// For a two-part name like "John Smith", returns "JS".
    /// For a single-part name like "John", returns "J".
    /// </summary>
    public static string GetInitials(this string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "?";

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
            return $"{parts[0][0]}{parts[^1][0]}".ToUpper();

        return parts[0][0].ToString().ToUpper();
    }
}
