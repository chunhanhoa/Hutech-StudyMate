using System.Net.Http;
using Microsoft.Extensions.Hosting;

namespace Check.Services;

public class SelfPingService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SelfPingService> _logger;
    private readonly string? _targetUrl;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(14); // dưới 15'

    public SelfPingService(IHttpClientFactory httpClientFactory, ILogger<SelfPingService> logger, IConfiguration cfg)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        // Ưu tiên biến do bạn tự set; sau đó Render cung cấp.
        _targetUrl =
            cfg["https://hutech-studymate.onrender.com"] ??
            Environment.GetEnvironmentVariable("https://hutech-studymate.onrender.com") ??
            Environment.GetEnvironmentVariable("https://hutech-studymate.onrender.com"); // vd: https://tên.onrender.com
        if (!string.IsNullOrWhiteSpace(_targetUrl) && !_targetUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            _targetUrl = "https://" + _targetUrl.Trim().TrimEnd('/');
        if (!string.IsNullOrWhiteSpace(_targetUrl))
            _targetUrl = _targetUrl.TrimEnd('/') + "/";
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_targetUrl))
        {
            _logger.LogInformation("SelfPingService: không có URL (thiếu SELF_PING_URL hoặc RENDER_EXTERNAL_URL). Bỏ qua.");
            return;
        }

        _logger.LogInformation("SelfPingService: bắt đầu ping {url} mỗi {minutes} phút.", _targetUrl, Interval.TotalMinutes);

        // Delay nhỏ sau khởi động
        try { await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken); } catch { }

        while (!stoppingToken.IsCancellationRequested)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                using var req = new HttpRequestMessage(HttpMethod.Get, _targetUrl);
                var resp = await client.SendAsync(req, stoppingToken);
                sw.Stop();
                if (resp.IsSuccessStatusCode)
                    _logger.LogDebug("SelfPing OK {status} {ms}ms", (int)resp.StatusCode, sw.ElapsedMilliseconds);
                else
                    _logger.LogWarning("SelfPing FAIL {status} {ms}ms", (int)resp.StatusCode, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SelfPing lỗi");
            }

            try { await Task.Delay(Interval, stoppingToken); }
            catch { }
        }
    }
}
