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