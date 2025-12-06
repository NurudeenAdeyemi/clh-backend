# Implementation Roadmap

## Current Status: Foundation Complete ✅

Your architecture has the essential building blocks. Now we build on top of it.

---

## Phase 1: Security & Domain Foundations 🔴 CRITICAL
**Timeline:** Week 1-2 | **Priority:** Must complete before building features

### 1.1 Authentication & Authorization (3-4 days)
**Packages:**
```bash
dotnet add src/CLHCRM.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/CLHCRM.Infrastructure package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/CLHCRM.Infrastructure package System.IdentityModel.Tokens.Jwt
```

**Implementation:**
- [ ] Create `ApplicationUser` entity extending `IdentityUser`
- [ ] Add Identity DbContext configuration
- [ ] Implement JWT token generation service
- [ ] Create `AuthController` (Login, Register, RefreshToken)
- [ ] Add JWT authentication middleware
- [ ] Implement role-based authorization (`[Authorize(Roles = "Admin")]`)
- [ ] Add claims-based policies (`[Authorize(Policy = "CanEditCourses")]`)
- [ ] Create password hashing service
- [ ] Implement token refresh mechanism

**Files to create:**
```
src/CLHCRM.Domain/Entities/ApplicationUser.cs
src/CLHCRM.Application/Features/Auth/Commands/LoginCommand.cs
src/CLHCRM.Application/Features/Auth/Commands/RegisterCommand.cs
src/CLHCRM.Application/Interfaces/ITokenService.cs
src/CLHCRM.Infrastructure/Services/JwtTokenService.cs
src/CLHCRM.Api/Controllers/AuthController.cs
```

**Configuration:**
```json
{
  "Jwt": {
    "Secret": "your-256-bit-secret-change-in-production",
    "Issuer": "CLHCRM.Api",
    "Audience": "CLHCRM.Client",
    "ExpiryMinutes": 60,
    "RefreshExpiryDays": 7
  }
}
```

---

### 1.2 Common Domain Patterns (1 day)

#### Result Pattern
**Why:** Avoid exceptions for business logic failures
```csharp
public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }
    public static Result Success() => new Result(true, null);
    public static Result Failure(string error) => new Result(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }
    public static Result<T> Success(T value) => new Result<T>(true, value, null);
    public static Result<T> Failure(string error) => new Result<T>(false, default, error);
}
```

**Files to create:**
```
src/CLHCRM.Domain/Common/Result.cs
src/CLHCRM.Domain/Common/Result{T}.cs
src/CLHCRM.Domain/Common/Error.cs
```

#### Base Entity
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public string CreatedBy { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public string? UpdatedBy { get; protected set; }
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public string? DeletedBy { get; protected set; }
}
```

**Files to create:**
```
src/CLHCRM.Domain/Common/BaseEntity.cs
src/CLHCRM.Domain/Common/BaseAuditableEntity.cs
src/CLHCRM.Domain/Common/IAuditable.cs
src/CLHCRM.Domain/Common/ISoftDeletable.cs
```

#### Value Objects
```csharp
public class Email : ValueObject
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Failure("Email cannot be empty");

        if (!IsValidEmail(email))
            return Result<Email>.Failure("Invalid email format");

        return Result<Email>.Success(new Email(email));
    }
}
```

**Files to create:**
```
src/CLHCRM.Domain/ValueObjects/Email.cs
src/CLHCRM.Domain/ValueObjects/PhoneNumber.cs
src/CLHCRM.Domain/ValueObjects/Money.cs
src/CLHCRM.Domain/ValueObjects/Address.cs
src/CLHCRM.Domain/Common/ValueObject.cs
```

#### Common Exceptions
```csharp
public class DomainException : Exception
public class NotFoundException : Exception
public class ValidationException : Exception
public class UnauthorizedException : Exception
```

**Files to create:**
```
src/CLHCRM.Domain/Exceptions/DomainException.cs
src/CLHCRM.Domain/Exceptions/NotFoundException.cs
src/CLHCRM.Domain/Exceptions/ValidationException.cs
src/CLHCRM.Application/Exceptions/UnauthorizedException.cs
```

---

### 1.3 Audit Fields & Soft Deletes (1 day)

**EF Core Interceptor:**
```csharp
public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public override InterceptionResult<int> SavingChanges(...)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void UpdateEntities(DbContext context)
    {
        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = _currentUserService.UserId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = _currentUserService.UserId;
            }
        }
    }
}
```

**Soft Delete Global Filter:**
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Apply soft delete filter globally
    modelBuilder.Entity<BaseEntity>().HasQueryFilter(e => !e.IsDeleted);
}
```

**Files to create:**
```
src/CLHCRM.Infrastructure/Persistence/Interceptors/AuditableEntityInterceptor.cs
src/CLHCRM.Infrastructure/Persistence/Interceptors/SoftDeleteInterceptor.cs
src/CLHCRM.Application/Interfaces/ICurrentUserService.cs
src/CLHCRM.Infrastructure/Services/CurrentUserService.cs
```

