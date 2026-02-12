# Anonymization API - Backend

ASP.NET Core 8.0 backend API for the Anonymization Platform, providing REST endpoints for data connection management, configuration, job execution, and user management.

## Tech Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: PostgreSQL 16
- **ORM**: Entity Framework Core with code-first approach
- **Authentication**: Auth0 (JWT Bearer)
- **Container**: Docker & Docker Compose
- **API Documentation**: Swagger/OpenAPI

## Prerequisites

- .NET SDK 8.0 or later
- PostgreSQL 16 or later
- Docker & Docker Compose (optional, for containerized deployment)
- Auth0 account with configured API

## Project Structure

```
src/
├── AnonymizationApi.csproj           # Project file with NuGet dependencies
├── Program.cs                         # Application startup and configuration
├── Controllers/                       # REST API endpoints
│   ├── ConnectionsController.cs       # Connection management endpoints
│   ├── ConfigurationsController.cs    # Configuration CRUD endpoints
│   ├── JobsController.cs              # Job execution endpoints
│   ├── UsersController.cs             # User management endpoints
│   ├── ActivityController.cs          # Audit logging endpoints
│   └── DashboardController.cs         # Statistics endpoints
├── Services/                          # Business logic layer
│   ├── ConnectionService.cs           # Connection operations
│   ├── ConfigurationService.cs        # Configuration operations
│   ├── JobService.cs                  # Job execution logic
│   ├── UserService.cs                 # User operations
│   └── ActivityService.cs             # Activity logging
├── Data/                              # Data access layer
│   └── ApplicationDbContext.cs        # Entity Framework DbContext
├── Models/                            # Domain models
│   ├── Connection.cs
│   ├── Configuration.cs
│   ├── Job.cs
│   ├── User.cs
│   ├── ActivityLog.cs
│   └── ConnectionMetadata.cs
├── DTOs/                              # Data Transfer Objects
│   ├── ConnectionDtos.cs
│   ├── ConfigurationDtos.cs
│   ├── JobDtos.cs
│   ├── UserDtos.cs
│   └── ActivityDtos.cs
├── Migrations/                        # EF Core database migrations
├── Properties/                        # Project properties
│   ├── launchSettings.json            # Debug/Release profiles
│   └── AssemblyInfo.cs
├── appsettings.json                   # Configuration file
├── appsettings.Development.json       # Development settings
└── appsettings.Production.json        # Production settings
```

## Installation & Setup

### 1. Clone Repository

```bash
git clone https://Stratsy@dev.azure.com/Stratsy/Anon/_git/Anon_backend.git
cd Anon_backend
```

### 2. Configure Environment Variables

Copy `.env.example` to `.env.local` and update with your configuration:

```bash
cp .env.example .env.local
```

