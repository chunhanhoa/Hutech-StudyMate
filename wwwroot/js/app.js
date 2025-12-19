const form = document.getElementById('uploadForm');
const fileInput = document.getElementById('file');
const mssvInput = document.getElementById('mssv');
const statusEl = document.getElementById('status');
const summary = document.getElementById('summary');
const gradesWrapper = document.getElementById('gradesWrapper');
const gradesTableBody = document.querySelector('#gradesTable tbody');
const dropZone = document.getElementById('dropZone');
const btnUpload = document.getElementById('btnUpload');
const dropZoneText = document.getElementById('dropZoneText');
const yearSelect = document.getElementById('yearSelect');
const deptSelect = document.getElementById('deptSelect');
const paginationEl = document.getElementById('pagination');
const paginationInfoEl = document.getElementById('paginationInfo');
const resultsLayout = document.querySelector('.results-layout');
const uploadPanel = document.getElementById('uploadPanel');
const resultYearSelect = document.getElementById('resultYearSelect');
const resultDeptSelect = document.getElementById('resultDeptSelect');
const btnNewAnalysis = document.getElementById('btnNewAnalysis');
const filterStatus = document.getElementById('filterStatus');
const filterElective = document.getElementById('filterElective');
const filterMajor = document.getElementById('filterMajor');

const pageSize = 10;
let busy = false;
let currentProgram = null;
let lastResult = null;
let currentPage = 1;
let gradesData = [];

