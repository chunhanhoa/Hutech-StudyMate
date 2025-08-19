using Check.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System; // thêm
using System.Text; // thêm

var builder = WebApplication.CreateBuilder(args);

// Thay cấu hình cứng bằng PORT động (Render cung cấp biến PORT)
var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}
else
{
    builder.WebHost.UseUrls("http://localhost:5000");
}

builder.Services.AddControllers();
builder.Services.AddSingleton<IProgramService, ProgramService>();
builder.Services.AddSingleton<IExcelGradeParser, ExcelGradeParser>();
builder.Services.AddHttpClient(); // thêm

// Cấu hình HttpClient cho ping với timeout và retry policy tốt hơn
builder.Services.AddHttpClient("SelfPing", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "Hutech-StudyMate-SelfPing/1.0");
});

builder.Services.AddHostedService<SelfPingService>(); // thêm

var app = builder.Build();

// Đăng ký Encoding cho định dạng .xls (ExcelDataReader)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "ProgramJson")),
    RequestPath = "/ProgramJson"
});

app.MapFallbackToFile("index.html");

app.UseRouting();
app.MapControllers();

// Giữ duy nhất một lệnh Run
app.Run();
