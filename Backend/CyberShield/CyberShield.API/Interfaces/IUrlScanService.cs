using CyberShield.API.Models;

namespace CyberShield.API.Interfaces
{
    public interface IUrlScanService
    {
        Task<ScanResult> ScanUrl(string url);
    }
}