const programs = {
  "2022": [
    { key: "an-toan-thong-tin-2022", department: "An toàn thông tin", file: "/ProgramJson/2022/An-toan-thong-tin-2022.json" },
    { key: "chan-nuoi-2022", department: "Chăn nuôi", file: "/ProgramJson/2022/Chan-nuoi-2022.json" },
    { key: "cong-nghe-det-may-2022", department: "Công nghệ dệt, may", file: "/ProgramJson/2022/Cong-nghe-det-may-2022.json" },
    { key: "cong-nghe-dien-anh-truyen-hinh-2022", department: "Công nghệ điện ảnh - truyền hình", file: "/ProgramJson/2022/Cong-nghe-dien-anh-truyen-hinh-2022.json" },
    { key: "cong-nghe-sinh-hoc-2022", department: "Công nghệ sinh học", file: "/ProgramJson/2022/Cong-nghe-sinh-hoc-2022.json" },
    { key: "cong-nghe-thong-tin-2022", department: "Công nghệ thông tin", file: "/ProgramJson/2022/Cong-nghe-thong-tin-2022.json" },
    { key: "cong-nghe-thuc-pham-2022", department: "Công nghệ thực phẩm", file: "/ProgramJson/2022/Cong-nghe-thuc-pham-2022.json" },
    { key: "dieu-duong-2022", department: "Điều dưỡng", file: "/ProgramJson/2022/Dieu-duong-2022.json" },
    { key: "digital-marketing-2022", department: "Digital Marketing", file: "/ProgramJson/2022/Digital-marketing-2022.json" },
    { key: "dinh-duong-khoa-hoc-thuc-pham-2022", department: "Dinh dưỡng khoa học thực phẩm", file: "/ProgramJson/2022/Dinh-duong-khoa-hoc-thuc-pham-2022.json" },
    { key: "dong-phuong-hoc-2022", department: "Đông phương học", file: "/ProgramJson/2022/Dong-phuong-hoc-2022.json" },
    { key: "duoc-hoc-2022", department: "Dược học", file: "/ProgramJson/2022/Duoc-hoc-2022.json" },
    { key: "he-thong-thong-tin-quan-ly-2022", department: "Hệ thống thông tin quản lý", file: "/ProgramJson/2022/He-thong-thong-tin-quan-ly-2022.json" },
    { key: "ke-toan-2022", department: "Kế toán", file: "/ProgramJson/2022/Ke-toan-2022.json" },
    { key: "khoa-hoc-du-lieu-2022", department: "Khoa học dữ liệu", file: "/ProgramJson/2022/Khoa-hoc-du-lieu-2022.json" },
    { key: "kien-truc-2022", department: "Kiến trúc", file: "/ProgramJson/2022/Kien-truc-2022.json" },
    { key: "kinh-doanh-quoc-te-2022", department: "Kinh doanh quốc tế", file: "/ProgramJson/2022/Kinh-doanh-quoc-te-2022.json" },
    { key: "kinh-doanh-thuong-mai-2022", department: "Kinh doanh thương mại", file: "/ProgramJson/2022/Kinh-doanh-thuong-mai-2022.json" },
    { key: "kinh-te-quoc-te-2022", department: "Kinh tế quốc tế", file: "/ProgramJson/2022/Kinh-te-quoc-te-2022.json" },
    { key: "ky-thuat-co-dien-tu-2022", department: "Kỹ thuật cơ điện tử", file: "/ProgramJson/2022/Ky-thuat-co-dien-tu-2022.json" },
    { key: "ky-thuat-co-khi-2022", department: "Kỹ thuật cơ khí", file: "/ProgramJson/2022/Ky-thuat-co-khi-2022.json" },
    { key: "ky-thuat-dien-2022", department: "Kỹ thuật điện", file: "/ProgramJson/2022/Ky-thuat-dien-2022.json" },
    { key: "ky-thuat-dien-tu-vien-thong-2022", department: "Kỹ thuật điện tử - viễn thông", file: "/ProgramJson/2022/Ky-thuat-dien-tu-vien-thong-2022.json" },
    { key: "ky-thuat-moi-truong-2022", department: "Kỹ thuật môi trường", file: "/ProgramJson/2022/Ky-thuat-moi-truong-2022.json" },
    { key: "ky-thuat-o-to-2022", department: "Kỹ thuật ô tô", file: "/ProgramJson/2022/Ky-thuat-o-to-2022.json" },
    { key: "ky-thuat-xay-dung-2022", department: "Kỹ thuật xây dựng", file: "/ProgramJson/2022/Ky-thuat-xay-dung-2022.json" },
    { key: "ky-thuat-xet-nghiem-y-hoc-2022", department: "Kỹ thuật xét nghiệm y học", file: "/ProgramJson/2022/Ky-thuat-xet-nghiem-y-hoc-2022.json" },
    { key: "ky-thuat-y-sinh-2022", department: "Kỹ thuật y sinh", file: "/ProgramJson/2022/Ky-thuat-y-sinh-2022.json" },
    { key: "logistic-va-quan-ly-cung-ung-2022", department: "Logistic và quản lý cung ứng", file: "/ProgramJson/2022/Logistic-va-quan-ly-cung-ung-2022.json" },
    { key: "luat-2022", department: "Luật", file: "/ProgramJson/2022/Luat-2022.json" },
    { key: "luat-kinh-te-2022", department: "Luật kinh tế", file: "/ProgramJson/2022/Luat-kinh-te-2022.json" },
    { key: "marketing-2022", department: "Marketing", file: "/ProgramJson/2022/Marketing-2022.json" },
    { key: "nghe-thuat-so-2022", department: "Nghệ thuật số", file: "/ProgramJson/2022/Nghe-thuat-so-2022.json" },
    { key: "ngon-ngu-anh-2022", department: "Ngôn ngữ Anh", file: "/ProgramJson/2022/Ngon-ngu-anh-2022.json" },
    { key: "ngon-ngu-han-2022", department: "Ngôn ngữ Hàn", file: "/ProgramJson/2022/Ngon-ngu-han-2022.json" },
    { key: "ngon-ngu-nhat-2022", department: "Ngôn ngữ Nhật", file: "/ProgramJson/2022/Ngon-ngu-nhat-2022.json" },
    { key: "ngon-ngu-trung-quoc-2022", department: "Ngôn ngữ Trung Quốc", file: "/ProgramJson/2022/Ngon-ngu-trung-quoc-2022.json" },
    { key: "quan-he-cong-chung-2022", department: "Quan hệ công chúng", file: "/ProgramJson/2022/Quan-he-cong-chung-2022.json" },
    { key: "quan-he-quoc-te-2022", department: "Quan hệ quốc tế", file: "/ProgramJson/2022/Quan-he-quoc-te-2022.json" },
    { key: "quan-ly-tai-nguyen-moi-truong-2022", department: "Quản lý tài nguyên môi trường", file: "/ProgramJson/2022/Quan-ly-tai-nguyen-moi-truong-2022.json" },
    { key: "quan-ly-xay-dung-2022", department: "Quản lý xây dựng", file: "/ProgramJson/2022/Quan-ly-xay-dung-2022.json" },
    { key: "quan-tri-dich-vu-du-lich-va-lu-hanh-2022", department: "Quản trị dịch vụ du lịch và lữ hành", file: "/ProgramJson/2022/Quan-tri-dich-vu-du-lich-va-lu-hanh-2022.json" },
    { key: "quan-tri-khach-san-2022", department: "Quản trị khách sạn", file: "/ProgramJson/2022/Quan-tri-khach-san-2022.json" },
    { key: "quan-tri-kinh-doanh-2022", department: "Quản trị kinh doanh", file: "/ProgramJson/2022/Quan-tri-kinh-doanh-2022.json" },
    { key: "quan-tri-nha-hang-va-dich-vu-an-uong-2022", department: "Quản trị nhà hàng và dịch vụ ăn uống", file: "/ProgramJson/2022/Quan-tri-nha-hang-va-dich-vu-an-uong-2022.json" },
    { key: "quan-tri-nhan-luc-2022", department: "Quản trị nhân lực", file: "/ProgramJson/2022/Quan-tri-nhan-luc-2022.json" },
    { key: "quan-tri-su-kien-2022", department: "Quản trị sự kiện", file: "/ProgramJson/2022/Quan-tri-su-kien-2022.json" },
    { key: "robot-va-tri-tue-nhan-tao-2022", department: "Robot và trí tuệ nhân tạo", file: "/ProgramJson/2022/Robot-va-tri-tue-nhan-tao-2022.json" },
    { key: "tai-chinh-ngan-hang-2022", department: "Tài chính ngân hàng", file: "/ProgramJson/2022/Tai-chinh-ngan-hang-2022.json" },
    { key: "tai-chinh-quoc-te-2022", department: "Tài chính quốc tế", file: "/ProgramJson/2022/Tai-chinh-quoc-te-2022.json" },
    { key: "tam-ly-hoc-2022", department: "Tâm lý học", file: "/ProgramJson/2022/Tam-ly-hoc-2022.json" },
    { key: "thanh-nhac-2022", department: "Thanh nhạc", file: "/ProgramJson/2022/Thanh-nhac-2022.json" },
    { key: "thiet-ke-do-hoa-2022", department: "Thiết kế đồ họa", file: "/ProgramJson/2022/Thiet-ke-do-hoa-2022.json" },
    { key: "thiet-ke-noi-that-2022", department: "Thiết kế nội thất", file: "/ProgramJson/2022/Thiet-ke-noi-that-2022.json" },
    { key: "thiet-ke-thoi-trang-2022", department: "Thiết kế thời trang", file: "/ProgramJson/2022/Thiet-ke-thoi-trang-2022.json" },
    { key: "thu-y-2022", department: "Thú y", file: "/ProgramJson/2022/Thu-y-2022.json" },
    { key: "thuong-mai-dien-tu-2022", department: "Thương mại điện tử", file: "/ProgramJson/2022/Thuong-mai-dien-tu-2022.json" },
    { key: "truyen-thong-da-phuong-tien-2022", department: "Truyền thông đa phương tiện", file: "/ProgramJson/2022/Truyen-thong-da-phuong-tien-2022.json" },
    { key: "tu-dong-hoa-2022", department: "Tự động hóa", file: "/ProgramJson/2022/Tu-dong-hoa-2022.json" }
  ],
  "2023": [
    { key: "an-toan-thong-tin-2023", department: "An toàn thông tin", file: "/ProgramJson/2023/An-toan-thong-tin-2023.json" },
    { key: "bat-dong-san-2023", department: "Bất động sản", file: "/ProgramJson/2023/Bat-dong-san-2023.json" },
    { key: "cong-nghe-det-may-2023", department: "Công nghệ dệt, may", file: "/ProgramJson/2023/Cong-nghe-det-may-2023.json" },
    { key: "cong-nghe-dien-anh-truyen-hinh-2023", department: "Công nghệ điện ảnh - truyền hình", file: "/ProgramJson/2023/Cong-nghe-dien-anh-truyen-hinh-2023.json" },
    { key: "cong-nghe-o-to-dien-2023", department: "Công nghệ ô tô điện", file: "/ProgramJson/2023/Cong-nghe-o-to-dien-2023.json" },
    { key: "cong-nghe-sinh-hoc-2023", department: "Công nghệ sinh học", file: "/ProgramJson/2023/Cong-nghe-sinh-hoc-2023.json" },
    { key: "cong-nghe-thong-tin-2023", department: "Công nghệ thông tin", file: "/ProgramJson/2023/Cong-nghe-thong-tin-2023.json" },
    { key: "cong-nghe-thuc-pham-2023", department: "Công nghệ thực phẩm", file: "/ProgramJson/2023/Cong-nghe-thuc-pham-2023.json" },
    { key: "dieu-duong-2023", department: "Điều dưỡng", file: "/ProgramJson/2023/Dieu-duong-2023.json" },
    { key: "digital-marketing-2023", department: "Digital Marketing", file: "/ProgramJson/2023/Digital-marketing-2023.json" },
    { key: "dong-phuong-hoc-2023", department: "Đông phương học", file: "/ProgramJson/2023/Dong-phuong-hoc-2023.json" },
    { key: "duoc-hoc-2023", department: "Dược học", file: "/ProgramJson/2023/Duoc-hoc-2023.json" },
    { key: "he-thong-thong-tin-quan-ly-2023", department: "Hệ thống thông tin quản lý", file: "/ProgramJson/2023/He-thong-thong-tin-quan-ly-2023.json" },
    { key: "ke-toan-2023", department: "Kế toán", file: "/ProgramJson/2023/Ke-toan-2023.json" },
    { key: "khoa-hoc-du-lieu-2023", department: "Khoa học dữ liệu", file: "/ProgramJson/2023/Khoa-hoc-du-lieu-2023.json" },
    { key: "kien-truc-2023", department: "Kiến trúc", file: "/ProgramJson/2023/Kien-truc-2023.json" },
    { key: "kinh-doanh-quoc-te-2023", department: "Kinh doanh quốc tế", file: "/ProgramJson/2023/Kinh-doanh-quoc-te-2023.json" },
    { key: "kinh-doanh-thuong-mai-2023", department: "Kinh doanh thương mại", file: "/ProgramJson/2023/Kinh-doanh-thuong-mai-2023.json" },
    { key: "kinh-te-quoc-te-2023", department: "Kinh tế quốc tế", file: "/ProgramJson/2023/Kinh-te-quoc-te-2023.json" },
    { key: "ky-thuat-co-dien-tu-2023", department: "Kỹ thuật cơ điện tử", file: "/ProgramJson/2023/Ky-thuat-co-dien-tu-2023.json" },
    { key: "ky-thuat-co-khi-2023", department: "Kỹ thuật cơ khí", file: "/ProgramJson/2023/Ky-thuat-co-khi-2023.json" },
    { key: "ky-thuat-dien-2023", department: "Kỹ thuật điện", file: "/ProgramJson/2023/Ky-thuat-dien-2023.json" },
    { key: "ky-thuat-dien-tu-vien-thong-2023", department: "Kỹ thuật điện tử - viễn thông", file: "/ProgramJson/2023/Ky-thuat-dien-tu-vien-thong-2023.json" },
    { key: "ky-thuat-dieu-khien-va-tu-dong-hoa-2023", department: "Kỹ thuật điều khiển và tự động hóa", file: "/ProgramJson/2023/Ky-thuat-dieu-khien-va-tu-dong-hoa-2023.json" },
    { key: "ky-thuat-o-to-2023", department: "Kỹ thuật ô tô", file: "/ProgramJson/2023/Ky-thuat-o-to-2023.json" },
    { key: "ky-thuat-xay-dung-2023", department: "Kỹ thuật xây dựng", file: "/ProgramJson/2023/Ky-thuat-xay-dung-2023.json" },
    { key: "ky-thuat-xet-nghiem-y-hoc-2023", department: "Kỹ thuật xét nghiệm y học", file: "/ProgramJson/2023/Ky-thuat-xet-nghiem-y-hoc-2023.json" },
    { key: "logistic-va-quan-ly-chuoi-cung-ung-2023", department: "Logistic và quản lý chuỗi cung ứng", file: "/ProgramJson/2023/Logistic-va-quan-ly-chuoi-cung-ung-2023.json" },
    { key: "luat-2023", department: "Luật", file: "/ProgramJson/2023/Luat-2023.json" },
    { key: "luat-kinh-te-2023", department: "Luật kinh tế", file: "/ProgramJson/2023/Luat-kinh-te-2023.json" },
    { key: "luat-thuong-mai-quoc-te-2023", department: "Luật thương mại quốc tế", file: "/ProgramJson/2023/Luat-thuong-mai-quoc-te-2023.json" },
    { key: "marketing-2023", department: "Marketing", file: "/ProgramJson/2023/Marketing-2023.json" },
    { key: "nghe-thuat-so-2023", department: "Nghệ thuật số", file: "/ProgramJson/2023/Nghe-thuat-so-2023.json" },
    { key: "ngon-ngu-anh-2023", department: "Ngôn ngữ Anh", file: "/ProgramJson/2023/Ngon-ngu-anh-2023.json" },
    { key: "ngon-ngu-han-2023", department: "Ngôn ngữ Hàn", file: "/ProgramJson/2023/Ngon-ngu-han-2023.json" },
    { key: "ngon-ngu-nhat-2023", department: "Ngôn ngữ Nhật", file: "/ProgramJson/2023/Ngon-ngu-nhat-2023.json" },
    { key: "ngon-ngu-trung-quoc-2023", department: "Ngôn ngữ Trung Quốc", file: "/ProgramJson/2023/Ngon-ngu-trung-quoc-2023.json" },
    { key: "quan-he-cong-chung-2023", department: "Quan hệ công chúng", file: "/ProgramJson/2023/Quan-he-cong-chung-2023.json" },
    { key: "quan-he-quoc-te-2023", department: "Quan hệ quốc tế", file: "/ProgramJson/2023/Quan-he-quoc-te-2023.json" },
    { key: "quan-ly-tai-nguyen-moi-truong-2023", department: "Quản lý tài nguyên môi trường", file: "/ProgramJson/2023/Quan-ly-tai-nguyen-moi-truong-2023.json" },
    { key: "quan-ly-the-duc-the-thao-2023", department: "Quản lý thể dục thể thao", file: "/ProgramJson/2023/Quan-ly-the-duc-the-thao-2023.json" },
    { key: "quan-ly-xay-dung-2023", department: "Quản lý xây dựng", file: "/ProgramJson/2023/Quan-ly-xay-dung-2023.json" },
    { key: "quan-tri-dich-vu-du-lich-va-lu-hanh-2023", department: "Quản trị dịch vụ du lịch và lữ hành", file: "/ProgramJson/2023/Quan-tri-dich-vu-du-lich-va-lu-hanh-2023.json" },
    { key: "quan-tri-khach-san-2023", department: "Quản trị khách sạn", file: "/ProgramJson/2023/Quan-tri-khach-san-2023.json" },
    { key: "quan-tri-kinh-doanh-2023", department: "Quản trị kinh doanh", file: "/ProgramJson/2023/Quan-tri-kinh-doanh-2023.json" },
    { key: "quan-tri-nha-hang-va-dich-vu-an-uong-2023", department: "Quản trị nhà hàng và dịch vụ ăn uống", file: "/ProgramJson/2023/Quan-tri-nha-hang-va-dich-vu-an-uong-2023.json" },
    { key: "quan-tri-nhan-luc-2023", department: "Quản trị nhân lực", file: "/ProgramJson/2023/Quan-tri-nhan-luc-2023.json" },
    { key: "quan-tri-su-kien-2023", department: "Quản trị sự kiện", file: "/ProgramJson/2023/Quan-tri-su-kien-2023.json" },
    { key: "robot-va-tri-tue-nhan-tao-2023", department: "Robot và trí tuệ nhân tạo", file: "/ProgramJson/2023/Robot-va-tri-tue-nhan-tao-2023.json" },
    { key: "tai-chinh-ngan-hang-2023", department: "Tài chính ngân hàng", file: "/ProgramJson/2023/Tai-chinh-ngan-hang-2023.json" },
    { key: "tai-chinh-quoc-te-2023", department: "Tài chính quốc tế", file: "/ProgramJson/2023/Tai-chinh-quoc-te-2023.json" },
    { key: "tam-ly-hoc-2023", department: "Tâm lý học", file: "/ProgramJson/2023/Tam-ly-hoc-2023.json" },
    { key: "thanh-nhac-2023", department: "Thanh nhạc", file: "/ProgramJson/2023/Thanh-nhac-2023.json" },
    { key: "thiet-ke-do-hoa-2023", department: "Thiết kế đồ họa", file: "/ProgramJson/2023/Thiet-ke-do-hoa-2023.json" },
    { key: "thiet-ke-noi-that-2023", department: "Thiết kế nội thất", file: "/ProgramJson/2023/Thiet-ke-noi-that-2023.json" },
    { key: "thiet-ke-thoi-trang-2023", department: "Thiết kế thời trang", file: "/ProgramJson/2023/Thiet-ke-thoi-trang-2023.json" },
    { key: "thu-y-2023", department: "Thú y", file: "/ProgramJson/2023/Thu-y-2023.json" },
    { key: "thuong-mai-dien-tu-2023", department: "Thương mại điện tử", file: "/ProgramJson/2023/Thuong-mai-dien-tu-2023.json" },
    { key: "truyen-thong-da-phuong-tien-2023", department: "Truyền thông đa phương tiện", file: "/ProgramJson/2023/Truyen-thong-da-phuong-tien-2023.json" },
  ],
  "2024": [
    { key: "an-toan-thong-tin-2024", department: "An toàn thông tin", file: "/ProgramJson/2024/An-toan-thong-tin-2024.json" },
    { key: "bat-dong-san-2024", department: "Bất động sản", file: "/ProgramJson/2024/Bat-dong-san-2024.json" },
    { key: "cong-nghe-dien-anh-truyen-hinh-2024", department: "Công nghệ điện ảnh - truyền hình", file: "/ProgramJson/2024/Cong-nghe-dien-anh-truyen-hinh-2024.json" },
    { key: "cong-nghe-o-to-dien-2024", department: "Công nghệ ô tô điện", file: "/ProgramJson/2024/Cong-nghe-o-to-dien-2024.json" },
    { key: "cong-nghe-sinh-hoc-2024", department: "Công nghệ sinh học", file: "/ProgramJson/2024/Cong-nghe-sinh-hoc-2024.json" },
    { key: "cong-nghe-tham-my-2024", department: "Công nghệ thẩm mỹ", file: "/ProgramJson/2024/Cong-nghe-tham-my-2024.json" },
    { key: "cong-nghe-thong-tin-2024", department: "Công nghệ thông tin", file: "/ProgramJson/2024/Cong-nghe-thong-tin-2024.json" },
    { key: "cong-nghe-thuc-pham-2024", department: "Công nghệ thực phẩm", file: "/ProgramJson/2024/Cong-nghe-thuc-pham-2024.json" },
    { key: "dieu-duong-2024", department: "Điều dưỡng", file: "/ProgramJson/2024/Dieu-duong-2024.json" },
    { key: "digital-marketing-2024", department: "Digital Marketing", file: "/ProgramJson/2024/Digital-marketing-2024.json" },
    { key: "dong-phuong-hoc-2024", department: "Đông phương học", file: "/ProgramJson/2024/Dong-phuong-hoc-2024.json" },
    { key: "duoc-hoc-2024", department: "Dược học", file: "/ProgramJson/2024/Duoc-hoc-2024.json" },
    { key: "he-thong-thong-tin-quan-ly-2024", department: "Hệ thống thông tin quản lý", file: "/ProgramJson/2024/He-thong-thong-tin-quan-ly-2024.json" },
    { key: "ke-toan-2024", department: "Kế toán", file: "/ProgramJson/2024/Ke-toan-2024.json" },
    { key: "khoa-hoc-du-lieu-2024", department: "Khoa học dữ liệu", file: "/ProgramJson/2024/Khoa-hoc-du-lieu-2024.json" },
    { key: "khoa-hoc-may-tinh-2024", department: "Khoa học máy tính", file: "/ProgramJson/2024/Khoa-hoc-may-tinh-2024.json" },
    { key: "kien-truc-2024", department: "Kiến trúc", file: "/ProgramJson/2024/Kien-truc-2024.json" },
    { key: "kinh-doanh-quoc-te-2024", department: "Kinh doanh quốc tế", file: "/ProgramJson/2024/Kinh-doanh-quoc-te-2024.json" },
    { key: "kinh-doanh-thuong-mai-2024", department: "Kinh doanh thương mại", file: "/ProgramJson/2024/Kinh-doanh-thuong-mai-2024.json" },
    { key: "kinh-te-quoc-te-2024", department: "Kinh tế quốc tế", file: "/ProgramJson/2024/Kinh-te-quoc-te-2024.json" },
    { key: "ky-thuat-co-dien-tu-2024", department: "Kỹ thuật cơ điện tử", file: "/ProgramJson/2024/Ky-thuat-co-dien-tu-2024.json" },
    { key: "ky-thuat-co-khi-2024", department: "Kỹ thuật cơ khí", file: "/ProgramJson/2024/Ky-thuat-co-khi-2024.json" },
    { key: "ky-thuat-dien-2024", department: "Kỹ thuật điện", file: "/ProgramJson/2024/Ky-thuat-dien-2024.json" },
    { key: "ky-thuat-dien-tu-vien-thong-2024", department: "Kỹ thuật điện tử - viễn thông", file: "/ProgramJson/2024/Ky-thuat-dien-tu-vien-thong-2024.json" },
    { key: "ky-thuat-dieu-khien-va-tu-dong-hoa-2024", department: "Kỹ thuật điều khiển và tự động hóa", file: "/ProgramJson/2024/Ky-thuat-dieu-khien-va-tu-dong-hoa-2024.json" },
    { key: "ky-thuat-may-tinh-2024", department: "Kỹ thuật máy tính", file: "/ProgramJson/2024/Ky-thuat-may-tinh-2024.json" },
    { key: "ky-thuat-nhiet-2024", department: "Kỹ thuật nhiệt", file: "/ProgramJson/2024/Ky-thuat-nhiet-2024.json" },
    { key: "ky-thuat-o-to-2024", department: "Kỹ thuật ô tô", file: "/ProgramJson/2024/Ky-thuat-o-to-2024.json" },
    { key: "ky-thuat-xay-dung-2024", department: "Kỹ thuật xây dựng", file: "/ProgramJson/2024/Ky-thuat-xay-dung-2024.json" },
    { key: "ky-thuat-xet-nghiem-y-hoc-2024", department: "Kỹ thuật xét nghiệm y học", file: "/ProgramJson/2024/Ky-thuat-xet-nghiem-y-hoc-2024.json" },
    { key: "logistic-va-quan-ly-chuoi-cung-ung-2024", department: "Logistic và quản lý chuỗi cung ứng", file: "/ProgramJson/2024/Logistic-va-quan-ly-chuoi-cung-ung-2024.json" },
    { key: "luat-2024", department: "Luật", file: "/ProgramJson/2024/Luat-2024.json" },
    { key: "luat-kinh-te-2024", department: "Luật kinh tế", file: "/ProgramJson/2024/Luat-kinh-te-2024.json" },
    { key: "luat-thuong-mai-quoc-te-2024", department: "Luật thương mại quốc tế", file: "/ProgramJson/2024/Luat-thuong-mai-quoc-te-2024.json" },
    { key: "marketing-2024", department: "Marketing", file: "/ProgramJson/2024/Marketing-2024.json" },
    { key: "nghe-thuat-so-2024", department: "Nghệ thuật số", file: "/ProgramJson/2024/Nghe-thuat-so-2024.json" },
    { key: "ngon-ngu-anh-2024", department: "Ngôn ngữ Anh", file: "/ProgramJson/2024/Ngon-ngu-anh-2024.json" },
    { key: "ngon-ngu-han-2024", department: "Ngôn ngữ Hàn", file: "/ProgramJson/2024/Ngon-ngu-han-2024.json" },
    { key: "ngon-ngu-nhat-2024", department: "Ngôn ngữ Nhật", file: "/ProgramJson/2024/Ngon-ngu-nhat-2024.json" },
    { key: "ngon-ngu-trung-quoc-2024", department: "Ngôn ngữ Trung Quốc", file: "/ProgramJson/2024/Ngon-ngu-trung-quoc-2024.json" },
    { key: "quan-he-cong-chung-2024", department: "Quan hệ công chúng", file: "/ProgramJson/2024/Quan-he-cong-chung-2024.json" },
    { key: "quan-ly-tai-nguyen-moi-truong-2024", department: "Quản lý tài nguyên môi trường", file: "/ProgramJson/2024/Quan-ly-tai-nguyen-moi-truong-2024.json" },
    { key: "quan-ly-the-duc-the-thao-2024", department: "Quản lý thể dục thể thao", file: "/ProgramJson/2024/Quan-ly-the-duc-the-thao-2024.json" },
    { key: "quan-ly-xay-dung-2024", department: "Quản lý xây dựng", file: "/ProgramJson/2024/Quan-ly-xay-dung-2024.json" },
    { key: "quan-tri-dich-vu-du-lich-va-lu-hanh-2024", department: "Quản trị dịch vụ du lịch và lữ hành", file: "/ProgramJson/2024/Quan-tri-dich-vu-du-lich-va-lu-hanh-2024.json" },
    { key: "quan-tri-khach-san-2024", department: "Quản trị khách sạn", file: "/ProgramJson/2024/Quan-tri-khach-san-2024.json" },
    { key: "quan-tri-kinh-doanh-2024", department: "Quản trị kinh doanh", file: "/ProgramJson/2024/Quan-tri-kinh-doanh-2024.json" },
    { key: "quan-tri-nha-hang-va-dich-vu-an-uong-2024", department: "Quản trị nhà hàng và dịch vụ ăn uống", file: "/ProgramJson/2024/Quan-tri-nha-hang-va-dich-vu-an-uong-2024.json" },
    { key: "quan-tri-nhan-luc-2024", department: "Quản trị nhân lực", file: "/ProgramJson/2024/Quan-tri-nhan-luc-2024.json" },
    { key: "quan-tri-su-kien-2024", department: "Quản trị sự kiện", file: "/ProgramJson/2024/Quan-tri-su-kien-2024.json" },
    { key: "robot-va-tri-tue-nhan-tao-2024", department: "Robot và trí tuệ nhân tạo", file: "/ProgramJson/2024/Robot-va-tri-tue-nhan-tao-2024.json" },
    { key: "tai-chinh-ngan-hang-2024", department: "Tài chính ngân hàng", file: "/ProgramJson/2024/Tai-chinh-ngan-hang-2024.json" },
    { key: "tai-chinh-quoc-te-2024", department: "Tài chính quốc tế", file: "/ProgramJson/2024/Tai-chinh-quoc-te-2024.json" },
    { key: "tam-ly-hoc-2024", department: "Tâm lý học", file: "/ProgramJson/2024/Tam-ly-hoc-2024.json" },
    { key: "thanh-nhac-2024", department: "Thanh nhạc", file: "/ProgramJson/2024/Thanh-nhac-2024.json" },
    { key: "thiet-ke-do-hoa-2024", department: "Thiết kế đồ họa", file: "/ProgramJson/2024/Thiet-ke-do-hoa-2024.json" },
    { key: "thiet-ke-noi-that-2024", department: "Thiết kế nội thất", file: "/ProgramJson/2024/Thiet-ke-noi-that-2024.json" },
    { key: "thiet-ke-thoi-trang-2024", department: "Thiết kế thời trang", file: "/ProgramJson/2024/Thiet-ke-thoi-trang-2024.json" },
    { key: "thu-y-2024", department: "Thú y", file: "/ProgramJson/2024/Thu-y-2024.json" },
    { key: "thuong-mai-dien-tu-2024", department: "Thương mại điện tử", file: "/ProgramJson/2024/Thuong-mai-dien-tu-2024.json" },
    { key: "tri-tue-nhan-tao-2024", department: "Trí tuệ nhân tạo", file: "/ProgramJson/2024/Tri-tue-nhan-tao-2024.json" },
    { key: "truyen-thong-da-phuong-tien-2024", department: "Truyền thông đa phương tiện", file: "/ProgramJson/2024/Truyen-thong-da-phuong-tien-2024.json" },
  ]
};

