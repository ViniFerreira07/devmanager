using DevManager.Api.Extensions;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/developers")]
public class DevelopersController : ControllerBase
{
    private readonly IDeveloperService _developerService;

    public DevelopersController(IDeveloperService developerService)
    {
        _developerService = developerService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<DeveloperDto>>> Get([FromQuery] DeveloperFilterRequest filter, CancellationToken cancellationToken)
    {
        var result = await _developerService.GetPagedAsync(filter, cancellationToken);
        return this.ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DeveloperDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var developer = await _developerService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(developer);
    }

    [HttpPost]
    public async Task<ActionResult<DeveloperDto>> Create([FromBody] CreateDeveloperRequest request, CancellationToken cancellationToken)
    {
        var developer = await _developerService.CreateAsync(request, cancellationToken);
        return developer.Success && developer.Data is not null
            ? CreatedAtAction(nameof(GetById), new { id = developer.Data.Id }, developer.Data)
            : BadRequest(new { success = false, message = developer.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DeveloperDto>> Update(Guid id, [FromBody] UpdateDeveloperRequest request, CancellationToken cancellationToken)
    {
        var developer = await _developerService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(developer);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _developerService.DeleteAsync(id, cancellationToken);
        return deleted.Success ? NoContent() : NotFound(new { success = false, message = deleted.Message });
    }
}
