# CLHCRM Backend - .NET 9 Web API

Training School CRM System backend built with .NET 9, following Onion Architecture principles.

## Architecture

This project follows **Onion Architecture** with the following layers:

```
/src
  /CLHCRM.Domain          - Domain entities, value objects, aggregates, events (no dependencies)
  /CLHCRM.Application     - Commands, queries, DTOs, interfaces (depends on Domain)
  /CLHCRM.Infrastructure  - Database, repositories, external services (depends on Application)
  /CLHCRM.Api             - Controllers, middleware, startup (depends on Application & Infrastructure)
/tests
  /CLHCRM.Domain.UnitTests
  /CLHCRM.Application.UnitTests
  /CLHCRM.Api.IntegrationTests
```

### Key Principles

- **Domain-Driven Design (DDD)**: Rich domain model with business logic
- **CQRS**: Separate read (queries) and write (commands) operations
- **Repository Pattern**: One repository per aggregate root (no generic repositories)
- **Vertical Slicing**: Features organized by use case
- **Test-Driven Development**: Write tests before implementation

## Technology Stack

- **.NET 9**: Web API framework
- **Entity Framework Core 9.0**: ORM
- **PostgreSQL**: Database (via Npgsql)
- **MediatR**: CQRS implementation
- **FluentValidation**: Input validation
- **xUnit**: Testing framework
- **Moq**: Mocking framework

## Project Structure

### Domain Layer
```
/Domain
  /Entities         - Core business entities
  /ValueObjects     - Immutable value objects
  /Aggregates       - Aggregate roots
  /Events           - Domain events
```

### Application Layer
```
/Application
  /Features
    /{FeatureName}
      /Commands     - Write operations
      /Queries      - Read operations
      /Validators   - FluentValidation validators
      /Dtos         - Data transfer objects
  /Interfaces       - Repository and service interfaces
```

### Infrastructure Layer
```
/Infrastructure
  /Persistence
    /Repositories   - Repository implementations
    /Migrations     - EF Core migrations
  /Services         - External service implementations
```

### API Layer
```
/Api
  /Controllers      - API controllers
  /Extensions       - Service registration extensions
  /Middleware       - Custom middleware
```

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) (or Docker)
- IDE: Visual Studio 2022, Rider, or VS Code

### Build

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/CLHCRM.Api/CLHCRM.Api.csproj
```

### Run

```bash
# Run API project
dotnet run --project src/CLHCRM.Api

# Run with watch (auto-reload)
dotnet watch run --project src/CLHCRM.Api
```

The API will be available at:
- HTTPS: `https://localhost:7000`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:7000/swagger`

### Test

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/CLHCRM.Domain.UnitTests

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Database Setup

### Connection String

Update `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=clhcrm;Username=postgres;Password=yourpassword"
  }
}
```

### Migrations

```bash
# Add a new migration
dotnet ef migrations add InitialCreate --project src/CLHCRM.Infrastructure --startup-project src/CLHCRM.Api

# Update database
dotnet ef database update --project src/CLHCRM.Infrastructure --startup-project src/CLHCRM.Api

# Remove last migration
dotnet ef migrations remove --project src/CLHCRM.Infrastructure --startup-project src/CLHCRM.Api

# Generate SQL script
dotnet ef migrations script --project src/CLHCRM.Infrastructure --startup-project src/CLHCRM.Api
```

### Docker PostgreSQL (Optional)

```bash
# Run PostgreSQL in Docker
docker run --name clhcrm-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=clhcrm -p 5432:5432 -d postgres:16

# Stop container
docker stop clhcrm-postgres

# Start container
docker start clhcrm-postgres
```

## Development Guidelines

### Adding a New Feature

1. **Define Domain Entities** (if needed)
   - Create entities in `Domain/Entities`
   - Add to appropriate aggregate

2. **Create Feature Folder**
   ```
   /Application/Features/{FeatureName}
   ```

3. **Implement Commands/Queries**
   - Command: `CreateProductCommand.cs`
   - Handler: `CreateProductCommandHandler.cs`
   - Validator: `CreateProductCommandValidator.cs`
   - DTO: `ProductDto.cs`

4. **Create Repository Interface**
   - Add to `Application/Interfaces`
   - Implement in `Infrastructure/Persistence/Repositories`

5. **Add Controller**
   - Create in `Api/Controllers`
   - Use MediatR to send commands/queries

6. **Write Tests**
   - Unit tests for handlers and validators
   - Integration tests for API endpoints

### Example: Creating a Product Feature

```csharp
// 1. Domain Entity
public class Product
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    public Product(string name, decimal price)
    {
        Id = Guid.NewGuid();
        Name = name;
        Price = price;
    }
}

// 2. Command
public class CreateProductCommand : IRequest<Guid>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// 3. Validator
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

// 4. Handler
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Name, request.Price);
        await _repository.AddAsync(product);
        return product.Id;
    }
}

// 5. Controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductCommand command)
    {
        var productId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetProduct), new { id = productId }, null);
    }
}
```

## API Documentation

- Swagger UI is available at `/swagger` when running in Development mode
- API follows RESTful conventions
- All endpoints use DTOs (never expose domain entities)
- Standard HTTP status codes are used

## Module Development Priority (MVP)

1. ✅ Project Setup
2. 🔄 Student Information System
3. ⏳ Admissions & Enrollment
4. ⏳ Fee & Financial Management
5. ⏳ Attendance Tracking
6. ⏳ Timetable Management
7. ⏳ Communication Module
8. ⏳ Reports & Analytics

## Resources

- [Architecture Documentation](../CLHCRM/BACKEND_ARCHITECTURE.md)
- [Requirements](../CLHCRM/REQUIREMENTS.md)
- [Admission Module Spec](../CLHCRM/docs/ADMISSION_MODULE.md)

## Contributing

1. Follow the established architecture patterns
2. Write tests for all new features
3. Use meaningful commit messages
4. Keep the domain layer clean (no dependencies)
5. Validate all inputs using FluentValidation
6. Use DTOs for all API responses

## License

Proprietary - Training School CRM System