---

### 1.4 Unit of Work Pattern (1 day)

```csharp
public interface IUnitOfWork : IDisposable
{
    IStudentRepository Students { get; }
    ICourseRepository Courses { get; }
    IApplicationRepository Applications { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
```

**Files to create:**
```
src/CLHCRM.Application/Interfaces/IUnitOfWork.cs
src/CLHCRM.Infrastructure/Persistence/UnitOfWork.cs
```

---

### 1.5 DTO Mapping (1 day)

**Option A: AutoMapper**
```bash
dotnet add src/CLHCRM.Application package AutoMapper
dotnet add src/CLHCRM.Application package AutoMapper.Extensions.Microsoft.DependencyInjection
```

**Option B: Mapster** (Recommended - faster)
```bash
dotnet add src/CLHCRM.Application package Mapster
dotnet add src/CLHCRM.Application package Mapster.DependencyInjection
```

**Files to create:**
```
src/CLHCRM.Application/Mappings/MappingProfile.cs (if AutoMapper)
src/CLHCRM.Application/Mappings/MappingConfig.cs (if Mapster)
```

---

## Phase 2: Production Hardening 🟡 IMPORTANT
**Timeline:** Week 3 | **Priority:** Before MVP deployment

### 2.1 API Versioning (0.5 days)
```bash
dotnet add src/CLHCRM.Api package Asp.Versioning.Http
dotnet add src/CLHCRM.Api package Asp.Versioning.Mvc.ApiExplorer
```

**Implementation:**
```csharp
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
```

**Controller:**
```csharp
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class StudentsController : ControllerBase
```

---

### 2.2 Enhanced Health Checks (0.5 days)
```bash
dotnet add src/CLHCRM.Api package AspNetCore.HealthChecks.NpgSql
dotnet add src/CLHCRM.Api package AspNetCore.HealthChecks.UI
dotnet add src/CLHCRM.Api package AspNetCore.HealthChecks.UI.Client
```

**Implementation:**
```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
    .AddCheck("api", () => HealthCheckResult.Healthy("API is running"))
    .AddCheck<DatabaseHealthCheck>("database-detailed");

builder.Services.AddHealthChecksUI().AddInMemoryStorage();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI(options => options.UIPath = "/health-ui");
```

---

### 2.3 Security Headers (0.5 days)
```bash
dotnet add src/CLHCRM.Api package NetEscapades.AspNetCore.SecurityHeaders
```

**Implementation:**
```csharp
app.UseSecurityHeaders(policies =>
    policies
        .AddDefaultSecurityHeaders()
        .AddStrictTransportSecurityMaxAgeIncludeSubDomains()
        .AddContentSecurityPolicy(builder =>
        {
            builder.AddDefaultSrc().Self();
            builder.AddScriptSrc().Self().UnsafeInline();
            builder.AddStyleSrc().Self().UnsafeInline();
        })
        .RemoveServerHeader()
);
```

---

### 2.4 Rate Limiting (0.5 days)
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
});

app.UseRateLimiter();
```

---

### 2.5 Response Compression (0.25 days)
```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

app.UseResponseCompression();
```

---

### 2.6 Secrets Management (1 day)

**Development:**
```bash
dotnet user-secrets init --project src/CLHCRM.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost..." --project src/CLHCRM.Api
```

**Production (Azure Key Vault):**
```bash
dotnet add src/CLHCRM.Api package Azure.Identity
dotnet add src/CLHCRM.Api package Azure.Extensions.AspNetCore.Configuration.Secrets
```

```csharp
if (builder.Environment.IsProduction())
{
    var keyVaultUri = new Uri(builder.Configuration["KeyVault:Uri"]!);
    builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
}
```

---

## Phase 3: Resilience & Monitoring 🟡 IMPORTANT
**Timeline:** Week 4 | **Priority:** Before production

### 3.1 Resilience with Polly (2 days)
```bash
dotnet add src/CLHCRM.Infrastructure package Microsoft.Extensions.Http.Resilience
```

**HTTP Client with Resilience:**
```csharp
builder.Services.AddHttpClient<IEmailService, EmailService>()
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.Delay = TimeSpan.FromSeconds(2);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(10);
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(30);
    });
```

**Database Resilience:**
```csharp
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorCodesToAdd: null);
    }));
```

---

### 3.2 Application Metrics (1 day)
```bash
dotnet add src/CLHCRM.Api package App.Metrics.AspNetCore.All
```

**Custom Metrics:**
```csharp
public class MetricsService
{
    private readonly IMetrics _metrics;

