using Check.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Thêm: ấn định cổng (có thể đổi nếu bị chiếm)
builder.WebHost.UseUrls("http://localhost:5000", "https://localhost:5001");

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
app.Run();
app.Run();
