namespace CyberShield.API.Models
{
    public class ScanResult
    {
        public int Id { get; set; }

        public string Url { get; set; } = string.Empty;

        public int RiskScore { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime ScannedAt { get; set; }
    }
}
