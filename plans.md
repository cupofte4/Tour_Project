<<<<<<< HEAD
# PROJECT PLAN (WEB - REACT + .NET)
## Ứng dụng thuyết minh du lịch

---

# 1. Mô tả dự án
Xây dựng web app thuyết minh du lịch:

- Frontend: React (JSX)
- Backend: ASP.NET Core Web API (.NET)
- Database: MySQL (Entity Framework Core)

Chức năng:
- Hiển thị bản đồ + vị trí user
- Xem POI (địa điểm)
- Khi user đến gần → phát audio
- Hoặc click / QR để nghe

---

# 2. Kiến trúc hệ thống

## Architecture

Frontend (React)
→ gọi API
→ ASP.NET Core
→ Entity Framework Core
→ MySQL

---

## Core Modules

### Backend (.NET)
1. Controllers (API)
2. Services (Business logic)
3. Models (Entity)
4. DbContext (EF Core)

### Frontend (React)
1. Map UI (Google Maps)
2. Geofence Logic (JS)
3. Audio Player
4. Pages (User / Manager / Admin)

---

# 3. Chức năng chính

## 3.1 GPS (Frontend)
- Dùng `navigator.geolocation`
- Lấy vị trí theo interval
- Chỉ hoạt động khi mở web

---

## 3.2 Geofence (React)
- Dùng công thức Haversine (bạn đã có backend rồi)
- Kiểm tra khoảng cách:
  → nếu < radius → trigger

Yêu cầu:
- debounce
- cooldown
- chọn POI gần nhất

---

## 3.3 Narration System
- Audio chạy trên browser

Tính năng:
- Queue audio
- Không phát chồng
- Auto stop audio cũ

---

## 3.4 POI System
- Lấy từ API (.NET)
- Bao gồm:
  - name
  - lat, lng
  - description
  - multi-language text
  - audio

---

# 4. UI/UX

## Map Screen
- Google Maps
- Hiển thị user
- Hiển thị POI
- Highlight gần nhất

## POI Detail
- Thông tin
- Hình ảnh
- Audio player
- Chọn ngôn ngữ

---

# 5. CMS (Manager / Admin)

## Manager
- CRUD Location
- Upload audio
- Xem stats

## Admin
- Quản lý user
- Quản lý manager
- Assign location

---

# 6. Analytics

Backend (.NET):
- LocationStats:
  - ViewsCount
  - AudioPlaysCount

Frontend:
- Gửi event về API

---

# 7. QR Mode
- Scan QR (React camera)
- Map → LocationId
- Trigger audio

---

# 8. Database (EF Core)

## User
## Location
## LocationStat
## LocationManagerAssignment

---

# 9. Phân quyền

- user
- manager
- admin

---

# 10. Phase

Phase 1:
- Setup React + .NET API

Phase 2:
- Auth + Role

Phase 3:
- Map + GPS

Phase 4:
- Geofence

Phase 5:
- Audio

Phase 6:
- CMS

Phase 7:
- Analytics

Phase 8:
- QR

---

# 11. Flow

1. React load
2. Call API (.NET)
3. Get locations
4. Track GPS
5. Detect gần
6. Play audio

---

# 12. Điểm mạnh

