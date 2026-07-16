using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using CyberShield.API.Interfaces;
using CyberShield.API.Models;
using CyberShield.API.Data;

namespace CyberShield.API.Services
{
    public class UrlScanService : IUrlScanService
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly VirusTotalSettings _virusTotalSettings;

        public UrlScanService(
    ApplicationDbContext context,
    HttpClient httpClient,
    IOptions<VirusTotalSettings> virusTotalOptions)
        {
            _context = context;
            _httpClient = httpClient;
            _virusTotalSettings = virusTotalOptions.Value;
        }

        private async Task<string?> SubmitUrlToVirusTotal(string url)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "x-apikey",
                _virusTotalSettings.ApiKey);

            var content = new FormUrlEncodedContent(
                new[]
                {
            new KeyValuePair<string, string>("url", url)
                });

            var response = await _httpClient.PostAsync(
                "https://www.virustotal.com/api/v3/urls",
                content);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            using var document = System.Text.Json.JsonDocument.Parse(json);

            return document.RootElement
                           .GetProperty("data")
                           .GetProperty("id")
                           .GetString();
        }

        private async Task<(int malicious, int suspicious)> GetVirusTotalResult(string analysisId)
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add(
                "x-apikey",
                _virusTotalSettings.ApiKey);

            var response = await _httpClient.GetAsync(
                $"https://www.virustotal.com/api/v3/analyses/{analysisId}");

            if (!response.IsSuccessStatusCode)
                return (0, 0);

            var json = await response.Content.ReadAsStringAsync();

            using var document = System.Text.Json.JsonDocument.Parse(json);

            var stats = document.RootElement
                                .GetProperty("data")
                                .GetProperty("attributes")
                                .GetProperty("stats");

            int malicious = stats.GetProperty("malicious").GetInt32();
            int suspicious = stats.GetProperty("suspicious").GetInt32();

            return (malicious, suspicious);
        }

        public async Task<ScanResult> ScanUrl(string url)
        {
            string? analysisId = await SubmitUrlToVirusTotal(url);

            int malicious = 0;
            int suspicious = 0;

            if (!string.IsNullOrEmpty(analysisId))
            {
                // VirusTotal may take a moment to finish the analysis
                await Task.Delay(3000);

                var vtResult = await GetVirusTotalResult(analysisId);

                malicious = vtResult.malicious;
                suspicious = vtResult.suspicious;
            }

            int riskScore = 0;
            string status = "Safe";

            if (url.Contains("login", StringComparison.OrdinalIgnoreCase))
                riskScore += 20;

            if (url.Contains("verify", StringComparison.OrdinalIgnoreCase))
                riskScore += 30;

            if (url.Contains("secure", StringComparison.OrdinalIgnoreCase))
                riskScore += 10;

            if (url.StartsWith("http://"))
                riskScore += 40;

            if (riskScore >= 50)
                status = "Suspicious";

            riskScore += malicious * 20;
            riskScore += suspicious * 10;

            if (malicious > 0)
            {
                status = "Malicious";
            }
            else if (riskScore >= 50)
            {
                status = "Suspicious";
            }

            var result = new ScanResult
            {
                Url = url,
                RiskScore = riskScore,
                Status = status,
                ScannedAt = DateTime.Now
            };

            _context.ScanResults.Add(result);
            await _context.SaveChangesAsync();

            return result;
        }
    }
}