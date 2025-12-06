# Database Setup Guide

This guide will help you set up PostgreSQL for the CLHCRM backend application.

## Connection String Configuration

The database connection is configured in the `appsettings.json` files:

### Development Environment
Location: `src/CLHCRM.Api/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=clhcrm_dev;Username=postgres;Password=postgres;Include Error Detail=true"
  }
}
```

### Production Environment
Location: `src/CLHCRM.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=clhcrm;Username=postgres;Password=yourpassword;Include Error Detail=true"
  }
}
```

### Local Override (Optional)
For personal local settings, copy `appsettings.Local.json.example` to `appsettings.Local.json`:

```bash
cp appsettings.Local.json.example src/CLHCRM.Api/appsettings.Local.json
```

Then update the connection string with your local credentials. This file is ignored by git.

## PostgreSQL Installation

### Option 1: Install PostgreSQL Locally

#### Windows
1. Download PostgreSQL from: https://www.postgresql.org/download/windows/
2. Run the installer and follow the wizard
3. Set a password for the `postgres` user (remember this!)
4. Default port is 5432
5. Optionally install pgAdmin for database management

#### macOS
```bash
# Using Homebrew
brew install postgresql@16
brew services start postgresql@16
```

#### Linux (Ubuntu/Debian)
```bash
sudo apt update
sudo apt install postgresql postgresql-contrib
sudo systemctl start postgresql
sudo systemctl enable postgresql
```

### Option 2: Use Docker (Recommended for Development)

#### Prerequisites
- Docker Desktop installed: https://www.docker.com/products/docker-desktop

#### Start PostgreSQL Container

```bash
# Run PostgreSQL 16 in Docker
docker run --name clhcrm-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=clhcrm_dev \
  -p 5432:5432 \
  -d postgres:16

# Verify container is running
docker ps
```

#### Container Management Commands

```bash
# Stop container
docker stop clhcrm-postgres

# Start container
docker start clhcrm-postgres

# View logs
docker logs clhcrm-postgres

# Remove container (if you want to start fresh)
docker rm -f clhcrm-postgres
```

#### Using Docker Compose (Optional)

Create `docker-compose.yml` in the project root:

```yaml
version: '3.8'
services:
  postgres:
    image: postgres:16
    container_name: clhcrm-postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: clhcrm_dev
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

volumes:
  postgres_data:
```

Then run:
```bash
docker-compose up -d
```

## Database Creation

### Manual Database Creation (If needed)

```bash
# Connect to PostgreSQL
psql -U postgres -h localhost

# Create database
CREATE DATABASE clhcrm_dev;

# List databases to verify
\l

# Exit
\q
```

## Entity Framework Core Migrations

### Install EF Core Tools (if not already installed)

```bash
dotnet tool install --global dotnet-ef
# Or update existing
dotnet tool update --global dotnet-ef
```

### Create Initial Migration

```bash
# Navigate to project root
cd /mnt/c/Sandbox/CLHCONFIG/CLHCRM-Backend

# Create migration
dotnet ef migrations add InitialCreate \
  --project src/CLHCRM.Infrastructure \
  --startup-project src/CLHCRM.Api
```

### Apply Migration to Database

```bash
# Update database with migrations
dotnet ef database update \
  --project src/CLHCRM.Infrastructure \
  --startup-project src/CLHCRM.Api
```

### Useful Migration Commands

```bash
# Remove last migration (if not applied)
dotnet ef migrations remove \
  --project src/CLHCRM.Infrastructure \
  --startup-project src/CLHCRM.Api

# List all migrations
dotnet ef migrations list \
  --project src/CLHCRM.Infrastructure \
  --startup-project src/CLHCRM.Api

# Generate SQL script from migrations
dotnet ef migrations script \
  --project src/CLHCRM.Infrastructure \
  --startup-project src/CLHCRM.Api \
  --output migrations.sql

# Drop database (WARNING: Deletes all data!)
dotnet ef database drop \
  --project src/CLHCRM.Infrastructure \
  --startup-project src/CLHCRM.Api
```

## Verify Database Connection

### Method 1: Run the API

```bash
dotnet run --project src/CLHCRM.Api
```

Check the console output for:
- No connection errors
- EF Core logging showing successful connection

### Method 2: Use a Database Client

#### pgAdmin (GUI)
1. Install pgAdmin: https://www.pgadmin.org/download/
2. Connect with:
   - Host: localhost
   - Port: 5432
   - Database: clhcrm_dev
   - Username: postgres
   - Password: postgres

#### psql (Command Line)
```bash
psql -U postgres -h localhost -d clhcrm_dev
```

#### DBeaver (Cross-platform GUI)
Download: https://dbeaver.io/download/