- Geofence (JS + C#)
- Audio queue
- Role-based system
- Analytics
- QR mode

---

# STATUS
updating...
=======
# plans.md

# Vibe Coding Plan for Vinh Khanh Food Street Guide App
## Stack direction
- Mobile: .NET MAUI, C#, XAML, MVVM
- Map: Microsoft.Maui.Controls.Maps
- Backend: ASP.NET Core Web API, EF Core, PostgreSQL
- AI services: Azure Translator, Azure AI Speech
- Detection strategy: GPS radius first, QR fallback
- Working style: Codex-assisted vibe coding with small, testable increments

---

# 1. PLAN

## 1.1 Project goal
Build a mobile app for Vinh Khanh Food Street that can:
- show stalls on a map
- detect when a tourist is near a stall
- display stall information
- play audio narration in the tourist's language
- translate Vietnamese content to foreign languages
- allow users to read and submit ratings/reviews

## 1.2 MVP scope
The first shippable version should include:
- map with stall pins
- current user location
- nearby stall detection using GPS radius
- stall detail page
- translated stall description
- text-to-speech narration
- review list and review submission
- simple admin-seeded data or mock data import

## 1.3 Non-goals for MVP
Do not build these first:
- real-time chat
- advanced recommendation engine
- indoor navigation
- beacon hardware integration
- social feed
- full moderation dashboard
- speech-to-text input

## 1.4 Product assumptions
- Each stall has fixed GPS coordinates
- GPS can be inaccurate, so trigger radius should be configurable
- Main supported languages for MVP: Vietnamese, English
- Optional stretch languages: Korean, Japanese, Chinese
- Tourist can use the app without creating an account for browsing
- Account is only required for posting a review

## 1.5 Delivery strategy
Build in four layers:
1. Mobile app skeleton
2. Core backend API
3. Map + location trigger logic
4. Translation + narration + reviews

## 1.6 Codex working rules
Use Codex with these constraints:
- make one small change per prompt
- keep commits scoped and reversible
- always ask Codex to explain modified files
- prefer working code over broad refactors
- after each generated step, run the app or tests before continuing
- do not let Codex invent missing APIs without creating interfaces/stubs first

Suggested Codex prompting pattern:
- "Implement only the X feature. Do not modify unrelated files."
- "Return the diff summary and explain how to run it."
- "Add tests for the new service before refactoring."
- "Use existing naming conventions and MVVM structure."

---

# 2. DESIGN

## 2.1 System architecture
The solution should have three parts:

### A. Mobile app
Responsibilities:
- render map and pins
- get user location
- detect nearby stalls
- show stall details
- play narration
- show translated content
- browse and submit reviews

### B. Backend API
Responsibilities:
- serve stall data
- serve review data
- authenticate users
- provide translated text or proxy translation service
- optionally generate or cache narration text/audio metadata

### C. External services
Responsibilities:
- Azure Translator: translate Vietnamese descriptions
- Azure AI Speech: generate speech audio or speech from translated text

## 2.2 Suggested repository structure
Use a mono-repo:

```text
vinhkhanh-guide/
  plans.md
  README.md
  docs/
  src/
    mobile/
      VinhKhanhGuide.Mobile/
    backend/
      VinhKhanhGuide.Api/
      VinhKhanhGuide.Application/
      VinhKhanhGuide.Domain/
      VinhKhanhGuide.Infrastructure/
  tests/
    VinhKhanhGuide.Api.Tests/
    VinhKhanhGuide.Application.Tests/
    VinhKhanhGuide.Mobile.Tests/
```

## 2.3 Mobile architecture
Use MVVM.

### Mobile layers
- Views
- ViewModels
- Services
- Models / DTOs
- Platform integrations

### Suggested folders
```text
VinhKhanhGuide.Mobile/
  App.xaml
  MauiProgram.cs
  Views/
  ViewModels/
  Services/
  Models/
  Converters/
  Resources/
  Platforms/
```

## 2.4 Backend architecture
Use clean separation:

- Domain: entities and business rules
- Application: interfaces, use cases, DTOs
- Infrastructure: EF Core, external services, repositories
- Api: controllers, auth, DI, swagger

## 2.5 Core domain entities

### Stall
- Id
- Name
- DescriptionVi
- Latitude
- Longitude
- TriggerRadiusMeters
- Category
- OpenHours
- ImageUrl
- AverageRating

### StallTranslation
- Id
- StallId
- LanguageCode
- Name
- Description
- LastGeneratedAt

### Review
- Id
- StallId
- UserId
- Rating
- Comment
- CreatedAt

### User
- Id
- DisplayName
- Email
- PasswordHash or external auth ID
- PreferredLanguage

## 2.6 API design
Minimum endpoints:

### Public
- `GET /api/stalls`
- `GET /api/stalls/{id}`
- `GET /api/stalls/nearby?lat={x}&lng={y}&radius={r}`
- `GET /api/stalls/{id}/reviews`
- `GET /api/stalls/{id}/translation?lang=en`

### Authenticated
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/stalls/{id}/reviews`

### Optional admin seed/import
- `POST /api/admin/stalls/import`

## 2.7 Map and proximity design
Store each stall with coordinates and a trigger radius.
Detection logic:
1. app gets current device location
2. app loads stall list
3. app calculates distance to each stall
4. if distance <= trigger radius, mark as nearby
5. if first entry into the radius, show prompt and allow narration
6. prevent repeated retriggers with cooldown and last triggered stall state

## 2.8 Translation design
Preferred design:
- source text stored in Vietnamese
- when app asks for another language, backend checks translation cache
- if cache exists, return it
- if not, call Azure Translator, store result, return it

This reduces repeated external API cost and latency.

## 2.9 Narration design
Two modes:

### Mode A: text-only TTS on device
- app receives translated text
- app uses TTS service on device or Azure speech result
- easiest for MVP

### Mode B: pre-generated audio
- backend stores generated audio URLs
- better for quality and consistency
- more storage and orchestration

Use Mode A first.

## 2.10 Review design
Rules:
- anonymous users can read reviews
- logged-in users can submit reviews
- one user can submit one review per stall, editable later
- rating must be 1 to 5
- comment length limit for MVP

## 2.11 UI design
Required screens:
- Splash / startup
- Home map screen
- Stall detail screen
- Review list and create review screen
- Language selector
- Login / register
- Settings

## 2.12 Risk controls
Main implementation risks:
- GPS inaccuracy
- external API failures
- over-scoped MVP
- Codex generating inconsistent architecture

Mitigations:
- use configurable trigger radius
- add fallback QR scan entry point
- keep Azure behind interfaces
- work in slices, not large code dumps
- require tests or manual checks after every slice

---

# 3. BUILD

## 3.1 Build order
Follow this exact order.

## Phase 1: foundation
Goal: create solution skeleton and run both app and API locally.

Tasks:
1. create solution and projects
2. set up .NET MAUI app
3. set up ASP.NET Core Web API
4. add project references
5. configure DI
6. configure EF Core + PostgreSQL connection
7. add Swagger
8. create base README with run instructions

Definition of done:
- mobile app boots
- API boots
- Swagger opens
- project compiles without placeholder errors

Recommended Codex prompts:
- "Create the solution structure with one .NET MAUI project and a clean ASP.NET Core backend. Keep code minimal and compilable."
- "Add EF Core and PostgreSQL wiring but do not add business logic yet."

## Phase 2: stall data
Goal: app can display stalls from backend.

Tasks:
1. create `Stall` entity and migration
2. create seed data for 5 to 10 stalls in Vinh Khanh mock dataset
3. implement `GET /api/stalls`
4. implement `GET /api/stalls/{id}`
5. create mobile API client service
6. create home page ViewModel
7. list stalls in a simple page before map integration

Definition of done:
- API returns stall list
- mobile app displays stall names and images
- clicking a stall opens detail page

Recommended Codex prompts:
- "Implement stall entity, seed data, and read-only endpoints only."
- "Add a stall list screen in the MAUI app using MVVM and API consumption."

## Phase 3: map integration
Goal: app shows stalls on map and user location.

Tasks:
1. add MAUI Maps package and configuration
2. create map page
3. request location permission
4. show user location
5. render stall pins
6. allow tap on pin to open stall details

Definition of done:
- user sees map
- stalls appear as pins
- selecting a pin opens the correct detail page

Recommended Codex prompts:
- "Add map screen with pins from existing stall API. Keep pin selection simple and reliable."
- "Request location permission and center map around the user."

## Phase 4: proximity trigger
Goal: detect when the user is near a stall.

Tasks:
1. create `ILocationService`
2. create `IProximityService`
3. implement Haversine distance calculation
4. poll or subscribe to location updates
5. compare current location to stall coordinates
6. raise nearby-stall event
7. add cooldown to avoid repeated triggers
8. show popup/banner for nearby stall

Definition of done:
- moving near a stall triggers a nearby event
- the same stall does not retrigger repeatedly in a short interval

Recommended Codex prompts:
- "Implement proximity detection with configurable radius and cooldown. Do not add TTS yet."
- "Add unit tests for distance calculation and trigger gating."

## Phase 5: stall detail experience
Goal: detailed stall page becomes useful.

Tasks:
1. build stall detail UI
2. show name, description, category, hours, rating, image
3. add language selector
4. add button for narration
5. add button for reviews

Definition of done:
- detail page is visually clear
- language can be changed
- narration button is visible
- reviews page opens

## Phase 6: translation
Goal: translated text is available on demand.

Tasks:
1. define `ITranslationService`
2. implement Azure Translator adapter in infrastructure
3. add translation cache entity/table
4. implement `GET /api/stalls/{id}/translation?lang=xx`
5. connect mobile detail page to translation endpoint
6. show loading and error state

Definition of done:
- app can switch between Vietnamese and English
- translated text is shown correctly
- repeated calls reuse cached translation

Recommended Codex prompts:
- "Introduce translation behind an interface and provide a fake implementation first."
- "Then add Azure Translator adapter without changing calling code."

## Phase 7: narration
Goal: app can narrate stall description in selected language.

Tasks:
1. define `ISpeechService` in mobile
2. create MVP implementation using device TTS or Azure speech text input
3. connect narration button to selected language content
4. stop previous narration when new narration begins
5. show play / stop state in UI

Definition of done:
- pressing narration reads the stall text aloud
- speech uses selected language where supported
- app does not overlap multiple speech sessions

Recommended Codex prompts:
- "Implement text-to-speech service abstraction and connect it to the stall detail page."
- "Add cancellation so repeated taps do not stack audio."

## Phase 8: reviews
Goal: users can read and submit ratings.

Tasks:
1. create `Review` entity and migration
2. implement `GET /api/stalls/{id}/reviews`
3. implement login/register
4. implement `POST /api/stalls/{id}/reviews`
5. add review list page
6. add create review form
7. update average rating after new review

Definition of done:
- guests can read reviews
- authenticated users can submit a review
- new review appears in list
- average rating updates

Recommended Codex prompts:
- "Implement review read/write APIs with validation."
- "Create a simple MAUI review page using existing auth token storage."

## Phase 9: QR fallback
Goal: allow manual stall recognition if GPS is weak.

Tasks:
1. decide QR payload format, e.g. `stall:{id}`
2. add QR scan page
3. parse QR result
4. open stall detail directly

Definition of done:
- scanning a valid QR opens the correct stall page

## Phase 10: polish
Tasks:
1. add loading indicators
2. add empty/error states
3. improve map visuals
4. improve review card UI
5. add app icon and branding
6. improve README and demo notes

---

# 4. TEST

## 4.1 Testing strategy
Use a mix of:
- unit tests for business logic
- integration tests for API endpoints
- manual device tests for map, GPS, TTS
- smoke tests after every Codex-generated step

## 4.2 Unit tests
Write tests for:

### Proximity logic
- distance calculation between two points
- stall enters radius
- stall outside radius
- cooldown prevents retrigger
- nearest stall selection

### Translation logic
- cache hit returns cached translation
- cache miss calls provider
- provider failure returns controlled error

### Review logic
- invalid rating rejected
- one-review-per-user-per-stall rule
- average rating recomputation

### Auth logic
- login success
- invalid credentials fail
- token generated correctly

## 4.3 API integration tests
Test endpoints:
- `GET /api/stalls`
- `GET /api/stalls/{id}`
- `GET /api/stalls/nearby`
- `GET /api/stalls/{id}/reviews`
- `POST /api/stalls/{id}/reviews`
- `GET /api/stalls/{id}/translation?lang=en`

Assertions:
- correct status code
- correct JSON shape
- validation errors when expected
- auth required where expected

## 4.4 Mobile manual test checklist

### Map
- app asks for location permission
- current location is shown
- pins load correctly
- tapping pin opens correct stall

### Proximity
- mock location or physical movement triggers nearby stall
- trigger does not spam repeatedly
- cooldown works

### Translation
- switching language updates text
- loading/error states appear correctly
- fallback to Vietnamese works

### Narration
- narration starts
- narration stops
- switching stall stops previous narration
- language selection affects spoken output

### Reviews
- guests can read reviews
- logged-in user can submit review
- invalid form is blocked
- new review appears after submission

## 4.5 Test environments
Use:
- local backend with PostgreSQL in Docker
- MAUI Android emulator first
- real Android device for GPS and TTS validation
- optional iPhone build later if environment is ready

## 4.6 Mocking strategy
For faster iteration, mock these first:
- translation service
- speech service
- location provider
- authentication provider

Then swap in real Azure and device services later.

## 4.7 Codex testing rules
Every Codex iteration should end with one of:
- unit tests added and passed
- integration test added and passed
- manual smoke test checklist updated with actual result

Never accept a generated feature with no verification step.

---

# 5. CODING SLICES FOR CODEX

## Slice 1
Create solution skeleton with MAUI app and ASP.NET Core API.

## Slice 2
Add domain entities and seed stalls.

## Slice 3
Expose stall endpoints and verify via Swagger.

## Slice 4
Consume stall list in MAUI app.

## Slice 5
Add map and pins.

## Slice 6
Add proximity detection service and tests.

## Slice 7
Add stall detail page.

## Slice 8
Add translation abstraction and fake implementation.

## Slice 9
Replace fake translator with Azure Translator.

## Slice 10
Add speech abstraction and device TTS.

## Slice 11
Add reviews backend and UI.

## Slice 12
Add auth and protected review submission.

## Slice 13
Add QR fallback.

## Slice 14
Polish, test, document, demo.

---

# 6. DEFINITION OF SUCCESS

The project is successful when:
- a tourist can open the app and see food stalls on the map
- the app can detect when the tourist is near a stall
- the stall detail can be shown in another language
- the app can narrate the stall information
- the tourist can read reviews
- an authenticated user can submit a review
- the codebase remains understandable enough for continued Codex-assisted development

---

# 7. FINAL RECOMMENDATION

For vibe coding with Codex, prioritize:
1. stable architecture first
2. visible progress every slice
3. interfaces around map, translation, and speech
4. mock-first development
5. aggressive scope control

Do not start with all features at once.
Start with a thin vertical slice:
- seeded stall data
- mobile list page
- stall detail page
- map pin page
Then add proximity, translation, narration, and reviews one by one.
>>>>>>> origin/feature/food_court_app
