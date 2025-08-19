using System.Net;
using System.Net.Http.Json;
using CDN.Freelancers.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Xunit;

namespace UnitTests;

public class ControllerHttpStatusTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public ControllerHttpStatusTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Force in-memory db (default) and set environment variables
            builder.UseSetting("UseSqlite", "false");
            builder.UseSetting("environment", "Development");
        });
    }

    private record FreelancerRequestDto(string Username, string Email, string? PhoneNumber = null, List<string>? Skillsets = null, List<string>? Hobbies = null);

    [Fact]
    public async Task Get_List_Default_Ok()
    {
        var client = _factory.CreateClient();
    var resp = await client.GetAsync("/api/v1/freelancers");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_List_Invalid_Paging_Returns_400()
    {
        var client = _factory.CreateClient();
    var resp = await client.GetAsync("/api/v1/freelancers?page=0&pageSize=0");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await resp.Content.ReadFromJsonAsync<ProblemDetailsLike>();
        problem!.Title.Should().Contain("Invalid");
    }

    [Fact]
    public async Task Search_With_Term_Returns_200()
    {
        var client = _factory.CreateClient();
        // Seed one
        await client.PostAsJsonAsync("/api/v1/freelancers", new FreelancerRequestDto("bob","bob@example.com"));
        var resp = await client.GetAsync("/api/v1/freelancers?term=bob");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_Then_Get_And_Delete_Full_Lifecycle_StatusCodes()
    {
        var client = _factory.CreateClient();
        // Create
    var createResp = await client.PostAsJsonAsync("/api/v1/freelancers", new FreelancerRequestDto("alice","alice@example.com", Skillsets: new(){"C#"}, Hobbies: new(){"Chess"}));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<Freelancer>();
        created.Should().NotBeNull();

        // Get existing
    var getResp = await client.GetAsync($"/api/v1/freelancers/{created!.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Duplicate create (username) -> 409
    var dupUserResp = await client.PostAsJsonAsync("/api/v1/freelancers", new FreelancerRequestDto("alice","alice2@example.com"));
        dupUserResp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Duplicate create (email) -> 409
    var dupEmailResp = await client.PostAsJsonAsync("/api/v1/freelancers", new FreelancerRequestDto("alice2","alice@example.com"));
        dupEmailResp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        // Update success -> 204
    var updateResp = await client.PutAsJsonAsync($"/api/v1/freelancers/{created.Id}", new FreelancerRequestDto("alice","alice@example.com", PhoneNumber: "123"));
        updateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Update with missing required fields -> 400
    var badUpdateResp = await client.PutAsJsonAsync($"/api/v1/freelancers/{created.Id}", new { Username = "", Email = "" });
        badUpdateResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

    // Archive -> 204 (PATCH body)
	var archiveResp = await client.PatchAsJsonAsync($"/api/v1/freelancers/{created.Id}", new { isArchived = true });
    archiveResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Delete -> 204
    var deleteResp = await client.DeleteAsync($"/api/v1/freelancers/{created.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Get after delete -> 404
    var getAfterDelete = await client.GetAsync($"/api/v1/freelancers/{created.Id}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_NotFound_Returns_404()
    {
        var client = _factory.CreateClient();
    var updateResp = await client.PutAsJsonAsync($"/api/v1/freelancers/{Guid.NewGuid()}", new FreelancerRequestDto("u","e@example.com"));
        updateResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_NotFound_Returns_404()
    {
        var client = _factory.CreateClient();
    var deleteResp = await client.DeleteAsync($"/api/v1/freelancers/{Guid.NewGuid()}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Patch_Archive_NotFound_Returns_404()
    {
        var client = _factory.CreateClient();
	var archiveResp = await client.PatchAsJsonAsync($"/api/v1/freelancers/{Guid.NewGuid()}", new { isArchived = true });
        archiveResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Invalid_Body_Returns_400()
    {
        var client = _factory.CreateClient();
    var createResp = await client.PostAsJsonAsync("/api/v1/freelancers", new { Username = "", Email = "" });
        createResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_Invalid_Email_Returns_400()
    {
        var client = _factory.CreateClient();
    var createResp = await client.PostAsJsonAsync("/api/v1/freelancers", new { Username = "userX", Email = "not-an-email" });
        createResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Paging_Upper_Limit_Enforced()
    {
        var client = _factory.CreateClient();
    var resp = await client.GetAsync("/api/v1/freelancers?page=1&pageSize=101");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private class ProblemDetailsLike
    {
        public string? Title { get; set; }
        public string? Detail { get; set; }
        public int? Status { get; set; }
    }
}
