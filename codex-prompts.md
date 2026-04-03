# codex-prompts.md

# Codex Prompts for Vinh Khanh Food Street Guide App
Use these prompts together with `plans.md`.

## How to use
For each slice:
1. paste one prompt into Codex
2. let Codex generate code
3. review changed files
4. run build/tests manually
5. only then move to the next prompt

## Global instruction to prepend when needed
Use this when Codex starts drifting:



---

# Slice 1 — Solution skeleton

## Prompt 1.1
```text
Create the initial mono-repo structure for this project:

- src/mobile/VinhKhanhGuide.Mobile (.NET MAUI app)
- src/backend/VinhKhanhGuide.Api (ASP.NET Core Web API)
- src/backend/VinhKhanhGuide.Application
- src/backend/VinhKhanhGuide.Domain
- src/backend/VinhKhanhGuide.Infrastructure
- tests/VinhKhanhGuide.Api.Tests
- tests/VinhKhanhGuide.Application.Tests

Requirements:
- keep the code minimal and compilable
- wire project references correctly
- do not add business logic yet
- add a root solution file if missing
- add a basic README section with build/run commands
- explain each created or modified file
- return a concise diff summary
```

## Prompt 1.2
```text
Set up dependency injection and startup wiring for the ASP.NET Core backend.

Requirements:
- keep Api/Application/Domain/Infrastructure separation clean
- add placeholder service registration methods in Application and Infrastructure
- add Swagger
- do not add entities or database logic yet
- do not modify mobile code
- explain each modified file and provide run commands
```

---

# Slice 2 — Database and stall entity

## Prompt 2.1
```text
Implement the first domain entity set for the backend:

Entities:
- Stall
- optional simple value objects only if really necessary

Requirements:
- add EF Core DbContext in Infrastructure
- configure PostgreSQL support
- add a first migration-ready setup
- include only fields needed for MVP stall browsing:
  Id, Name, DescriptionVi, Latitude, Longitude, TriggerRadiusMeters, Category, OpenHours, ImageUrl, AverageRating
- do not add reviews/auth yet
- keep naming clear and simple
- explain all modified files
```

## Prompt 2.2
```text
Add seed data for 5 to 10 mock Vinh Khanh food stalls.

Requirements:
- use realistic but clearly mock/sample data
- seed through EF Core in a maintainable way
- make sure the API can start with seeded data
- do not add admin/import features yet
- explain how to reset and reseed the database
```

---

# Slice 3 — Stall read APIs

## Prompt 3.1
```text
Implement read-only stall APIs.

Endpoints:
- GET /api/stalls
- GET /api/stalls/{id}

Requirements:
- use DTOs in Application layer
- keep controller thin
- add application service/use case interfaces as needed
- return proper 404 for missing stall
- do not add nearby query yet
- add API integration tests for these endpoints
- explain all modified files and how to run tests
```

## Prompt 3.2
```text
Implement GET /api/stalls/nearby?lat={x}&lng={y}&radius={r}

Requirements:
- calculate distance in backend using a simple and testable approach
- return stalls within radius sorted by nearest first
- add unit tests for the distance logic
- add integration test for the endpoint
- do not add mobile changes yet
- explain all modified files
```

---

# Slice 4 — Mobile app consumes stall data

## Prompt 4.1
```text
In the .NET MAUI mobile app, implement a basic stall list screen using MVVM.

Requirements:
- create models/DTOs as needed on mobile side
- create an API client service to call GET /api/stalls
- create a ViewModel and View for listing stalls
- display name, category, and average rating
- keep UI simple and clean
- no map yet
- do not add mock data if backend API is already available
- explain every modified file and how to run the mobile app
```

## Prompt 4.2
```text
Add navigation from the stall list screen to a stall detail screen.

Requirements:
- create a basic detail page
- pass selected stall id and load data from backend
- display at least name, Vietnamese description, open hours, category, image, rating
- keep MVVM structure
- do not add translation, speech, or reviews yet
- explain each modified file
```

---

# Slice 5 — Map integration

## Prompt 5.1
```text
Add Microsoft.Maui.Controls.Maps integration to the mobile app.

Requirements:
- configure the project correctly for MAUI Maps
- create a map screen that loads stall pins from the existing backend API
- do not implement current user location yet
- tapping a pin should open the related stall detail page or select the stall clearly
- keep UI stable and minimal
- explain every modified file and any platform setup needed
```