function setStatus(text, cls) {
  statusEl.textContent = text;
  statusEl.className = 'status ' + (cls || '');
}

function clearResult() {
  summary.innerHTML = '';
  gradesTableBody.innerHTML = '';
  summary.classList.add('hidden');
  gradesWrapper.classList.add('hidden');

  // Ẩn chatbot AI khi xóa kết quả
  const aiChatSection = document.getElementById('aiChatSection');
  if (aiChatSection) {
    aiChatSection.classList.add('hidden');
  }

  // Ẩn dashboard và destroy chart
  const dashboardSection = document.getElementById('dashboardSection');
  if (dashboardSection) {
    dashboardSection.classList.add('hidden');
  }
  if (window.gpaChartInstance) {
    window.gpaChartInstance.destroy();
    window.gpaChartInstance = null;
  }

  resultsLayout.classList.add('hidden');
  uploadPanel.classList.remove('hidden');
  gradesData = [];
  currentPage = 1;
}

Object.keys(programs).sort().forEach(y => {
  const opt = document.createElement('option');
  opt.value = y;
  opt.textContent = y;
  yearSelect.appendChild(opt);

  const resultOpt = document.createElement('option');
  resultOpt.value = y;
  resultOpt.textContent = y;
  resultYearSelect.appendChild(resultOpt);
});

