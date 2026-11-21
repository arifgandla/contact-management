using ContactManagement.Core.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace ContactManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Respond<T>(ApiResponse<T> response) =>
        response.Success ? Ok(response) : BadRequest(response);
}
