using BeanShare.Application.Features.Spaces.Commands;
using BeanShare.Contracts.Spaces;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Spaces;

public sealed class DemoteMemberEndpoint : Endpoint<DemoteMemberRequest, MemberActionResponse>
{
    private readonly IMediator _mediator;

    public DemoteMemberEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/spaces/{spaceId}/members/{userId}/demote");
        AllowAnonymous(); // TODO: Add authentication when OIDC is configured
        Summary(s =>
        {
            s.Summary = "Demote admin to member";
            s.Description = "Demotes a space admin to member role. Requires admin privileges. Cannot demote the last admin.";
            s.ExampleRequest = new DemoteMemberRequest
            {
                SpaceId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };
        });
    }

    public override async Task HandleAsync(DemoteMemberRequest req, CancellationToken ct)
    {
        var command = new DemoteMemberCommand(req.SpaceId, req.UserId);
        var result = await _mediator.Send(command, ct);

        if (result.IsFailure)
        {
            foreach (var error in result.Errors)
            {
                AddError(error.Code, error.Message);
            }
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var response = new MemberActionResponse
        {
            SpaceId = req.SpaceId,
            UserId = req.UserId,
            Message = "Member demoted to regular member successfully"
        };

        await SendOkAsync(response, ct);
    }
}