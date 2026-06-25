using DevManager.Api.Extensions;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/states")]
public class StatesController : ControllerBase
{
    private readonly IStateService _stateService;

    public StatesController(IStateService stateService)
    {
        _stateService = stateService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StateDto>>> GetAll(CancellationToken cancellationToken)
    {
        var states = await _stateService.GetAllAsync(cancellationToken);
        return this.ToActionResult(states);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StateDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var state = await _stateService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(state);
    }

    [HttpPost]
    public async Task<ActionResult<StateDto>> Create([FromBody] CreateStateRequest request, CancellationToken cancellationToken)
    {
        var state = await _stateService.CreateAsync(request, cancellationToken);
        return state.Success && state.Data is not null
            ? CreatedAtAction(nameof(GetById), new { id = state.Data.Id }, state.Data)
            : BadRequest(new { success = false, message = state.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<StateDto>> Update(Guid id, [FromBody] UpdateStateRequest request, CancellationToken cancellationToken)
    {
        var state = await _stateService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(state);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _stateService.DeleteAsync(id, cancellationToken);
        return deleted.Success ? NoContent() : NotFound(new { success = false, message = deleted.Message });
    }
}
