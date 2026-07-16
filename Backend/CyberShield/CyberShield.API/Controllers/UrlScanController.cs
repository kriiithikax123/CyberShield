using Microsoft.AspNetCore.Mvc;
using CyberShield.API.Interfaces;
using CyberShield.API.DTOs;

namespace CyberShield.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlScanController : ControllerBase
    {
        private readonly IUrlScanService _urlScanService;

        public UrlScanController(IUrlScanService urlScanService)
        {
            _urlScanService = urlScanService;
        }

        [HttpPost]
        public async Task<IActionResult> ScanUrl(UrlScanRequest request)
        {
            var result = await _urlScanService.ScanUrl(request.Url);

            return Ok(result);
        }
    }
}
