# PROMPTS STORAGE
## Project: GPS Travel Audio Guide App

---

# 1. Generate Backend API
Prompt:
"Generate FastAPI backend structure for a GPS travel audio guide app with authentication, locations, favorites, manager dashboard and admin management."

---

# 2. Generate Database Models
Prompt:
"Create SQLAlchemy models for Users, Locations and Manager system with role-based access (user, manager, admin). Locations must belong to a manager."

---

# 3. Generate Login API (UPDATED)
Prompt:
"Write FastAPI login and register API with JWT authentication.

Requirements:
- Register must include role selection:
  + user (experience app)
  + manager (business)
- Hash password
- Validate input"

---

# 4. Generate Map UI
Prompt:
"Create ReactJS component that displays Google Maps, shows user GPS location and nearby locations from API."

---

# 5. Distance Calculation
Prompt:
"Write function to calculate distance between user GPS and location GPS and trigger audio when distance < 50 meters."

---

# 6. Auto Play Audio
Prompt:
"Create React component that automatically plays audio description when user enters location radius."

---

# 7. Admin Dashboard (UPDATED)
Prompt:
"Generate admin dashboard UI to:
- manage locations
- manage users
- manage managers
- assign locations to managers"

---

# 8. Manager Dashboard 🔥 NEW
Prompt:
"Generate manager dashboard UI with:

- Sidebar layout
- Pages:
  + My Locations
  + Statistics

- Responsive
- Clean UI
- Call backend APIs
- list of owned locations (stalls)
- CRUD locations
- manage audio content
- view statistics (views count, audio plays count, time-based data)
Data source: location_stats table"

---

# 9. Project Structure
Prompt:
"Generate full project folder structure for React Frontend + FastAPI Backend + MySQL database including manager module."

---

# 10. API Design Prompt (UPDATED)
Prompt:
"Design REST API endpoints for:
- authentication
- locations
- favorites
- manager dashboard
- admin management
- assign location to manager"

---

# 10. PRD Prompt (UPDATED)
Prompt:
"Write a Product Requirements Document (PRD) for my project.
However, you MUST use my existing HTML presentation template and KEEP the layout and CSS exactly the same.

IMPORTANT RULES:

* Do NOT change CSS
* Do NOT change layout
* Do NOT change HTML structure
* Do NOT change class names
* Do NOT change colors or fonts
* Do NOT remove sections
* Do NOT redesign UI
* Only replace TEXT CONTENT inside the HTML
* You may add more cards/tables using existing classes
* The output must be a full HTML file

---

My project:

A GPS-based tourism narration application.

Core idea:
When users enter a location (geofence), the app automatically plays audio narration about that place.

Technology:
- Frontend: React (PWA)
- Backend: FastAPI
- Database: MySQL
- Map: Google Maps API
- Audio: Text-to-Speech (TTS)

System capabilities:
- GPS tracking
- Geofencing
- Auto audio playback
- Multi-language support (VI, EN, ZH, DE)
- Offline support (PWA)
- Admin content management

---

## 🔥 NEW SYSTEM EXTENSIONS (MUST INCLUDE)

### Role-Based System:
- user (default)
- manager (NEW)
- admin

### Manager Module:
- Each manager owns and manages multiple locations (stalls)
- Manager dashboard:
  - Manage owned locations (CRUD)
  - Manage narration content
  - View statistics:
    + view count
    + audio play count
    + time-based analytics

### Admin Module (Extended):
- Manage users
- Manage managers
- Assign existing locations to managers
- Control system-wide data

### Registration Flow (UPDATED):
- User selects role during registration:
  - "I want to experience the app" → user
  - "I want to do business" → manager

---

Rewrite the content of the sections to match my FULL system:

- Overview (include full architecture + roles)
- Startup Flow (include role-based flow)
- GPS / Geofence / Audio
- Content Module (include manager ownership)
- Audio / TTS
- Authentication / RBAC (IMPORTANT: include user, manager, admin)
- Localization
- Offline / PWA
- Map System
- Admin Dashboard (include manager management + assign logic)
- Design Patterns
- End-to-End Flow (include user + manager + admin flows)

---

Writing style:

* Technical documentation
* Software architecture document
* Product requirement document
* Clear structure
* Suitable for university graduation project
* Professional engineering documentation

---

OUTPUT REQUIREMENT:

Do NOT create new sections.
Do NOT change layout.
Only rewrite the content inside these sections to match my updated system.

The HTML template structure IS the PRD structure.

Return the FULL HTML file with updated content.
Do NOT explain anything.
Do NOT use markdown.
Only return HTML."

# 12. UI Prompt (UPDATED)
Prompt:
"Design UI for:
- User app (map + audio)
- Manager dashboard
- Admin dashboard
Modern, responsive, grid-based layout."

---

# 13. Sequence Flow Prompt
Prompt:
"Write system sequence diagram including:
- user flow
- manager flow
- admin assigning locations to manager"

---

# 14. Consistency Rules (UPDATED)
Luôn giữ thống nhất:
- Backend: FastAPI
- Frontend: ReactJS
- Database: MySQL
- Map: Google Maps
- GPS trigger distance: 50m
- Languages: VI, EN, ZH, DE
- Roles:
  - user
  - manager 🔥
  - admin

---

# 15. Core Features Reminder (UPDATED)

Main Features:
- Authentication (role-based)
- Map + GPS
- Locations
- Auto audio
- Favorites
- Manager dashboard 🔥
- Admin management
- Multilingual audio