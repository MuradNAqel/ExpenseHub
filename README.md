# ExpenseHub

ExpenseHub is a demo expense-claim platform built with a Vue frontend, .NET backend microservices, SQL Server, RabbitMQ, and Docker Compose.

The app lets an employee create expense claims with multiple expense items, lets a manager review, approve, reject, search, filter, paginate, and inspect claim details, and shows a manager dashboard with real aggregate data from the database.

## Demo URLs

When running with Docker Compose:

| App | URL |
| --- | --- |
| Frontend | `http://localhost:8088` |
| Employee expenses | `http://localhost:8088/Employee/expenses` |
| Manager expenses | `http://localhost:8088/manager/expenses` |
| Manager dashboard | `http://localhost:8088/manager/dashboard` |
| Expenses API | `http://localhost:8082` |
| Notifications API | `http://localhost:8080` |
| RabbitMQ management | `http://localhost:15672` |
| SQL Server | `localhost,1433` |

RabbitMQ login:

```text
username: guest
password: guest
```

SQL Server login:

```text
server: localhost,1433
database: ExpenseHubDb
user: sa
password: ExpenseHub!2026
trust server certificate: true
```

## Architecture

```text
Vue frontend
    |
    | HTTP /api
    v
Expenses.Api  ---- publishes ExpenseClaimCreatedEvent ----> RabbitMQ
    |                                                   |
    | SQL Server / Dapper                              v
    v                                             Notifications.Api
ExpenseHubDb                                          |
                                                       | stores notification records
                                                       v
                                                   ExpenseHubDb
```

The solution uses a small microservice structure:

```text
src/
  BuildingBlocks/
    EventBus/
      EventBus/
      EventBus.RabbitMQ/
    NotificationCore/
      NotificationCore/
      NotificationCore.Telegram/
    WebApi/
      WebApi/
  Services/
    Expenses/
      Expenses.Api/
    Notifications/
      Notifications.Api/
  FrontEnd/
```

### Backend Microservices

`Expenses.Api`

- Owns expense claims and expense items.
- Creates claims.
- Lists claims with pagination, status filters, date filters, and search.
- Gets claim details.
- Approves and rejects claims.
- Provides dashboard aggregate data.
- Publishes `ExpenseClaimCreatedEvent` to RabbitMQ after creating a claim.

`Notifications.Api`

- Consumes `ExpenseClaimCreatedEvent` from RabbitMQ.
- Creates notification records.
- Contains Telegram notification integration building block support.

### Building Blocks

`EventBus`

- Shared event abstractions.
- Contains `IEventBus`, `IIntegrationEventHandler`, and integration events.

`EventBus.RabbitMQ`

- RabbitMQ implementation for publishing and consuming integration events.

`WebApi`

- Shared ASP.NET Core API concerns.
- Contains the global exception handler and extension methods used by the APIs.

`NotificationCore`

- Notification abstractions.

`NotificationCore.Telegram`

- Telegram notification implementation support.

## Backend Design

The backend follows a pragmatic CQRS-style structure.

Commands change state:

- `CreateExpenseClaimCommand`
- `ApproveExpenseClaimCommand`
- `RejectExpenseClaimCommand`
- `CreateNotificationCommand`

Queries read data:

- `GetAllExpenseClaimsQuery`
- `GetExpenseClaimByIdQuery`
- `GetExpenseDashboardQuery`
- `GetAllNotificationMessagesQuery`
- `GetNotificationMessageByIdQuery`

Handlers are registered through small interfaces:

```csharp
ICommandHandler<TCommand, TResult>
IQueryHandler<TQuery, TResult>
```

Controllers stay thin. They receive HTTP requests, create or bind commands/queries, call the service layer, and return HTTP responses.

Dapper is used instead of Entity Framework. SQL queries are explicit and close to the use case, which makes the demo easy to explain:

- Pagination uses `OFFSET` / `FETCH`.
- Dashboard data uses SQL aggregate queries.
- Claims and items are read from SQL Server through typed DTO/data-row mapping.

## Frontend Design

The frontend is a Vue 3 + Vite app.

Important folders:

```text
src/FrontEnd/src/
  app/
    api/httpClient.ts
    layouts/
    router/
  core/
    api/expensesApi.ts
  features/
    dashboard/
    expenses/
    managers/
  shared/
    components/
```

