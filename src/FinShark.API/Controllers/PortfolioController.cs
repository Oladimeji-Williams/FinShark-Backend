using FinShark.API.Extensions;
using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Commands.AddStockToPortfolio;
using FinShark.Application.Stocks.Commands.RemoveStockFromPortfolio;
using FinShark.Application.Stocks.Queries.GetPortfolioStocks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinShark.API.Controllers;

/// <summary>
/// Portfolio controller for user-specific portfolio operations
/// </summary>
[ApiController]
[Route("api/portfolio")]
[Produces("application/json")]
public sealed class PortfolioController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PortfolioController> _logger;

    public PortfolioController(IMediator mediator, ILogger<PortfolioController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<GetStockResponseDto>>>> GetPortfolio(CancellationToken cancellationToken)
    { 
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Unauthorized portfolio request: missing user id");
            return Unauthorized(ApiResponse<IEnumerable<GetStockResponseDto>>.FailureResponse("User not authenticated"));
        }

        _logger.LogInformation("GET /api/portfolio - Retrieving portfolio for user {UserId}", userId);
        var stocks = await _mediator.Send(new GetPortfolioStocksQuery(userId), cancellationToken);
        return Ok(ApiResponse<IEnumerable<GetStockResponseDto>>.SuccessResponse(stocks, "Portfolio retrieved successfully"));
    }

    [HttpPost("{stockId:int:min(1)}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> AddToPortfolio(int stockId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Unauthorized portfolio add: missing user id");
            return Unauthorized(ApiResponse<bool>.FailureResponse("User not authenticated"));
        }

        _logger.LogInformation("POST /api/portfolio/{StockId} - Adding stock to portfolio for user {UserId}", stockId, userId);
        var result = await _mediator.Send(new AddStockToPortfolioCommand(userId, stockId), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(result, result ? "Stock added to portfolio" : "Stock not added (already exists or not found)"));
    }

    [HttpDelete("{stockId:int:min(1)}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromPortfolio(int stockId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Unauthorized portfolio remove: missing user id");
            return Unauthorized(ApiResponse<bool>.FailureResponse("User not authenticated"));
        }

        _logger.LogInformation("DELETE /api/portfolio/{StockId} - Removing stock from portfolio for user {UserId}", stockId, userId);
        var result = await _mediator.Send(new RemoveStockFromPortfolioCommand(userId, stockId), cancellationToken);
        return Ok(ApiResponse<bool>.SuccessResponse(result, result ? "Stock removed from portfolio" : "Stock not found in portfolio"));
    }
}
