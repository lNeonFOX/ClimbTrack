# Architecture

## Layers

```mermaid
flowchart TD
    UI["Presentation: Razor pages + reusable components<br/>(RouteCard, StatCard, GradeBadge, ClimbForm, RouteForm, Pager, GradeChart)"]
    APP["Application: RouteService, ClimbService, StatisticsService, RecommendationService, CurrentUserService"]
    PURE["Pure logic: RecommendationEngine, StatisticsCalculator"]
    DATA["Persistence: AppDbContext (EF Core) + ASP.NET Core Identity"]
    DB[(SQLite)]
    UI --> APP
    APP --> PURE
    APP --> DATA
    DATA --> DB
```

- Pattern: layered services with dependency injection (MVVM-like separation of UI, services and models).
- One short-lived `DbContext` per service call, created with `IDbContextFactory` (the recommended pattern for Blazor Server, avoids long-lived contexts per circuit).
- Whole app is interactive (Blazor Server, SignalR circuit) except `/Account/*`, which is static SSR because it sets auth cookies.

## State management

| State | Where | Why |
|---|---|---|
| Catalog filters | `RouteFilterState` (scoped, event-based) | Shared between components; survives navigation inside the circuit |
| Auth state | `CascadingAuthenticationState` | Standard Blazor mechanism |
| Form state | `EditForm` + `ClimbInput` / `RouteInput` | Local, validated with DataAnnotations |
| Durable data | SQLite | Never only in component state |

## ERD

```mermaid
erDiagram
    ApplicationUser ||--o{ ClimbRecord : logs
    ClimbingRoute ||--o{ ClimbRecord : "is climbed in"
    ApplicationUser {
        string Id PK
        string UserName
        string Email
        int PreferredGrade
    }
    ClimbingRoute {
        int Id PK
        string Name "unique"
        int Grade "0-10"
        string WallType
        string Description
        string ImageUrl
    }
    ClimbRecord {
        int Id PK
        string UserId FK
        int RouteId FK
        date Date
        int Attempts "1-100"
        bool IsCompleted
        string Notes
    }
```

Indexes: `ClimbingRoute(Name)` unique, `ClimbingRoute(Grade, WallType)`, `ClimbRecord(UserId, Date)`, `ClimbRecord(UserId, RouteId)`.
Deleting a user cascades to their records; deleting a route with records is blocked.

## Scalability notes

Queries use `AsNoTracking`, projections, server-side paging and indexes. The database provider is swappable (replace `UseSqlite` with PostgreSQL/SQL Server) without touching services. Statistics are computed from records, so no counters can drift.