Update the following values:
- `DATABASE_URL` - PostgreSQL connection string
- `AUTH0_DOMAIN` - Your Auth0 domain
- `AUTH0_CLIENT_ID` - Your Auth0 machine-to-machine client ID
- `AUTH0_CLIENT_SECRET` - Your Auth0 machine-to-machine client secret
- `AUTH0_AUDIENCE` - Your Auth0 API identifier
- `CORS_FRONTEND_URL` - Frontend URL for CORS (default: http://localhost:5173)

### 3. Install Dependencies

```bash
cd src
dotnet restore
```

### 4. Database Setup

Create and apply migrations:

```bash
# Add initial migration (if not already done)
dotnet ef migrations add InitialCreate

# Apply migrations to database
dotnet ef database update
```

### 5. Run Locally

```bash
dotnet run
```

The API will be available at `http://localhost:5000`

### 6. View API Documentation

Navigate to `http://localhost:5000/swagger/index.html` to view Swagger documentation and test endpoints.

## Docker Setup

### Build Docker Image

```bash
docker build -t anon-backend:latest .
```

### Run with Docker Compose

Copy `.env.local` and ensure environment variables are set:

```bash
docker-compose up
```

This will start:
- PostgreSQL database on port 5432
- ASP.NET Core API on port 5000
- (Optional) React frontend on port 5173

## API Endpoints

### Connections
- `GET /api/connections` - List all connections
- `GET /api/connections/{id}` - Get connection details
- `POST /api/connections` - Create new connection
- `PUT /api/connections/{id}` - Update connection
- `DELETE /api/connections/{id}` - Delete connection
- `POST /api/connections/{id}/test` - Test connection
- `GET /api/connections/{id}/metadata` - Get schema metadata
- `POST /api/connections/{id}/refresh-metadata` - Refresh metadata

### Configurations
- `GET /api/configurations` - List all configurations
- `GET /api/configurations/{id}` - Get configuration
- `POST /api/configurations` - Create configuration
- `PUT /api/configurations/{id}` - Update configuration
- `DELETE /api/configurations/{id}` - Delete configuration

### Jobs
- `GET /api/jobs` - List all jobs
- `GET /api/jobs/{id}` - Get job details
- `POST /api/jobs` - Create new job
- `PUT /api/jobs/{id}` - Update job
- `DELETE /api/jobs/{id}` - Delete job
- `POST /api/jobs/{id}/start` - Start job execution
- `GET /api/jobs/{id}/progress` - Get job progress

### Users
- `GET /api/users` - List all users
- `GET /api/users/{id}` - Get user
- `POST /api/users` - Create user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

### Activity & Dashboard
- `GET /api/activity` - Get activity logs
- `GET /api/dashboard/stats` - Get dashboard statistics

All endpoints except `GET /api/dashboard/stats/health` require Auth0 JWT authentication.

## Database Models

### Connection
Data source definitions (PostgreSQL, MySQL, S3, Azure, etc.)

### Configuration
De-identification, consistency, and referential integrity rules

### Job
Processing jobs that execute configurations on data

### User
User accounts with role-based access control

### ActivityLog
Audit trail of all system operations

### ConnectionMetadata
Cached schema/metadata from data sources

## Authentication

The API uses Auth0 JWT tokens for authentication. Include the token in the Authorization header:

```
Authorization: Bearer <your_auth0_token>
```

Obtain a token from your Auth0 application and include it with requests to protected endpoints.

## Development

### Run Tests

```bash
dotnet test
```

### Database Migrations

Create a new migration:

```bash
dotnet ef migrations add MigrationName
```

Apply migrations:

```bash
dotnet ef database update
```

Revert last migration:

```bash
dotnet ef database update PreviousMirationName
```

### Code Style

The project follows C# coding standards:
- Use async/await for I/O operations
- Dependency injection for services
- DTO pattern for API contracts
- Entity Framework Core for data access

## Troubleshooting

### Application won't start
- Verify `.env.local` contains all required variables
- Check PostgreSQL is running and accessible
- Review logs in `appsettings.Development.json`

### Database connection fails
- Verify `DATABASE_URL` connection string
- Ensure PostgreSQL is running: `psql -U postgres`
- Check network connectivity to database host

### Auth0 validation fails
- Verify `AUTH0_DOMAIN` and `AUTH0_AUDIENCE` are correct
- Check Auth0 application is configured for this API
- Ensure JWT token hasn't expired

## Deployment

### Production Deployment

1. Set `ASPNETCORE_ENVIRONMENT=Production`
2. Configure production PostgreSQL database
3. Update Auth0 secrets in deployment environment
4. Use strong database and Auth0 credentials
5. Enable HTTPS in production
6. Configure proper CORS settings
7. Set up monitoring and logging

### Kubernetes Deployment

See `kubernetes/` directory for Kubernetes manifests (if available)

## Contributing

1. Create feature branch: `git checkout -b feature/your-feature`
2. Make changes and commit: `git commit -am 'Add feature'`
3. Push to remote: `git push origin feature/your-feature`
4. Create pull request

## License

Proprietary - Stratsy

## Support

For issues or questions:
1. Check existing issues in Azure DevOps
2. Create new issue with detailed description
3. Contact the development team
