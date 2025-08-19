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

    [Fact]
    public async Task GetPagedAsync_Returns_Correct_Page_Metadata()
    {
        var repo = CreateRepository();
        for(int i=0;i<25;i++)
        {
            await repo.AddAsync(new Freelancer { Username = $"user{i:00}", Email = $"user{i:00}@example.com" });
        }
        var page2 = await repo.GetPagedAsync(page:2, pageSize:10, includeArchived:false);
        Assert.Equal(25, page2.TotalCount);
        Assert.Equal(3, page2.TotalPages);
        Assert.Equal(2, page2.Page);
        Assert.Equal(10, page2.Items.Count);
        // Ensure ordering by Username asc (user00 ... user24)
        Assert.StartsWith("user10", page2.Items.First().Username);
    }

    [Fact]
    public async Task SearchPagedAsync_Filters_And_Paginates()
    {
        var repo = CreateRepository();
        // Add some with pattern 'match' and others not matching
        for(int i=0;i<15;i++)
        {
            await repo.AddAsync(new Freelancer { Username = $"match{i:00}", Email = $"m{i:00}@example.com" });
        }
        for(int i=0;i<5;i++)
        {
            await repo.AddAsync(new Freelancer { Username = $"other{i:00}", Email = $"o{i:00}@example.com" });
        }
        var result = await repo.SearchPagedAsync("match", page:2, pageSize:5);
        Assert.Equal(15, result.TotalCount); // only 'match*'
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(5, result.Items.Count);
        Assert.All(result.Items, x => Assert.StartsWith("match", x.Username));
    }
}
