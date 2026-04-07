using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Extensions;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class SpaceEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;
    private readonly HttpClient _client;
    private static readonly Guid DefaultUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SecondUserId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public SpaceEndpointsTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task CreateSpace_WithValidRequest_ShouldReturnCreatedSpace()
    {
        var request = new CreateSpaceRequest { Name = "Test Coffee Space" };

        var response = await _client.PostAsJsonAsync("/api/spaces", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdSpace = await response.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        createdSpace.Should().NotBeNull();
        createdSpace!.SpaceId.Should().NotBeEmpty();
        createdSpace.InviteCode.Should().NotBeNullOrEmpty();
        createdSpace.Message.Should().Contain("Test Coffee Space");
    }

    [Fact]
    public async Task CreateSpace_WithEmptyName_ShouldReturnBadRequest()
    {
        var request = new CreateSpaceRequest { Name = "" };

        var response = await _client.PostAsJsonAsync("/api/spaces", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserSpaces_ShouldReturnUserSpaces()
    {
        var createRequest = new CreateSpaceRequest { Name = "User Spaces Test" };
        await _client.PostAsJsonAsync("/api/spaces", createRequest);

        var response = await _client.GetAsync("/api/spaces");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var spaces = await response.Content.ReadFromJsonAsync<GetUserSpacesResponse>();
        spaces.Should().NotBeNull();
        spaces!.Spaces.Should().NotBeEmpty();
        spaces.Spaces.Should().Contain(s => s.Name == "User Spaces Test");
    }

    [Fact]
    public async Task JoinSpace_WithValidInviteCode_ShouldJoinSpace()
    {
        var createRequest = new CreateSpaceRequest { Name = "Join Test Space" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        var joinRequest = new JoinSpaceRequest { InviteCode = createdSpace!.InviteCode };

        _client.WithTestUser(SecondUserId, "second@test.com");
        var response = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);
        _client.AsDefaultUser();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var joinResponse = await response.Content.ReadFromJsonAsync<JoinSpaceResponse>();
        joinResponse.Should().NotBeNull();
        joinResponse!.SpaceId.Should().Be(createdSpace.SpaceId);
        joinResponse.SpaceName.Should().Be("Join Test Space");
    }

    [Fact]
    public async Task JoinSpace_WithInvalidInviteCode_ShouldReturnBadRequest()
    {
        var joinRequest = new JoinSpaceRequest { InviteCode = "INVALID" };

        var response = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSpaceById_WithValidId_ShouldReturnSpace()
    {
        var createRequest = new CreateSpaceRequest { Name = "Get By ID Test" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        var response = await _client.GetAsync($"/api/spaces/{createdSpace!.SpaceId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var space = await response.Content.ReadFromJsonAsync<GetSpaceByIdResponse>();
        space.Should().NotBeNull();
        space!.Id.Should().Be(createdSpace.SpaceId);
        space.Name.Should().Be("Get By ID Test");
        space.InviteCode.Should().Be(createdSpace.InviteCode);
    }

    [Fact]
    public async Task GetSpaceById_WithInvalidId_ShouldReturnNotFound()
    {
        var invalidId = Guid.NewGuid();

        var response = await _client.GetAsync($"/api/spaces/{invalidId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SpaceWorkflow_CreateJoinList_ShouldWorkEndToEnd()
    {
        var createRequest = new CreateSpaceRequest { Name = "Workflow Test Space" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        createdSpace.Should().NotBeNull();

        var joinRequest = new JoinSpaceRequest { InviteCode = createdSpace!.InviteCode };
        _client.WithTestUser(SecondUserId, "second@test.com");
        var joinResponse = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);
        _client.AsDefaultUser();
        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await _client.GetAsync("/api/spaces");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var spaces = await listResponse.Content.ReadFromJsonAsync<GetUserSpacesResponse>();
        spaces!.Spaces.Should().Contain(s => s.Name == "Workflow Test Space");

        var getResponse = await _client.GetAsync($"/api/spaces/{createdSpace.SpaceId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var space = await getResponse.Content.ReadFromJsonAsync<GetSpaceByIdResponse>();
        space!.Name.Should().Be("Workflow Test Space");
        space.Members.Should().NotBeEmpty();
        space.Members.Should().Contain(m => m.Role == "Admin");
    }

    [Fact]
    public async Task PromoteMember_WithValidRequest_ShouldReturnOk()
    {
        var createRequest = new CreateSpaceRequest { Name = "Promote Test Space" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        _client.WithTestUser(SecondUserId, "second@test.com");
        var joinRequest = new JoinSpaceRequest { InviteCode = createdSpace!.InviteCode };
        var joinResponse = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);
        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _client.AsDefaultUser();
        var getResponse = await _client.GetAsync($"/api/spaces/{createdSpace.SpaceId}");
        var space = await getResponse.Content.ReadFromJsonAsync<GetSpaceByIdResponse>();
        var memberToPromote = space!.Members.FirstOrDefault(m => m.UserId == SecondUserId);

        memberToPromote.Should().NotBeNull("Second user should be a member");
        memberToPromote!.Role.Should().Be("Member");

        var promoteRequest = new PromoteMemberRequest
        {
            SpaceId = createdSpace.SpaceId,
            UserId = memberToPromote.UserId
        };
        var response = await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/members/{memberToPromote.UserId}/promote", promoteRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MemberActionResponse>();
        result.Should().NotBeNull();
        result!.SpaceId.Should().Be(createdSpace.SpaceId);
        result.UserId.Should().Be(memberToPromote.UserId);
        result.Message.Should().Contain("promoted");
    }

    [Fact]
    public async Task PromoteMember_WithInvalidSpaceId_ShouldReturnNotFound()
    {
        var invalidSpaceId = Guid.NewGuid();
        var invalidUserId = Guid.NewGuid();
        var promoteRequest = new PromoteMemberRequest
        {
            SpaceId = invalidSpaceId,
            UserId = invalidUserId
        };

        var response = await _client.PostAsJsonAsync($"/api/spaces/{invalidSpaceId}/members/{invalidUserId}/promote", promoteRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DemoteMember_WithValidRequest_ShouldReturnOk()
    {
        var createRequest = new CreateSpaceRequest { Name = "Demote Test Space" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        _client.WithTestUser(SecondUserId, "second@test.com");
        var joinRequest = new JoinSpaceRequest { InviteCode = createdSpace!.InviteCode };
        var joinResponse = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);
        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _client.AsDefaultUser();
        var promoteRequest = new PromoteMemberRequest
        {
            SpaceId = createdSpace.SpaceId,
            UserId = SecondUserId
        };
        await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/members/{SecondUserId}/promote", promoteRequest);

        var demoteRequest = new DemoteMemberRequest
        {
            SpaceId = createdSpace.SpaceId,
            UserId = SecondUserId
        };
        var response = await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/members/{SecondUserId}/demote", demoteRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MemberActionResponse>();
        result.Should().NotBeNull();
        result!.SpaceId.Should().Be(createdSpace.SpaceId);
        result.UserId.Should().Be(SecondUserId);
        result.Message.Should().Contain("demoted");
    }

    [Fact]
    public async Task DemoteMember_WithLastAdmin_ShouldReturnBadRequest()
    {
        var createRequest = new CreateSpaceRequest { Name = "Last Admin Test Space" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        var getResponse = await _client.GetAsync($"/api/spaces/{createdSpace!.SpaceId}");
        var space = await getResponse.Content.ReadFromJsonAsync<GetSpaceByIdResponse>();
        var adminMember = space!.Members.FirstOrDefault(m => m.Role == "Admin");

        var demoteRequest = new DemoteMemberRequest
        {
            SpaceId = createdSpace.SpaceId,
            UserId = adminMember!.UserId
        };
        var response = await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/members/{adminMember.UserId}/demote", demoteRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task MembershipWorkflow_PromoteAndDemote_ShouldWorkEndToEnd()
    {
        var createRequest = new CreateSpaceRequest { Name = "Membership Workflow Test" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        _client.WithTestUser(SecondUserId, "second@test.com");
        var joinRequest = new JoinSpaceRequest { InviteCode = createdSpace!.InviteCode };
        var joinResponse = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);
        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        _client.AsDefaultUser();
        var promoteRequest = new PromoteMemberRequest
        {
            SpaceId = createdSpace.SpaceId,
            UserId = SecondUserId
        };
        var promoteResponse = await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/members/{SecondUserId}/promote", promoteRequest);
        promoteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getAfterPromoteResponse = await _client.GetAsync($"/api/spaces/{createdSpace.SpaceId}");
        var spaceAfterPromote = await getAfterPromoteResponse.Content.ReadFromJsonAsync<GetSpaceByIdResponse>();
        var promotedMember = spaceAfterPromote!.Members.FirstOrDefault(m => m.UserId == SecondUserId);
        promotedMember!.Role.Should().Be("Admin");

        var demoteRequest = new DemoteMemberRequest
        {
            SpaceId = createdSpace.SpaceId,
            UserId = SecondUserId
        };
        var demoteResponse = await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/members/{SecondUserId}/demote", demoteRequest);
        demoteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var getAfterDemoteResponse = await _client.GetAsync($"/api/spaces/{createdSpace.SpaceId}");
        var spaceAfterDemote = await getAfterDemoteResponse.Content.ReadFromJsonAsync<GetSpaceByIdResponse>();
        var demotedMember = spaceAfterDemote!.Members.FirstOrDefault(m => m.UserId == SecondUserId);
        demotedMember!.Role.Should().Be("Member");
    }

    [Fact]
    public async Task UpdateSpace_WithValidRequest_ShouldReturnOk()
    {
        var createRequest = new CreateSpaceRequest { Name = "Original Space Name" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        var updateRequest = new UpdateSpaceRequest
        {
            SpaceId = createdSpace!.SpaceId,
            Name = "Updated Space Name"
        };
        var response = await _client.PutAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SpaceActionResponse>();
        result.Should().NotBeNull();
        result!.SpaceId.Should().Be(createdSpace.SpaceId);
        result.Name.Should().Be("Updated Space Name");
        result.Message.Should().Contain("updated");
    }

    [Fact]
    public async Task UpdateSpace_WithEmptyName_ShouldReturnBadRequest()
    {
        var createRequest = new CreateSpaceRequest { Name = "Test Space" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        var updateRequest = new UpdateSpaceRequest
        {
            SpaceId = createdSpace!.SpaceId,
            Name = ""
        };
        var response = await _client.PutAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeactivateSpace_WithValidRequest_ShouldReturnOk()
    {
        var createRequest = new CreateSpaceRequest { Name = "Space To Deactivate" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        var deactivateRequest = new DeactivateSpaceRequest
        {
            SpaceId = createdSpace!.SpaceId
        };
        var response = await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/deactivate", deactivateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<SpaceActionResponse>();
        result.Should().NotBeNull();
        result!.SpaceId.Should().Be(createdSpace.SpaceId);
        result.IsActive.Should().BeFalse();
        result.Message.Should().Contain("deactivated");
    }

    [Fact]
    public async Task RemoveMember_SelfRemoval_ShouldReturnOk()
    {
        var createRequest = new CreateSpaceRequest { Name = "Self Removal Test Space" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        _client.WithTestUser(SecondUserId, "second@test.com");
        var joinRequest = new JoinSpaceRequest { InviteCode = createdSpace!.InviteCode };
        var joinResponse = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);
        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var removeRequest = new RemoveMemberRequest
        {
            SpaceId = createdSpace.SpaceId,
            UserId = SecondUserId
        };
        var response = await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/members/{SecondUserId}/remove", removeRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<MemberActionResponse>();
        result.Should().NotBeNull();
        result!.SpaceId.Should().Be(createdSpace.SpaceId);
        result.UserId.Should().Be(SecondUserId);
        result.Message.Should().Contain("removed");

        _client.AsDefaultUser();
    }

    [Fact]
    public async Task RemoveMember_LastAdminSelfRemoval_ShouldReturnBadRequest()
    {
        var createRequest = new CreateSpaceRequest { Name = "Last Admin Removal Test" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        var getResponse = await _client.GetAsync($"/api/spaces/{createdSpace!.SpaceId}");
        var space = await getResponse.Content.ReadFromJsonAsync<GetSpaceByIdResponse>();
        var adminMember = space!.Members.FirstOrDefault(m => m.Role == "Admin");

        var removeRequest = new RemoveMemberRequest
        {
            SpaceId = createdSpace.SpaceId,
            UserId = adminMember!.UserId
        };
        var response = await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/members/{adminMember.UserId}/remove", removeRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegenerateInviteCode_WithValidRequest_ShouldReturnOk()
    {
        var createRequest = new CreateSpaceRequest { Name = "Invite Code Test Space" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        var regenerateRequest = new RegenerateInviteCodeRequest
        {
            SpaceId = createdSpace!.SpaceId
        };
        var response = await _client.PutAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/invite-code", regenerateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<InviteCodeResponse>();
        result.Should().NotBeNull();
        result!.SpaceId.Should().Be(createdSpace.SpaceId);
        result.InviteCode.Should().NotBe(createdSpace.InviteCode);
        result.InviteCode.Should().NotBeNullOrEmpty();
        result.Message.Should().Contain("regenerated");
    }

    [Fact]
    public async Task RegenerateInviteCode_WithDeactivatedSpace_ShouldReturnBadRequest()
    {
        var createRequest = new CreateSpaceRequest { Name = "Deactivated Space Test" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        var deactivateRequest = new DeactivateSpaceRequest { SpaceId = createdSpace!.SpaceId };
        await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/deactivate", deactivateRequest);

        var regenerateRequest = new RegenerateInviteCodeRequest
        {
            SpaceId = createdSpace.SpaceId
        };
        var response = await _client.PutAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/invite-code", regenerateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SpaceManagementWorkflow_UpdateDeactivateRegenerateInvite_ShouldWorkEndToEnd()
    {
        var createRequest = new CreateSpaceRequest { Name = "Management Workflow Test" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();


        var updateRequest = new UpdateSpaceRequest
        {
            SpaceId = createdSpace!.SpaceId,
            Name = "Updated Management Test Space"
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var regenerateRequest = new RegenerateInviteCodeRequest
        {
            SpaceId = createdSpace.SpaceId
        };
        var regenerateResponse = await _client.PutAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/invite-code", regenerateRequest);
        regenerateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var regenerateResult = await regenerateResponse.Content.ReadFromJsonAsync<InviteCodeResponse>();
        regenerateResult!.InviteCode.Should().NotBe(createdSpace.InviteCode);

        var joinRequest = new JoinSpaceRequest { InviteCode = regenerateResult.InviteCode };
        _client.WithTestUser(SecondUserId, "second@test.com");
        var joinResponse = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);
        _client.AsDefaultUser();
        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var deactivateRequest = new DeactivateSpaceRequest
        {
            SpaceId = createdSpace.SpaceId
        };
        var deactivateResponse = await _client.PostAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/deactivate", deactivateRequest);
        deactivateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var secondRegenerateResponse = await _client.PutAsJsonAsync($"/api/spaces/{createdSpace.SpaceId}/invite-code", regenerateRequest);
        secondRegenerateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}