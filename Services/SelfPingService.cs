using System.Net.Http;
using Microsoft.Extensions.Hosting;

namespace Check.Services;

public class SelfPingService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SelfPingService> _logger;
    private readonly string? _targetUrl;
    private static readonly TimeSpan FastInterval = TimeSpan.FromMinutes(8);  // Ping nhanh hơn
    private static readonly TimeSpan SlowInterval = TimeSpan.FromMinutes(14); // Ping chậm khi ổn định

    public SelfPingService(IHttpClientFactory httpClientFactory, ILogger<SelfPingService> logger, IConfiguration cfg)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        
        // Sửa lại cách đọc config - tìm URL từ nhiều nguồn
        _targetUrl = 
            cfg["SelfPing:Url"] ??
            Environment.GetEnvironmentVariable("SELF_PING_URL") ??
            Environment.GetEnvironmentVariable("RENDER_EXTERNAL_URL") ??
            "https://hutech-studymate.onrender.com"; // URL mặc định của bạn
            
        if (!string.IsNullOrWhiteSpace(_targetUrl) && !_targetUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            _targetUrl = "https://" + _targetUrl.Trim().TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(_targetUrl))
            _targetUrl = _targetUrl.TrimEnd('/') + "/";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_targetUrl))
        {
            _logger.LogInformation("SelfPingService: Không có URL để ping. Tắt service.");
            return;
        }

        _logger.LogInformation("SelfPingService: Bắt đầu ping {url} để giữ server thức", _targetUrl);

        // Delay ngắn hơn sau khởi động
        try { await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); } catch { }

        int consecutiveSuccesses = 0;
        int consecutiveFailures = 0;

        while (!stoppingToken.IsCancellationRequested)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(15); // Timeout dài hơn cho server lạnh
                
                using var req = new HttpRequestMessage(HttpMethod.Head, _targetUrl); // Dùng HEAD thay vì GET để tiết kiệm băng thông
                req.Headers.Add("User-Agent", "SelfPing/1.0");
                
                var resp = await client.SendAsync(req, stoppingToken);
                sw.Stop();
                
                if (resp.IsSuccessStatusCode)
                {
                    consecutiveSuccesses++;
                    consecutiveFailures = 0;
                    
                    if (consecutiveSuccesses <= 3 || consecutiveSuccesses % 10 == 0)
                        _logger.LogInformation("✅ Ping thành công #{count} - {status} ({ms}ms)", 
                            consecutiveSuccesses, (int)resp.StatusCode, sw.ElapsedMilliseconds);
                    else
                        _logger.LogDebug("✅ Ping OK #{count} - {ms}ms", consecutiveSuccesses, sw.ElapsedMilliseconds);
                }
                else
                {
                    consecutiveFailures++;
                    consecutiveSuccesses = 0;
                    _logger.LogWarning("⚠️ Ping lỗi HTTP {status} ({ms}ms) - lần thứ {failures}", 
                        (int)resp.StatusCode, sw.ElapsedMilliseconds, consecutiveFailures);
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                consecutiveFailures++;
                consecutiveSuccesses = 0;
                _logger.LogWarning("⏰ Ping timeout sau {ms}ms - lần thứ {failures}", sw.ElapsedMilliseconds, consecutiveFailures);
            }
            catch (Exception ex)
            {
                consecutiveFailures++;
                consecutiveSuccesses = 0;
                _logger.LogWarning("❌ Ping thất bại: {error} - lần thứ {failures}", ex.Message, consecutiveFailures);
            }

            // Chọn interval dựa trên tình trạng server
            var interval = (consecutiveSuccesses >= 5 && consecutiveFailures == 0) ? SlowInterval : FastInterval;
            
            if (consecutiveFailures >= 3)
            {
                interval = TimeSpan.FromMinutes(5); // Ping thường xuyên hơn khi có vấn đề
                _logger.LogWarning("🔄 Server có vấn đề, tăng tần suất ping lên mỗi 5 phút");
            }

            try { await Task.Delay(interval, stoppingToken); }
            catch { break; }
        }
        
        _logger.LogInformation("SelfPingService đã dừng");
    }
}
