using CDN.Freelancers.Application;
using CDN.Freelancers.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using CDN.Freelancers.Domain;


// Create the web application builder (entry point for ASP.NET Core apps)
var builder = WebApplication.CreateBuilder(args);

// Get configuration settings from appsettings.json or environment variables
var useSqlite = builder.Configuration.GetValue<bool>("UseSqlite");

// Configures routing options
builder.Services.AddRouting(o => {
    o.LowercaseUrls = true;          // Consistent, SEO / cache friendly URLs
    o.LowercaseQueryStrings = true;  // Normalise query casing
});

// Add Entity Framework Core
builder.Services.AddDbContext<FreelancerDbContext>(o => {
    if (useSqlite) {
        var cs = builder.Configuration.GetConnectionString("Freelancers") ?? "Data Source=freelancers.db";
        o.UseSqlite(cs);
    }
    else {
        o.UseInMemoryDatabase("FreelancersDb");
    }
});

// Register application services in the Dependency Injection (DI) container with scoped lifetime
// This allows a new instance of FreelancerRepository to be created for each HTTP request and shared within that request
builder.Services.AddScoped<IFreelancerRepository, FreelancerRepository>();

// OpenAPI/Swagger generator
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => {
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "CDN Freelancers API",
        Version = "v1",
        Description = "REST API for managing freelancer directory: CRUD, archive/unarchive, wildcard search over username & email."
    });
    // Include XML doc comments
    var xmlFiles = new[] {"Presentation.xml", "Domain.xml", "Application.xml", "Infrastructure.xml"};
    foreach (var f in xmlFiles) {
        var path = System.IO.Path.Combine(AppContext.BaseDirectory, f);
        if (System.IO.File.Exists(path)) c.IncludeXmlComments(path, includeControllerXmlComments: true);
    }
});

// Add Model-View-Controller (MVC) controllers & JSON options
builder.Services.AddControllers().AddJsonOptions(o => {
    o.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// finalize service configuration
var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles(); // Looks for index.html by default
app.UseStaticFiles();  // Serves static files from wwwroot
app.MapControllers(); // Maps controller routes

// Ensure the database is created at startup
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<FreelancerDbContext>();
    db.Database.EnsureCreated();
}

// Starts the ASP.NET Core application and begins listening for incoming HTTP requests
app.Run();

// Public partial Program class enables WebApplicationFactory<Program> usage in integration tests.
// Marking the Program class as partial allows the test framework to extend or modify it during testing
public partial class Program { }
