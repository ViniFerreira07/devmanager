using DevManager.Api.Extensions;
using DevManager.Application.DTOs;
using DevManager.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/cities")]
public class CitiesController : ControllerBase
{
    private readonly ICityService _cityService;

    public CitiesController(ICityService cityService)
    {
        _cityService = cityService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CityDto>>> GetAll(CancellationToken cancellationToken)
    {
        var cities = await _cityService.GetAllAsync(cancellationToken);
        return this.ToActionResult(cities);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CityDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var city = await _cityService.GetByIdAsync(id, cancellationToken);
        return this.ToActionResult(city);
    }

    [HttpPost]
    public async Task<ActionResult<CityDto>> Create([FromBody] CreateCityRequest request, CancellationToken cancellationToken)
    {
        var city = await _cityService.CreateAsync(request, cancellationToken);
        return city.Success && city.Data is not null
            ? CreatedAtAction(nameof(GetById), new { id = city.Data.Id }, city.Data)
            : BadRequest(new { success = false, message = city.Message });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CityDto>> Update(Guid id, [FromBody] UpdateCityRequest request, CancellationToken cancellationToken)
    {
        var city = await _cityService.UpdateAsync(id, request, cancellationToken);
        return this.ToActionResult(city);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _cityService.DeleteAsync(id, cancellationToken);
        return deleted.Success ? NoContent() : NotFound(new { success = false, message = deleted.Message });
    }
}
