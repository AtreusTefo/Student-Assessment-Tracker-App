# AGENTS.md - Student Assessment Tracker Master Instructions

## Project Context
- **Name:** Student Assessment Tracker
- **Backend:** ASP.NET Core 8 Web API following Clean Architecture.
- **Frontend:** Angular 21 (Standalone, Zoneless) SPA.
- **Architecture:** Decoupled layers (Domain, Application, Infrastructure, Presentation).
- **Primary IDEs:** VS Code 2026, Visual Studio Community 2026.
- **Main Goal:** A three-role system (Admin, Teacher, Student) for managing academic performance, file submissions, and automated grading.

## AI Behavior Guidelines
- **No Emojis:** Do not use emojis in documentation, code comments.
- **For Claude:** Prioritize Clean Architecture principles. Place logic in the Application layer (Services/Use Cases), not Controllers or Domain entities.
- **For Gemini/GPT:** Be extremely concise. Avoid conversational filler.
- **General:** If logic is ambiguous, explicitly state the ambiguity and request clarification from the user in a concise format. Reference ARCHITECTURE.md before suggesting structural changes.

## Error Handling & Documentation-First Strategy
When encountering errors or bugs, always follow this process:

1. **Check Existing Documentation First:** Before attempting to fix any error, search the project documentation.
2. **Search Priority Folders:**
   - `docs/error-fixes/` - Contains documented error solutions and quick references.
   - `docs/daily-reports/` - Contains session logs and previously encountered issues.
   - `docs/implementation/` - Contains implementation notes and architectural decisions.
3. **Identify Existing Solutions:** Review the documentation to determine if the error has already been diagnosed and fixed.
4. **Apply Documented Solution:** If a solution exists in the documentation, apply it first. If the documented solution works, proceed to next error. If the documented solution does not work, find a new solution and proceed to step 5.
5. **Update or Create Documentation:** If the documented solution didn't work:
   - Implement the new solution.
   - Update the original documentation entry with the new solution, root cause, and implementation details.
   - Link references to relevant daily reports or implementation notes.
   If the error is not documented:
   - Solve the problem following architectural and coding standards.
   - Document the root cause, solution, and prevention steps in `docs/error-fixes/`.
   - Link the new documentation to relevant daily reports or implementation notes.
6. **Prevent Regressions:** By maintaining comprehensive error documentation, we avoid reintroduction of already-fixed bugs and reduce troubleshooting time for common issues.

This approach ensures efficiency, consistency, and knowledge preservation across development sessions.

## Documentation Standards
- **Style:** Professional, technical, and objective.
- **Format:** Use standard Markdown. No emojis allowed.

## Coding Standards & Patterns
- **Backend (.NET 8):**
  - Clean Architecture: Domain (Entities), Application (DTOs/Services/Validators), Infrastructure (EF Core/Repositories), Presentation (Controllers).
  - Validation: Use FluentValidation 12.1 for all request payloads.
  - Logging: Use Serilog for structured logging.
- **Frontend (Angular 21):**
  - Modern Angular: Use Standalone Components and Zoneless change detection.
  - Reactivity: Use RxJS Observables and HttpClient with withFetch().
  - UI: DataTables.net v2 for student lists; Responsive CSS for dashboards.
- **Naming:** PascalCase for C#; camelCase for TypeScript and JSON.

## Project Structure Reference
- **Backend API:** StudentAssessmentTrackerAPI/
- **Frontend App:** StudentApp/src/app/
- **Core Logic:** StudentAssessmentTrackerAPI/Application/
- **Database:** StudentAssessmentTrackerAPI/Infrastructure/Data/

## Environment Commands
### Backend
- Build: dotnet build
- Run: dotnet run --project StudentAssessmentTrackerAPI
- Migrations: dotnet ef migrations add <Name>
### Frontend
- Install: cd StudentApp && npm install
- Run: npm start
- Build: npm run build

