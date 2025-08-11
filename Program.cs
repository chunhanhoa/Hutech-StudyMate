using Check.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// (BỎ dòng UseUrls cố định tránh xung đột 5000/5001)
// Hỗ trợ đặt PORT qua biến môi trường (triển khai hosting PaaS)
var portEnv = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var dynPort))
{
    builder.WebHost.ConfigureKestrel(o => o.ListenAnyIP(dynPort));
}

builder.Services.AddControllers();

builder.Services.AddSingleton<IProgramService, ProgramService>();
builder.Services.AddSingleton<IExcelGradeParser, ExcelGradeParser>();

var app = builder.Build();

// Thêm: phục vụ index.html khi truy cập "/"
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "ProgramJson")),
    RequestPath = "/ProgramJson"
});

// (tùy chọn) fallback để luôn trả index cho route front-end (giữ nếu cần)
app.MapFallbackToFile("index.html");

app.UseRouting();
app.MapControllers();

app.Run();
