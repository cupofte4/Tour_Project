# PROMPTS STORAGE
## Project: GPS Travel Audio Guide App

File này dùng để lưu các prompt hiệu quả khi làm việc với AI để đảm bảo code và kiến trúc dự án nhất quán.

---

# 1. Generate Backend API
Prompt:
"Generate FastAPI backend structure for a GPS travel audio guide app with authentication, locations, favorites and admin management."

---

# 2. Generate Database Models
Prompt:
"Create SQLAlchemy models for Users and Locations tables with multilingual description fields and GPS coordinates."

---

# 3. Generate Login API
Prompt:
"Write FastAPI login and register API with JWT authentication and password hashing."

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

# 7. Admin Dashboard
Prompt:
"Generate admin dashboard UI to manage locations, edit description, upload audio and manage users."

---

# 8. Project Structure
Prompt:
"Generate full project folder structure for React Frontend + FastAPI Backend + MySQL database for GPS audio travel guide app."

---

# 9. API Design Prompt
Prompt:
"Design REST API endpoints for authentication, locations, favorites, history and admin management."

---

# 10. PRD Prompt
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

My project:
A GPS-based tourism narration application.
When users enter a location (geofence), the app automatically plays audio narration about that place.
The system supports multiple languages, offline mode, maps, admin content management, and AI-assisted content writing.

Rewrite the content of the sections to match my system:

- Overview
- Startup Flow
- GPS / Geofence / Audio
- Content Module
- Audio / TTS
- Authentication / RBAC
- Localization
- Offline / PWA
- Map System
- Admin Dashboard
- Design Patterns
- End-to-End Flow

Writing style:

* Technical documentation
* Software architecture document
* Product requirement document
* Clear structure
* Suitable for university graduation project
* Professional engineering documentation

OUTPUT REQUIREMENT:

Do NOT create new sections.
Do NOT change layout.
Only rewrite the content inside these sections to match my GPS tourism narration system.

The HTML template structure IS the PRD structure.
Return the FULL HTML file with updated content.
Do NOT explain anything.
Do NOT use markdown.
Only return HTML."

---

# 11. UI Prompt
Prompt:
"Design modern UI for travel guide app including map screen, location list, location detail and audio player."

---

# 12. Sequence Flow Prompt
Prompt:
"Write system sequence diagram flow for GPS travel audio guide app from user opening app to auto playing audio when near location."

---

# 13. Consistency Rules
Luôn giữ thống nhất:
- Backend: FastAPI
- Frontend: ReactJS
- Database: MySQL
- Map: Google Maps
- GPS trigger distance: 50m
- Languages: VI, EN, ZH, DE
- Roles: user, admin

---

# 14. Core Features Reminder
Main Features:
- Authentication
- Map + GPS
- Locations
- Auto audio
- Favorites
- Admin management
- Multilingual audio