## Prompt 5.2
```text
Add current user location support to the MAUI app.

Requirements:
- request location permission properly
- center the map around the user if permission is granted
- handle permission denied state gracefully
- do not add proximity triggers yet
- avoid platform-specific duplication unless necessary
- explain all modified files and manual test steps
```

---

# Slice 6 — Proximity detection

## Prompt 6.1
```text
Implement proximity detection on the mobile side.

Requirements:
- create ILocationService abstraction
- create IProximityService abstraction
- implement distance calculation with a testable method
- detect when the user is within a stall's TriggerRadiusMeters
- expose a nearby-stall event or callback usable by the UI
- do not add text-to-speech yet
- explain all modified files
```

## Prompt 6.2
```text
Add trigger cooldown and duplicate suppression to the proximity logic.

Requirements:
- avoid repeated retriggers for the same stall in a short period
- make cooldown configurable
- add unit tests for:
  - inside radius triggers
  - outside radius does not trigger
  - cooldown blocks duplicate trigger
- keep implementation simple
- explain all modified files and how to run tests
```

## Prompt 6.3
```text
Connect proximity detection to the map UI.

Requirements:
- when a nearby stall is detected, show a non-intrusive banner, popup, or card
- allow the user to open stall details from that prompt
- do not auto-play narration yet
- avoid repeated UI spam
- explain each modified file and manual test steps
```

---

# Slice 7 — Translation abstraction first

## Prompt 7.1
```text
Introduce translation on the backend behind an interface.

Requirements:
- create ITranslationService in Application
- create a fake/in-memory implementation in Infrastructure first
- add StallTranslation cache entity/table
- implement GET /api/stalls/{id}/translation?lang=en
- return Vietnamese source text when lang=vi
- do not integrate Azure yet
- add tests for cache hit/miss behavior using the fake translator
- explain all modified files
```

## Prompt 7.2
```text
Connect the mobile stall detail page to the translation endpoint.

Requirements:
- add a language selector on the detail page
- support at least Vietnamese and English in UI flow
- load translated content when user changes language
- show loading and error states clearly
- do not add speech yet
- explain all modified files and manual test steps
```

---

# Slice 8 — Azure Translator integration

## Prompt 8.1
```text
Replace the fake backend translation implementation with a real Azure Translator adapter.

Requirements:
- keep the existing ITranslationService interface unchanged if possible
- read Azure configuration from settings/options
- keep translation caching behavior
- handle provider failures gracefully
- do not change controller contracts unless necessary
- add or update tests around provider failure paths
- explain every modified file and what environment variables or config values are needed
```

---

# Slice 9 — Speech / narration

## Prompt 9.1
```text
Add speech/narration support to the MAUI app behind an ISpeechService abstraction.

Requirements:
- first implement a simple device text-to-speech version
- connect it to the stall detail page
- narrate the currently selected language text
- add play and stop controls
- prevent overlapping narration sessions
- do not use Azure speech yet
- explain all modified files and manual test steps
```

## Prompt 9.2
```text
Improve the speech experience.

Requirements:
- stop current narration when user changes stall or language
- reflect current speaking state in the UI
- handle unsupported language or TTS failure gracefully
- keep implementation minimal and readable
- explain all modified files
```

---

# Slice 10 — Reviews

## Prompt 10.1
```text
Implement backend reviews for stalls.

Requirements:
- add Review entity
- add GET /api/stalls/{id}/reviews
- add POST /api/stalls/{id}/reviews
- validate rating range 1 to 5
- update stall average rating after new review
- do not add authentication yet; temporarily allow a simple placeholder user id if needed
- add tests for validation and average rating update
- explain all modified files
```

## Prompt 10.2
```text
Add review UI to the MAUI app.

Requirements:
- create a review list screen for a stall
- show rating, comment, created date
- create a simple submit review form
- validate required fields on mobile side
- do not add login yet
- explain all modified files and manual test steps
```

---

# Slice 11 — Authentication

## Prompt 11.1
```text
Add simple authentication to the backend.

Requirements:
- implement register and login endpoints
- use JWT
- create minimal User entity for MVP
- keep password handling safe using established ASP.NET patterns
- do not build role management beyond what is needed
- add integration tests for login/register success and failure
- explain all modified files and config values needed
```

