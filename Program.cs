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
builder.Services.AddSingleton<IAIService, GroqAIService>();
builder.Services.AddHttpClient(); // thêm
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<UserService>();
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "MISSING_CLIENT_ID";
        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "MISSING_CLIENT_SECRET";
    });


// Cấu hình HttpClient cho ping với timeout và retry policy tốt hơn
builder.Services.AddHttpClient("SelfPing", client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("User-Agent", "Hutech-StudyMate-SelfPing/1.0");
});

// builder.Services.AddHostedService<SelfPingService>(); // đã comment để tắt ping tự động

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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Giữ duy nhất một lệnh Run
app.Run();
