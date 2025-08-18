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
| GET    | /api/freelancers?includeArchived=false     | List (filter archived)            |
| GET    | /api/freelancers/{id}                      | Get by Id                         |
| POST   | /api/freelancers                           | Create                            |
| PUT    | /api/freelancers/{id}                      | Replace (incl. skillsets/hobbies) |
| PATCH  | /api/freelancers/{id}/archive?archive=true | false                             |
| DELETE | /api/freelancers/{id}                      | Delete                            |
| GET    | /api/freelancers/search?term=abc           | Wildcard search                   |

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

## Quick cURL Examples

```
# Create
curl -X POST http://localhost:5000/api/freelancers \
	-H "Content-Type: application/json" \
	-d '{"username":"alice","email":"alice@example.com","phoneNumber":"123","skillsets":["C#"],"hobbies":["Chess"]}'

# List
curl http://localhost:5000/api/freelancers?includeArchived=false

# Search
curl "http://localhost:5000/api/freelancers/search?term=ali"

# Archive
curl -X PATCH "http://localhost:5000/api/freelancers/<id>/archive?archive=true"

# Delete
curl -X DELETE http://localhost:5000/api/freelancers/<id>
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

## Notes & Next Steps (Potential Improvements)

- Validation (FluentValidation)
- Pagination & sorting
- Global error/exception handling middleware
- DTO mapping (e.g. Mapster/AutoMapper)
- Authentication & authorization
- Proper migrations instead of EnsureCreated
- Caching layer for read endpoints

All mandatory basic requirements are implemented; UI fulfills the optional interface requirement.
