using System.Linq;
using System.Threading.Tasks;
using CDN.Freelancers.Domain;
using CDN.Freelancers.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace UnitTests;

public class SkillsetTests
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
    public async Task Deleting_Skill_Removes_Association_From_Freelancers()
    {
        var (ctx, repo, conn) = CreateSqliteRepo();
        try
        {
            ctx.Skillsets.Add(new Skillset { Name = "C#" });
            await ctx.SaveChangesAsync();

            var f = new Freelancer
            {
                Username = "alice",
                Email = "alice@example.com",
                FreelancerSkillsets = new() { new Freelancer_Skillset { Skillset = new Skillset { Name = "C#" } } }
            };
            await repo.AddAsync(f);

            var loaded = await repo.GetAsync(f.Id);
            Assert.NotNull(loaded);
            Assert.Contains(loaded!.FreelancerSkillsets, s => s.Skillset.Name == "C#");

            var skill = await ctx.Skillsets.FirstAsync(s => s.Name == "C#");
            ctx.Skillsets.Remove(skill);
            await ctx.SaveChangesAsync();

            var afterDelete = await repo.GetAsync(f.Id);
            Assert.NotNull(afterDelete);
            Assert.DoesNotContain(afterDelete!.FreelancerSkillsets, s => s.Skillset.Name == "C#");
        }
        finally
        {
            conn.Dispose();
            ctx.Dispose();
        }
    }
}
