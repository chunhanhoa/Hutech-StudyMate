using Check.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System; // thêm

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
builder.Services.AddHostedService<SelfPingService>(); // thêm

var app = builder.Build();

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
