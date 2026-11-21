# Contact Management API

A production-style, layered ASP.NET Core 10 Web API for managing contacts and categories. Every user is fully isolated — all reads and writes are scoped to the authenticated user's own data.

## Tech stack

| Area | Choice |
| --- | --- |
| Framework | .NET 10 / ASP.NET Core Web API / C# |
| Data | SQL Server + Entity Framework Core 10 (Fluent API) |
| Auth | JWT bearer, PBKDF2 password hashing |
| API | Swagger/OpenAPI (Swashbuckle 11) |
| Mapping | AutoMapper 17 |
| Validation | Data Annotations + service-level checks |
| Tests | xUnit, Moq, WebApplicationFactory, EF InMemory |

## Architecture

```
ContactManagement/
├── ContactManagement.slnx
├── ContactManagement.API/            Controllers, Middleware, Extensions, Program.cs, appsettings.json
├── ContactManagement.Core/           Entities, DTOs, Interfaces, Enums, Exceptions
├── ContactManagement.Infrastructure/ Data (DbContext + configurations), Repositories, Migrations
├── ContactManagement.Services/       Services, Validators, Mappings, Extensions
├── ContactManagement.Tests/          Unit + integration tests
└── docs/ContactManagement.postman_collection.json
```

Dependency flow: `API → Services → Core ← Infrastructure ← API`. Core has no dependencies. Controllers stay thin; all rules live in services; all SQL lives in repositories behind interfaces.

## Prerequisites

- .NET 10 SDK
- SQL Server (LocalDB, Express, container, or full instance)
- EF CLI: `dotnet tool install --global dotnet-ef --version 10.0.0`

## Recreate from scratch

```powershell
mkdir D:\Projects\ContactManagement
cd D:\Projects\ContactManagement
dotnet new sln -n ContactManagement
dotnet new webapi -n ContactManagement.API --framework net10.0 --no-openapi
dotnet new classlib -n ContactManagement.Core --framework net10.0
dotnet new classlib -n ContactManagement.Infrastructure --framework net10.0
dotnet new classlib -n ContactManagement.Services --framework net10.0
dotnet new xunit -n ContactManagement.Tests --framework net10.0

dotnet sln add ContactManagement.API/ContactManagement.API.csproj `
  ContactManagement.Core/ContactManagement.Core.csproj `
  ContactManagement.Infrastructure/ContactManagement.Infrastructure.csproj `
  ContactManagement.Services/ContactManagement.Services.csproj `
  ContactManagement.Tests/ContactManagement.Tests.csproj

dotnet add ContactManagement.Infrastructure/ContactManagement.Infrastructure.csproj reference ContactManagement.Core/ContactManagement.Core.csproj
dotnet add ContactManagement.Services/ContactManagement.Services.csproj reference ContactManagement.Core/ContactManagement.Core.csproj
dotnet add ContactManagement.API/ContactManagement.API.csproj reference ContactManagement.Core/ContactManagement.Core.csproj ContactManagement.Infrastructure/ContactManagement.Infrastructure.csproj ContactManagement.Services/ContactManagement.Services.csproj
dotnet add ContactManagement.Tests/ContactManagement.Tests.csproj reference ContactManagement.Core/ContactManagement.Core.csproj ContactManagement.Infrastructure/ContactManagement.Infrastructure.csproj ContactManagement.Services/ContactManagement.Services.csproj ContactManagement.API/ContactManagement.API.csproj
```

### NuGet packages

```powershell
# Infrastructure
dotnet add ContactManagement.Infrastructure/ContactManagement.Infrastructure.csproj package Microsoft.EntityFrameworkCore.SqlServer --version 10.0.0
dotnet add ContactManagement.Infrastructure/ContactManagement.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design --version 10.0.0

# Services
dotnet add ContactManagement.Services/ContactManagement.Services.csproj package Microsoft.Extensions.Logging.Abstractions --version 10.0.0
dotnet add ContactManagement.Services/ContactManagement.Services.csproj package Microsoft.Extensions.Configuration.Abstractions --version 10.0.0
dotnet add ContactManagement.Services/ContactManagement.Services.csproj package Microsoft.Extensions.DependencyInjection.Abstractions --version 10.0.0
dotnet add ContactManagement.Services/ContactManagement.Services.csproj package System.IdentityModel.Tokens.Jwt
dotnet add ContactManagement.Services/ContactManagement.Services.csproj package AutoMapper

# API
dotnet add ContactManagement.API/ContactManagement.API.csproj package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.0
dotnet add ContactManagement.API/ContactManagement.API.csproj package Swashbuckle.AspNetCore
dotnet add ContactManagement.API/ContactManagement.API.csproj package Microsoft.EntityFrameworkCore.Design --version 10.0.0
dotnet add ContactManagement.API/ContactManagement.API.csproj package Microsoft.EntityFrameworkCore.Tools --version 10.0.0

# Tests
dotnet add ContactManagement.Tests/ContactManagement.Tests.csproj package Moq
dotnet add ContactManagement.Tests/ContactManagement.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing --version 10.0.0
dotnet add ContactManagement.Tests/ContactManagement.Tests.csproj package Microsoft.EntityFrameworkCore.InMemory --version 10.0.0
```

## Configuration

`ContactManagement.API/appsettings.json` contains a LocalDB connection string and placeholder JWT values. Override secrets for real environments:

