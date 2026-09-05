# Amori — Architecture Overview

Amori is a private couples application built as a monorepo. One React Native codebase targets both iOS and Android. The backend is a single ASP.NET Core modular monolith backed by PostgreSQL and hosted on AWS.

---

## Top-Level Directories

| Directory   | Responsibility |
|-------------|----------------|
| `ios/`      | Native iOS project. Managed by React Native CLI. Do not edit directly unless writing native modules. |
| `android/`  | Native Android project. Managed by React Native CLI. Do not edit directly unless writing native modules. |
| `src/`      | All React Native / TypeScript application code. |
| `backend/`  | ASP.NET Core Web API. Single deployable API project. |
| `shared/`   | API request/response contracts that are the source of truth between the mobile client and the backend. |
| `tests/`    | All test code, separated by platform (mobile / backend) and type (unit / integration). |
| `docs/`     | Architecture decisions, API documentation, database schema notes, and feature-level documentation. |

---

## Mobile (`src/`)

Feature-based structure. Each feature is self-contained.

| Path | Responsibility |
|------|----------------|
| `src/app/` | App bootstrap: navigation container, global providers, environment config. |
| `src/components/` | Reusable UI components shared across features (buttons, cards, inputs, modals, etc.). |
| `src/features/<name>/` | All code for a single product feature. Contains its own screens, components, hooks, services, types, and utils. |
| `src/services/api/` | HTTP client and all API call functions. |
| `src/services/auth/` | Token management and session handling. |
| `src/services/notifications/` | Push notification registration and handling. |
| `src/services/realtime/` | SignalR connection and event subscription. |
| `src/services/storage/` | AWS S3 upload/download wrappers for photos and voice notes. |
| `src/services/analytics/` | Analytics event tracking. |
| `src/stores/` | Global state management (e.g. Zustand or Redux Toolkit slices). |
| `src/hooks/` | Global/shared custom React hooks. |
| `src/types/` | Global TypeScript types and interfaces. |
| `src/utils/` | Pure utility/helper functions. |
| `src/constants/` | App-wide constants. |
| `src/theme/` | Design tokens: colours, typography, spacing, shadows. |
| `src/assets/` | Static assets: images, icons, sounds, fonts. |
| `src/navigation/` | Root navigator and route type definitions. |

---

## Backend (`backend/Amori.Api/`)

Modular monolith. Each feature has its own folder under `Features/`. No microservices.

| Path | Responsibility |
|------|----------------|
| `Controllers/` | Thin HTTP controllers. Delegate work to feature handlers. |
| `Features/<Name>/` | All logic for a single domain feature (commands, queries, handlers, validators). |
| `Data/` | EF Core DbContext, entity configurations, migrations, and seed data. |
| `Domain/` | Domain entities, enums, and value objects. No dependencies on infrastructure. |
| `Infrastructure/` | Implementations of storage (S3), push notifications, SignalR hubs, and authentication. |
| `Common/` | Cross-cutting concerns: exception types, middleware, standard response wrappers, extension methods. |
| `Configuration/` | Strongly-typed settings classes bound from `appsettings.json`. |

---

## Shared API Contracts (`shared/`)

Request and response DTOs that both the mobile client and the backend reference. These are **not** domain entities. They represent the public API surface.

| Path | Responsibility |
|------|----------------|
| `shared/api/requests/` | Request payloads sent from mobile to the API. |
| `shared/api/responses/` | Response shapes returned by the API. |
| `shared/types/` | Enums and primitive types shared across both sides. |

---

## Tests (`tests/`)

| Path | Responsibility |
|------|----------------|
| `tests/mobile/unit/` | Unit tests for React Native components, hooks, utilities, and stores. |
| `tests/mobile/integration/` | Integration tests for mobile service calls and navigation flows. |
| `tests/backend/unit/` | Unit tests for domain logic, feature handlers, and validators. |
| `tests/backend/integration/` | Integration tests against the real database and API endpoints. |

---

## Documentation (`docs/`)

| Path | Responsibility |
|------|----------------|
| `docs/architecture/` | High-level design decisions, ADRs, and this overview. |
| `docs/api/` | API endpoint documentation. |
| `docs/database/` | Database schema diagrams and migration notes. |
| `docs/features/` | Per-feature documentation: purpose, flows, and data shapes. |
