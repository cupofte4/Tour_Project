# PROJECT PLAN
## Ứng dụng thuyết minh du lịch sử dụng GPS

## 1. Mô tả dự án
Xây dựng ứng dụng thuyết minh du lịch sử dụng GPS. Khi người dùng đến gần một địa điểm du lịch, ứng dụng sẽ tự động phát nội dung thuyết minh bằng âm thanh theo ngôn ngữ đã chọn.

Hệ thống mở rộng:
- Hỗ trợ chủ quản lý sạp (manager)
- Cho phép quản lý sạp và nội dung thuyết minh riêng

---

# 2. Chức năng chính

## Đối với người dùng
- Đăng ký / đăng nhập
- Chọn loại tài khoản khi đăng ký:
  - Tôi muốn trải nghiệm app → user
  - Tôi muốn kinh doanh → manager
- Xem bản đồ
- Xem danh sách địa điểm
- Nghe thuyết minh tự động khi đến gần địa điểm
- Chọn ngôn ngữ
- Xem danh sách địa điểm yêu thích

## Đối với chủ quản lý sạp (Manager) 🔥 NEW
- Truy cập dashboard riêng
- Xem danh sách sạp sở hữu
- Thêm / sửa / xoá sạp
- Quản lý nội dung thuyết minh của sạp
- Xem thống kê:
  - lượt truy cập
  - lượt nghe audio
  - dữ liệu theo thời gian

## Đối với quản trị viên (Admin)
- Thêm địa điểm du lịch
- Chỉnh sửa nội dung thuyết minh
- Quản lý người dùng
- Quản lý chủ sạp (manager) 🔥 NEW
- Gán sạp có sẵn cho manager 🔥 NEW

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
- ManagerId 🔥 NEW (sạp thuộc manager nào)

## Bảng Users
- Id
- Username
- Password
- FullName
- Phone
- Gender
- Avatar
- Role (user | manager | admin) 🔥 UPDATED
- IsLocked

---

# 4. Tiến độ dự án

## Phase 1 – Setup Project
- [ ] Tạo cấu trúc project
- [ ] Setup Frontend
- [ ] Setup Backend API
- [ ] Kết nối Database
- [ ] Login / Register API (có chọn role)

## Phase 2 – User Features
- [ ] Hiển thị bản đồ
- [ ] Lấy vị trí GPS người dùng
- [ ] Hiển thị danh sách địa điểm
- [ ] Hiển thị chi tiết địa điểm
- [ ] Tự động phát audio khi đến gần địa điểm
- [ ] Chọn ngôn ngữ thuyết minh
- [ ] Địa điểm yêu thích

## Phase 3 – Manager Features 🔥 NEW
- [ ] Manager Dashboard UI
- [ ] API lấy danh sách sạp theo manager
- [ ] CRUD sạp
- [ ] API thống kê (views, audio plays)
- [ ] Quản lý nội dung thuyết minh

## Phase 4 – Admin Features
- [ ] Thêm địa điểm
- [ ] Sửa địa điểm
- [ ] Xóa địa điểm
- [ ] Quản lý người dùng
- [ ] Upload audio thuyết minh
- [ ] Quản lý manager 🔥 NEW
- [ ] Gán sạp cho manager 🔥 NEW

## Phase 5 – Hoàn thiện
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
6. Authentication & RBAC (user / manager / admin)
7. Manager Dashboard 🔥 NEW
8. Localization / Multi-language
9. Offline / PWA
10. Map System
11. Admin Dashboard
12. Design Patterns
13. End-to-End Flow

---

# 6. User Flow

### User
1. User mở app
2. App lấy vị trí GPS
3. App hiển thị các địa điểm gần
4. User di chuyển đến địa điểm
5. Khi khoảng cách < 50m
6. App tự động phát audio thuyết minh
7. Lưu lịch sử / yêu thích

### Manager 🔥 NEW
1. Manager đăng nhập
2. Truy cập dashboard
3. Quản lý sạp
4. Xem thống kê hoạt động

### Admin
1. Admin đăng nhập
2. Quản lý hệ thống
3. Gán sạp cho manager

---

# 7. Mốc thời gian dự kiến

| Tuần | Công việc |
|-----|-----------|
| 1 | Setup project |
| 2 | Login / Register + Role |
| 3 | Map + GPS |
| 4 | Locations API |
| 5 | Audio auto play |
| 6 | Favorite |
| 7 | Manager Dashboard 🔥 |
| 8 | Admin nâng cao |
| 9 | Testing |
| 10 | Báo cáo |

---

# 8. Trạng thái dự án
Status: In Progress