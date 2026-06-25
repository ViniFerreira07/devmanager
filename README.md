# DevManager

Corporate developer management system with full CRUD operations, authentication, and reporting capabilities.

## Technologies

### Backend (.NET)
- **Framework**: .NET 8
- **Architecture**: Clean Architecture (Domain, Application, Infrastructure, API)
- **Database**: PostgreSQL with Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **PDF Reports**: QuestPDF

### Frontend (Next.js)
- **Framework**: Next.js 14 (App Router)
- **Language**: TypeScript
- **Styling**: Tailwind CSS v4
- **Form Validation**: react-hook-form + zod
- **UI Components**: Radix UI + custom components

## Architecture

```
DevManager/
├── DevManager.Domain/          # Entities, interfaces, enums
├── DevManager.Application/     # Services, DTOs, validators
├── DevManager.Infrastructure/  # DbContext, repositories, identity
├── DevManager.Api/             # Controllers, middleware, startup
└── DevManager.Tests/          # Unit tests
```

## Docker

```bash
# Build and start all services
docker compose up -d

# Services started:
# - PostgreSQL (port 5432)
# - .NET API (port 5000)
# - Next.js Frontend (port 3000)
```

## Endpoints

### Authentication
- `POST /api/auth/login` - Login with email/password
- `POST /api/auth/register` - Register new user

### Developers
- `GET /api/developers` - List with pagination and filters
- `GET /api/developers/{id}` - Get by ID
- `POST /api/developers` - Create developer
- `PUT /api/developers/{id}` - Update developer
- `DELETE /api/developers/{id}` - Soft delete developer

### States
- `GET /api/states` - List all states
- `POST /api/states` - Create state
- `PUT /api/states/{id}` - Update state
- `DELETE /api/states/{id}` - Delete state

### Cities
- `GET /api/cities` - List all cities
- `POST /api/cities` - Create city
- `PUT /api/cities/{id}` - Update city
- `DELETE /api/cities/{id}` - Delete city

### Programming Languages
- `GET /api/languages` - List all languages
- `POST /api/languages` - Create language
- `PUT /api/languages/{id}` - Update language
- `DELETE /api/languages/{id}` - Delete language

### Reports
- `GET /api/reports/developers/pdf` - Generate PDF report

## Seed User

```
Email: admin@devmanager.com
Password: Admin@123
```

## Technical Decisions

1. **Clean Architecture**: Separation of concerns with independent layers
2. **Soft Delete**: Developers are soft-deleted (DeletedAt timestamp) for audit purposes
3. **JWT Authentication**: Stateless authentication with bearer tokens
4. **SHA256 Password Hashing**: Password hashing using .NET's built-in cryptographic services
5. **Global Query Filters**: Soft-deleted entities excluded from queries automatically
6. **FluentValidation**: Fluent validation API for request validation
7. **AutoMapper**: DTO to entity mapping with profiles
8. **QuestPDF**: Generates developer reports in PDF format