## Prompt 11.2
```text
Protect review submission with authentication.

Requirements:
- require auth for POST /api/stalls/{id}/reviews
- use the authenticated user identity instead of placeholder user id
- keep GET reviews public
- update tests accordingly
- do not modify unrelated endpoints
- explain all modified files
```

## Prompt 11.3
```text
Add basic login/register flow to the MAUI app.

Requirements:
- create login and register screens using MVVM
- store JWT securely
- use the token for authenticated review submission
- support guest browsing for non-protected features
- keep UI minimal
- explain all modified files and manual test steps
```

---

# Slice 12 — QR fallback

## Prompt 12.1
```text
Add QR fallback support to the MAUI app.

Requirements:
- choose a simple QR payload format like stall:{id}
- create a QR scan page or stub integration point
- when a valid QR is scanned, open the stall detail page
- if camera scanning is too platform-specific for now, first implement the parsing/navigation logic behind an interface and add a fake scanner result path for testing
- explain all modified files
```

---

# Slice 13 — UX polish

## Prompt 13.1
```text
Polish the mobile UX without changing architecture.

Requirements:
- add loading indicators for network operations
- add empty states and error states
- improve visual spacing on list, detail, review, and map screens
- keep styling simple and consistent
- do not introduce a heavy design system
- explain all modified files
```

## Prompt 13.2
```text
Polish the backend developer experience.

Requirements:
- improve Swagger descriptions
- add example requests/responses where helpful
- improve README run instructions
- document required configuration keys for PostgreSQL, JWT, and Azure services
- explain all modified files
```

---

# Slice 14 — Test hardening

## Prompt 14.1
```text
Review the backend test coverage and add missing tests for the MVP.

Focus on:
- stalls read endpoints
- nearby query
- translation cache logic
- review validation
- auth login/register

Requirements:
- add only meaningful tests
- do not rewrite production code unless required to make it testable
- explain all modified files and test commands
```

## Prompt 14.2
```text
Review the mobile code for testable logic and extract small units for testing where worthwhile.

Focus on:
- proximity calculations
- cooldown gating
- simple state transitions in ViewModels

Requirements:
- avoid overengineering UI tests
- prioritize pure logic tests
- explain all modified files and how to run tests
```

---

# Recovery prompts when Codex drifts

## Recovery A — Too much refactor
```text
Stop. Revert to the smallest implementation that satisfies the requested feature.
Do not refactor unrelated files.
Preserve public APIs unless absolutely necessary.
Show a concise plan before making code changes.
```

## Recovery B — Invented dependencies
```text
Do not invent unavailable SDKs or unsupported APIs.
If an external dependency is needed, add an interface and a fake/stub implementation first.
Then explain what real package or service should be wired later.
```

## Recovery C — Broke build
```text
Your last change introduced compile or startup errors.
Fix the build only.
Do not add new features.
Explain the root cause and list the exact files changed.
```

## Recovery D — Missing tests
```text
Add verification for the feature you just implemented.
Prefer unit or integration tests where practical.
If a feature must be manually tested, provide a short reproducible checklist.
Do not add unrelated features.
```

---

# Suggested commit message style

Use small commits such as:
- `chore: scaffold solution structure`
- `feat(api): add stall read endpoints`
- `feat(mobile): show stall list from api`
- `feat(mobile): add map pins for stalls`
- `feat(mobile): detect nearby stalls with cooldown`
- `feat(api): add translation endpoint with cache`
- `feat(mobile): add stall narration`
- `feat(api): add review submission`
- `feat(auth): add jwt login and register`

---

# Final operating advice

For the fastest safe vibe coding loop:
1. ask Codex for one slice only
2. run build/tests immediately
3. inspect changed files before merging
4. keep Azure and device integrations behind interfaces
5. prefer a thin vertical slice over broad scaffolding

Recommended first execution order:
- 1.1
- 1.2
- 2.1
- 2.2
- 3.1
- 4.1
- 4.2
- 5.1
- 5.2
- 6.1
- 6.2
- 6.3
- 7.1
- 7.2
- 8.1
- 9.1
- 9.2
- 10.1
- 10.2
- 11.1
- 11.2
- 11.3
- 12.1
- 13.1
- 13.2
- 14.1
- 14.2
