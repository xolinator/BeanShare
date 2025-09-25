using BeanShare.Application.Features.Spaces.Commands;
using BeanShare.Contracts.Spaces;
using FastEndpoints;
using MediatR;

namespace BeanShare.Api.Endpoints.Spaces;

public sealed class PromoteMemberEndpoint : Endpoint<PromoteMemberRequest, MemberActionResponse>
{
    private readonly IMediator _mediator;

    public PromoteMemberEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post("/api/spaces/{spaceId}/members/{userId}/promote");
        AllowAnonymous(); // TODO: Add authentication when OIDC is configured
        Summary(s =>
        {
            s.Summary = "Promote member to admin";
            s.Description = "Promotes a space member to admin role. Requires admin privileges.";
            s.ExampleRequest = new PromoteMemberRequest
            {
                SpaceId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };
        });
    }

    public override async Task HandleAsync(PromoteMemberRequest req, CancellationToken ct)
    {
        var command = new PromoteMemberCommand(req.SpaceId, req.UserId);
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
            Message = "Member promoted to admin successfully"
        };

        await SendOkAsync(response, ct);
    }
}