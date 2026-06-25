using DevManager.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace DevManager.Api.Extensions;

public static class ControllerResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.Success && result.Data is not null)
        {
            return controller.Ok(result.Data);
        }

        return result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? controller.NotFound(new { success = false, message = result.Message })
            : controller.BadRequest(new { success = false, message = result.Message });
    }
}
