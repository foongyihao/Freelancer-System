using CDN.Freelancers.Core;
using CDN.Freelancers.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests;

/// <summary>
/// Unit tests for <see cref="FreelancerRepository"/> validating core CRUD, search and archive behaviors
/// using the EF Core InMemory provider to avoid external dependencies.
/// </summary>
public class RepositoryTests
{
    private FreelancerRepository CreateRepository()
    {
        var opts = new DbContextOptionsBuilder<FreelancerDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var ctx = new FreelancerDbContext(opts);
        return new FreelancerRepository(ctx);
    }

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
        var results = await repo.SearchAsync("acme");
        Assert.Single(results);
    }

    [Fact]
    public async Task Archive_Works()
    {
        var repo = CreateRepository();
        var f = new Freelancer { Username = "carol", Email = "carol@example.com" };
        await repo.AddAsync(f);
        await repo.ArchiveAsync(f.Id, true);
        var list = await repo.GetAllAsync(includeArchived:false);
        Assert.DoesNotContain(list, x => x.Id == f.Id);
    }
}
