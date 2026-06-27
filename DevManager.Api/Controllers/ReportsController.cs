using DevManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IDeveloperService _developerService;

    public ReportsController(IDeveloperService developerService)
    {
        _developerService = developerService;
    }

    [HttpGet("developers")]
    public async Task<IActionResult> GetDevelopersReport(CancellationToken cancellationToken)
    {
        var pdfBytes = await _developerService.GenerateReportPdfAsync(cancellationToken);
        return pdfBytes.Success && pdfBytes.Data is not null
            ? File(pdfBytes.Data, "application/pdf", "relatorio-desenvolvedores.pdf")
            : BadRequest(new { success = false, message = pdfBytes.Message });
    }
}
