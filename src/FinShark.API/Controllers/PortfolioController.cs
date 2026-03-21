using FinShark.API.Extensions;
using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Commands.AddStockToPortfolio;
using FinShark.Application.Stocks.Commands.RemoveStockFromPortfolio;
using FinShark.Application.Stocks.Queries.GetPortfolioStocks;
using FinShark.Domain.Exceptions;
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

    [HttpPost("symbol/{symbol}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> AddToPortfolioBySymbol(string symbol, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Unauthorized portfolio add by symbol: missing user id");
            return Unauthorized(ApiResponse<bool>.FailureResponse("User not authenticated"));
        }

        if (string.IsNullOrWhiteSpace(symbol))
        {
            _logger.LogWarning("Bad request for portfolio add by symbol: missing symbol");
            return BadRequest(ApiResponse<bool>.FailureResponse("Symbol is required"));
        }

        _logger.LogInformation("POST /api/portfolio/symbol/{Symbol} - Adding stock to portfolio for user {UserId}", symbol, userId);
        try
        {
            var result = await _mediator.Send(new AddStockToPortfolioBySymbolCommand(userId, symbol), cancellationToken);
            return Ok(ApiResponse<bool>.SuccessResponse(result, result ? "Stock added to portfolio" : "Stock not added (already exists or not found)"));
        }
        catch (StockNotFoundException ex)
        {
            _logger.LogWarning(ex, "Stock not found during portfolio-by-symbol add for user {UserId}", userId);
            return NotFound(ApiResponse<bool>.FailureResponse(ex.Message));
        }
        catch (FMPServiceException ex)
        {
            _logger.LogWarning(ex, "FMP service error while adding stock by symbol for user {UserId}", userId);
            return BadRequest(ApiResponse<bool>.FailureResponse(ex.Message, ex.FmpStatusCode, ex.Suggestion));
        }
    }

    [HttpDelete("{stockId:int:min(1)}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveFromPortfolio(
        int stockId,
        CancellationToken cancellationToken,
        [FromQuery] bool hardDelete = false)
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            _logger.LogWarning("Unauthorized portfolio remove: missing user id");
            return Unauthorized(ApiResponse<bool>.FailureResponse("User not authenticated"));
        }

        if (hardDelete && !User.IsInRole("Admin"))
        {
            _logger.LogWarning("Hard delete attempt by non-admin for portfolio item with stock {StockId}", stockId);
            return Forbid();
        }

        _logger.LogInformation("DELETE /api/portfolio/{StockId} - Removing stock from portfolio for user {UserId}, hardDelete={HardDelete}", stockId, userId, hardDelete);
        var result = await _mediator.Send(new RemoveStockFromPortfolioCommand(userId, stockId, HardDelete: hardDelete), cancellationToken);

        return Ok(ApiResponse<bool>.SuccessResponse(result, result ? (hardDelete ? "Stock hard removed from portfolio" : "Stock soft removed from portfolio") : "Stock not found in portfolio"));
    }
}