```powershell
dotnet user-secrets init --project ContactManagement.API/ContactManagement.API.csproj
dotnet user-secrets set "Jwt:Key" "a-long-random-secret-of-at-least-32-characters" --project ContactManagement.API/ContactManagement.API.csproj
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=ContactManagementDb;User Id=...;Password=...;TrustServerCertificate=True" --project ContactManagement.API/ContactManagement.API.csproj
```

Environment variables also work: `Jwt__Key`, `ConnectionStrings__DefaultConnection`, etc.

## Database migration

```powershell
dotnet ef migrations add InitialCreate --project ContactManagement.Infrastructure --startup-project ContactManagement.API --output-dir Migrations
dotnet ef database update --project ContactManagement.Infrastructure --startup-project ContactManagement.API
dotnet ef migrations remove --project ContactManagement.Infrastructure --startup-project ContactManagement.API
```

The initial migration is already included in `ContactManagement.Infrastructure/Migrations`.

Schema highlights: unique index on `Users.Email`, unique `(UserId, Email)` on contacts, unique `(UserId, CategoryName)` on categories, indexes on user/favorite/company search columns, cascade from `User` to owned entities, and a composite `(ContactId, CategoryId)` join key with restricted category deletion.

## Run

```powershell
dotnet build ContactManagement.slnx
dotnet run --project ContactManagement.API/ContactManagement.API.csproj
```

- Swagger UI: `http://localhost:5136/swagger` (or just `http://localhost:5136/` — root redirects to Swagger)
- Import `docs/ContactManagement.postman_collection.json` into Postman. Run **Login** first; it stores the JWT in the `token` variable used by all protected requests.

## Endpoints

| Method | Route | Description |
| --- | --- | --- |
| POST | `/api/auth/register` | Register a user |
| POST | `/api/auth/login` | Login, returns JWT |
| POST | `/api/contacts` | Create contact |
| GET | `/api/contacts` | List contacts (paged, filtered, sorted) |
| GET | `/api/contacts/search` | Search full name, email, phone, company |
| GET | `/api/contacts/favorites` | List favorite contacts |
| GET | `/api/contacts/{id}` | Get contact |
| PUT | `/api/contacts/{id}` | Update contact |
| DELETE | `/api/contacts/{id}` | Delete contact |
| PATCH | `/api/contacts/{id}/favorite?isFavorite=true` | Toggle favorite |
| POST | `/api/categories` | Create category |
| GET | `/api/categories` | List categories |
| GET | `/api/categories/{id}` | Get category |
| PUT | `/api/categories/{id}` | Update category |
| DELETE | `/api/categories/{id}` | Delete category |
| POST | `/api/categories/{categoryId}/contacts/{contactId}` | Assign contact |
| DELETE | `/api/categories/{categoryId}/contacts/{contactId}` | Remove contact |
| GET | `/api/categories/{categoryId}/contacts` | Contacts in category |

Query parameters for contact listing/search: `search`, `company`, `isFavorite`, `categoryId`, `sortBy` (`FullName`, `Email`, `Company`, `CreatedAt`, `UpdatedAt`, `IsFavorite`), `sortDirection` (`Asc`, `Desc`), `page`, `pageSize` (1–100).

## Sample requests and responses

Register:

```http
POST /api/auth/register
Content-Type: application/json

{
  "fullName": "Jane Doe",
  "email": "jane@example.com",
  "password": "Str0ng!Password"
}
```

```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": 1,
    "fullName": "Jane Doe",
    "email": "jane@example.com",
    "token": "eyJhbGciOiJIUzI1NiIs...",
    "tokenType": "Bearer",
    "expiresAt": "2026-11-21T13:00:00Z"
  }
}
```

Create contact (requires `Authorization: Bearer <token>`):

```http
POST /api/contacts
Content-Type: application/json

{
  "fullName": "John Doe",
  "email": "john@example.com",
  "phoneNumber": "+15551234567",
  "company": "Acme Corp",
  "jobTitle": "Engineer",
  "isFavorite": false,
  "categoryIds": []
}
```

```json
{
  "success": true,
  "message": "Contact created successfully",
  "data": {
    "contactId": 1,
    "fullName": "John Doe",
    "email": "john@example.com",
    "phoneNumber": "+15551234567",
    "company": "Acme Corp",
    "jobTitle": "Engineer",
    "isFavorite": false,
    "categories": []
  }
}
```

Validation failure:

```json
{
  "success": false,
  "message": "Validation failed.",
  "errors": ["Email is required", "Password must contain uppercase, lowercase, digit and special character."]
}
```

Not found / forbidden / unauthorized are returned as HTTP 404 / 403 / 401 with the same envelope. All errors flow through `ExceptionHandlingMiddleware`; raw exception details are never returned in Production.

## Tests

```powershell
dotnet test ContactManagement.slnx
```

Covers service unit tests (create, duplicate email, cross-user access, paging, favorite validation, password hashing) and HTTP integration tests (unauthorized access, register/login, contact lifecycle, category assignment, and user isolation).

## Security notes

- Passwords are stored as PBKDF2-SHA256 (100k iterations) with a random 16-byte salt; plaintext is never persisted.
- JWTs are signed with HMAC-SHA256; issuer, audience, lifetime, and signature are all validated.
- Repository queries always include the authenticated `UserId`, preventing IDOR.
- Duplicate contact emails and category names are enforced per user, both in services and via unique database indexes.
- `AutoMapper`, Data Annotations, and service-level validation prevent malformed input from reaching the database.
- NuGet audit reports NU1902/NU1903 for transitive design-time-only packages pulled in by `Microsoft.EntityFrameworkCore.Design`; these are build-time tooling dependencies and are not part of the deployed application.
