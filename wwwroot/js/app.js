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
    { key: "quan-tri-dich-vu-va-du-lich-2022", department: "Quản trị dịch vụ và du lịch", file: "/ProgramJson/2022/Quan-tri-dich-vu-va-du-lich-2022.json" },
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
    function addSubjects(arr, isNonAcc = false) {
      if (!Array.isArray(arr)) return;
      arr.forEach(s => {
        if (s && s.code) {
          const up = s.code.toUpperCase();
          if (s.name) codeNameMap[up] = s.name;
          if (isNonAcc) nonAccCodes.add(up);
        }
      });
    }
    (json.courses || []).forEach(c => {
      const isNonAcc = (c.category || '').toLowerCase().includes('không tích lũy');
      if (c.subjects) addSubjects(c.subjects, isNonAcc);
      if (c.subcategories) c.subcategories.forEach(sc => {
        const scNonAcc = isNonAcc || (sc.name || '').toLowerCase().includes('không tích lũy');
        if (sc.subjects) addSubjects(sc.subjects, scNonAcc);
        if (sc.groups) sc.groups.forEach(g => addSubjects(g.subjects, scNonAcc));
      });
      if (c.groups) c.groups.forEach(g => addSubjects(g.subjects, isNonAcc));
    });
    currentProgram = {
      year: json.academic_year || year,
      department: json.department || entry.department,
      programCode: json.program_code || '',
      totalCredits: json.total_credits || '',
      nonAccTotal: json.non_accumulated_credits || '',
      codeNameMap,
      nonAccCodes
    };
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
  if (fileInput.files.length && dropZoneText)
    dropZoneText.textContent = 'Đã chọn: ' + fileInput.files[0].name;
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
  if (!file.name.toLowerCase().endsWith('.xlsx')) { setStatus('File phải là .xlsx', 'error'); return; }

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
    setTimeout(() => {
      const deptEntry = Object.values(programs).flat()
        .find(p => p.department === merged.department);
      if (deptEntry) resultDeptSelect.value = deptEntry.key;
    }, 100);
  }

  const baseItems = [
    ['MSSV', merged.studentId],
    ['Program Code', merged.programCode || '—'],
    ['Khoa', merged.department || '—'],
    ['Niên khóa', merged.academicYear || '—'],
    ['Tổng TC CTĐT', merged.totalCredits],
    ['Số môn đã học', data.grades.length]
  ];
  summary.innerHTML = baseItems.map(([k, v]) =>
    `<div class="item"><span class="k">${k}</span><span class="v">${escapeHtml(String(v))}</span></div>`
  ).join('');
  summary.classList.remove('hidden');

  const map = currentProgram && currentProgram.codeNameMap;
  const nonAccSet = currentProgram && currentProgram.nonAccCodes;

  let matched = 0, unmatched = 0, accCredits = 0, nonAccCredits = 0;
  let sumGpa4Weighted = 0, sumScore10Weighted = 0;

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
    if (inProgram && Number.isFinite(credits)) {
      if (isNonAcc) nonAccCredits += credits;
      else {
        accCredits += credits;
        if (Number.isFinite(gpa4Num)) sumGpa4Weighted += gpa4Num * credits;
        if (Number.isFinite(score10Num)) sumScore10Weighted += score10Num * credits;
      }
    }
  }

  if (gradesData.length) {
    renderGradesPage(1);
    gradesWrapper.classList.remove('hidden');
  }

  const gpa4Avg = accCredits > 0 && sumGpa4Weighted > 0 ? (sumGpa4Weighted / accCredits).toFixed(2) : '—';
  const gpa10Avg = accCredits > 0 && sumScore10Weighted > 0 ? (sumScore10Weighted / accCredits).toFixed(2) : '—';
  const totalAccRequired = Number(currentProgram?.totalCredits);
  const totalNonAccRequired = Number(currentProgram?.nonAccTotal);
  const accDisplay = Number.isFinite(totalAccRequired) ? `${accCredits} / ${totalAccRequired}` : `${accCredits}`;
  const nonAccDisplay = Number.isFinite(totalNonAccRequired) ? `${nonAccCredits} / ${totalNonAccRequired}` : `${nonAccCredits}`;

  summary.insertAdjacentHTML('beforeend', `
    <div class="item"><span class="k">Thuộc CTĐT</span><span class="v">${matched}</span></div>
    <div class="item"><span class="k">Ngoài CTĐT</span><span class="v">${unmatched}</span></div>
    <div class="item"><span class="k">TC tích lũy</span><span class="v">${accDisplay}</span></div>
    <div class="item"><span class="k">TC không tích lũy</span><span class="v">${nonAccDisplay}</span></div>
    <div class="item"><span class="k">GPA TL (4)</span><span class="v">${gpa4Avg}</span></div>
    <div class="item"><span class="k">GPA TL (10)</span><span class="v">${gpa10Avg}</span></div>`);

  lastResult = data;
  resultsLayout.classList.remove('hidden');
  resultsLayout.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

