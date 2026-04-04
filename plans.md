# PROJECT PLAN
## Ứng dụng thuyết minh du lịch sử dụng GPS

## 1. Mô tả dự án
Xây dựng ứng dụng thuyết minh du lịch sử dụng GPS. Khi người dùng đến gần một địa điểm du lịch, ứng dụng sẽ tự động phát nội dung thuyết minh bằng âm thanh theo ngôn ngữ đã chọn.

---

# 2. Chức năng chính

## Đối với người dùng
- Đăng ký / đăng nhập
- Xem bản đồ
- Xem danh sách địa điểm
- Nghe thuyết minh tự động khi đến gần địa điểm
- Chọn ngôn ngữ
- Xem danh sách địa điểm yêu thích

## Đối với quản trị viên
- Thêm địa điểm du lịch
- Chỉnh sửa nội dung thuyết minh
- Quản lý người dùng

---

# 3. Database
## Bảng Locations
- Id
- Name
- Description
- Image
- Images
- Address
- Phone
- ReviewsJson
- Latitude
- Longitude
- TextVi
- TextEn
- TextZh
- TextDe

## Bảng Users
- Id
- Username
- Password
- FullName
- Phone
- Gender
- Avatar
- Role
- IsLocked

---

# 4. Tiến độ dự án

## Phase 1 – Setup Project
- [ ] Tạo cấu trúc project
- [ ] Setup Frontend
- [ ] Setup Backend API
- [ ] Kết nối Database
- [ ] Login / Register API

## Phase 2 – User Features
- [ ] Hiển thị bản đồ
- [ ] Lấy vị trí GPS người dùng
- [ ] Hiển thị danh sách địa điểm
- [ ] Hiển thị chi tiết địa điểm
- [ ] Tự động phát audio khi đến gần địa điểm
- [ ] Chọn ngôn ngữ thuyết minh
- [ ] Địa điểm yêu thích

## Phase 3 – Admin Features
- [ ] Thêm địa điểm
- [ ] Sửa địa điểm
- [ ] Xóa địa điểm
- [ ] Quản lý người dùng
- [ ] Upload audio thuyết minh

## Phase 4 – Hoàn thiện
- [ ] UI/UX
- [ ] Testing
- [ ] Fix bug
- [ ] Deploy
- [ ] Viết báo cáo
- [ ] Làm slide

---

# 5. TODO – Product Requirements Document (PRD)

## PRD Sections

1. Overview (System Architecture)
2. Startup Flow
3. GPS + Geofence + Audio Narration
4. Content Module / POI Management
5. Audio / TTS System
6. Authentication & RBAC
7. Localization / Multi-language
8. Offline / PWA
9. Map System
10. Admin Dashboard
11. Design Patterns
12. End-to-End Flow

---

# 6. User Flow
1. User mở app
2. App lấy vị trí GPS
3. App hiển thị các địa điểm gần
4. User di chuyển đến địa điểm
5. Khi khoảng cách < 50m
6. App tự động phát audio thuyết minh
7. Lưu lịch sử / yêu thích

---

# 7. Mốc thời gian dự kiến

| Tuần | Công việc |
|-----|-----------|
| 1 | Setup project |
| 2 | Login / Register |
| 3 | Map + GPS |
| 4 | Locations API |
| 5 | Audio auto play |
| 6 | Favorite |
| 7 | Admin |
| 8 | UI |
| 9 | Testing |
| 10 | Báo cáo |

---

# 8. Trạng thái dự án
Status: In Progress