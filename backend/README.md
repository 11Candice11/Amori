# Amori Backend API

A C# ASP.NET Core 9.0 Web API for the Amori relationship companion application.

## Prerequisites

- .NET 9.0 SDK or later
- PostgreSQL 12+
- Insomnia (or Postman) for testing

## Project Structure

```
Amori.Api/
├── Controllers/              # API controllers (Health)
├── Domain/
│   ├── Entities/             # Domain models (User, Relationship, RelationshipMember)
│   └── Enums/                # Enumerations (UserStatus, RelationshipStatus, etc.)
├── Data/
│   ├── Context/              # AmoriDbContext
│   ├── Configurations/       # EF Core entity configurations
│   └── Migrations/           # Database migrations
├── Features/                 # Feature-based folder structure
│   ├── Auth/                 # Authentication (Register, Login)
│   ├── Users/                # User endpoints (GetMe)
│   └── Relationships/        # Relationship management
├── Infrastructure/
│   ├── Authentication/       # JWT token service, password hashing
│   └── Relationships/        # Relationship access service
├── Common/
│   ├── Extensions/           # Service collection extensions
│   ├── Middleware/           # Exception handling middleware
│   ├── Exceptions/           # Custom exceptions
│   └── Responses/            # API response models
├── Configuration/            # Configuration classes
├── Program.cs                # Application startup
├── appsettings.json          # Base configuration
└── appsettings.Development.json  # Development configuration
```

## Getting Started

### 1. PostgreSQL Setup

Start a PostgreSQL server locally or use a connection string to a remote PostgreSQL instance.

#### Option A: PostgreSQL with Docker

```bash
docker run --name amori-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=amori_dev \
  -p 5432:5432 \
  -d postgres:latest
```

#### Option B: Local PostgreSQL

Ensure PostgreSQL is running on `localhost:5432` with username `postgres` and password `postgres`.

### 2. Database Configuration

Edit `appsettings.Development.json` to configure your database connection if needed:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=amori_dev;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "SecretKey": "your-secret-key-at-least-32-characters-long"
  }
}
```

### 3. Build and Restore Dependencies

```bash
cd backend
dotnet restore
dotnet build
```

### 4. Create Database and Run Migrations

```bash
cd backend/Amori.Api
dotnet ef database update
```

This will create the database and apply all migrations.

### 5. Run the API

```bash
cd backend/Amori.Api
dotnet run
```

The API will start on `https://localhost:7001` and `http://localhost:5000` (in development).

### 6. Access Swagger Documentation

Once running, visit:
- **Swagger UI**: https://localhost:7001/swagger/index.html

## JWT Authentication

The API uses JWT Bearer tokens for authentication.

### Obtaining a Token

1. Register a new user via `POST /api/auth/register`
2. Login via `POST /api/auth/login`
3. Copy the `accessToken` from the response

### Using the Token in Requests

Include the token in the Authorization header:

```
Authorization: Bearer <your-token-here>
```

## Running Tests

```bash
cd backend
dotnet test Amori.Api.Tests
```

Tests use Testcontainers to spin up a PostgreSQL container automatically.

## API Endpoints

### Health
- `GET /api/health` - Verify API is running (no authentication required)

### Authentication
- `POST /api/auth/register` - Register a new user
- `POST /api/auth/login` - Login and get JWT token

### Users
- `GET /api/users/me` - Get current authenticated user (requires JWT)

### Relationships
- `POST /api/relationships` - Create a new relationship (requires JWT)
- `GET /api/relationships/me` - Get current user's relationship (requires JWT)
- `POST /api/relationships/{relationshipId}/join` - Join an existing relationship (requires JWT)

## Insomnia Test Flow

### Step 1: Test Health Endpoint
```
GET http://localhost:5000/api/health
```

### Step 2: Register User 1 (Cat)
```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "name": "Cat",
  "email": "cat@example.com",
  "password": "SecurePass123!"
}
```

Save the `accessToken` from the response as `{{cat_token}}`.

### Step 3: Register User 2 (Partner)
```
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "name": "Partner",
  "email": "partner@example.com",
  "password": "SecurePass123!"
}
```