function renderGradesPage(page) {
  if (!gradesData.length) return;
  const totalPages = Math.ceil(gradesData.length / pageSize) || 1;
  page = Math.min(Math.max(page, 1), totalPages);
  currentPage = page;
  const start = (page - 1) * pageSize;
  const end = Math.min(start + pageSize, gradesData.length);
  const slice = gradesData.slice(start, end);

  const map = currentProgram && currentProgram.codeNameMap;
  const nonAccSet = currentProgram && currentProgram.nonAccCodes;

  gradesTableBody.innerHTML = slice.map((g, idx) => {
    const abs = start + idx;
    const code = (g.courseCode ?? g.CourseCode ?? '').trim();
    const codeU = code.toUpperCase();
    let name = g.courseName ?? g.CourseName ?? '';
    const inProgram = !!(map && codeU && map[codeU]);
    if ((!name || !name.trim()) && inProgram) name = map[codeU] || '';
    const credits = Number(g.credits ?? g.Credits);
    const score10Num = Number(g.score10 ?? g.Score10);
    const gpa4Num = Number(g.gpa ?? g.Gpa ?? g.gpa4 ?? g.Gpa4);
    const isNonAcc = !!(nonAccSet && nonAccSet.has(codeU));
    let rowClass = 'incorrect';
    if (inProgram) rowClass = isNonAcc ? 'nonacc' : 'correct';
    return `<tr class="${rowClass}">
      <td class="num">${abs + 1}</td>
      <td>${escapeHtml(code)}</td>
      <td>${escapeHtml(name)}</td>
      <td class="num">${Number.isFinite(credits) ? credits : ''}</td>
      <td class="num">${Number.isFinite(score10Num) ? score10Num.toFixed(2) : ''}</td>
      <td>${escapeHtml(g.letterGrade ?? g.LetterGrade ?? '')}</td>
      <td class="num">${Number.isFinite(gpa4Num) ? gpa4Num.toFixed(2) : ''}</td>
    </tr>`;
  }).join('');

  updatePagination(totalPages);
  updatePaginationInfo(start + 1, end, gradesData.length);
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

// Dropdown kết quả (thay đổi CTĐT sau khi xem)
resultYearSelect.addEventListener('change', () => {
  resultDeptSelect.innerHTML = '<option value="">-- Chọn khoa / viện --</option>';
  if (!resultYearSelect.value) { resultDeptSelect.disabled = true; return; }
  const list = programs[resultYearSelect.value] || [];
  list.forEach(p => {
    const opt = document.createElement('option');
    opt.value = p.key;
    opt.textContent = p.department;
    resultDeptSelect.appendChild(opt);
  });
  resultDeptSelect.disabled = false;
});

resultDeptSelect.addEventListener('change', async () => {
  if (!resultYearSelect.value || !resultDeptSelect.value) return;
  const year = resultYearSelect.value;
  const key = resultDeptSelect.value;
  const entry = (programs[year] || []).find(p => p.key === key);
  if (!entry) return;
  try {
    const res = await fetch(entry.file);
    if (!res.ok) throw new Error('Không tải được chương trình');
    const json = await res.json();
    const codeNameMap = {};
    const nonAccCodes = new Set();
    function addSubjects(arr, isNonAcc = false) {
      if (!Array.isArray(arr)) return;
      arr.forEach(s => {
        if (s && s.code) {
          const up = s.code.toUpperCase();
            if (s.name) codeNameMap[up] = s.name;
            if (isNonAcc) nonAccCodes.add(up);
        }
      });
    }
    (json.courses || []).forEach(c => {
      const isNonAcc = (c.category || '').toLowerCase().includes('không tích lũy');
      if (c.subjects) addSubjects(c.subjects, isNonAcc);
      if (c.subcategories) c.subcategories.forEach(sc => {
        const scNonAcc = isNonAcc || (sc.name || '').toLowerCase().includes('không tích lũy');
        if (sc.subjects) addSubjects(sc.subjects, scNonAcc);
        if (sc.groups) sc.groups.forEach(g => addSubjects(g.subjects, scNonAcc));
      });
      if (c.groups) c.groups.forEach(g => addSubjects(g.subjects, isNonAcc));
    });
    currentProgram = {
      year: json.academic_year || year,
      department: json.department || entry.department,
      programCode: json.program_code || '',
      totalCredits: json.total_credits || '',
      nonAccTotal: json.non_accumulated_credits || '',
      codeNameMap,
      nonAccCodes
    };
    if (lastResult) renderResult(lastResult);
  } catch (e) {
    setStatus('Lỗi tải chương trình: ' + e.message, 'error');
  }
});

if (btnNewAnalysis) {
  btnNewAnalysis.addEventListener('click', () => {
    clearResult();
    form.reset();
    setStatus('', '');
    if (dropZoneText) dropZoneText.textContent = 'Chọn hoặc kéo thả file vào đây';
  });
}