using BeanShare.Application.Features.Spaces.Queries;
using BeanShare.Contracts.Spaces;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Spaces;

public sealed class GetUserSpacesEndpoint : EndpointWithoutRequest<GetUserSpacesResponse>
{
    private readonly IMediator _mediator;

    public GetUserSpacesEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Get("/api/spaces");
        Summary(s =>
        {
            s.Summary = "Get user's coffee spaces";
            s.Description = "Retrieve all coffee spaces the current user is a member of";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var query = new GetUserSpacesQuery();
        var result = await _mediator.Send(query, ct);

        if (result.IsFailure)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Code, error.Message);
            }
            await SendErrorsAsync();
            return;
        }

        var response = new GetUserSpacesResponse
        {
            Spaces = result.Value.Spaces.Select(s => new SpaceListItem
            {
                Id = s.Id.Value,
                Name = s.Name,
                InviteCode = s.InviteCode,
                MemberCount = s.MemberCount,
                CreatedAt = s.CreatedAt,
                UserRole = "Member" // TODO: Map actual user role from membership
            }).ToList()
        };

        await SendOkAsync(response, ct);
    }
}