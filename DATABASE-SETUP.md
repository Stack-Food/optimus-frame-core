# Database Setup Guide - OptimusFrame

This guide explains how to set up the PostgreSQL database for OptimusFrame Core API.

## Table of Contents
- [Quick Start (Docker Compose)](#quick-start-docker-compose)
- [Manual Setup](#manual-setup)
- [Entity Framework Migrations](#entity-framework-migrations)
- [Health Checks](#health-checks)
- [Troubleshooting](#troubleshooting)

---

## Quick Start (Docker Compose)

The easiest way to set up the database is using Docker Compose, which automatically initializes the database with the correct schema.

### Prerequisites
- Docker and Docker Compose installed
- Port 5432 available (or change in docker-compose.yml)

### Steps

1. **Start all services:**
   ```bash
   docker-compose up -d
   ```

2. **Verify database initialization:**
   ```bash
   docker logs optimus-frame-postgres
   ```

   You should see:
   ```
   OptimusFrame database initialized successfully
   ```

3. **Verify health:**
   ```bash
   curl http://localhost:8082/health
   ```

That's it! The database is automatically created and initialized with the schema.

---

## Manual Setup

If you prefer to set up the database manually without Docker:

### Prerequisites
- PostgreSQL 16+ installed
- .NET 8 SDK installed

### Option 1: Using the SQL Script

1. **Start PostgreSQL:**
   ```bash
   # Linux/Mac
   sudo systemctl start postgresql

   # Windows
   # Use pgAdmin or start PostgreSQL service
   ```

2. **Run the setup script:**
   ```bash
   psql -U postgres -f database-setup.sql
   ```

3. **Verify setup:**
   ```bash
   psql -U postgres -d optimusframe_db -c "\dt"
   ```

### Option 2: Using Entity Framework Migrations

1. **Update connection string** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=optimusframe_db;Username=postgres;Password=yourpassword"
     }
   }
   ```

2. **Apply migrations:**
   ```bash
   cd src/OptimusFrame.Core.API
   dotnet ef database update --project ../OptimusFrame.Core.Infrastructure
   ```

---

## Entity Framework Migrations

### View Migrations
```bash
dotnet ef migrations list --project src/OptimusFrame.Core.Infrastructure --startup-project src/OptimusFrame.Core.API
```

### Create New Migration
```bash
dotnet ef migrations add YourMigrationName --project src/OptimusFrame.Core.Infrastructure --startup-project src/OptimusFrame.Core.API
```

### Apply Migrations
```bash
dotnet ef database update --project src/OptimusFrame.Core.Infrastructure --startup-project src/OptimusFrame.Core.API
```

### Rollback Migration
```bash
dotnet ef database update PreviousMigrationName --project src/OptimusFrame.Core.Infrastructure --startup-project src/OptimusFrame.Core.API
```

### Remove Last Migration
```bash
dotnet ef migrations remove --project src/OptimusFrame.Core.Infrastructure --startup-project src/OptimusFrame.Core.API
```

---

## Health Checks

OptimusFrame includes comprehensive health check endpoints to monitor database and service health.

### Endpoints

#### 1. General Health Check
```bash
GET http://localhost:8082/health
```

**Response:**
```json
{
  "status": "Healthy",
  "totalDuration": 45.2,
  "checks": [
    {
      "name": "postgresql",
      "status": "Healthy",
      "description": null,
      "duration": 42.1,
      "exception": null,
      "data": {},
      "tags": ["db", "sql", "postgresql"]
    },
    {
      "name": "rabbitmq",
      "status": "Healthy",
      "description": null,
      "duration": 3.1,
      "exception": null,
      "data": {},
      "tags": ["messaging", "rabbitmq"]
    }
  ]
}
```

#### 2. Readiness Check
```bash
GET http://localhost:8082/health/ready
```

Checks if the application is ready to serve traffic (database and messaging are available).

#### 3. Liveness Check
```bash
GET http://localhost:8082/health/live
```

Checks if the application is alive (always returns healthy if app is running).

### Health Status Values
- `Healthy` - Service is functioning normally
- `Degraded` - Service is running but with issues
- `Unhealthy` - Service is not functioning

### Kubernetes Integration

Use health checks in your Kubernetes deployments:

```yaml
livenessProbe:
  httpGet:
    path: /health/live
    port: 8080
  initialDelaySeconds: 30
  periodSeconds: 10

readinessProbe:
  httpGet:
    path: /health/ready
    port: 8080
  initialDelaySeconds: 10
  periodSeconds: 5
```

---

## Database Schema

### Media Table

| Column | Type | Description |
|--------|------|-------------|
| MediaId | UUID | Primary key |
| UserName | VARCHAR(255) | User identifier |
| FileName | VARCHAR(500) | Original file name |
| Base64 | TEXT | Base64 encoded data (legacy) |
| UrlBucket | VARCHAR(1000) | S3 bucket URL |
| Status | INTEGER | Processing status (0-4) |
| OutputUri | VARCHAR(1000) | Output S3 URL (nullable) |
| ErrorMessage | TEXT | Error details (nullable) |
| CreatedAt | TIMESTAMP | Upload timestamp |
| CompletedAt | TIMESTAMP | Completion timestamp (nullable) |

### Media Status Enum

| Value | Status | Description |
|-------|--------|-------------|
| 0 | Process | Currently processing |
| 1 | Uploaded | Upload complete, waiting for processing |
| 2 | Error | Processing error (legacy) |
| 3 | Completed | Processing completed successfully |
| 4 | Failed | Processing failed |

### Indexes

- `IX_Media_UserName` - For user video lookups
- `IX_Media_Status` - For status filtering
- `IX_Media_CreatedAt` - For date range queries
- `IX_Media_UserName_Status` - Composite for user + status queries

---

## Troubleshooting

### Connection Refused

**Problem:** `Connection refused` when connecting to PostgreSQL

**Solution:**
1. Check if PostgreSQL is running:
   ```bash
   docker ps | grep postgres
   ```

2. Check PostgreSQL logs:
   ```bash
   docker logs optimus-frame-postgres
   ```

3. Verify port is not in use:
   ```bash
   # Windows
   netstat -ano | findstr :5432

   # Linux/Mac
   lsof -i :5432
   ```

### Authentication Failed

**Problem:** `password authentication failed for user "postgres"`

**Solution:**
1. Check credentials in `appsettings.json`
2. For Docker: Check `docker-compose.yml` environment variables
3. Reset PostgreSQL password:
   ```bash
   docker-compose down -v
   docker-compose up -d
   ```

### Table Does Not Exist

**Problem:** `relation "Media" does not exist`

**Solution:**
1. Apply migrations:
   ```bash
   dotnet ef database update --project src/OptimusFrame.Core.Infrastructure --startup-project src/OptimusFrame.Core.API
   ```

2. Or run SQL script:
   ```bash
   docker exec -i optimus-frame-postgres psql -U postgres -d optimusframe_db < database-setup.sql
   ```

### Migration Pending

**Problem:** `Pending migrations detected`

**Solution:**
The API automatically applies migrations on startup. If this fails:

1. Apply manually:
   ```bash
   dotnet ef database update --project src/OptimusFrame.Core.Infrastructure --startup-project src/OptimusFrame.Core.API
   ```

2. Check migration logs in API console output

### Health Check Failing

**Problem:** `/health` endpoint returns `Unhealthy`

**Solution:**
1. Check detailed response to see which service is unhealthy
2. For PostgreSQL:
   ```bash
   docker logs optimus-frame-postgres
   ```
3. For RabbitMQ:
   ```bash
   docker logs optimus-rabbitmq
   ```
4. Verify connection strings in `appsettings.json`

---

## Environment Variables

### PostgreSQL
- `POSTGRES_DB` - Database name (default: `optimusframe_db`)
- `POSTGRES_USER` - Database user (default: `postgres`)
- `POSTGRES_PASSWORD` - Database password (default: `postgres`)

### Connection String Format
```
Host=<hostname>;Port=<port>;Database=<database>;Username=<username>;Password=<password>
```

### Example
```bash
# Development
Host=localhost;Port=5432;Database=optimusframe_db;Username=postgres;Password=postgres

# Docker
Host=postgres;Port=5432;Database=optimusframe_db;Username=postgres;Password=postgres

# Production
Host=your-rds-endpoint.region.rds.amazonaws.com;Port=5432;Database=optimusframe_db;Username=admin;Password=securepassword
```

---

## Backup and Restore

### Backup Database

```bash
# Using Docker
docker exec optimus-frame-postgres pg_dump -U postgres optimusframe_db > backup.sql

# Manual
pg_dump -U postgres optimusframe_db > backup.sql
```

### Restore Database

```bash
# Using Docker
docker exec -i optimus-frame-postgres psql -U postgres optimusframe_db < backup.sql

# Manual
psql -U postgres optimusframe_db < backup.sql
```

---

## Production Considerations

### Security
- [ ] Use strong passwords
- [ ] Enable SSL/TLS connections
- [ ] Restrict network access
- [ ] Use connection pooling
- [ ] Enable audit logging

### Performance
- [ ] Configure appropriate connection pool size
- [ ] Enable query caching
- [ ] Set up read replicas for scaling
- [ ] Monitor query performance
- [ ] Optimize indexes based on query patterns

### High Availability
- [ ] Set up PostgreSQL replication
- [ ] Use managed database service (AWS RDS, Azure Database)
- [ ] Configure automated backups
- [ ] Implement disaster recovery plan
- [ ] Monitor database metrics

---

## Additional Resources

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)
- [Health Checks in ASP.NET Core](https://docs.microsoft.com/aspnet/core/host-and-deploy/health-checks)
- [Docker Compose Documentation](https://docs.docker.com/compose/)

---

## Support

For issues or questions:
- Open an issue on GitHub
- Check existing documentation in `/docs` folder
- Review health check endpoint responses for diagnostics