    public void RecordStudentEnrollment()
    {
        _metrics.Measure.Counter.Increment(new CounterOptions
        {
            Name = "students_enrolled_total",
            MeasurementUnit = Unit.Items
        });
    }
}
```

---

### 3.3 Enhanced Swagger (ongoing)
```bash
dotnet add src/CLHCRM.Api package Swashbuckle.AspNetCore.Annotations
```

**XML Documentation:**
```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
  <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

**Controller Documentation:**
```csharp
/// <summary>
/// Creates a new student
/// </summary>
/// <param name="command">Student creation details</param>
/// <returns>The created student ID</returns>
/// <response code="201">Student created successfully</response>
/// <response code="400">Invalid input</response>
/// <response code="401">Unauthorized</response>
[HttpPost]
[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> CreateStudent([FromBody] CreateStudentCommand command)
```

---

## Phase 4: Feature Support (As Needed)
**Timeline:** Weeks 5-8 | **Priority:** Based on feature requirements

### 4.1 Email Service (2 days)
```bash
dotnet add src/CLHCRM.Infrastructure package SendGrid
```

### 4.2 File Storage (2 days)
```bash
dotnet add src/CLHCRM.Infrastructure package Azure.Storage.Blobs
```

### 4.3 Background Jobs (2 days)
```bash
dotnet add src/CLHCRM.Infrastructure package Hangfire
dotnet add src/CLHCRM.Infrastructure package Hangfire.PostgreSql
```

### 4.4 Caching (1 day)
```bash
dotnet add src/CLHCRM.Infrastructure package StackExchange.Redis
dotnet add src/CLHCRM.Api package Microsoft.AspNetCore.OutputCaching
```

### 4.5 Domain Events (2 days)
Create event dispatcher and handlers

---

## Suggested Next Steps (This Week)

### Day 1-2: Authentication
- Implement JWT authentication
- Add user registration/login
- Configure authorization policies

### Day 3: Domain Patterns
- Create Result pattern
- Create BaseEntity with audit fields
- Create common value objects

### Day 4: Repository Pattern Enhancement
- Implement Unit of Work
- Add audit interceptors
- Add soft delete support

### Day 5: DTO Mapping
- Choose and configure Mapster
- Create mapping profiles
- Update existing code to use DTOs

---

## Progress Tracking

```
Phase 1: Security & Foundations
[ ] 1.1 Authentication & Authorization
[ ] 1.2 Common Domain Patterns
[ ] 1.3 Audit Fields & Soft Deletes
[ ] 1.4 Unit of Work Pattern
[ ] 1.5 DTO Mapping

Phase 2: Production Hardening
[ ] 2.1 API Versioning
[ ] 2.2 Enhanced Health Checks
[ ] 2.3 Security Headers
[ ] 2.4 Rate Limiting
[ ] 2.5 Response Compression
[ ] 2.6 Secrets Management

Phase 3: Resilience & Monitoring
[ ] 3.1 Resilience with Polly
[ ] 3.2 Application Metrics
[ ] 3.3 Enhanced Swagger

Phase 4: Feature Support
[ ] 4.1 Email Service (when needed for Admission Module)
[ ] 4.2 File Storage (when needed for documents)
[ ] 4.3 Background Jobs (when needed)
[ ] 4.4 Caching (when performance needed)
[ ] 4.5 Domain Events (for clean architecture)
```

---

## Estimated Timeline

- **Phase 1:** 1-2 weeks (10-15 days)
- **Phase 2:** 1 week (5 days)
- **Phase 3:** 1 week (5 days)
- **Phase 4:** As needed (2-4 weeks)

**Total:** 4-6 weeks to complete all critical components

---

## Quick Reference: Package Additions

```bash
# Phase 1: Security & Foundations
dotnet add src/CLHCRM.Api package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/CLHCRM.Infrastructure package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/CLHCRM.Application package Mapster

# Phase 2: Production Hardening
dotnet add src/CLHCRM.Api package Asp.Versioning.Http
dotnet add src/CLHCRM.Api package AspNetCore.HealthChecks.NpgSql
dotnet add src/CLHCRM.Api package NetEscapades.AspNetCore.SecurityHeaders

# Phase 3: Resilience & Monitoring
dotnet add src/CLHCRM.Infrastructure package Microsoft.Extensions.Http.Resilience
dotnet add src/CLHCRM.Api package App.Metrics.AspNetCore.All
```

---

## Decision Points

### Mapping: AutoMapper vs Mapster
**Recommendation:** Mapster
- Faster performance
- Less "magic"
- Compile-time safety

### Background Jobs: Hangfire vs Quartz.NET
**Recommendation:** Hangfire
- Built-in dashboard
- Easier setup
- Better for most use cases

### Caching: In-Memory vs Redis
**Recommendation:** Start with In-Memory, add Redis later
- In-memory sufficient for MVP
- Redis when multiple instances or cross-service caching needed

---

Would you like me to start implementing any of these phases? I recommend starting with Phase 1 (Security & Foundations) as it's critical for building any features on top.
