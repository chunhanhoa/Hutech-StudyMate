# Hutech-StudyMate – Phân tích bảng điểm & CTĐT + AI

Chờ xíu xiu cho trang load nha :> | Bạn có thể truy cập dự án tại đây nè: <a href="https://hutech-studymate.onrender.com/" target="_blank">Hutech-StudyMate</a>

## Giới thiệu

**Hutech-StudyMate** là ứng dụng web toàn diện giúp sinh viên HUTECH quản lý lộ trình học tập hiệu quả. Ứng dụng cho phép tải lên bảng điểm (Excel), tự động phân tích tiến độ dựa trên Chương Trình Đào Tạo (CTĐT), và cung cấp trợ lý AI thông minh để tư vấn học tập.

Phiên bản mới nhất đã được nâng cấp với giao diện hiện đại, tích hợp hệ thống tài khoản, sổ tay điện tử và khả năng phân tích chuyên sâu hơn.

Ứng dụng được xây dựng trên nền tảng **ASP.NET Core 9.0 (Minimal API)** kết hợp với **Vanilla JS** và **MongoDB**.

## Tính năng chính

### Trợ lý AI học tập (Groq AI)
- **Tư vấn cá nhân hóa**: Phân tích dữ liệu điểm số để đưa ra lời khuyên cụ thể.
- **Gợi ý đăng ký môn**: Đề xuất các môn học tiếp theo dựa trên tiên quyết và lộ trình.
- **Chiến lược cải thiện GPA**: Tư vấn cách nâng cao điểm số hiệu quả.
- **Chat thời gian thực**: Tương tác mượt mà với tốc độ phản hồi nhanh từ Llama 3.1 70B.

### Phân tích bảng điểm chuyên sâu
- **Upload linh hoạt**: Hỗ trợ kéo thả file `.xlsx`, `.xls`.
- **Parser đa tầng**: Xử lý tốt các định dạng file cũ và mới, tự động fallback khi gặp lỗi.
- **Nhận diện thông minh**: Tự động khớp mã môn học (VD: CMP1074, COS120) với CTĐT.

### Quản lý Chương trình đào tạo (CTĐT)
- **Dữ liệu động**: Tải CTĐT theo Niên khóa và Khoa/Viện.
- **Phân loại môn học**:
  - Môn tích lũy / Không tích lũy.
  - Môn bắt buộc / Tự chọn.
  - Môn ngoài CTĐT.
- **Bộ lọc nâng cao**: Lọc theo Chuyên ngành, Khối tự chọn (4 môn thay thế, Đồ án tốt nghiệp...).

### Thống kê & Báo cáo
- **Trực quan hóa**: Bảng kết quả phân trang, màu sắc trạng thái rõ ràng (✓ Đạt, ⚠ Cảnh báo, ✗ Rớt/Chưa học).
- **Tính toán điểm**: GPA hệ 4, điểm trung bình hệ 10 có trọng số.
- **Tổng hợp tín chỉ**: Theo dõi sát sao số tín chỉ đã đạt và còn thiếu.

### Hệ thống tài khoản & Tiện ích
- **Đăng nhập/Đăng ký**: Hỗ trợ đăng nhập nhanh qua **Google** hoặc tài khoản thường.
- **Sổ tay điện tử**: Tra cứu nhanh các quy chế, quy định và hướng dẫn học vụ.
- **Giao diện hiện đại**: Thiết kế Responsive, tối ưu cho cả Mobile và Desktop.

## Công nghệ sử dụng

- **Backend**: ASP.NET Core 9.0, Minimal API
- **Frontend**: Vanilla JavaScript, HTML5, CSS3 (Inter font, Modern UI)
- **Database**: MongoDB (Lưu trữ người dùng, dữ liệu ứng dụng)
- **AI**: Groq API (Model Llama 3.1 70B)
- **Excel Processing**: EPPlus + ExcelDataReader
- **Authentication**: Cookie Auth + Google OAuth 2.0
- **Deployment**: Docker + Render.com

## Lưu ý bảo trì

- **Dữ liệu CTĐT**: Các file JSON CTĐT nằm trong thư mục `ProgramJson/<năm>/`.
- **Niên khóa hỗ trợ**: Hiện tại hỗ trợ đầy đủ các niên khóa từ 2022 đến nay.
- **Cập nhật CTĐT**: Khi thêm mới, đảm bảo giữ đúng cấu trúc JSON để parser hoạt động chính xác.

## Giấy phép

Dự án được phát triển với mục đích học tập và phục vụ cộng đồng sinh viên HUTECH.

**Phát triển bởi**: Nhóm phát triển Hutech-StudyMate