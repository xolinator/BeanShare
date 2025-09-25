using BeanShare.Api.Endpoints.Spaces.Validators;
using BeanShare.Application.Features.Spaces.Commands;
using BeanShare.Contracts.Spaces;
using FastEndpoints;
using MediatR;

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
        Validator<DemoteMemberRequestValidator>();
        Summary(s =>
        {
            s.Summary = "Demote admin to member";
            s.Description = "Demotes a space admin to member role. Requires admin privileges. Cannot demote the last admin. Route parameters must match request body.";
            s.ExampleRequest = new DemoteMemberRequest
            {
                SpaceId = Guid.NewGuid(),
                UserId = Guid.NewGuid()
            };
        });
    }

    public override async Task HandleAsync(DemoteMemberRequest req, CancellationToken ct)
    {
        var routeSpaceId = Route<Guid>("spaceId");
        var routeUserId = Route<Guid>("userId");

        if (req.SpaceId != routeSpaceId)
        {
            AddError("RouteParameterMismatch", "Route SpaceId must match request SpaceId");
        }

        if (req.UserId != routeUserId)
        {
            AddError("RouteParameterMismatch", "Route UserId must match request UserId");
        }

        if (ValidationFailed)
        {
            await SendErrorsAsync(cancellation: ct);
            return;
        }

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