Key decisions:

- `app/api/httpClient.ts` owns the shared Axios client.
- `core/api/expensesApi.ts` owns shared Expenses API functions and DTO types.
- `features/*` owns feature pages and feature-specific components.
- `shared/components/ExpenseClaimDetailsDialog.vue` is reused by employee and manager pages.

Frontend features:

- Employee claim creation dialog.
- Expense item entry with category, amount, date, and description.
- Employee claim table with pagination.
- Manager claim table with pagination, search, status filters, approval, rejection, and details dialog.
- Manager dashboard with summary cards, status chart, monthly chart, category totals, top employees, and recent claims.

## Technologies and Libraries

### Backend

- .NET `net10.0`
- ASP.NET Core Web API
- Dapper `2.1.79`
- Microsoft.Data.SqlClient `7.0.1`
- SQL Server 2022
- RabbitMQ
- RabbitMQ.Client `7.2.1`
- Scalar.AspNetCore `2.16.4`
- Microsoft.AspNetCore.OpenApi
- Docker
- Docker Compose
- nginx for serving the built frontend

### Frontend

- Vue `3.5`
- Vite `8`
- TypeScript
- Vue Router
- Pinia
- Axios
- PrimeVue
- PrimeIcons
- Tailwind CSS
- VeeValidate
- Zod
- Day.js
- Vue TSC
- ESLint / Oxlint / Prettier

## Run With Docker

Recommended for the demo.

From the repository root:

```bash
docker compose up -d --build
```

Open:

```text
http://localhost:8088
```

Check running containers:

```bash
docker compose ps
```

View logs:

```bash
docker compose logs -f expenses.api
docker compose logs -f notifications.api
docker compose logs -f frontend
docker compose logs -f rabbitmq
docker compose logs -f sqlserver
```

Stop the stack:

```bash
docker compose down
```

Stop and remove the SQL Server volume:

```bash
docker compose down -v
```

Use `-v` only when you want to delete the local demo database data.

## Run Locally Without Docker Frontend

You can run infrastructure and APIs in Docker, then run the frontend with Vite.

Start backend infrastructure and APIs:

```bash
docker compose up -d rabbitmq sqlserver expenses.api notifications.api
```

Run the frontend:

```bash
cd src/FrontEnd
npm install
npm run dev
```

Open:

```text
http://127.0.0.1:5173
```

The Vite dev server proxies `/api` to:

```text
http://localhost:8082
```

This means the simplest local frontend setup is to keep `expenses.api` running through Docker on port `8082`. If you run `Expenses.Api` directly with `dotnet run`, either update `src/FrontEnd/vite.config.ts` to proxy `/api` to `http://localhost:5069`, or set `VITE_API_BASE_URL` for the frontend.

## Run Backend Locally

Prerequisites:

- .NET SDK compatible with `net10.0`
- SQL Server available
- RabbitMQ available

Build the solution:

```bash
dotnet build ExpenseHub.sln
```

Run Expenses API:

```bash
dotnet run --project src/Services/Expenses/Expenses.Api/Expenses.Api.csproj
```

Default local URL:

```text
http://localhost:5069
```

Run Notifications API:

```bash
dotnet run --project src/Services/Notifications/Notifications.Api/Notifications.Api.csproj
```

Default local URL:

```text
http://localhost:5251
```

If running APIs locally against Docker infrastructure, use connection strings equivalent to:

```text
Server=localhost,1433;Database=ExpenseHubDb;User Id=sa;Password=ExpenseHub!2026;TrustServerCertificate=True;Encrypt=False;
```

RabbitMQ host from the local machine:

```text
localhost
```

RabbitMQ host from inside Docker Compose:

```text
rabbitmq
```

## Frontend Commands

From `src/FrontEnd`:

```bash
npm install
npm run dev
npm run build
npm run build-only
npm run type-check
npm run lint
npm run format
```

`npm run build` runs both type-check and production build.

## API Reference

In development, the APIs expose OpenAPI and Scalar.

Docker URLs:

```text
Expenses API Scalar:      http://localhost:8082/scalar/v1
Expenses API OpenAPI:     http://localhost:8082/openapi/v1.json
Notifications Scalar:     http://localhost:8080/scalar/v1
Notifications OpenAPI:    http://localhost:8080/openapi/v1.json
```

