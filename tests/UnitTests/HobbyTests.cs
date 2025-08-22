using System.Threading.Tasks;
using CDN.Freelancers.Domain;
using CDN.Freelancers.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests;

public class HobbyTests
{
    private (FreelancerDbContext ctx, FreelancerRepository repo, SqliteConnection conn) CreateSqliteRepo()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        var options = new DbContextOptionsBuilder<FreelancerDbContext>()
            .UseSqlite(conn)
            .Options;
        var ctx = new FreelancerDbContext(options);
        ctx.Database.EnsureCreated();
        return (ctx, new FreelancerRepository(ctx), conn);
    }

    [Fact]
    public async Task Deleting_Hobby_Removes_Association_From_Freelancers()
    {
        var (ctx, repo, conn) = CreateSqliteRepo();
        try
        {
            ctx.Hobbies.Add(new Hobby { Name = "Chess" });
            await ctx.SaveChangesAsync();

            var f = new Freelancer
            {
                Username = "alice",
                Email = "alice@example.com",
                FreelancerHobbies = new() { new Freelancer_Hobby { Hobby = new Hobby { Name = "Chess" } } }
            };
            await repo.AddAsync(f);

            var loaded = await repo.GetAsync(f.Id);
            Assert.NotNull(loaded);
            Assert.Contains(loaded!.FreelancerHobbies, h => h.Hobby.Name == "Chess");

            var hobby = await ctx.Hobbies.FirstAsync(h => h.Name == "Chess");
            ctx.Hobbies.Remove(hobby);
            await ctx.SaveChangesAsync();

            var afterDelete = await repo.GetAsync(f.Id);
            Assert.NotNull(afterDelete);
            Assert.DoesNotContain(afterDelete!.FreelancerHobbies, h => h.Hobby.Name == "Chess");
        }
        finally
        {
            conn.Dispose();
            ctx.Dispose();
        }
    }
}
