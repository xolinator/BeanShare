namespace BeanShare.SharedUi.Components.Consumption.Models;

public sealed class PresetOption
{
    public Guid? GlobalPresetId { get; set; }
    public Guid? SpacePresetId { get; set; }
    public string Name { get; set; } = "";
    public string CoffeeType { get; set; } = "";
    public string Preparation { get; set; } = "";
    public int Grams { get; set; }
    public string? Description { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsGlobal { get; set; }
    public int DisplayOrder { get; set; }

    public bool IsGlobalPreset => GlobalPresetId.HasValue;
    public bool IsSpacePreset => SpacePresetId.HasValue;
}