Useful Expenses endpoints:

```http
GET    /api/expenses
GET    /api/expenses?page=1&pageSize=10
GET    /api/expenses?status=Pending&page=1&pageSize=10
GET    /api/expenses?search=travel&page=1&pageSize=10
GET    /api/expenses/dashboard
GET    /api/expenses/{id}
POST   /api/expenses
POST   /api/expenses/{id}/approve
POST   /api/expenses/{id}/reject
```

Create claim example:

```json
{
  "employeeId": 1001,
  "title": "Amman client visit",
  "items": [
    {
      "category": "Travel",
      "amount": 35.5,
      "description": "Taxi to client office",
      "expenseDate": "2026-06-21"
    },
    {
      "category": "Meals",
      "amount": 12.75,
      "description": "Lunch during client visit",
      "expenseDate": "2026-06-21"
    }
  ]
}
```

Reject claim example:

```json
{
  "reason": "Receipt is missing"
}
```

Useful Notifications endpoints:

```http
GET /api/notifications
GET /api/notifications/{id}
```

## Database

The APIs initialize required database objects on startup.

Main tables:

```text
ExpenseClaims
ExpenseItems
NotificationMessages
```

Connect with SQL Server Management Studio or Azure Data Studio:

```text
Server: localhost,1433
Authentication: SQL Server Authentication
User: sa
Password: ExpenseHub!2026
Trust server certificate: true
Database: ExpenseHubDb
```

Useful demo queries:

```sql
USE ExpenseHubDb;

SELECT TOP 50 *
FROM ExpenseClaims
ORDER BY CreatedAt DESC;

SELECT TOP 50 *
FROM ExpenseItems
ORDER BY Id DESC;

SELECT TOP 50 *
FROM NotificationMessages
ORDER BY CreatedAt DESC;
```

## Demo Flow

1. Start the full stack:

   ```bash
   docker compose up -d --build
   ```

2. Open the frontend:

   ```text
   http://localhost:8088
   ```

3. Go to Employee Expenses:

   ```text
   http://localhost:8088/Employee/expenses
   ```

4. Create a claim with multiple items.

5. Open Manager Expenses:

   ```text
   http://localhost:8088/manager/expenses
   ```

6. Show:

   - pagination
   - status filter
   - search
   - claim details dialog
   - approve action
   - reject action with rejection reason

7. Open the rejected claim details and show that the rejection reason is visible.

8. Open Manager Dashboard:

   ```text
   http://localhost:8088/manager/dashboard
   ```

9. Show real dashboard data:

   - total claims
   - requested amount
   - pending amount
   - approved amount
   - average claim
   - status chart
   - monthly requested amount
   - spend by category
   - top employees
   - recent claims

10. Open RabbitMQ management:

    ```text
    http://localhost:15672
    ```

11. Explain that creating an expense claim publishes an integration event consumed by the notifications service.

12. Open SQL Server and show rows in:

    ```text
    ExpenseClaims
    ExpenseItems
    NotificationMessages
    ```

## Troubleshooting

Rebuild only the frontend:

```bash
docker compose up -d --build frontend
```

Rebuild only the Expenses API:

```bash
docker compose up -d --build expenses.api
```

Check API through the Dockerized frontend nginx proxy:

```bash
curl "http://localhost:8088/api/expenses?page=1&pageSize=1"
```

Check Expenses API directly:

```bash
curl "http://localhost:8082/api/expenses?page=1&pageSize=1"
```

If SQL Server is still starting, wait and check logs:

```bash
docker compose logs -f sqlserver
```

If the frontend loads but API calls fail, verify these containers are running:

```bash
docker compose ps frontend expenses.api sqlserver rabbitmq
```

If you need a clean database:

```bash
docker compose down -v
docker compose up -d --build
```

## Current Demo Notes

- Employee names are currently represented by a frontend static employee list.
- Currency formatting is set to Jordanian dinar using `JOD`.
- The frontend production container is served by nginx on port `8088`.
- The frontend uses relative `/api` requests. In Docker, nginx proxies those requests to `expenses.api:8080`.
- In local Vite dev mode, `/api` is proxied to `http://localhost:8082`.
