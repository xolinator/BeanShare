using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class SpaceEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly PostgreSqlFixture _fixture;
    private readonly HttpClient _client;

    public SpaceEndpointsTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
        _client = _fixture.CreateClient();
    }

    [Fact]
    public async Task CreateSpace_WithValidRequest_ShouldReturnCreatedSpace()
    {
        // Arrange
        var request = new CreateSpaceRequest { Name = "Test Coffee Space" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/spaces", request);

        // Assert
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
        // Arrange
        var request = new CreateSpaceRequest { Name = "" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/spaces", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetUserSpaces_ShouldReturnUserSpaces()
    {
        // Arrange - Create a space first
        var createRequest = new CreateSpaceRequest { Name = "User Spaces Test" };
        await _client.PostAsJsonAsync("/api/spaces", createRequest);

        // Act
        var response = await _client.GetAsync("/api/spaces");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var spaces = await response.Content.ReadFromJsonAsync<GetUserSpacesResponse>();
        spaces.Should().NotBeNull();
        spaces!.Spaces.Should().NotBeEmpty();
        spaces.Spaces.Should().Contain(s => s.Name == "User Spaces Test");
    }

    [Fact]
    public async Task JoinSpace_WithValidInviteCode_ShouldJoinSpace()
    {
        // Arrange - Create a space first
        var createRequest = new CreateSpaceRequest { Name = "Join Test Space" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        var joinRequest = new JoinSpaceRequest { InviteCode = createdSpace!.InviteCode };

        // Act
        var response = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var joinResponse = await response.Content.ReadFromJsonAsync<JoinSpaceResponse>();
        joinResponse.Should().NotBeNull();
        joinResponse!.SpaceId.Should().Be(createdSpace.SpaceId);
        joinResponse.SpaceName.Should().Be("Join Test Space");
    }

    [Fact]
    public async Task JoinSpace_WithInvalidInviteCode_ShouldReturnBadRequest()
    {
        // Arrange
        var joinRequest = new JoinSpaceRequest { InviteCode = "INVALID" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSpaceById_WithValidId_ShouldReturnSpace()
    {
        // Arrange - Create a space first
        var createRequest = new CreateSpaceRequest { Name = "Get By ID Test" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();

        // Act
        var response = await _client.GetAsync($"/api/spaces/{createdSpace!.SpaceId}");

        // Assert
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
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/spaces/{invalidId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SpaceWorkflow_CreateJoinList_ShouldWorkEndToEnd()
    {
        // Arrange & Act

        // 1. Create a space
        var createRequest = new CreateSpaceRequest { Name = "Workflow Test Space" };
        var createResponse = await _client.PostAsJsonAsync("/api/spaces", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdSpace = await createResponse.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        createdSpace.Should().NotBeNull();

        // 2. Join the space (idempotent operation)
        var joinRequest = new JoinSpaceRequest { InviteCode = createdSpace!.InviteCode };
        var joinResponse = await _client.PostAsJsonAsync("/api/spaces/join", joinRequest);
        joinResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. List user spaces
        var listResponse = await _client.GetAsync("/api/spaces");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var spaces = await listResponse.Content.ReadFromJsonAsync<GetUserSpacesResponse>();
        spaces!.Spaces.Should().Contain(s => s.Name == "Workflow Test Space");

        // 4. Get space by ID
        var getResponse = await _client.GetAsync($"/api/spaces/{createdSpace.SpaceId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var space = await getResponse.Content.ReadFromJsonAsync<GetSpaceByIdResponse>();
        space!.Name.Should().Be("Workflow Test Space");
        space.Members.Should().NotBeEmpty();
        space.Members.Should().Contain(m => m.Role == "Admin");
    }
}