yearSelect.addEventListener('change', () => {
  deptSelect.innerHTML = '<option value="">-- Chọn khoa / viện --</option>';
  currentProgram = null;
  if (!yearSelect.value) {
    deptSelect.disabled = true;
    return;
  }
  const list = programs[yearSelect.value] || [];
  list.forEach(p => {
    const opt = document.createElement('option');
    opt.value = p.key;
    opt.textContent = p.department;
    deptSelect.appendChild(opt);
  });
  deptSelect.disabled = false;
});

deptSelect.addEventListener('change', async () => {
  currentProgram = null;
  const year = yearSelect.value;
  const key = deptSelect.value;
  if (!year || !key) return;
  const entry = (programs[year] || []).find(p => p.key === key);
  if (!entry) return;
  try {
    const res = await fetch(entry.file);
    if (!res.ok) throw new Error('Không tải được chương trình');
    const json = await res.json();

    const codeNameMap = {};
    const nonAccCodes = new Set();
    const allSubjects = [];
    const electiveSubjects = [];
    let electiveGroups = [];
    let electiveTotalCredits = 0;

    function addBaseSubjects(arr, isNonAcc = false) {
      if (!Array.isArray(arr)) return;
      arr.forEach(s => {
        if (s && s.code) {
          const up = String(s.code).toUpperCase();
          if (s.name) codeNameMap[up] = s.name;
          if (isNonAcc) nonAccCodes.add(up);
          allSubjects.push({
            code: up,
            name: s.name || '',
            credits: Number(s.credits) || 0
          });
        }
      });
    }

    if (Array.isArray(json.courses)) {
      json.courses.forEach(c => {
        const isNonAcc = (c.category || '').toLowerCase().includes('không tích lũy');
        const catIsElective = (c.category || '').toLowerCase().includes('tự chọn');

        if (Array.isArray(c.subjects)) {
          addBaseSubjects(c.subjects, isNonAcc);
        }

        if (Array.isArray(c.groups)) {
          c.groups.forEach(g => {
            if (Array.isArray(g.subjects)) {
              addBaseSubjects(g.subjects, isNonAcc);
            }
          });
        }

        if (Array.isArray(c.subcategories)) {
          c.subcategories.forEach(sc => {
            const scNonAcc = isNonAcc || (sc.name || '').toLowerCase().includes('không tích lũy');
            const scIsElective = (sc.name || '').toLowerCase().includes('tự chọn');

            if (Array.isArray(sc.subjects)) {
              addBaseSubjects(sc.subjects, scNonAcc);
            }

            if (scIsElective) {
              electiveTotalCredits = Number(sc.total_credits) || electiveTotalCredits;

              if (Array.isArray(sc.subjects)) {
                sc.subjects.forEach(s => {
                  if (s?.code) {
                    electiveSubjects.push({
                      code: String(s.code).toUpperCase(),
                      name: s.name || '',
                      credits: Number(s.credits) || 0
                    });
                  }
                });
              }
              if (Array.isArray(sc.groups)) {
                electiveGroups = sc.groups.map(g => ({
                  group_name: g.group_name || '',
                  subjects: Array.isArray(g.subjects) ? g.subjects.map(s => ({
                    code: String(s.code).toUpperCase(),
                    name: s.name || '',
                    credits: Number(s.credits) || 0
                  })) : []
                }));
                sc.groups.forEach(g => {
                  if (Array.isArray(g.subjects)) {
                    g.subjects.forEach(s => {
                      if (s?.code) {
                        electiveSubjects.push({
                          code: String(s.code).toUpperCase(),
                          name: s.name || '',
                          credits: Number(s.credits) || 0
                        });
                      }
                    });
                  }
                });
              }
            } else if (Array.isArray(sc.groups)) {
              sc.groups.forEach(g => {
                if (Array.isArray(g.subjects)) {
                  addBaseSubjects(g.subjects, scNonAcc);
                }
              });
            }
          });
        }

        if (catIsElective) {
          if (Array.isArray(c.subjects)) {
            c.subjects.forEach(s => {
              if (s?.code) {
                electiveSubjects.push({
                  code: String(s.code).toUpperCase(),
                  name: s.name || '',
                  credits: Number(s.credits) || 0
                });
              }
            });
          }
          if (Array.isArray(c.groups)) {
            electiveGroups = c.groups.map(g => ({
              group_name: g.group_name || '',
              subjects: Array.isArray(g.subjects) ? g.subjects.map(s => ({
                code: String(s.code).toUpperCase(),
                name: s.name || '',
                credits: Number(s.credits) || 0
              })) : []
            }));
            c.groups.forEach(g => {
              if (Array.isArray(g.subjects)) {
                g.subjects.forEach(s => {
                  if (s?.code) {
                    electiveSubjects.push({
                      code: String(s.code).toUpperCase(),
                      name: s.name || '',
                      credits: Number(s.credits) || 0
                    });
                  }
                });
              }
            });
          }
        }
      });
    }

    const seen = new Set();
    const dedupElectives = electiveSubjects.filter(s => {
      const key = s.code;
      if (seen.has(key)) return false;
      seen.add(key);
      return true;
    });
    const dedupAll = allSubjects.filter(s => {
      const key = s.code;
      if (seen.has(key)) return false;
      seen.add(key);
      return true;
    });

    currentProgram = {
      year: json.academic_year || year,
      department: json.department || entry.department,
      programCode: json.program_code || '',
      totalCredits: json.total_credits || '',
      nonAccTotal: json.non_accumulated_credits || '',
      codeNameMap,
      nonAccCodes,
      allSubjects: dedupAll,
      electiveSubjects: dedupElectives,
      electiveGroups,
      electiveTotalCredits: electiveTotalCredits || 12,
      key: entry.key,
      courses: json.courses || []
    };
    updateMajorOptions();
    setStatus('Đã tải chương trình: ' + currentProgram.department, 'ok');
  } catch (e) {
    setStatus('Lỗi tải chương trình: ' + e.message, 'error');
  }
});

dropZone.addEventListener('dragenter', e => {
  e.preventDefault();
  e.stopPropagation();
  dropZone.classList.add('dragging');
});

dropZone.addEventListener('dragover', e => {
  e.preventDefault();
  e.stopPropagation();
});

dropZone.addEventListener('dragleave', e => {
  e.preventDefault();
  e.stopPropagation();
  dropZone.classList.remove('dragging');
});

dropZone.addEventListener('drop', e => {
  e.preventDefault();
  e.stopPropagation();
  dropZone.classList.remove('dragging');
  const files = e.dataTransfer.files;
  if (files && files.length) {
    fileInput.files = files;
    if (dropZoneText) dropZoneText.textContent = 'Đã chọn: ' + files[0].name;
  }
});

fileInput.addEventListener('change', () => {
  if (fileInput.files.length && dropZoneText) {
    dropZoneText.textContent = 'Đã chọn: ' + fileInput.files[0].name;
  }
});

