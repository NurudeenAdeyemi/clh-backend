# Logging Guide - Serilog Configuration

## Overview

The CLHCRM API uses **Serilog** for structured logging with multiple sinks for different environments.

## Configuration

### Logging Sinks

| Sink | Description | Environment |
|------|-------------|-------------|
| **Console** | Colored output for development | All |
| **File** | Rolling file logs | All |
| **Seq** | Structured log server (optional) | Development |

### Log Levels

| Level | When to Use | Example |
|-------|-------------|---------|
| **Trace** | Very detailed diagnostic info | `Log.Trace("Entering method X")` |
| **Debug** | Debugging information | `Log.Debug("Variable value: {Value}", value)` |
| **Information** | General informational messages | `Log.Information("User {UserId} created", userId)` |
| **Warning** | Warnings and potential issues | `Log.Warning("Slow query: {Duration}ms", duration)` |
| **Error** | Errors and exceptions | `Log.Error(ex, "Failed to process request")` |
| **Fatal** | Critical failures | `Log.Fatal(ex, "Application startup failed")` |

## Current Configuration

### Development Environment
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    },
    "WriteTo": [
      "Console",
      "File (logs/clhcrm-dev-.log)",
      "Seq (http://localhost:5341)"
    ]
  }
}
```

### Production Environment
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      "Console",
      "File (logs/clhcrm-.log)"
    ]
  }
}
```

## Log Output Locations

### Console Output
Visible in terminal when running the application:
```bash
dotnet run --project src/CLHCRM.Api
```

Example output:
```
[14:23:45 INF] Starting CLHCRM API...
[14:23:46 INF] CLHCRM API started successfully
[14:23:47 INF] HTTP GET /health responded 200 in 5.2340 ms
```

### File Logs
Location: `logs/` directory in project root

Files:
- **Development**: `logs/clhcrm-dev-20251206.log`
- **Production**: `logs/clhcrm-20251206.log`

Features:
- Daily rolling (new file each day)
- Retained for 30 days (production) / 7 days (development)
- Full structured data in text format

Example log entry:
```
2025-12-06 14:23:47.123 +00:00 [INF] [CLHCRM.Api.Controllers.StudentsController] Student created {StudentId="123", Name="John Doe"}
```

### Seq (Development Only)
Structured log viewer with powerful querying capabilities.

**Setup:**
```bash
docker run --name seq -d -e ACCEPT_EULA=Y -p 5341:5341 -p 80:80 datalust/seq:latest
```

**Access:** http://localhost:80

**Features:**
- Full-text search
- Filter by properties
- Query language
- Real-time tail
- Dashboards and alerts

## Using Serilog in Code

### In Controllers
```csharp
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/[controller]")]
public class StudentsController : ControllerBase
{
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(ILogger<StudentsController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateStudent(CreateStudentCommand command)
    {
        _logger.LogInformation("Creating student: {@Command}", command);

        try
        {
            var studentId = await _mediator.Send(command);

            _logger.LogInformation("Student created successfully: {StudentId}", studentId);

            return CreatedAtAction(nameof(GetStudent), new { id = studentId }, null);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for student creation");
            return BadRequest(ex.Errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create student");
            throw; // Global exception handler will catch
        }
    }
}
```

### In Application Handlers
```csharp
public class CreateStudentCommandHandler : IRequestHandler<CreateStudentCommand, Guid>
{
    private readonly ILogger<CreateStudentCommandHandler> _logger;
    private readonly IStudentRepository _repository;

    public CreateStudentCommandHandler(
        ILogger<CreateStudentCommandHandler> logger,
        IStudentRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task<Guid> Handle(CreateStudentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Handling CreateStudentCommand for {Email}", request.Email);

        var student = new Student(request.FirstName, request.LastName, request.Email);

        await _repository.AddAsync(student);

        _logger.LogInformation("Student {StudentId} created in database", student.Id);

        return student.Id;
    }
}
```

### In Domain Layer
```csharp
// Domain layer should NOT depend on logging
// Use domain events instead, which can be logged in handlers

public class Student
{
    public void Enroll(Course course)
    {
        if (!CanEnroll(course))
        {
            // Don't log here - throw domain exception
            throw new DomainException("Student cannot enroll in this course");
        }

        // Add domain event instead of logging
        AddDomainEvent(new StudentEnrolledEvent(Id, course.Id));
    }
}

// Then log in the event handler (Infrastructure layer)
public class StudentEnrolledEventHandler : INotificationHandler<StudentEnrolledEvent>
{
    private readonly ILogger<StudentEnrolledEventHandler> _logger;

    public async Task Handle(StudentEnrolledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Student {StudentId} enrolled in course {CourseId}",
            notification.StudentId,
            notification.CourseId);
    }
}
```

## Best Practices

### 1. Use Structured Logging
✅ **Good:**
```csharp
_logger.LogInformation("User {UserId} logged in from {IpAddress}", userId, ipAddress);
```

❌ **Avoid:**
```csharp
_logger.LogInformation($"User {userId} logged in from {ipAddress}");
```

