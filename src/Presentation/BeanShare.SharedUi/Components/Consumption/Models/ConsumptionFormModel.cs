using System.ComponentModel.DataAnnotations;

namespace BeanShare.SharedUi.Components.Consumption.Models;

public sealed class ConsumptionFormModel
{
    [Range(1, 500, ErrorMessage = "Grams must be between 1 and 500")]
    public int Grams { get; set; }

    public string? PresetName { get; set; }

    public Guid? PresetId { get; set; }

    public Guid? GlobalPresetId { get; set; }

    public Guid? SpacePresetId { get; set; }

    [Required]
    public DateTime ConsumedAt { get; set; } = DateTime.Now;

    public string ProductName { get; set; } = "";

    public string ProductBrand { get; set; } = "";

    public string ProductType { get; set; } = "";

    public Guid? ForUserId { get; set; }

    public List<Guid>? ForUserIds { get; set; }
}