form.addEventListener('reset', () => {
  setStatus('Đã xóa dữ liệu.', '');
  clearResult();
  if (dropZoneText) dropZoneText.textContent = 'Chọn hoặc kéo thả file vào đây';
});

form.addEventListener('submit', async (e) => {
  e.preventDefault();
  if (busy) return;
  if (!fileInput.files.length) { setStatus('Chưa có file.', 'error'); return; }
  if (!mssvInput.value.trim()) { setStatus('Chưa nhập MSSV.', 'error'); return; }
  if (!yearSelect.value) { setStatus('Chưa chọn niên khóa.', 'error'); return; }
  if (!deptSelect.value) { setStatus('Chưa chọn khoa / viện.', 'error'); return; }
  const file = fileInput.files[0];
  const lower = file.name.toLowerCase();
  if (!(lower.endsWith('.xlsx') || lower.endsWith('.xls'))) {
    setStatus('File phải là .xlsx hoặc .xls', 'error'); return;
  }

  clearResult();
  setStatus('Đang tải lên & phân tích...', 'loading');
  busy = true;
  const originalText = btnUpload.textContent;
  btnUpload.disabled = true;
  btnUpload.textContent = btnUpload.getAttribute('data-busy-text') || 'Đang xử lý...';

  const formData = new FormData();
  formData.append('mssv', mssvInput.value.trim());
  formData.append('file', file);
  if (currentProgram) {
    formData.append('academicYear', currentProgram.year);
    formData.append('department', currentProgram.department);
    formData.append('programCode', currentProgram.programCode);
  }

  const t0 = performance.now();
  try {
    const res = await fetch('/api/upload', { method: 'POST', body: formData });
    const t1 = performance.now();
    if (!res.ok) throw new Error(await res.text() || ('HTTP ' + res.status));
    const data = await res.json();
    renderResult(data);
    setStatus(`Hoàn thành trong ${(t1 - t0).toFixed(0)} ms`, 'ok');
  } catch (err) {
    setStatus('Lỗi: ' + (err.message || err), 'error');
  } finally {
    busy = false;
    btnUpload.disabled = false;
    btnUpload.textContent = originalText;
  }
});

dropZone.addEventListener('click', () => fileInput.click());

function updateSummaryItem(label, value) {
  const items = summary.querySelectorAll('.item');
  for (const it of items) {
    const k = it.querySelector('.k');
    const v = it.querySelector('.v');
    if (k && v && k.textContent.trim() === label) {
      v.textContent = value ?? '—';
      break;
    }
  }
}

function checkElectiveCredits() {
  const learnedCodes = new Set(
    gradesData
      .map(g => (g.courseCode ?? g.CourseCode ?? '').trim().toUpperCase())
      .filter(code => code)
  );
  let totalElectiveCredits = 0;
  const electiveCodes = (currentProgram.electiveSubjects || []).map(s => s.code);
  for (const grade of gradesData) {
    const code = (grade.courseCode ?? grade.CourseCode ?? '').trim().toUpperCase();
    if (electiveCodes.includes(code)) {
      totalElectiveCredits += Number(grade.credits ?? grade.Credits ?? 0);
    }
  }
  const req = Number(currentProgram.electiveTotalCredits ?? 12) || 12;
  return {
    totalElectiveCredits,
    missingCredits: Math.max(0, req - totalElectiveCredits),
    required: req
  };
}

function getNotLearnedSubjects() {
  if (!currentProgram) return [];
  const learnedCodes = new Set(
    gradesData
      .map(g => (g.courseCode ?? g.CourseCode ?? '').trim().toUpperCase())
      .filter(code => code)
  );

  // Tìm subcategory thể chất
  const physicalSubcategory = (currentProgram.courses || [])
    .find(c => c.category && c.category.toLowerCase().includes('không tích lũy'))
    ?.subcategories?.find(sc => sc.name && sc.name.toLowerCase().includes('thể chất'));

  if (!physicalSubcategory) {
    // Nếu không có subcategory thể chất, dùng logic cũ
    return (currentProgram.allSubjects || []).filter(subject =>
      !learnedCodes.has(subject.code)
    );
  }

  const physicalGroups = physicalSubcategory.groups || [];

  // Kiểm tra xem sinh viên đã học nhóm thể chất nào chưa
  let activePhysicalGroup = null;
  let completedPhysicalGroup = null;

  for (const group of physicalGroups) {
    const groupCodes = new Set((group.subjects || []).map(s => s.code.toUpperCase()));
    let groupCredits = 0;
    let hasAnySubject = false;

    for (const grade of gradesData) {
      const code = (grade.courseCode ?? g.CourseCode ?? '').trim().toUpperCase();
      if (groupCodes.has(code)) {
        hasAnySubject = true;
        groupCredits += Number(grade.credits ?? grade.Credits ?? 0);
      }
    }

    if (hasAnySubject) {
      if (groupCredits >= 5) {
        completedPhysicalGroup = group;
        break; // Đã hoàn thành nhóm này
      } else {
        activePhysicalGroup = group; // Đang học nhóm này
        break;
      }
    }
  }

  // Lọc môn chưa học
  const notLearnedSubjects = [];

  for (const subject of (currentProgram.allSubjects || [])) {
    // Bỏ qua nếu đã học
    if (learnedCodes.has(subject.code)) continue;

    // Xử lý đặc biệt cho môn thể chất
    if (subject.code.startsWith('PHT')) {
      // Nếu đã hoàn thành một nhóm thể chất, bỏ qua tất cả môn thể chất
      if (completedPhysicalGroup) continue;

      // Nếu đang học một nhóm thể chất cụ thể
      if (activePhysicalGroup) {
        const activeGroupCodes = new Set(
          (activePhysicalGroup.subjects || []).map(s => s.code.toUpperCase())
        );
        // Chỉ hiển thị môn thuộc nhóm đang học
        if (activeGroupCodes.has(subject.code)) {
          notLearnedSubjects.push(subject);
        }
        // Bỏ qua các môn thể chất không thuộc nhóm đang học
        continue;
      }

      // Nếu chưa học môn thể chất nào, hiển thị tất cả
      notLearnedSubjects.push(subject);
    } else {
      // Môn không phải thể chất
      notLearnedSubjects.push(subject);
    }
  }

  return notLearnedSubjects;
}

function updateMajorOptions() {
  if (!filterMajor) return;
  filterMajor.innerHTML = '<option value="all">Tất cả</option>';
  const groups = currentProgram?.electiveGroups || [];
  const ci = s => (s || '').toLowerCase();
  groups
    .filter(g => !ci(g.group_name).includes('tốt nghiệp'))
    .forEach(g => {
      const opt = document.createElement('option');
      opt.value = g.group_name;
      opt.textContent = g.group_name;
      filterMajor.appendChild(opt);
    });
}

function filterElectiveSubjects(subjects, filterValue) {
  const groups = currentProgram?.electiveGroups || [];
  const ci = (s) => (s || '').toLowerCase();
  const getCode = (it) => {
    const code = (it?.courseCode ?? it?.CourseCode ?? it?.code ?? '').trim();
    return code ? code.toUpperCase() : '';
  };
  if (filterValue === 'all') return subjects;

  if (filterValue === 'thesis') {
    const thesis = groups.find(g => ci(g.group_name).includes('tốt nghiệp'));
    const thesisCodes = new Set((thesis?.subjects || []).map(s => (s.code || '').toUpperCase()));
    return subjects.filter(item => thesisCodes.has(getCode(item)));
  }

  if (filterValue === 'four-subjects') {
    let allowedCodes = new Set(
      groups
        .filter(g => !ci(g.group_name).includes('tốt nghiệp'))
        .flatMap(g => (g.subjects || []))
        .map(s => (s.code || '').toUpperCase())
    );
    const majorVal = filterMajor?.value;
    if (majorVal && majorVal !== 'all') {
      const major = groups.find(g => g.group_name === majorVal);
      allowedCodes = new Set((major?.subjects || []).map(s => (s.code || '').toUpperCase()));
    }
    return subjects.filter(item => allowedCodes.has(getCode(item)));
  }

  return subjects;
}

// Dashboard functions
function aggregateDataBySemester(grades) {
  if (!grades || grades.length === 0) return [];

  // Group grades by semester
  const semesterMap = new Map();

  for (const grade of grades) {
    const semester = (grade.semester || grade.Semester || '').trim();
    if (!semester) continue;

    const credits = Number(grade.credits || grade.Credits) || 0;
    const score10 = Number(grade.score10 || grade.Score10);
    const gpa4 = Number(grade.gpa || grade.Gpa || grade.gpa4 || grade.Gpa4);

    if (!Number.isFinite(credits) || credits === 0) continue;
    if (!Number.isFinite(gpa4)) continue;

    if (!semesterMap.has(semester)) {
      semesterMap.set(semester, {
        semester,
        grades: []
      });
    }

    semesterMap.get(semester).grades.push({
      credits,
      gpa4,
      score10
    });
  }

  // Calculate semester GPA and accumulated GPA
  const semesterData = [];
  let totalWeightedGpa = 0;
  let totalCredits = 0;

  // Sort semesters chronologically
  const sortedSemesters = Array.from(semesterMap.values()).sort((a, b) => {
    // Extract year and semester number for sorting
    const parseS = (s) => {
      // Try to match "HK1 22-23" or "1-2022" format
      const match = s.match(/(\d+)\s*-\s*(\d+)/);
      if (match) {
        return { year: parseInt(match[1]), sem: parseInt(match[2]) };
      }
      // Try to match "Nhóm X" format (fallback)
      const groupMatch = s.match(/Nhóm\s+(\d+)/);
      if (groupMatch) {
        return { year: 0, sem: parseInt(groupMatch[1]) };
      }
      return { year: 0, sem: 0 };
    };
    const aInfo = parseS(a.semester);
    const bInfo = parseS(b.semester);
    if (aInfo.year !== bInfo.year) return aInfo.year - bInfo.year;
    return aInfo.sem - bInfo.sem;
  });

  for (const semData of sortedSemesters) {
    let semWeightedGpa = 0;
    let semCredits = 0;

    for (const g of semData.grades) {
      semWeightedGpa += g.gpa4 * g.credits;
      semCredits += g.credits;
      totalWeightedGpa += g.gpa4 * g.credits;
      totalCredits += g.credits;
    }

    const semesterGPA = semCredits > 0 ? semWeightedGpa / semCredits : 0;
    const accumulatedGPA = totalCredits > 0 ? totalWeightedGpa / totalCredits : 0;

    semesterData.push({
      semester: semData.semester,
      semesterGPA: parseFloat(semesterGPA.toFixed(2)),
      accumulatedGPA: parseFloat(accumulatedGPA.toFixed(2)),
      credits: semCredits
    });
  }

  return semesterData;
}