### 2. Log at Appropriate Levels
- **Debug**: Diagnostic info for developers
- **Information**: Important business events
- **Warning**: Recoverable issues
- **Error**: Unhandled exceptions
- **Fatal**: Application crashes

### 3. Include Context
```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["UserId"] = userId,
    ["TenantId"] = tenantId
}))
{
    _logger.LogInformation("Processing order {OrderId}", orderId);
    // All logs in this scope will include UserId and TenantId
}
```

### 4. Don't Log Sensitive Data
❌ **Never log:**
- Passwords
- Credit card numbers
- Social security numbers
- Authentication tokens
- Personal identifiable information (PII) without masking

✅ **Use masking:**
```csharp
_logger.LogInformation("Processing payment for card ending in {CardLast4}", cardNumber.Substring(cardNumber.Length - 4));
```

### 5. Use Semantic Logging
```csharp
// Define log event IDs
public static class LogEvents
{
    public const int StudentCreated = 1001;
    public const int StudentUpdated = 1002;
    public const int StudentDeleted = 1003;
}

// Use in logging
_logger.LogInformation(LogEvents.StudentCreated, "Student {StudentId} created", studentId);
```

## HTTP Request Logging

Automatic request/response logging is configured via `UseSerilogRequestLogging()`:

```
[14:23:47 INF] HTTP GET /api/students responded 200 in 23.4567 ms
```

Logged properties:
- `RequestMethod` - HTTP method (GET, POST, etc.)
- `RequestPath` - URL path
- `StatusCode` - HTTP response code
- `Elapsed` - Request duration in milliseconds
- `RequestHost` - Host header
- `RequestScheme` - http or https
- `UserAgent` - Client user agent

## Exception Logging

Global exception handler automatically logs all unhandled exceptions:

```csharp
// GlobalExceptionHandlerMiddleware.cs
_logger.LogError(ex, "An unhandled exception occurred");
```

Includes:
- Full exception details
- Stack trace
- Request context
- Timestamp

## Performance Considerations

### Async Logging
Serilog writes asynchronously to prevent blocking requests.

### Batching
File and Seq sinks batch writes for efficiency.

### Level Control
Reduce log level in production to minimize I/O:
- Development: `Debug`
- Staging: `Information`
- Production: `Information` or `Warning`

### Sampling
For high-traffic endpoints, consider sampling:
```csharp
if (Random.Shared.Next(100) < 10) // Log 10% of requests
{
    _logger.LogDebug("Detailed debug info");
}
```

## Viewing Logs

### During Development
1. **Console**: Real-time logs in terminal
2. **Seq**: http://localhost:80 (if running)
3. **Files**: `logs/clhcrm-dev-*.log`

### In Production
1. **Centralized Logging**: Ship logs to:
   - **Seq** (self-hosted or Seq cloud)
   - **Azure Application Insights**
   - **AWS CloudWatch**
   - **Datadog**
   - **Elasticsearch (ELK Stack)**

2. **File Monitoring**: Use tools like:
   - **tail -f logs/clhcrm-*.log**
   - **less +F logs/clhcrm-*.log**
   - **Log management tools**

## Running Seq Locally (Optional)

### Start Seq with Docker
```bash
docker run --name seq -d \
  -e ACCEPT_EULA=Y \
  -p 5341:5341 \
  -p 80:80 \
  -v seq-data:/data \
  datalust/seq:latest
```

### Access Seq
- **UI**: http://localhost:80
- **Ingestion**: http://localhost:5341

### Stop/Start Seq
```bash
docker stop seq
docker start seq
```

### Remove Seq
```bash
docker rm -f seq
docker volume rm seq-data
```

## Troubleshooting

### No Logs Appearing
1. Check `appsettings.json` configuration
2. Verify minimum log level
3. Check file permissions for `logs/` directory
4. Ensure Serilog packages are installed

### Logs Not in Seq
1. Verify Seq is running: `docker ps | grep seq`
2. Check Seq URL in appsettings: `http://localhost:5341`
3. Test connection: `curl http://localhost:5341/api`

### Too Many Logs
1. Increase minimum log level to `Information` or `Warning`
2. Add overrides for noisy namespaces:
   ```json
   {
     "Serilog": {
       "MinimumLevel": {
         "Override": {
           "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
         }
       }
     }
   }
   ```

### Logs Directory Not Created
Serilog creates the directory automatically. If issues occur:
```bash
mkdir -p logs
chmod 755 logs
```

## Configuration Examples

### Minimal Console-Only (Testing)
```json
{
  "Serilog": {
    "MinimumLevel": "Debug",
    "WriteTo": [
      { "Name": "Console" }
    ]
  }
}
```

### Production with Monitoring
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/clhcrm-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "https://your-seq-server.com",
          "apiKey": "your-api-key"
        }
      }
    ]
  }
}
```

## References

- [Serilog Documentation](https://serilog.net/)
- [Serilog ASP.NET Core](https://github.com/serilog/serilog-aspnetcore)
- [Seq Documentation](https://docs.datalust.co/docs)
- [Structured Logging Best Practices](https://github.com/serilog/serilog/wiki/Writing-Log-Events)
