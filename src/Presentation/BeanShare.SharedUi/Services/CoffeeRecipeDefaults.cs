namespace BeanShare.SharedUi.Services;

/// <summary>
/// Standard coffee recipe names and their typical gram amounts.
/// Used by QR code creation, reassignment, and scan-confirmation forms.
/// </summary>
public static class CoffeeRecipeDefaults
{
    public static readonly IReadOnlyList<(string Name, int Grams)> Recipes = new[]
    {
        ("Espresso", 8),
        ("Double Espresso", 16),
        ("Filter Coffee", 15),
        ("Pour Over", 18),
        ("French Press", 12),
        ("Cold Brew", 20),
    };

    /// <summary>
    /// Returns the default gram amount for a known recipe, or 8 if unrecognized.
    /// </summary>
    public static int GetGrams(string recipeName)
    {
        foreach (var (name, grams) in Recipes)
        {
            if (name == recipeName)
                return grams;
        }

        return 8;
    }
}