## Connection String Parameters Explained

```
Host=localhost              # Database server address
Port=5432                   # PostgreSQL default port
Database=clhcrm_dev         # Database name
Username=postgres           # Database user
Password=postgres           # User password
Include Error Detail=true   # Show detailed errors (dev only)
```

### Additional Parameters (Optional)

```
Pooling=true                          # Enable connection pooling (default: true)
Minimum Pool Size=0                   # Min connections in pool
Maximum Pool Size=100                 # Max connections in pool
Connection Lifetime=0                 # Max connection lifetime in seconds
Timeout=15                            # Connection timeout in seconds
Command Timeout=30                    # Command execution timeout
SSL Mode=Prefer                       # SSL connection mode (Disable|Allow|Prefer|Require)
Trust Server Certificate=true         # Trust SSL cert without validation
```

Example production connection string:
```
Host=db.example.com;Port=5432;Database=clhcrm;Username=clhcrm_user;Password=SecureP@ssw0rd;SSL Mode=Require;Pooling=true;Maximum Pool Size=50
```

## Security Best Practices

### Development
- Use simple passwords like `postgres` for local development
- Use `appsettings.Development.json` for dev-specific settings
- Never commit `appsettings.Local.json` to git

### Production
- **Never** store passwords in `appsettings.json`
- Use environment variables:
  ```bash
  export ConnectionStrings__DefaultConnection="Host=..."
  ```
- Use Azure Key Vault, AWS Secrets Manager, or similar
- Use connection string encryption
- Enable SSL/TLS for database connections
- Use separate database users with minimal permissions

### Using Environment Variables

```bash
# Windows PowerShell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=clhcrm_dev;Username=postgres;Password=postgres"

# Windows CMD
set ConnectionStrings__DefaultConnection=Host=localhost;Port=5432;Database=clhcrm_dev;Username=postgres;Password=postgres

# Linux/macOS
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=clhcrm_dev;Username=postgres;Password=postgres"
```

## Troubleshooting

### Cannot connect to database

**Error:** `Npgsql.NpgsqlException: Connection refused`

**Solutions:**
1. Verify PostgreSQL is running:
   ```bash
   # Docker
   docker ps | grep postgres

   # Windows Service
   Get-Service postgresql*

   # Linux
   systemctl status postgresql
   ```

2. Check port 5432 is accessible:
   ```bash
   netstat -an | grep 5432
   ```

3. Verify connection string is correct

### Authentication failed

**Error:** `password authentication failed for user "postgres"`

**Solutions:**
1. Verify password in connection string
2. Reset PostgreSQL password:
   ```bash
   # Docker
   docker exec -it clhcrm-postgres psql -U postgres
   ALTER USER postgres WITH PASSWORD 'newpassword';
   ```

### Database does not exist

**Error:** `database "clhcrm_dev" does not exist`

**Solutions:**
1. Create database manually (see above)
2. Or run migrations which will create it:
   ```bash
   dotnet ef database update --project src/CLHCRM.Infrastructure --startup-project src/CLHCRM.Api
   ```

### Migration pending

**Error:** `Pending model changes`

**Solutions:**
1. Create a new migration:
   ```bash
   dotnet ef migrations add DescribeYourChanges \
     --project src/CLHCRM.Infrastructure \
     --startup-project src/CLHCRM.Api
   ```

2. Apply migrations:
   ```bash
   dotnet ef database update \
     --project src/CLHCRM.Infrastructure \
     --startup-project src/CLHCRM.Api
   ```

## Next Steps

Once the database is set up:

1. ✅ PostgreSQL installed and running
2. ✅ Database created
3. ✅ Connection string configured
4. 🔄 Create your first entities in `src/CLHCRM.Domain/Entities`
5. 🔄 Create entity configurations
6. 🔄 Generate migrations
7. 🔄 Apply migrations
8. 🔄 Run the application

## Quick Start Checklist

```bash
# 1. Start PostgreSQL (Docker)
docker run --name clhcrm-postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=clhcrm_dev -p 5432:5432 -d postgres:16

# 2. Verify connection string in appsettings.Development.json

# 3. Build project
dotnet build

# 4. When ready, create migration (after creating entities)
dotnet ef migrations add InitialCreate --project src/CLHCRM.Infrastructure --startup-project src/CLHCRM.Api

# 5. Apply migration
dotnet ef database update --project src/CLHCRM.Infrastructure --startup-project src/CLHCRM.Api

# 6. Run application
dotnet run --project src/CLHCRM.Api

# 7. Access Swagger UI
# Open browser: https://localhost:7000/swagger
```

## References

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
- [Docker PostgreSQL Image](https://hub.docker.com/_/postgres)
