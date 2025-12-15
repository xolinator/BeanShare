using BeanShare.Domain.Common;
using BeanShare.Domain.ValueObjects;

namespace BeanShare.Domain.Entities;

/// <summary>
/// Represents a user's favorited preset for quick access buttons.
/// Supports both global presets (GlobalPresetId) and space-specific presets (PresetRecipeId).
/// Exactly one of GlobalPresetId or PresetRecipeId should be set.
/// </summary>
public sealed class UserPresetFavorite : Entity
{
    public UserPresetFavoriteId Id { get; private set; }
    public UserId UserId { get; private set; }
    public SpaceId SpaceId { get; private set; }
    public GlobalPresetId? GlobalPresetId { get; private set; }
    public PresetRecipeId? PresetRecipeId { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private UserPresetFavorite()
    {
        Id = new UserPresetFavoriteId(Guid.Empty);
        UserId = default!;
        SpaceId = default!;
    }

    private UserPresetFavorite(
        UserPresetFavoriteId id,
        UserId userId,
        SpaceId spaceId,
        GlobalPresetId? globalPresetId,
        PresetRecipeId? presetRecipeId,
        int displayOrder,
        DateTime createdAt)
    {
        Id = id;
        UserId = userId;
        SpaceId = spaceId;
        GlobalPresetId = globalPresetId;
        PresetRecipeId = presetRecipeId;
        DisplayOrder = displayOrder;
        CreatedAt = createdAt;
    }

    public static UserPresetFavorite CreateForGlobalPreset(
        UserId userId,
        SpaceId spaceId,
        GlobalPresetId globalPresetId,
        int displayOrder,
        DateTime createdAt)
    {
        return new UserPresetFavorite(
            new UserPresetFavoriteId(Guid.NewGuid()),
            userId,
            spaceId,
            globalPresetId,
            null,
            displayOrder,
            createdAt);
    }

    public static UserPresetFavorite CreateForSpacePreset(
        UserId userId,
        SpaceId spaceId,
        PresetRecipeId presetRecipeId,
        int displayOrder,
        DateTime createdAt)
    {
        return new UserPresetFavorite(
            new UserPresetFavoriteId(Guid.NewGuid()),
            userId,
            spaceId,
            null,
            presetRecipeId,
            displayOrder,
            createdAt);
    }

    public void UpdateDisplayOrder(int newOrder)
    {
        DisplayOrder = newOrder;
    }

    public bool IsGlobalPreset => GlobalPresetId is not null;
    public bool IsSpacePreset => PresetRecipeId is not null;

    protected override object GetId() => Id;
}

public sealed record UserPresetFavoriteId(Guid Value);
