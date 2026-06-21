# E-Commerce Microservices Platform

A production-grade, cloud-native e-commerce backend demonstrating modern distributed systems architecture. Built with .NET 10, this platform orchestrates Catalog, Inventory, and Orders services through event-driven communication, ensuring loose coupling and independent scalability. Each microservice follows Clean Architecture with CQRS, maintains its own PostgreSQL database, leverages Redis for high-performance caching, and communicates asynchronously via RabbitMQ - all containerized with Docker for consistent development and deployment across environments.

## Technology Stack

- **.NET 10** - Latest .NET framework
- **PostgreSQL** - Primary database (per service)
- **Redis** - Distributed caching
- **RabbitMQ** - Message broker for async communication
- **EasyNetQ v8** - RabbitMQ client library
- **Entity Framework Core** - ORM
- **MediatR** - CQRS implementation
- **Docker** - Containerization
- **Docker Compose** - Local development orchestration
- **Serilog + Seq** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **Polly** - Resilience patterns (retry, circuit breaker)

## Project Structure

```
ECommerceMicroservices/
├── EventContracts/                    # Shared event contracts
│   └── Events/
│       ├── ProductCreatedEvent.cs
│       ├── ProductUpdatedEvent.cs
│       ├── ProductDeletedEvent.cs
│       ├── InventoryReservedEvent.cs
│       ├── InventoryReleasedEvent.cs
│       ├── InventoryAdjustedEvent.cs
│       ├── OrderCreatedEvent.cs
│       └── OrderCancelledEvent.cs
│
├── CatalogService/                    # Product catalog management
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── Product.cs
│   │   │   └── Category.cs
│   ├── Application/
│   │   ├── Products/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   └── Categories/
│   ├── Infrastructure/
│   │   ├── Persistence/
│   │   ├── Caching/
│   │   └── Messaging/
│   └── Api/Controllers/
│
├── InventoryService/                  # Stock management
│   ├── Domain/
│   │   └── Entities/
│   │       └── InventoryItem.cs
│   ├── Application/
│   │   ├── Commands/
│   │   │   ├── ReserveInventory/
│   │   │   ├── ReleaseInventory/
│   │   │   └── AdjustInventory/
│   │   ├── Queries/
│   │   └── Consumers/
│   ├── Infrastructure/
│   └── Api/Controllers/
│
├── OrdersService/                     # Order processing
│   ├── Domain/
│   │   ├── Entities/
│   │   │   ├── Order.cs
│   │   │   └── OrderItem.cs
│   │   └── Enums/
│   │       └── OrderStatus.cs
│   ├── Application/
│   │   ├── Orders/
│   │   │   ├── Commands/
│   │   │   └── Queries/
│   │   └── Consumers/
│   ├── Infrastructure/
│   └── Api/Controllers/
│
├── docker-compose.yml                 # Local development setup
└── README.md
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [PostgreSQL](https://www.postgresql.org/download/) (optional, for local development without Docker)

## Quick Start

### 1. Clone the Repository

```bash
git clone <repository-url>
cd ecom-microservices-v2
```

### 2. Start Infrastructure with Docker

```bash
docker-compose up -d
```

This will start:
- PostgreSQL instances (3 databases)
- Redis cache
- RabbitMQ message broker
- Seq log server
- All microservices

### 3. Access the Services

| Service | API (Swagger) | Port |
|---------|--------------|------|
| Catalog Service | http://localhost:5000/swagger | 5000 |
| Inventory Service | http://localhost:5002/swagger | 5002 |
| Orders Service | http://localhost:5004/swagger | 5004 |

### 4. Infrastructure Tools

| Tool | URL | Credentials |
|------|-----|-------------|
| RabbitMQ Management | http://localhost:15672 | guest/guest |
| Seq Log Viewer | http://localhost:5341 | - |

## Running Services Individually

### Catalog Service

```bash
cd CatalogService
dotnet ef migrations add InitialCreate
dotnet run
```

### Inventory Service

```bash
cd InventoryService
dotnet ef migrations add InitialCreate
dotnet run
```

### Orders Service

```bash
cd OrdersService
dotnet ef migrations add InitialCreate
dotnet run
```

## API Endpoints

### Catalog Service

#### Categories

```
POST   /api/categories          - Create a category
PUT    /api/categories/{id}     - Update a category
DELETE /api/categories/{id}     - Delete a category
GET    /api/categories/{id}     - Get category by ID
GET    /api/categories          - Get all categories
```

#### Products

```
POST   /api/products            - Create a product
PUT    /api/products/{id}       - Update a product
DELETE /api/products/{id}       - Delete a product
GET    /api/products/{id}       - Get product by ID
GET    /api/products            - Get all products (with filtering)
```

### Inventory Service

#### Inventory

```
POST   /api/inventory/reserve   - Reserve inventory
POST   /api/inventory/release   - Release inventory
POST   /api/inventory/adjust    - Adjust inventory
GET    /api/inventory/{productId} - Get inventory by product ID
```

### Orders Service

#### Orders

```
POST   /api/orders                    - Create an order
POST   /api/orders/{orderId}/cancel   - Cancel an order
GET    /api/orders/{orderId}          - Get order by ID
GET    /api/orders                    - Get all orders (with filtering)
```

## Event Flow

### Order Creation Workflow

1. **Create Order** → Orders Service saves order with `Pending` status
2. **OrderCreated Event** → Published to RabbitMQ
3. **Inventory Service** → Subscribes to OrderCreated event (future implementation)
4. **Order Status Update** → Status changes based on events

### Product Creation Workflow

1. **Create Product** → Catalog Service saves product to database
2. **ProductCreated Event** → Published to RabbitMQ
3. **Inventory Service** → Consumes event, creates inventory record
4. **Order Service** → Can reference product for new orders

## Configuration

### Connection Strings

Each service has its own `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "CatalogDb": "Host=localhost;Port=5432;Database=CatalogDb;Username=postgres;Password=postgres",
    "InventoryDb": "Host=localhost;Port=5433;Database=InventoryDb;Username=postgres;Password=postgres",
    "OrdersDb": "Host=localhost;Port=5434;Database=OrdersDb;Username=postgres;Password=postgres",
    "Redis": "localhost:6379",
    "RabbitMQ": "host=localhost;username=guest;password=guest"
  }
}
```

### Logging

All services use Serilog with Seq. Logs are structured and include:
- Request/Response logging
- Database command logging
- Event publishing/consumption
- Error tracking with full stack traces
- Performance metrics

## Development Guidelines

### Architecture Rules

1. **No Direct Service-to-Service Calls** - All communication via RabbitMQ
2. **Database per Service** - Each service owns its database
3. **CQRS Pattern** - Commands and Queries are separated
4. **Domain-Driven Design** - Business logic in domain entities
5. **Async First** - All operations are asynchronous

### Coding Standards

- Use constructor injection for dependencies
- One class per file
- Meaningful names that reflect business domain
- Rich domain models with encapsulated business logic
- Immutable DTOs using record types
- Comprehensive error handling
- Structured logging for all operations

### Adding a New Microservice

1. Create new .NET 10 Web API project
2. Add NuGet packages (matching existing services)
3. Implement Clean Architecture structure
4. Create Domain entities and events
5. Add CQRS commands and queries
6. Implement Redis caching
7. Configure RabbitMQ publisher/subscriber
8. Add Docker support
9. Update root docker-compose.yml
10. Add event contracts to EventContracts project

## Testing

```bash
# Run all tests
dotnet test

