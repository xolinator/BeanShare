using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Entities;

/// <summary>
/// Tracks whether a global preset is enabled or disabled for a specific space.
/// By default (when no record exists), global presets are considered enabled.
/// Records are only created when a space admin explicitly disables a global preset.
/// </summary>
public sealed class SpaceGlobalPresetConfig : Entity
{
    public SpaceGlobalPresetConfigId Id { get; private set; }
    public SpaceId SpaceId { get; private set; }
    public GlobalPresetId GlobalPresetId { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private SpaceGlobalPresetConfig()
    {
        Id = new SpaceGlobalPresetConfigId(Guid.Empty);
        SpaceId = default!;
        GlobalPresetId = default!;
    }

    private SpaceGlobalPresetConfig(
        SpaceGlobalPresetConfigId id,
        SpaceId spaceId,
        GlobalPresetId globalPresetId,
        bool isEnabled,
        DateTime createdAt)
    {
        Id = id;
        SpaceId = spaceId;
        GlobalPresetId = globalPresetId;
        IsEnabled = isEnabled;
        CreatedAt = createdAt;
        UpdatedAt = null;
    }

    public static SpaceGlobalPresetConfig Create(
        SpaceId spaceId,
        GlobalPresetId globalPresetId,
        bool isEnabled,
        DateTime createdAt)
    {
        return new SpaceGlobalPresetConfig(
            new SpaceGlobalPresetConfigId(Guid.NewGuid()),
            spaceId,
            globalPresetId,
            isEnabled,
            createdAt);
    }

    public void Enable(DateTime updatedAt)
    {
        IsEnabled = true;
        UpdatedAt = updatedAt;
    }

    public void Disable(DateTime updatedAt)
    {
        IsEnabled = false;
        UpdatedAt = updatedAt;
    }

    public void SetEnabled(bool isEnabled, DateTime updatedAt)
    {
        IsEnabled = isEnabled;
        UpdatedAt = updatedAt;
    }

    protected override object GetId() => Id;
}

public sealed record SpaceGlobalPresetConfigId(Guid Value);