Save the `accessToken` from the response as `{{partner_token}}`.

### Step 4: Login User 1 (to verify login works)
```
POST http://localhost:5000/api/auth/login
Content-Type: application/json

{
  "email": "cat@example.com",
  "password": "SecurePass123!"
}
```

### Step 5: Get Current User (Cat)
```
GET http://localhost:5000/api/users/me
Authorization: Bearer {{cat_token}}
```

### Step 6: Create Relationship
```
POST http://localhost:5000/api/relationships
Authorization: Bearer {{cat_token}}
Content-Type: application/json

{
  "startDate": "2024-01-01"
}
```

Save the `id` from the response as `{{relationship_id}}`.

### Step 7: Join Relationship (Partner)
```
POST http://localhost:5000/api/relationships/{{relationship_id}}/join
Authorization: Bearer {{partner_token}}
```

### Step 8: Get My Relationship
```
GET http://localhost:5000/api/relationships/me
Authorization: Bearer {{cat_token}}
```

Should show both Cat and Partner as members.

## Environment Variables

For production, set these environment variables instead of hardcoding them in `appsettings.json`:

```bash
# Database
Database__ConnectionString=your-connection-string

# JWT
Jwt__SecretKey=your-secret-key-32-chars-or-more
Jwt__Issuer=https://api.amori.app
Jwt__Audience=amori-mobile
Jwt__ExpiryMinutes=60

# CORS
Cors__AllowedOrigins=http://localhost:8081,http://localhost:19006
```

## Development Commands

```bash
# Build the solution
dotnet build

# Run the API
dotnet run

# Run tests
dotnet test

# Create a new migration
cd backend/Amori.Api
dotnet ef migrations add YourMigrationName

# Update database to latest migration
dotnet ef database update

# View pending migrations
dotnet ef migrations list
```

## Logging

The API uses ASP.NET Core's built-in logging. Configure log levels in `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

In development, set `Default` to `Debug` for more detailed logs.

## Error Handling

The API returns consistent JSON error responses:

```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Specific error 1", "Specific error 2"]
}
```

HTTP status codes:
- `200 OK` - Successful request
- `201 Created` - Resource created
- `400 Bad Request` - Invalid input
- `401 Unauthorized` - Missing or invalid authentication
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource already exists or invalid state
- `422 Unprocessable Entity` - Validation failed
- `500 Internal Server Error` - Unexpected server error

## Security Notes

1. **Never commit secrets** - Use `appsettings.Development.json` locally and environment variables for production
2. **Password hashing** - Passwords are hashed using BCrypt with a work factor of 12
3. **JWT secret** - Must be at least 32 characters long in production
4. **CORS** - Currently permissive for development; should be restricted in production
5. **HTTPS** - Enforced in production, optional in development

## Architecture Decisions

- **Feature-based folder structure** - Organizes code by feature rather than by layer
- **Entity Framework Core** - ORM for database access
- **Dependency Injection** - All services registered in `Program.cs`
- **Global exception handling** - Middleware catches all exceptions and returns consistent responses
- **Authentication** - JWT Bearer tokens for stateless authentication
- **Repository pattern** - Not explicitly used; DbContext queries embedded in controllers for simplicity

## Future Improvements

- [ ] Add more comprehensive error handling
- [ ] Implement logging best practices
- [ ] Add request validation middleware
- [ ] Implement caching
- [ ] Add database seeding for development
- [ ] Add role-based authorization
- [ ] Implement API versioning
- [ ] Add OpenAPI/Swagger documentation decorators

## Troubleshooting

### Connection String Issues

If you get a connection error, verify:
1. PostgreSQL is running: `psql -U postgres -d postgres -c "SELECT 1"`
2. Connection string is correct in `appsettings.Development.json`
3. Database exists: `createdb amori_dev`

### Migration Issues

If migrations fail:
1. Ensure `dotnet ef` CLI is installed: `dotnet tool install --global dotnet-ef`
2. Delete failed migrations and reapply
3. Check database logs for specific errors

### JWT Token Issues

- Token must start with `Bearer ` (with space)
- Token must be valid and not expired
- Secret key must match between token generation and validation

## Support

For issues or questions, refer to the main project README.
