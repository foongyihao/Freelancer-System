# CDN Freelancers API & Simple Frontend

Implementation of the basic requirements for the Backend .NET Developer Assessment (CRUD, search, archive, clean architecture, tests) plus an optional lightweight frontend. Recently refactored to unify pagination & search into a single endpoint and simplify repository methods.

## Tech Stack

- .NET 9 ASP.NET Core (controllers) + static HTML/JS (no framework) UI
- Clean Architecture style layering (Domain, Application, Infrastructure, Presentation)
- EF Core (SQLite by default, optional InMemory)
- xUnit unit tests
- Swagger (Dev only)

## Project Demo
<img width="5088" height="3822" alt="image" src="https://github.com/user-attachments/assets/2b540b9c-022d-4624-ae76-f885568b0e90" />
<img width="5088" height="3356" alt="image" src="https://github.com/user-attachments/assets/7a0e3a2b-14f5-4144-8e51-9f0d3dea4427" />

## Database Schema
- The Freelancer entity stores core information such as ID, username, email, phone number, and archive status.
- A Skillset entity maintains unique identifiers and names for each skill.
- Freelancers are connected to skills through the Freelancer\_Skillset linking entity, enabling many-to-many relationships.
- A Hobby entity stores unique identifiers and names for each hobby.
- Freelancers are connected to hobbies through the Freelancer\_Hobby linking entity, also allowing many-to-many relationships.

  
<p align="center">
	<img width="643" height="372" alt="image" src="https://github.com/user-attachments/assets/f4259be9-0781-4a6e-a85f-79ce804bbabf" />
</p>



## Project Layout

```
├── Application                     # Application layer: interfaces and business logic
│   ├── Application.csproj          # Project file with dependencies
│   └── FreelancerContracts.cs      # Repository interface, PaginatedResult, exceptions
├── Domain                          # Domain layer: core business entities
│   ├── Domain.csproj               # Project file with minimal dependencies
├── Infrastructure                  # Infrastructure layer: implementation of interfaces
│   ├── FreelancerDbContext.cs      # EF Core database context
│   ├── FreelancerRepository.cs     # Implementation of IFreelancerRepository
│   └── Infrastructure.csproj       # Project file with dependencies on EF Core
└── Presentation                    # Presentation layer: API controllers and UI
    ├── Controllers
    │   └── freelancerController.cs # RESTful API controller
    ├── Presentation.csproj          # Project file with ASP.NET dependencies
    ├── Program.cs                   # Application startup and configuration
    ├── Requests
    │   ├── FreelancerPatchRequest.cs # DTO for PATCH operations
    │   └── FreelancerRequest.cs      # DTO for POST/PUT operations
    ├── appsettings.json              # Application configuration
    ├── freelancers.db                # SQLite database file
    └── wwwroot
        ├── pages                     # HTML pages
        └── feature                   # JavaScript for frontend functionality
```

### Clean Architecture Explanation

This project follows the Clean Architecture principles, organizing code into layers with clear separation of concerns:

1. **Domain Layer**:

   - Contains business entities (Freelancer, Skillset, Hobby)
   - Has no dependencies on other layers or external frameworks
   - Defines the core business rules and data structures

2. **Application Layer**:

   - Defines interfaces that the outer layers must implement
   - Contains use case logic and service interfaces
   - Depends only on the Domain layer
   
3. **Infrastructure Layer**:

   - Implements interfaces defined in the Application layer
   - Contains database context, repositories, and external service implementations
   - Depends on Application and Domain layers
   
4. **Presentation Layer**:

   - Contains controllers, DTOs (Data Transfer Objects), and UI components
   - Depends on Application layer for business operations
   - Responsible for formatting data for display and processing user input
   - The API controllers and frontend code live here

## Running

Fetches dependencies and compile code

```
dotnet restore
dotnet build
```

Set development environment:

```
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Presentation/Presentation.csproj
```

Then open: http://localhost:5000  (serves the simple UI)
Swagger: http://localhost:5000/swagger

## Features:

- List freelancers (toggle include archived)
- Create & update (form auto-fills on Edit)
- Archive / Unarchive
- Delete
- Search by username/email (wildcard contains) combined with listing (single endpoint)
- Pagination & filtering (skills, hobbies) in same unified endpoint

## Core API Endpoints

| Method | Route                                     | Description                       |
| ------ | ----------------------------------------- | --------------------------------- |
| GET    | /api/v1/freelancers?includeArchived=false | List (filter archived)            |
| GET    | /api/v1/freelancers/{id}                  | Get by Id                         |
| POST   | /api/v1/freelancers                       | Create                            |
| PUT    | /api/v1/freelancers/{id}                  | Replace (incl. skillsets/hobbies) |
| PATCH  | /api/v1/freelancers/{id}                  | Partial update (archive toggle)   |
| DELETE | /api/v1/freelancers/{id}                  | Delete                            |
| GET    | /api/v1/freelancers?term=abc              | List + wildcard search (with paging & filters) |

Query parameters (unified list/search):

```
term            (optional) case-insensitive substring for username/email
includeArchived (bool, default false) include archived profiles
page            (int, default 1) 1-based page number
pageSize        (int, default 10, max 100) page size
skill           (optional) substring match inside any skill name
hobby           (optional) substring match inside any hobby name
```

Request body (POST/PUT):

```
{
	"username": "alice",
	"email": "alice@example.com",
	"phoneNumber": "123456",
	"skillsets": ["C#","SQL"],
	"hobbies": ["Chess"]
}
```

Partial archive toggle (PATCH):

```
{ "isArchived": true }
```

## Database Configuration

`appsettings.json`:

```
"UseSqlite": true,
"ConnectionStrings": { "Freelancers": "Data Source=freelancers.db" }
```

Switch to InMemory for quick testing: set `UseSqlite` to false.
Switch to SQL Server: add `Microsoft.EntityFrameworkCore.SqlServer` package and adjust DbContext registration.

Database schema is ensured via `EnsureCreated()` (good for demos). For production add migrations.

## Tests

```
dotnet test
```

Current tests cover: add/get, search (via unified paged call), archive filter, pagination, filtering by skill/hobby, duplicates.

## CI

GitHub Actions workflow (`.github/workflows/ci.yml`) builds and tests on every push / PR to `main`.

![CI](https://github.com/foongyihao/Freelancer-System/actions/workflows/ci.yml/badge.svg)