## Critical Business Rules
1. Roles: Three distinct JWT roles: Admin, Teacher, Student.
2. Student IDs: Format is strictly STU-XXXXXXXX.
3. Performance Levels:
   - Needs Support: < 50%
   - Satisfactory: 50–55%
   - Good: 56–75%
   - Excellent: > 75%
4. File Uploads: Allowed formats (PDF, DOC, DOCX, JPG, PNG) | Max 10MB.
5. Security: Use BCrypt for password hashing; JWT for all authenticated requests.

## Data Integrity, Referential Integrity & Data Consistency Standards

### Backend Database & ORM (EF Core)
- **Foreign Key Constraints:** Define explicit foreign key relationships in DbContext with `OnDelete(DeleteBehavior.*)` policies. Use Restrict or Cascade based on business context.
- **Unique Constraints:** Enforce unique indexes on Student IDs (STU-XXXXXXXX format) and email addresses at the database schema level.
- **Transaction Management:** Wrap multi-step operations (grade calculations, bulk submissions, user role assignments) in database transactions to ensure ACID compliance.
- **Validation at Domain Level:** Domain entities must validate business rules before persistence (e.g., grade ranges, performance calculations).
- **Migration Safety:** Always create EF Core migrations for schema changes. Ensure backward compatibility and test migrations on staging before production deployment.

### Application Layer (Services & Use Cases)
- **Atomic Operations:** Ensure create/update/delete operations are atomic. If operation fails, rollback all related changes.
- **Concurrency Handling:** Implement optimistic concurrency control using EF Core shadow properties (RowVersion) for critical entities (Grades, Submissions, User Profiles).
- **Business Logic Validation:** Validate all incoming data against business rules in Application Services (not just Controllers). Prevent orphaned records and cascading failures.
- **Logging & Auditing:** Log all data modifications using Serilog. Include entity IDs, user context, operation type, and timestamps for traceability.

### Frontend Data Synchronization (Angular)
- **State Management:** Maintain a single source of truth for entities. Use RxJS Subjects/BehaviorSubjects to manage shared state across components.
- **Optimistic Updates:** For non-critical updates, update UI optimistically and sync with backend. Implement rollback on failure with user notification.
- **Request Ordering:** Enforce sequential execution of dependent API calls (e.g., create submission, then upload file). Use RxJS operators (switchMap, mergeMap) appropriately.
- **HTTP Interceptors:** Implement error handling interceptors that detect data consistency conflicts (409 Conflict) and trigger cache invalidation.
- **Local Caching:** Cache read-only reference data (roles, performance levels, file formats). Invalidate cache on backend modifications.

### Cross-Stack Consistency Guarantees
#### 1. Request Validation Symmetry
- Frontend validation (TypeScript validators, Angular Reactive Forms) must mirror backend validation (FluentValidation).
- Maintain parity in business rule checks across layers.

#### 2. Timestamp Synchronization
- Use UTC timestamps in backend storage and APIs.
- Convert to local time only in frontend presentation.
- Store all timestamps in ISO 8601 format.

#### 3. API Response Versioning
- Include versioning headers in API responses.
- Use these headers to detect compatibility issues early.

#### 4. Conflict Resolution
- **Last-Write-Wins:** Use this for grade updates within the same session.
- **Merge Strategy:** Use this for multi-user submissions (Teacher + Student), with field-level conflict detection.
- **User Notification:** Notify frontend users of concurrent modifications and provide options to reload or merge.

#### 5. Idempotency
- Design endpoints to be idempotent for repeated operations.
- Use idempotency keys or natural identifiers for file uploads and grade assignments.

### Testing & Validation
- **Integration Tests:** Test full workflows (create student → assign grade → verify calculations) to catch cross-layer inconsistencies.
- **Database Seeding:** Use consistent test data seeds across all test environments. Validate referential integrity in test setup.
- **Contract Testing:** Validate API response structure consistency (DTOs match database entities, calculated fields are accurate).
- **Scenario Testing:** Test edge cases (simultaneous submissions, role changes mid-operation, network failures during file uploads).