function renderDashboard(semesterData) {
  if (!semesterData || semesterData.length === 0) return;

  const dashboardSection = document.getElementById('dashboardSection');
  const canvas = document.getElementById('gpaChart');

  if (!dashboardSection || !canvas) return;

  // Destroy existing chart if any
  if (window.gpaChartInstance) {
    window.gpaChartInstance.destroy();
  }

  const ctx = canvas.getContext('2d');

  window.gpaChartInstance = new Chart(ctx, {
    type: 'bar',
    data: {
      labels: semesterData.map(d => d.semester),
      datasets: [
        {
          type: 'line',
          label: 'Điểm TB tích lũy (hệ 4)',
          data: semesterData.map(d => d.accumulatedGPA),
          borderColor: '#e64444d6',
          backgroundColor: 'rgba(255, 107, 107, 0.1)',
          borderWidth: 3,
          tension: 0.3,
          pointRadius: 5,
          pointHoverRadius: 7,
          pointBackgroundColor: '#fe7f7fff',
          pointBorderColor: '#fff',
          pointBorderWidth: 2,
          order: 1
        },
        {
          type: 'bar',
          label: 'Điểm TB học kỳ (hệ 4)',
          data: semesterData.map(d => d.semesterGPA),
          backgroundColor: '#6CBEEF',
          borderColor: '#4FAEEB',
          borderWidth: 1,
          borderRadius: 6,
          order: 2
        }
      ]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: true,
          position: 'top',
          labels: {
            usePointStyle: true,
            padding: 15,
            font: {
              size: 13,
              weight: '600'
            }
          }
        },
        tooltip: {
          mode: 'index',
          intersect: false,
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          padding: 12,
          titleFont: {
            size: 14,
            weight: 'bold'
          },
          bodyFont: {
            size: 13
          },
          callbacks: {
            label: function (context) {
              let label = context.dataset.label || '';
              if (label) {
                label += ': ';
              }
              label += context.parsed.y.toFixed(2);
              return label;
            }
          }
        },
        zoom: {
          pan: {
            enabled: true,
            mode: 'x',
            modifierKey: null,
          },
          zoom: {
            wheel: {
              enabled: true,
              speed: 0.1,
            },
            pinch: {
              enabled: true
            },
            mode: 'x',
          },
          limits: {
            x: { min: 'original', max: 'original' },
            y: { min: 0, max: 4.0 }
          }
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          max: 4.5,
          ticks: {
            stepSize: 0.5,
            font: {
              size: 12
            },
            callback: function (value) {
              // Only display tick marks up to 4.0
              if (value <= 4.0) {
                // Always show 1 decimal place (e.g., 0.0, 0.5, 1.0, 2.0, 3.0, 4.0)
                return value.toFixed(1);
              }
              return null; // Hide ticks above 4.0
            }
          },
          grid: {
            color: 'rgba(0, 0, 0, 0.05)'
          }
        },
        x: {
          ticks: {
            font: {
              size: 12,
              weight: '500'
            }
          },
          grid: {
            display: false
          }
        }
      },
      interaction: {
        mode: 'index',
        intersect: false,
      }
    }
  });

  // Zoom button handlers
  const zoomInBtn = document.getElementById('zoomIn');
  const zoomOutBtn = document.getElementById('zoomOut');
  const resetZoomBtn = document.getElementById('resetZoom');

  if (zoomInBtn) {
    zoomInBtn.onclick = () => {
      if (window.gpaChartInstance) {
        window.gpaChartInstance.zoom(1.2);
      }
    };
  }

  if (zoomOutBtn) {
    zoomOutBtn.onclick = () => {
      if (window.gpaChartInstance) {
        window.gpaChartInstance.zoom(0.8);
      }
    };
  }

  if (resetZoomBtn) {
    resetZoomBtn.onclick = () => {
      if (window.gpaChartInstance) {
        window.gpaChartInstance.resetZoom();
      }
    };
  }

  dashboardSection.classList.remove('hidden');
}

function renderResult(data) {
  uploadPanel.classList.add('hidden');
  const merged = {
    studentId: data.studentId,
    programCode: data.programCode || (currentProgram && currentProgram.programCode),
    curriculumFound: data.curriculumFound ?? !!currentProgram,
    department: data.department || (currentProgram && currentProgram.department),
    academicYear: data.academicYear || (currentProgram && currentProgram.year),
    totalCredits: data.totalCredits || (currentProgram && currentProgram.totalCredits) || '—'
  };

  if (merged.academicYear) {
    resultYearSelect.value = merged.academicYear;
    resultYearSelect.dispatchEvent(new Event('change'));
    const list = programs[merged.academicYear] || [];
    const desiredKey = (currentProgram && currentProgram.key) ||
      (list.find(p => p.department === merged.department)?.key);
    if (desiredKey) resultDeptSelect.value = desiredKey;
  }

  const map = currentProgram && currentProgram.codeNameMap;
  const nonAccSet = currentProgram && currentProgram.nonAccCodes;

  let matched = 0, unmatched = 0, accCredits = 0, nonAccCredits = 0;
  let sumGpa4Weighted = 0, sumScore10Weighted = 0;

  const bestScores = new Map();

  gradesData = data.grades.slice();
  for (const g of gradesData) {
    const code = (g.courseCode ?? g.CourseCode ?? '').trim();
    if (!code) continue;
    const codeUpper = code.toUpperCase();
    const inProgram = !!(map && map[codeUpper]);
    if (inProgram) matched++; else unmatched++;

    const credits = Number(g.credits ?? g.Credits);
    const score10Num = Number(g.score10 ?? g.Score10);
    const gpa4Num = Number(g.gpa ?? g.Gpa ?? g.gpa4 ?? g.Gpa4);
    const isNonAcc = !!(nonAccSet && nonAccSet.has(codeUpper));

    if (inProgram && Number.isFinite(credits) && Number.isFinite(score10Num)) {
      const existing = bestScores.get(codeUpper);
      if (!existing || score10Num > existing.score10) {
        bestScores.set(codeUpper, {
          credits,
          score10: score10Num,
          gpa4: gpa4Num,
          isNonAcc
        });
      }
    }
  }

  for (const [code, scores] of bestScores) {
    if (scores.isNonAcc) {
      nonAccCredits += scores.credits;
    } else {
      accCredits += scores.credits;
      if (Number.isFinite(scores.gpa4)) sumGpa4Weighted += scores.gpa4 * scores.credits;
      if (Number.isFinite(scores.score10)) sumScore10Weighted += scores.score10 * scores.credits;
    }
  }

  // Create colorful stat cards
  const totalRequiredCredits = Number(currentProgram.totalCredits) || 0;
  const remainingCredits = Math.max(0, totalRequiredCredits - accCredits);
  const gpa4 = accCredits > 0 && sumGpa4Weighted > 0 ? (sumGpa4Weighted / accCredits).toFixed(2) : '0.00';
  const gpa10 = accCredits > 0 && sumScore10Weighted > 0 ? (sumScore10Weighted / accCredits).toFixed(2) : '0.00';

  const statsCards = [
    { label: 'Tổng môn học', value: data.grades.length, color: 'blue' },
    { label: 'Tín chỉ tích lũy', value: accCredits, color: 'yellow' },
    { label: 'Điểm trung bình (10)', value: gpa10, color: 'light-blue' },
    { label: 'GPA hiện tại (4)', value: gpa4, color: 'green', highlight: true },
  ];

  summary.innerHTML = statsCards.map(card =>
    `<div class="info-box ${card.color}">
       <span class="info-label">${card.label}</span>
       <span class="info-value${card.highlight ? ' highlight' : ''}">${escapeHtml(String(card.value))}</span>
     </div>`
  ).join('');
  summary.classList.remove('hidden');

  // Render dashboard with GPA trends
  const semesterData = aggregateDataBySemester(gradesData);
  if (semesterData.length > 0) {
    renderDashboard(semesterData);
  }

  if (gradesData.length || currentProgram.allSubjects.length) {
    renderGradesPage(1);
    gradesWrapper.classList.remove('hidden');
  }
  updateMajorOptions();

  currentProgram && (function ensureKey() {
    if (!currentProgram.key && merged.academicYear && merged.department) {
      const list = programs[merged.academicYear] || [];
      const found = list.find(p => p.department === merged.department);
      if (found) currentProgram.key = found.key;
    }
  })();

  lastResult = data;
  resultsLayout.classList.remove('hidden');
  window.scrollTo({ top: 0, behavior: 'smooth' });

  const aiChatSection = document.getElementById('aiChatSection');
  if (aiChatSection) {
    aiChatSection.classList.remove('hidden');
  }

  if (window.studyChatBot) {
    const studyAnalysis = {
      studentId: merged.studentId,
      department: merged.department,
      academicYear: merged.academicYear,
      programCode: merged.programCode,
      totalCredits: merged.totalCredits,
      grades: data.grades,
      currentProgram: currentProgram,
      summary: {
        totalSubjects: data.grades.length,
        matchedSubjects: matched,
        unmatchedSubjects: unmatched,
        accumulatedCredits: accCredits,
        nonAccumulatedCredits: nonAccCredits,
        gpa4: accCredits > 0 && sumGpa4Weighted > 0 ? (sumGpa4Weighted / accCredits) : null,
        gpa10: accCredits > 0 && sumScore10Weighted > 0 ? (sumScore10Weighted / accCredits) : null,
        /* electiveCredits: totalElectiveCredits,
         missingElectiveCredits: missingCredits,
         requiredElectiveCredits: required,
         notLearnedSubjects: getNotLearnedSubjects()*/
      }
    };
    window.studyChatBot.updateStudyData(studyAnalysis);
  }
}

function formatNumber(val, dec = 2, trimZero = true) {
  if (!Number.isFinite(val)) return '';
  // Sửa lại: luôn có 1 số thập phân cho điểm TK(10)
  const fixed = val.toFixed(dec);
  if (dec === 1) return fixed; // luôn giữ 1 số thập phân
  return trimZero ? fixed.replace(/(\.\d*?[1-9])0+$/, '$1').replace(/\.0+$/, '') : fixed;
}

