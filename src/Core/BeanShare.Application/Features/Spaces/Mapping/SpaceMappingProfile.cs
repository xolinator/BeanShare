using BeanShare.Application.Features.Spaces.Dtos;
using BeanShare.Domain.Aggregates.Space;
using Mapster;

namespace BeanShare.Application.Features.Spaces.Mapping;

public sealed class SpaceMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Space, SpaceDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.InviteCode, src => src.InviteCode.Value)
            .Map(dest => dest.CreatedBy, src => src.Members.First(m => m.Role == Domain.Enums.SpaceRole.Admin).UserId)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.MemberCount, src => src.Members.Count)
            .Map(dest => dest.Members, src => src.Members);

        config.NewConfig<Space, SpaceSummaryDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.InviteCode, src => src.InviteCode.Value)
            .Map(dest => dest.MemberCount, src => src.Members.Count)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        config.NewConfig<SpaceMembership, MembershipDto>()
            .Map(dest => dest.UserId, src => src.UserId)
            .Map(dest => dest.Email, src => "user@example.com") // Will be later retrieved from User Service
            .Map(dest => dest.Role, src => src.Role.ToString())
            .Map(dest => dest.JoinedAt, src => src.JoinedAt);
    }
}