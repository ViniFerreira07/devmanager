using DevManager.Api.Extensions;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/languages")]
public class LanguagesController : ControllerBase
{
    private readonly IProgrammingLanguageService _languageService;

    public LanguagesController(IProgrammingLanguageService languageService)
    {
        _languageService = languageService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProgrammingLanguageDto>>> GetAll(CancellationToken cancellationToken)
    {
        var languages = await _languageService.GetAllAsync(cancellationToken);
        return this.ToActionResult(languages);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProgrammingLanguageDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var language = await _languageService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(language);
    }

    [HttpPost]
    public async Task<ActionResult<ProgrammingLanguageDto>> Create([FromBody] CreateProgrammingLanguageRequest request, CancellationToken cancellationToken)
    {
        var language = await _languageService.CreateAsync(request, cancellationToken);
        return language.Success && language.Data is not null
            ? CreatedAtAction(nameof(GetById), new { id = language.Data.Id }, language.Data)
            : BadRequest(new { success = false, message = language.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProgrammingLanguageDto>> Update(Guid id, [FromBody] UpdateProgrammingLanguageRequest request, CancellationToken cancellationToken)
    {
        var language = await _languageService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(language);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _languageService.DeleteAsync(id, cancellationToken);
        return deleted.Success ? NoContent() : NotFound(new { success = false, message = deleted.Message });
    }
}