# Run specific service tests
dotnet test CatalogService.Tests/
dotnet test InventoryService.Tests/
dotnet test OrdersService.Tests/
```

## Monitoring & Observability

- **Health Checks**: Each service exposes `/health` endpoint
- **Centralized Logging**: All logs flow to Seq
- **Metrics**: Application metrics via health checks
- **Tracing**: Request tracking via correlation IDs
- **Dashboard**: RabbitMQ Management UI for message monitoring

## Performance Considerations

- **Redis Caching**: 15-minute TTL for product and inventory data
- **Database Connection Pooling**: Configured per service
- **Async Messaging**: Non-blocking event-driven communication
- **Circuit Breakers**: Prevent cascading failures
- **Retry Policies**: Exponential backoff for transient failures
- **Prefetch Count**: Optimized RabbitMQ consumer settings

## Security

- Services run as non-root users in Docker
- Database credentials via environment variables
- Network isolation via Docker networks
- Input validation at API and domain level
- No sensitive data in logs

## Troubleshooting

### Common Issues

1. **Docker not running**
   ```bash
   docker ps  # Check if Docker is running
   ```

2. **Database connection failed**
   ```bash
   docker-compose logs catalog-db
   docker-compose logs inventory-db
   docker-compose logs orders-db
   ```

3. **RabbitMQ connection issues**
   ```bash
   # Check RabbitMQ is healthy
   curl http://localhost:15672/api/health/checks/alarms
   ```

4. **Migrations not applied**
   ```bash
   dotnet ef database update --project CatalogService
   ```

### Viewing Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f catalog-service

# Last 100 lines
docker-compose logs --tail=100 inventory-service
```

## Contributing

1. Follow the established architecture patterns
2. Maintain consistent coding standards
3. Add event contracts to the shared library
4. Update documentation for new features
5. Ensure all tests pass before submitting PRs

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For issues and questions:
- Check the Seq logs for error details
- Review RabbitMQ queues for message issues
- Verify database migrations are applied
- Check Docker container health status

---

**Built with .NET 10, following Microservices Best Practices**
