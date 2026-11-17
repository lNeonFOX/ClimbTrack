# Coverage of the 20 assessment criteria

| # | Criterion | Evidence in this repository |
|---|---|---|
| 1 | Code structure and design | Layered folders (Components, Services, Data, Models, State); pure logic isolated; DI everywhere |
| 2 | Component reusability | `RouteCard`, `GradeBadge`, `StatCard`, `Pager`, `GradeChart`, `ClimbForm`, `RouteForm` with `[Parameter]`s and `EventCallback`s |
| 3 | Cross-platform | .NET 10 runs on Windows/macOS/Linux; browser UI for desktop and mobile; CI on Linux |
| 4 | State management | `RouteFilterState` (scoped, event-based), cascading auth state, DB for durable data |
| 5 | UI design | Bootstrap 5 + custom theme, grade colour tiers, consistent cards |
| 6 | UX | Preselected route from "Log climb", delete confirmation, loading/empty/error states, skip link, labels, aria attributes |
| 7 | Responsiveness | Bootstrap grid, CSS media queries, CSS-only collapsing menu, scrollable tables |
| 8 | Core functionality | Auth, catalog + filters, journal CRUD, dashboard, recommendations |
| 9 | Advanced features | Role-based admin, JSON API, health check, lockout, antiforgery |
| 10 | Error handling | Service-level validation, `ServiceResult`, try/catch with logging, `ErrorBoundary`, error and not-found pages |
| 11 | Efficiency | Async EF Core, `AsNoTracking`, projections, `ExecuteDeleteAsync`, server-side paging |
| 12 | Scalability | Provider-agnostic EF Core, indexes, stateless services, page-size cap |
| 13 | Resource utilization | Short-lived contexts, no redundant counters, single query for statistics |
| 14 | Testing coverage | Unit, service, bUnit and integration tests; coverage measured with coverlet (report the real number) |
| 15 | Quality assurance | `docs/QA_CHECKLIST.md` (fill in per browser/device) |
| 16 | Documentation | README, ARCHITECTURE (diagrams, ERD), QA checklist, presentation notes, API table |
| 17 | Innovation | Personalised, deterministic recommendation engine; grade-distribution chart without JS libraries |
| 18 | Creative .NET use | LINQ, primary constructors, `TimeProvider`, `IDbContextFactory`, `EF.Functions.Like`, `ExecuteDelete`, static SSR + interactive mix |
| 19 | Professionalism | CI workflow, `.gitignore`, consistent naming, security notes |
| 20 | Presentation | done |
