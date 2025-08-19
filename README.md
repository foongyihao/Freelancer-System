# CDN Freelancers API & Simple Frontend

Implementation of the basic requirements for the Backend .NET Developer Assessment (CRUD, search, archive, clean architecture, tests) plus an optional lightweight frontend.

## Tech Stack

- .NET 9 Minimal API + static HTML/JS (no framework) UI
- Clean Architecture style layering (Core, Application, Infrastructure, WebApi)
- EF Core (SQLite by default, optional InMemory)
- xUnit unit tests
- Swagger (Dev only)

## Project Layout

```
src/
├── Application/         # business rules / services
├── Core/                # Data Models
├── Infrastructure/      # EF Core DbContext + repository
├── WebApi/              # HTTP endpoints / controller
tests/
└── UnitTests/           # Repository unit tests
```

## Running
Fetches dependencies and compile code
```
dotnet restore
dotnet build
```

Set development environment:
```
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/WebApi/WebApi.csproj
```

Then open: http://localhost:5000  (serves the simple UI)
Swagger: http://localhost:5000/swagger

## Features:

- List freelancers (toggle include archived)
- Create & update (form auto-fills on Edit)
- Archive / Unarchive
- Delete
- Search by username/email (wildcard contains)

## Core API Endpoints

| Method | Route                                      | Description                       |
| ------ | ------------------------------------------ | --------------------------------- |
| GET    | /api/v1/freelancers?includeArchived=false     | List (filter archived)            |
| GET    | /api/v1/freelancers/{id}                      | Get by Id                         |
| POST   | /api/v1/freelancers                           | Create                            |
| PUT    | /api/v1/freelancers/{id}                      | Replace (incl. skillsets/hobbies) |
| PATCH  | /api/v1/freelancers/{id}                      | Partial update (archive toggle)   |
| DELETE | /api/v1/freelancers/{id}                      | Delete                            |
| GET    | /api/v1/freelancers/search?term=abc           | Wildcard search                   |
| GET    | /api/v1/freelancers?term=abc                  | Wildcard search (via query term)  |

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

Current tests cover: add/get, search, archive filter.