function formatGpa4(val) {
  if (!Number.isFinite(val)) return '';
  if (val === 0) return '0';
  return val.toFixed(1);
}

function renderGradesPage(page) {
  const filterStatusValue = filterStatus.value;
  const filterElectiveValue = filterElective.value;

  let displayData = [];
  const learnedCodes = new Set(gradesData.map(g => (g.courseCode ?? g.CourseCode ?? '').trim().toUpperCase()));

  if (filterStatusValue === 'all-learned') {
    // Môn đã học: tất cả các môn có trong file excel
    displayData = gradesData;
    displayData = filterElectiveSubjects(displayData, filterElectiveValue);
  } else if (filterStatusValue === 'passed') {
    // Môn đã đậu: những môn đã học và không bị rớt
    displayData = gradesData.filter(g => {
      const isFailed = g.isFailed ?? g.IsFailed ?? false;
      return !isFailed;
    });
    displayData = filterElectiveSubjects(displayData, filterElectiveValue);
  } else if (filterStatusValue === 'failed') {
    // Môn đã rớt: những môn đã học và bị rớt
    displayData = gradesData.filter(g => {
      const isFailed = g.isFailed ?? g.IsFailed ?? false;
      return isFailed;
    });
    displayData = filterElectiveSubjects(displayData, filterElectiveValue);
  } else if (filterStatusValue === 'not-learned') {
    // Môn chưa học: những môn chưa có trong file excel
    let notLearned = getNotLearnedSubjects();
    displayData = notLearned;

    if (filterElectiveValue !== 'all') {
      let specificSubjects = [];
      const ci = (s) => (s || '').toLowerCase();

      if (filterElectiveValue === 'thesis') {
        const thesis = currentProgram.electiveGroups.find(g => ci(g.group_name).includes('tốt nghiệp'));
        specificSubjects = thesis ? thesis.subjects : [];
      } else if (filterElectiveValue === 'four-subjects') {
        const majorVal = filterMajor.value;
        if (majorVal && majorVal !== 'all') {
          const major = currentProgram.electiveGroups.find(g => g.group_name === majorVal);
          specificSubjects = major ? major.subjects : [];
        } else {
          specificSubjects = currentProgram.electiveGroups
            .filter(g => !ci(g.group_name).includes('tốt nghiệp'))
            .flatMap(g => g.subjects);
        }
      }

      const displayCodes = new Set(displayData.map(item => item.code));

      for (const subj of specificSubjects) {
        const codeU = subj.code;
        if (displayCodes.has(codeU)) continue;

        const grade = gradesData.find(g => (g.courseCode ?? g.CourseCode ?? '').trim().toUpperCase() === codeU);
        if (grade) {
          displayData.push(grade);
        } else {
          displayData.push(subj);
        }
      }
    }
  }

  const totalPages = Math.ceil(displayData.length / pageSize) || 1;
  page = Math.min(Math.max(page, 1), totalPages);
  currentPage = page;
  const start = (page - 1) * pageSize;
  const end = Math.min(start + pageSize, displayData.length);
  const slice = displayData.slice(start, end);

  const map = currentProgram && currentProgram.codeNameMap;
  const nonAccSet = currentProgram && currentProgram.nonAccCodes;

  gradesTableBody.innerHTML = slice.map((g, idx) => {
    const abs = start + idx;
    const code = (g.courseCode ?? g.CourseCode ?? g.code ?? '').trim();
    const codeU = code.toUpperCase();
    let name = g.courseName ?? g.CourseName ?? g.name ?? '';
    const inProgram = !!(map && codeU && map[codeU]);
    if (inProgram && !name.trim()) name = map[codeU] || '';
    const credits = Number(g.credits ?? g.Credits ?? 3);
    const score10Num = Number(g.score10 ?? g.Score10);
    const letter = g.letterGrade ?? g.LetterGrade ?? '';
    const gpa4Num = Number(g.gpa ?? g.Gpa ?? g.gpa4 ?? g.Gpa4);
    const isNonAcc = !!(nonAccSet && nonAccSet.has(codeU));
    const isFailed = g.isFailed ?? g.IsFailed ?? false;

    // XÁC ĐỊNH CLASS CHO DÒNG - KIỂM TRA isFailed TRƯỚC TIÊN
    let rowClass = '';

    // 1. KIỂM TRA MÔN RỚT TRƯỚC (ưu tiên cao nhất)
    if (isFailed) {
      rowClass = 'failed';
    }
    // 2. Sau đó mới kiểm tra môn chưa học (không có điểm)
    else if (Number.isNaN(score10Num)) {
      rowClass = 'not-learned';
    }
    // 3. Môn đã học và đậu - kiểm tra thuộc CTĐT
    else if (inProgram) {
      rowClass = isNonAcc ? 'nonacc' : 'correct';
    }
    // 4. Ngoài CTĐT
    else {
      rowClass = 'incorrect';
    }

    return `<tr class="${rowClass}">
      <td class="stt">${abs + 1}</td>
      <td class="code">${escapeHtml(code)}</td>
      <td class="name">${escapeHtml(name)}</td>
      <td class="credits">${Number.isFinite(credits) ? credits : ''}</td>
      <td class="score10">${Number.isFinite(score10Num) ? formatNumber(score10Num, 1, false) : ''}</td>
      <td class="letter">${letter ? escapeHtml(letter) : ''}</td>
      <td class="gpa4">${Number.isFinite(gpa4Num) ? formatGpa4(gpa4Num) : ''}</td>
    </tr>`;
  }).join('');

  updatePagination(totalPages);
  updatePaginationInfo(start + 1, end, displayData.length);
}

function updatePagination(totalPages) {
  if (!paginationEl) return;
  if (totalPages <= 1) { paginationEl.innerHTML = ''; return; }
  const maxVisible = 7;
  let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
  let endPage = Math.min(totalPages, startPage + maxVisible - 1);
  if (endPage - startPage + 1 < maxVisible)
    startPage = Math.max(1, endPage - maxVisible + 1);
  const buttons = [];
  if (startPage > 1) {
    buttons.push(`<button class="page-btn" data-page="1">1</button>`);
    if (startPage > 2) buttons.push(`<span class="page-dots">...</span>`);
  }
  for (let p = startPage; p <= endPage; p++) {
    buttons.push(`<button class="page-btn ${p === currentPage ? 'active' : ''}" data-page="${p}">${p}</button>`);
  }
  if (endPage < totalPages) {
    if (endPage < totalPages - 1) buttons.push(`<span class="page-dots">...</span>`);
    buttons.push(`<button class="page-btn" data-page="${totalPages}">${totalPages}</button>`);
  }
  const prevBtn = currentPage > 1 ? `<button class="nav-btn" data-page="${currentPage - 1}">‹ Trước</button>` : '';
  const nextBtn = currentPage < totalPages ? `<button class="nav-btn" data-page="${currentPage + 1}">Sau ›</button>` : '';
  paginationEl.innerHTML = `${prevBtn}${buttons.join('')}${nextBtn}`;
}

function updatePaginationInfo(start, end, total) {
  if (paginationInfoEl)
    paginationInfoEl.textContent = `Hiển thị ${start}-${end} trong tổng số ${total} môn học`;
}

if (paginationEl) {
  paginationEl.addEventListener('click', e => {
    const btn = e.target.closest('button[data-page]');
    if (!btn) return;
    const page = Number(btn.getAttribute('data-page'));
    if (Number.isFinite(page) && page !== currentPage) renderGradesPage(page);
  });
}

