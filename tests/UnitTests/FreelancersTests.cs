using System.Net;
using System.Net.Http.Json;
using CDN.Freelancers.Domain;
using CDN.Freelancers.Infrastructure;
using CDN.Freelancers.Application;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace UnitTests;

public class FreelancersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public FreelancersTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseSetting("UseSqlite", "false");
            builder.UseSetting("environment", "Development");
        });
    }

    private FreelancerRepository CreateRepository()
    {
        var opts = new DbContextOptionsBuilder<FreelancerDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var ctx = new FreelancerDbContext(opts);
        return new FreelancerRepository(ctx);
    }

    private record FreelancerRequestDto(string Username, string Email, string? PhoneNumber = null, List<string>? Skillsets = null, List<string>? Hobbies = null);

    // Repository-centric tests
    [Fact]
    public async Task Add_And_Get_Freelancer()
    {
        var repo = CreateRepository();
        var f = new Freelancer { Username = "alice", Email = "alice@example.com" };
        await repo.AddAsync(f);
        var loaded = await repo.GetAsync(f.Id);
        Assert.NotNull(loaded);
        Assert.Equal("alice", loaded!.Username);
    }

    [Fact]
    public async Task Search_Finds_By_Email()
    {
        var repo = CreateRepository();
        await repo.AddAsync(new Freelancer { Username = "bob", Email = "bob@acme.com" });
        var results = await repo.GetPagedAsync(page: 1, pageSize: 10, includeArchived: true, term: "acme");
        Assert.Single(results.Items);
    }

    [Fact]
    public async Task Archive_Works()
    {
        var repo = CreateRepository();
        var f = new Freelancer { Username = "carol", Email = "carol@example.com" };
        await repo.AddAsync(f);
        await repo.ArchiveAsync(f.Id, true);
        var list = await repo.GetPagedAsync(page: 1, pageSize: 10, includeArchived: false);
        Assert.DoesNotContain(list.Items, x => x.Id == f.Id);
    }

    [Fact]
    public async Task Duplicate_Add_Throws()
    {
        var repo = CreateRepository();
        await repo.AddAsync(new Freelancer { Username = "dup", Email = "dup@example.com" });
        await Assert.ThrowsAsync<DuplicateFreelancerException>(async () =>
            await repo.AddAsync(new Freelancer { Username = "dup", Email = "other@example.com" }));
        await Assert.ThrowsAsync<DuplicateFreelancerException>(async () =>
            await repo.AddAsync(new Freelancer { Username = "other", Email = "dup@example.com" }));
    }

    [Fact]
    public async Task Filtering_By_Skill_And_Hobby_Works()
    {
        var repo = CreateRepository();
        await repo.AddAsync(new Freelancer
        {
            Username = "skillA",
            Email = "a@ex.com",
            FreelancerSkillsets = new() { new Freelancer_Skillset { Skillset = new Skillset { Name = "C#" } } },
            FreelancerHobbies = new() { new Freelancer_Hobby { Hobby = new Hobby { Name = "Chess" } } }
        });
        await repo.AddAsync(new Freelancer
        {
            Username = "skillB",
            Email = "b@ex.com",
            FreelancerSkillsets = new() { new Freelancer_Skillset { Skillset = new Skillset { Name = "Go" } } },
            FreelancerHobbies = new() { new Freelancer_Hobby { Hobby = new Hobby { Name = "Cycling" } } }
        });
        var page = await repo.GetPagedAsync(1, 10, includeArchived: false, skillFilter: "c#");
        Assert.Single(page.Items);
        Assert.Equal("skillA", page.Items.First().Username);
        var page2 = await repo.GetPagedAsync(1, 10, includeArchived: false, hobbyFilter: "cyc");
        Assert.Single(page2.Items);
        Assert.Equal("skillB", page2.Items.First().Username);
    }

    [Fact]
    public async Task GetPagedAsync_Returns_Correct_Page_Metadata()
    {
        var repo = CreateRepository();
        for (int i = 0; i < 25; i++)
        {
            await repo.AddAsync(new Freelancer { Username = $"user{i:00}", Email = $"user{i:00}@example.com" });
        }
        var page2 = await repo.GetPagedAsync(page: 2, pageSize: 10, includeArchived: false);
        Assert.Equal(25, page2.TotalCount);
        Assert.Equal(3, page2.TotalPages);
        Assert.Equal(2, page2.Page);
        Assert.Equal(10, page2.Items.Count);
        Assert.StartsWith("user10", page2.Items.First().Username);
    }

    [Fact]
    public async Task Search_With_Term_Filters_And_Paginates()
    {
        var repo = CreateRepository();
        for (int i = 0; i < 15; i++)
        {
            await repo.AddAsync(new Freelancer { Username = $"match{i:00}", Email = $"m{i:00}@example.com" });
        }
        for (int i = 0; i < 5; i++)
        {
            await repo.AddAsync(new Freelancer { Username = $"other{i:00}", Email = $"o{i:00}@example.com" });
        }
        var result = await repo.GetPagedAsync(page: 2, pageSize: 5, includeArchived: true, term: "match");
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(5, result.Items.Count);
        Assert.All(result.Items, x => Assert.StartsWith("match", x.Username));
    }

    // HTTP status and controller behavior tests
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
        await client.PostAsJsonAsync("/api/v1/freelancers", new FreelancerRequestDto("bob", "bob@example.com"));
        var resp = await client.GetAsync("/api/v1/freelancers?term=bob");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Create_Then_Get_And_Delete_Full_Lifecycle_StatusCodes()
    {
        var client = _factory.CreateClient();
        var createResp = await client.PostAsJsonAsync("/api/v1/freelancers", new FreelancerRequestDto("alice", "alice@example.com", Skillsets: new() { "C#" }, Hobbies: new() { "Chess" }));
        createResp.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await createResp.Content.ReadFromJsonAsync<Freelancer>();
        created.Should().NotBeNull();

        var getResp = await client.GetAsync($"/api/v1/freelancers/{created!.Id}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var dupUserResp = await client.PostAsJsonAsync("/api/v1/freelancers", new FreelancerRequestDto("alice", "alice2@example.com"));
        dupUserResp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var dupEmailResp = await client.PostAsJsonAsync("/api/v1/freelancers", new FreelancerRequestDto("alice2", "alice@example.com"));
        dupEmailResp.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var updateResp = await client.PutAsJsonAsync($"/api/v1/freelancers/{created.Id}", new FreelancerRequestDto("alice", "alice@example.com", PhoneNumber: "123"));
        updateResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var badUpdateResp = await client.PutAsJsonAsync($"/api/v1/freelancers/{created.Id}", new { Username = "", Email = "" });
        badUpdateResp.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var archiveResp = await client.PatchAsJsonAsync($"/api/v1/freelancers/{created.Id}", new { isArchived = true });
        archiveResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var deleteResp = await client.DeleteAsync($"/api/v1/freelancers/{created.Id}");
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDelete = await client.GetAsync($"/api/v1/freelancers/{created.Id}");
        getAfterDelete.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_NotFound_Returns_404()
    {
        var client = _factory.CreateClient();
        var updateResp = await client.PutAsJsonAsync($"/api/v1/freelancers/{Guid.NewGuid()}", new FreelancerRequestDto("u", "e@example.com"));
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
