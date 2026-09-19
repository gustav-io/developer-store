# Developer Evaluation Project

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

<!-- 
## API Structure
This section includes links to the detailed documentation for the different API resources:
- [API General](./docs/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)

## Running the project

### Docker (API + PostgreSQL)

```bash
docker compose up --build
```

- Swagger: http://localhost:8080/swagger
- Development seeds an admin: `admin@developerstore.local` / `Admin@123` → `POST /api/auth` returns the JWT; click **Authorize** in Swagger or use `src/Ambev.DeveloperEvaluation.WebApi/Ambev.DeveloperEvaluation.WebApi.http`.
- Migrations run automatically at startup in Development.
- If port 5432 is taken locally: `POSTGRES_PORT=5434 docker compose up --build`.

### Local

```bash
docker compose up -d ambev.developerevaluation.database
dotnet run --project src/Ambev.DeveloperEvaluation.WebApi
```

### Tests

```bash
dotnet test tests/Ambev.DeveloperEvaluation.Unit          # no external dependencies
dotnet test tests/Ambev.DeveloperEvaluation.Integration   # PostgreSQL via Testcontainers (Docker required)
dotnet test tests/Ambev.DeveloperEvaluation.Functional    # HTTP end-to-end via WebApplicationFactory + Testcontainers
```

## Sales API

| Verb | Route | Description |
|---|---|---|
| POST | `/api/sales` | Create a sale (discount tiers applied per item) |
| GET | `/api/sales` | List with `_page`, `_size`, `_order`, filters (`saleNumber`, `customerName`, `branchName`, `status`, `_minDate`, `_maxDate`, `_minTotalAmount`, `_maxTotalAmount`; `*` for partial text) |
| GET | `/api/sales/{id}` | Get a sale |
| PUT | `/api/sales/{id}` | Replace header and items |
| DELETE | `/api/sales/{id}` | Remove a sale |
| PATCH | `/api/sales/{id}/cancel` | Cancel a sale |
| PATCH | `/api/sales/{id}/items/{itemId}/cancel` | Cancel one item |

Errors follow `.doc/general-api.md`: `{ "type", "error", "detail" }` with `ValidationError` (400), `BusinessRuleViolation` (400), `ResourceNotFound` (404), `AuthenticationError` (401).

## Architecture overview

- **Domain** — `Sale` is an aggregate root: items, discount tiers, the 20-unit cap and cancellation rules are enforced inside it. Customer, Branch and Product are *External Identities* (`CustomerRef`, `BranchRef`, `ProductRef`) with denormalized descriptions, persisted as owned types.
- **Application** — one folder per feature (`CreateSale`, `ListSales`, …) with command, validator, handler and result; validators run in the MediatR pipeline (`ValidationBehavior`).
- **Events** — the aggregate raises `SaleCreated/Modified/Cancelled` and `ItemCancelled`; `DefaultContext` publishes them after `SaveChanges`; a handler logs each one and forwards a flat integration contract to **Rebus** (in-memory transport here — Azure Service Bus, RabbitMQ, etc. are a one-line transport change). A sample consumer logs the message to show the round-trip.
- **Persistence** — EF Core + PostgreSQL; `SaleNumber` comes from a database sequence.
- **Tests** — unit (domain + handlers, no infrastructure), integration (repository on a real PostgreSQL container), functional (HTTP through the whole pipeline).