function escapeHtml(str) {
  if (!str) return '';
  return String(str)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

resultYearSelect.addEventListener('change', () => {
  resultDeptSelect.innerHTML = '<option value="">-- Chọn khoa / viện --</option>';
  if (!resultYearSelect.value) {
    resultDeptSelect.disabled = true;
    return;
  }
  const list = programs[resultYearSelect.value] || [];
  list.forEach(p => {
    const opt = document.createElement('option');
    opt.value = p.key;
    opt.textContent = p.department;
    resultDeptSelect.appendChild(opt);
  });
  resultDeptSelect.disabled = false;
  if (!summary.classList.contains('hidden')) {
    updateSummaryItem('Niên khóa', resultYearSelect.value);
  }
});

resultDeptSelect.addEventListener('change', async () => {
  if (!resultYearSelect.value || !resultDeptSelect.value) return;
  const year = resultYearSelect.value;
  const key = resultDeptSelect.value;
  const entry = (programs[year] || []).find(p => p.key === key);
  if (!entry) return;
  if (!summary.classList.contains('hidden')) {
    updateSummaryItem('Khoa', entry.department);
    updateSummaryItem('Niên khóa', year);
  }
  try {
    const res = await fetch(entry.file);
    if (!res.ok) throw new Error('Không tải được chương trình');
    const json = await res.json();

    const codeNameMap = {};
    const nonAccCodes = new Set();
    const allSubjects = [];
    const electiveSubjects = [];
    let electiveGroups = [];
    let electiveTotalCredits = 0;

    function addBaseSubjects(arr, isNonAcc = false) {
      if (!Array.isArray(arr)) return;
      arr.forEach(s => {
        if (s && s.code) {
          const up = String(s.code).toUpperCase();
          if (s.name) codeNameMap[up] = s.name;
          if (isNonAcc) nonAccCodes.add(up);
          allSubjects.push({
            code: up,
            name: s.name || '',
            credits: Number(s.credits) || 0
          });
        }
      });
    }

    if (Array.isArray(json.courses)) {
      json.courses.forEach(c => {
        const isNonAcc = (c.category || '').toLowerCase().includes('không tích lũy');
        const catIsElective = (c.category || '').toLowerCase().includes('tự chọn');

        if (Array.isArray(c.subjects)) {
          addBaseSubjects(c.subjects, isNonAcc);
        }

        if (Array.isArray(c.groups)) {
          c.groups.forEach(g => {
            if (Array.isArray(g.subjects)) {
              addBaseSubjects(g.subjects, isNonAcc);
            }
          });
        }

        if (Array.isArray(c.subcategories)) {
          c.subcategories.forEach(sc => {
            const scNonAcc = isNonAcc || (sc.name || '').toLowerCase().includes('không tích lũy');
            const scIsElective = (sc.name || '').toLowerCase().includes('tự chọn');


            if (Array.isArray(sc.subjects)) {
              addBaseSubjects(sc.subjects, scNonAcc);
            }

            if (scIsElective) {
              electiveTotalCredits = Number(sc.total_credits) || electiveTotalCredits;

              if (Array.isArray(sc.subjects)) {
                sc.subjects.forEach(s => {
                  if (s?.code) {
                    electiveSubjects.push({
                      code: String(s.code).toUpperCase(),
                      name: s.name || '',
                      credits: Number(s.credits) || 0
                    });
                  }
                });
              }
              if (Array.isArray(sc.groups)) {
                electiveGroups = sc.groups.map(g => ({
                  group_name: g.group_name || '',
                  subjects: Array.isArray(g.subjects) ? g.subjects.map(s => ({
                    code: String(s.code).toUpperCase(),
                    name: s.name || '',
                    credits: Number(s.credits) || 0
                  })) : []
                }));
                sc.groups.forEach(g => {
                  if (Array.isArray(g.subjects)) {
                    g.subjects.forEach(s => {
                      if (s?.code) {
                        electiveSubjects.push({
                          code: String(s.code).toUpperCase(),
                          name: s.name || '',
                          credits: Number(s.credits) || 0
                        });
                      }
                    });
                  }
                });
              }
            } else if (Array.isArray(sc.groups)) {
              sc.groups.forEach(g => {
                if (Array.isArray(g.subjects)) {
                  addBaseSubjects(g.subjects, scNonAcc);
                }
              });
            }
          });
        }

        if (catIsElective) {
          if (Array.isArray(c.subjects)) {
            c.subjects.forEach(s => {
              if (s?.code) {
                electiveSubjects.push({
                  code: String(s.code).toUpperCase(),
                  name: s.name || '',
                  credits: Number(s.credits) || 0
                });
              }
            });
          }
          if (Array.isArray(c.groups)) {
            electiveGroups = c.groups.map(g => ({
              group_name: g.group_name || '',
              subjects: Array.isArray(g.subjects) ? g.subjects.map(s => ({
                code: String(s.code).toUpperCase(),
                name: s.name || '',
                credits: Number(s.credits) || 0
              })) : []
            }));
            c.groups.forEach(g => {
              if (Array.isArray(g.subjects)) {
                g.subjects.forEach(s => {
                  if (s?.code) {
                    electiveSubjects.push({
                      code: String(s.code).toUpperCase(),
                      name: s.name || '',
                      credits: Number(s.credits) || 0
                    });
                  }
                });
              }
            });
          }
        }
      });
    }

    const seen = new Set();
    const dedupElectives = electiveSubjects.filter(s => {
      const key = s.code;
      if (seen.has(key)) return false;
      seen.add(key);
      return true;
    });
    const dedupAll = allSubjects.filter(s => {
      const key = s.code;
      if (seen.has(key)) return false;
      seen.add(key);
      return true;
    });

    currentProgram = {
      year: json.academic_year || year,
      department: json.department || entry.department,
      programCode: json.program_code || '',
      totalCredits: json.total_credits || '',
      nonAccTotal: json.non_accumulated_credits || '',
      codeNameMap,
      nonAccCodes,
      allSubjects: dedupAll,
      electiveSubjects: dedupElectives,
      electiveGroups,
      electiveTotalCredits: electiveTotalCredits || 12,
      key: entry.key,
      courses: json.courses || []
    };
    updateMajorOptions();
    if (lastResult) renderResult(lastResult);
  } catch (e) {
    setStatus('Lỗi tải chương trình: ' + e.message, 'error');
  }
});

if (btnNewAnalysis) {
  btnNewAnalysis.addEventListener('click', async () => {
    // Check if we should prompt for registration
    if (hasAnalysisResult && currentStudentId && !hasShownRegisterPrompt) {
      const loggedIn = await isUserLoggedIn();
      const alreadyRegistered = await isStudentIdRegistered(currentStudentId);

      // Show prompt if not logged in and not registered
      if (!loggedIn && !alreadyRegistered) {
        hasShownRegisterPrompt = true;

        // Define callback to clear result after prompt is handled
        const clearResultCallback = () => {
          clearResult();
          form.reset();
          setStatus('', '');
          if (dropZoneText) dropZoneText.textContent = 'Chọn hoặc kéo thả file vào đây';
        };

        showRegisterPromptModal(clearResultCallback);
        return; // Don't clear yet, wait for user decision
      }
    }

    // Clear result and reset form
    clearResult();
    form.reset();
    setStatus('', '');
    if (dropZoneText) dropZoneText.textContent = 'Chọn hoặc kéo thả file vào đây';
  });
}

filterStatus.addEventListener('change', () => {
  renderGradesPage(1);
});

filterElective.addEventListener('change', () => {
  renderGradesPage(1);
});

if (filterMajor) {
  filterMajor.addEventListener('change', () => {
    renderGradesPage(1);
  });
}

// ==================== AUTO-REGISTRATION FEATURE ====================
let hasAnalysisResult = false;
let currentStudentId = null;
let hasShownRegisterPrompt = false;
let isLoggedIn = false; // Track login status
let studentIdRegistered = false; // Track if student is registered

// Track when analysis is complete
function markAnalysisComplete(studentId) {
  hasAnalysisResult = true;
  currentStudentId = studentId;
  hasShownRegisterPrompt = false;

  // Check login and registration status immediately
  checkUserStatus();
}

// Check user login and registration status
async function checkUserStatus() {
  if (!currentStudentId) return;

  isLoggedIn = await isUserLoggedIn();
  studentIdRegistered = await isStudentIdRegistered(currentStudentId);
}

// Update renderResult to mark analysis as complete
const originalRenderResult = window.renderResult || renderResult;
window.renderResult = function (data) {
  if (typeof originalRenderResult === 'function') {
    originalRenderResult.call(this, data);
  }
  if (data && data.studentId) {
    markAnalysisComplete(data.studentId);
  }
};

// Check if user is logged in
async function isUserLoggedIn() {
  try {
    const res = await fetch('/api/account/me');
    return res.ok;
  } catch {
    return false;
  }
}

// Check if StudentId already registered
async function isStudentIdRegistered(studentId) {
  try {
    const res = await fetch(`/api/account/check-mssv/${encodeURIComponent(studentId)}`);
    if (res.ok) {
      const data = await res.json();
      return data.exists;
    }
  } catch {
  }
  return false;
}

// Auto-register function
async function autoRegisterStudent(studentId) {
  try {
    const res = await fetch('/api/account/auto-register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ studentId })
    });

    if (res.ok) {
      const data = await res.json();
      return { success: true, username: data.username, password: data.password };
    } else {
      const data = await res.json();
      return { success: false, message: data.message };
    }
  } catch (error) {
    return { success: false, message: 'Lỗi kết nối server' };
  }
}

// Show success modal with password
function showSuccessModal(username, password) {
  const modal = document.getElementById('autoRegisterSuccessModal');
  const usernameEl = document.getElementById('generatedUsername');
  const passwordEl = document.getElementById('generatedPassword');

  if (modal && usernameEl && passwordEl) {
    usernameEl.textContent = username;
    passwordEl.textContent = password;
    modal.style.display = 'flex';
  }
}

// Beforeunload handler - MUST be synchronous
window.addEventListener('beforeunload', function (e) {
  // Only show prompt if we have analysis result and haven't shown it yet
  if (!hasAnalysisResult || !currentStudentId || hasShownRegisterPrompt) {
    return;
  }

  // Only show if user is NOT logged in and NOT already registered
  if (isLoggedIn || studentIdRegistered) {
    return;
  }

  // Show browser's default confirmation dialog
  e.preventDefault();
  e.returnValue = 'Bạn chưa lưu kết quả phân tích. Bạn có muốn tạo tài khoản để lưu lại không?';
  return e.returnValue;
});

// Show register prompt modal
let pendingActionAfterPrompt = null;

function showRegisterPromptModal(afterPromptCallback) {
  const modal = document.getElementById('autoRegisterPromptModal');
  if (modal) {
    modal.style.display = 'flex';
    pendingActionAfterPrompt = afterPromptCallback;
  }
}

// Handle register prompt response
function handleRegisterPrompt(shouldRegister) {
  const modal = document.getElementById('autoRegisterPromptModal');
  if (modal) {
    modal.style.display = 'none';
  }

  if (shouldRegister && currentStudentId) {
    performAutoRegistration();
  } else {
    // User declined - execute pending action if any
    if (pendingActionAfterPrompt) {
      pendingActionAfterPrompt();
      pendingActionAfterPrompt = null;
    }
  }
}

// Perform auto-registration
async function performAutoRegistration() {
  const result = await autoRegisterStudent(currentStudentId);

  if (result.success) {
    showSuccessModal(result.username, result.password);
  } else {
    alert(result.message || 'Đăng ký thất bại');
    // Still execute pending action even if registration failed
    if (pendingActionAfterPrompt) {
      pendingActionAfterPrompt();
      pendingActionAfterPrompt = null;
    }
  }
}

// Copy password to clipboard
function copyPassword() {
  const passwordEl = document.getElementById('generatedPassword');
  if (passwordEl) {
    const textArea = document.createElement('textarea');
    textArea.value = passwordEl.textContent;
    document.body.appendChild(textArea);
    textArea.select();
    document.execCommand('copy');
    document.body.removeChild(textArea);

    const btn = document.getElementById('copyPasswordBtn');
    if (btn) {
      const originalText = btn.textContent;
      btn.textContent = '✓ Đã sao chép!';
      setTimeout(() => {
        btn.textContent = originalText;
      }, 2000);
    }
  }
}

// Close success modal and redirect to login
function closeSuccessModalAndLogin() {
  const modal = document.getElementById('autoRegisterSuccessModal');
  if (modal) {
    modal.style.display = 'none';
  }

  // Execute pending action if any (e.g., clear result for new analysis)
  if (pendingActionAfterPrompt) {
    pendingActionAfterPrompt();
    pendingActionAfterPrompt = null;
  }

  window.location.href = '/login.html';
}

// Make functions available globally
window.handleRegisterPrompt = handleRegisterPrompt;
window.copyPassword = copyPassword;
window.closeSuccessModalAndLogin = closeSuccessModalAndLogin;
