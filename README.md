# Hutech-StudyMate – Phân tích bảng điểm & CTĐT + AI

Chờ xíu xiu cho trang load nha :> | Bạn có thể truy cập dự án tại đây nè:  <a href="https://hutech-studymate.onrender.com/" target="_blank">Hutech-StudyMate</a>

## Giới thiệu

**Hutech-StudyMate** là ứng dụng web giúp sinh viên tải bảng điểm (Excel) và tự động:
- Nhận diện các môn học đã học
- Đối chiếu với Chương Trình Đào Tạo (CTĐT) theo Niên khóa + Khoa/Viện
- Thống kê tín chỉ tích lũy / chưa tích lũy / ngoài CTĐT
- Tính GPA có trọng số
- Phân loại môn đúng CTĐT, ngoài CTĐT, không tích lũy
- Hỗ trợ nhiều định dạng bảng điểm (kể cả .xls cũ, xuất web, CSV / HTML bảng)
- Trợ lý AI học tập: Tư vấn cá nhân hóa dựa trên kết quả phân tích

Ứng dụng thuần **ASP.NET Core (Minimal Hosting)** + **Vanilla JS** (không framework front-end nặng).

## Tính năng chính

### Trợ lý AI học tập
- **Tư vấn cá nhân hóa**: Dựa trên kết quả phân tích thực tế của bạn
- **Gợi ý đăng ký môn**: Môn nào nên học tiếp theo
- **Chiến lược cải thiện GPA**: Lời khuyên cụ thể để nâng cao điểm
- **Tư vấn chuyên ngành**: Hướng dẫn chọn môn tự chọn phù hợp
- **Lập kế hoạch học tập**: Lộ trình rõ ràng tới tốt nghiệp
- **Chat thời gian thực**: Tương tác trực tiếp bằng tiếng Việt
- **Sử dụng Groq AI**: API miễn phí, phản hồi nhanh

### Phân tích bảng điểm
- Upload kéo thả / chọn file (.xlsx, .xls) – tự kiểm tra định dạng
- Parser đa tầng:
  - EPPlus cho .xlsx
  - ExcelDataReader cho .xls hoặc fallback
  - Tự động fallback sang phân tích văn bản (CSV / TSV / bảng HTML)
- Nhận dạng mã môn qua regex linh hoạt (VD: CMP1074, COS120, LAW123…)

### Chương trình đào tạo (CTĐT)
- Tải JSON CTĐT động theo Niên khóa + Khoa/Viện
- Hợp nhất map mã môn → tên môn
- Phân loại:
  - Môn tích lũy
  - Môn không tích lũy (ví dụ: thể chất / quy định "không tích lũy")
  - Môn ngoài CTĐT
- Tổng hợp số tín chỉ yêu cầu / đã đạt (tách tích lũy & không tích lũy)

### Thống kê & Trình bày
- Bảng kết quả phân trang (mặc định 10 dòng/trang)
- Gắn màu trạng thái từng môn (✓ đúng CTĐT, ⚠ không tích lũy, ✗ ngoài CTĐT)
- Tính GPA hệ 4 và điểm 10 có trọng số theo tín chỉ
- Tổng hợp:
  - Số môn đã học
  - Số môn khớp / lệch CTĐT
  - Tín chỉ tích lũy / không tích lũy / yêu cầu

### Giao diện người dùng
- Thuần HTML/CSS/JS (không phụ thuộc framework UI)
- Drag & drop khu vực upload
- Tự động cuộn tới phần kết quả sau khi xử lý
- Khả năng chọn lại CTĐT khác để so sánh mà không cần upload lại file
- **Chatbot tích hợp**: Nút nổi trợ lý AI luôn sẵn sàng hỗ trợ

### Khả năng phục vụ & Triển khai
- Phục vụ tĩnh thư mục wwwroot + ProgramJson thông qua StaticFileProvider
- Hỗ trợ PORT động (Render / dịch vụ hosting)
- Dockerfile đa stage (build → runtime)
- Encoding provider đăng ký để đọc .xls (mã hóa Windows-1252 / code pages)

### Xử lý lỗi & Độ bền
- Fallback nhiều lớp đọc Excel
- Chặn file rỗng / định dạng không hợp lệ với thông báo thân thiện
- Không crash nếu thiếu trường trong JSON CTĐT (bỏ qua an toàn)

## Lưu ý bảo trì

- Khi thêm CTĐT mới: đặt vào `ProgramJson/<năm>/...json`
- Hiện tại hỗ trợ niên khóa: **2022**, **2023**
- Giữ đồng nhất `code` (UPPERCASE không bắt buộc nhưng parser chuẩn hóa)
- Tránh đổi format JSON trừ khi cập nhật logic parse
- Khi thêm niên khóa mới, cần cập nhật object `programs` trong `wwwroot/js/app.js`

## Công nghệ sử dụng

- **Backend**: ASP.NET Core 9.0, Minimal API
- **Frontend**: Vanilla JavaScript, HTML5, CSS3
- **AI**: Groq API (Llama 3.1 70B)
- **Excel**: EPPlus + ExcelDataReader
- **Hosting**: Render.com với Docker

## Giấy phép

Dự án mục đích học tập 

**Phát triển bởi**: Nhóm phát triển Hutech-StudyMate