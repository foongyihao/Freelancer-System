using CDN.Freelancers.Application;
using CDN.Freelancers.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using CDN.Freelancers.Domain;


var builder = WebApplication.CreateBuilder(args);

var useSqlite = builder.Configuration.GetValue<bool>("UseSqlite");
builder.Services.AddRouting(o =>
{
    o.LowercaseUrls = true;
    o.LowercaseQueryStrings = true;
});
builder.Services.AddDbContext<FreelancerDbContext>(o =>
{
    if (useSqlite)
    {
        var cs = builder.Configuration.GetConnectionString("Freelancers") ?? "Data Source=freelancers.db";
        o.UseSqlite(cs);
    }
    else
    {
        o.UseInMemoryDatabase("FreelancersDb"); // default
    }
});

builder.Services.AddScoped<IFreelancerRepository, FreelancerRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CDN Freelancers API",
        Version = "v1",
        Description = "REST API for managing freelancer directory: CRUD, archive/unarchive, wildcard search over username & email."
            + "\n\nEnvironment Modes:" +
            "\n- Development: detailed errors, Swagger UI enabled, relaxed diagnostics." +
            "\n- Production: disable detailed errors & Swagger UI (or protect it), add proper logging & security headers.",
        Contact = new OpenApiContact { Name = "CDN", Email = "support@example.com" }
    });
    // Include XML comments if generated
    var xmlFiles = new[] {"Presentation.xml", "Domain.xml", "Application.xml", "Infrastructure.xml"};
    foreach (var f in xmlFiles)
    {
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, f);
        if (System.IO.File.Exists(path)) c.IncludeXmlComments(path, includeControllerXmlComments: true);
    }
});
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(o =>
{
    o.SuppressModelStateInvalidFilter = true; // rely on manual validation in controllers
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
// Ensure database exists (demo purpose; replace with migrations in production)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FreelancerDbContext>();
    db.Database.EnsureCreated();
}

app.Run();

// Expose Program class publicly for WebApplicationFactory<T> in integration tests
public partial class Program { }
