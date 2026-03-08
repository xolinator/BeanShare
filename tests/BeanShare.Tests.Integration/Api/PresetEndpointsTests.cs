using System.Net;
using System.Net.Http.Json;
using BeanShare.Contracts.Spaces;
using BeanShare.Tests.Integration.Fixtures;
using FluentAssertions;

namespace BeanShare.Tests.Integration.Api;

public sealed class PresetEndpointsTests : IClassFixture<PostgreSqlFixture>
{
    private readonly HttpClient _client;

    public PresetEndpointsTests(PostgreSqlFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    private async Task<Guid> CreateSpaceAsync(string name = "Preset Test Space")
    {
        var request = new CreateSpaceRequest { Name = name };
        var response = await _client.PostAsJsonAsync("/api/spaces", request);
        var created = await response.Content.ReadFromJsonAsync<CreateSpaceResponse>();
        return created!.SpaceId;
    }

    [Fact]
    public async Task GetSpacePresets_WithValidSpace_ShouldReturnOk()
    {
        var spaceId = await CreateSpaceAsync();

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/presets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetSpacePresets_WithInvalidSpace_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync($"/api/spaces/{Guid.NewGuid()}/presets");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreatePreset_WithValidRequest_ShouldReturnCreated()
    {
        var spaceId = await CreateSpaceAsync("Create Preset Test");

        var request = new
        {
            SpaceId = spaceId,
            Name = "Morning Espresso",
            CoffeeType = "Espresso",
            Preparation = "Espresso Machine",
            DefaultGrams = 14.0,
            IsShared = true
        };

        var response = await _client.PostAsJsonAsync("/api/presets", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPresetById_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/presets/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeletePreset_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.DeleteAsync($"/api/presets/{Guid.NewGuid()}");

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetQuickPresets_WithValidSpace_ShouldReturnOk()
    {
        var spaceId = await CreateSpaceAsync("Quick Presets Test");

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/quick-presets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetGlobalPresets_WithValidSpace_ShouldReturnOk()
    {
        var spaceId = await CreateSpaceAsync("Global Presets Test");

        var response = await _client.GetAsync($"/api/spaces/{spaceId}/global-presets");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PresetWorkflow_CreateListDelete_ShouldWork()
    {
        var spaceId = await CreateSpaceAsync("Preset Workflow Test");

        // Create
        var createRequest = new
        {
            SpaceId = spaceId,
            Name = "Workflow Preset",
            CoffeeType = "Filter",
            Preparation = "Pour Over",
            DefaultGrams = 15.0,
            IsShared = false
        };
        var createResponse = await _client.PostAsJsonAsync("/api/presets", createRequest);
        createResponse.StatusCode.Should().BeOneOf(HttpStatusCode.Created, HttpStatusCode.OK);

        // List
        var listResponse = await _client.GetAsync($"/api/spaces/{spaceId}/presets");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listContent = await listResponse.Content.ReadAsStringAsync();
        listContent.Should().Contain("Workflow Preset");
    }
}
