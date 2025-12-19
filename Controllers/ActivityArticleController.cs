using Check.Models;
using Check.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Check.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ActivityArticleController : ControllerBase
{
    private readonly MongoDbContext _context;

    public ActivityArticleController(MongoDbContext context)
    {
        _context = context;
        // Initialize data if needed (simple seeding)
        SeedData().Wait();
    }

    private async Task SeedData()
    {
        var count = await _context.ActivityArticles.CountDocumentsAsync(_ => true);
        if (count == 0)
        {
            var articles = new List<ActivityArticle>
            {
                new ActivityArticle
                {
                    Title = "[ĐĂNG KÝ] THAM GIA CHƯƠNG TRÌNH TÌM HIỂU PHÁP LUẬT VÀ TRẬT TỰ AN TOÀN GIAO THÔNG",
                    PublishDate = "26/11/2025 15:50",
                    ThumbnailUrl = "https://news.hutech.edu.vn/img/w762/h411/2023/11/news/c87103a2-231127-cover-web762x411_587a8a25.jpg",
                    Description = "MỜI CÁC BẠN SINH VIÊN THAM GIA CHƯƠNG TRÌNH TÌM HIỂU PHÁP LUẬT VÀ TRẬT TỰ AN TOÀN GIAO THÔNG...",
                    Content = @"
                        <p style=""font-weight: bold;"">MỜI CÁC BẠN SINH VIÊN THAM GIA CHƯƠNG TRÌNH TÌM HIỂU PHÁP LUẬT VÀ TRẬT TỰ AN TOÀN GIAO THÔNG...</p>
                        <p>An toàn giao thông không chỉ là trách nhiệm của cá nhân trực tiếp điều khiển phương tiện...</p>
                        <ul style=""list-style-type: none; padding-left: 0;"">
                            <li>- <strong>Thời gian:</strong> Lúc 08h00 ngày 27/11/2025 (thứ 5)</li>
                            <li>- <strong>Địa điểm:</strong> Trường Đại học Quốc tế Hồng Bàng</li>
                            <li>- <strong>Đăng ký:</strong> <a href=""#"">https://forms.gle/A5yUai2BQ5dZ8cWj8</a></li>
                        </ul>"
                },
                new ActivityArticle
                {
                    Title = "Mùa hè xanh HUTECH 2025 chính thức mở cổng đăng ký từ ngày 07/6",
                    PublishDate = "07/06/2025 08:00",
                    ThumbnailUrl = "https://news.hutech.edu.vn/img/w762/h411/2024/06/news/b990fd2c-240607-cover-web762x411_338c9d19.jpg",
                    Description = "Mùa hè xanh HUTECH 2025 Trường Đại học Công nghệ TP.HCM (HUTECH) đã sẵn sàng tuyển quân...",
                    Content = "<p>Nội dung chi tiết Mùa hè xanh...</p>"
                },
                new ActivityArticle
                {
                    Title = "SINH VIÊN HUTECH NẮM GIỮ BÍ KÍP \"ĂN SẠCH, SỐNG XANH\" CÙNG CHUYÊN GIA",
                    PublishDate = "05/06/2025 10:30",
                    ThumbnailUrl = "https://news.hutech.edu.vn/img/w762/h411/2024/06/news/1739c9f0-240607-cover-web762x411_a15a8c98.jpg",
                    Description = "Ngày 12/6 tới đây, HUTECH sẽ tổ chức chương trình Tư vấn sức khỏe học đường...",
                    Content = "<p>Nội dung chi tiết Talkshow...</p>"
                }
            };

            await _context.ActivityArticles.InsertManyAsync(articles);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetArticles()
    {
        var articles = await _context.ActivityArticles.Find(_ => true)
            .SortByDescending(a => a.CreatedAt)
            .ToListAsync();
        return Ok(articles);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetArticle(string id)
    {
         var article = await _context.ActivityArticles.Find(a => a.Id == id).FirstOrDefaultAsync();
         if (article == null) return NotFound();
         return Ok(article);
    }
}
