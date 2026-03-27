using FinShark.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FinShark.API.Controllers;

[ApiController]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public abstract class ApiControllerBase : ControllerBase;
