using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Commands.CreateStock;
using FinShark.Application.Stocks.Commands.DeleteStock;
using FinShark.Application.Stocks.Commands.UpdateStock;
using FinShark.Application.Stocks.Queries.GetStockById;
using FinShark.Application.Stocks.Queries.GetStockQuoteFromFmp;
using FinShark.Application.Stocks.Queries.GetStocks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinShark.API.Controllers;

/// <summary>
/// Stocks API Controller
/// Handles HTTP requests for stock operations
/// Implements CQRS pattern through MediatR
/// </summary>
[Route("api/[controller]")]
public sealed class StocksController : ApiControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StocksController> _logger;

    public StocksController(
        IMediator mediator,
        ILogger<StocksController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get stocks with optional filtering, sorting, and pagination
    /// </summary>
    /// <returns>List of stocks</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<GetStockResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<GetStockResponseDto>>>> GetAllStocks(
        [FromQuery] StockQueryRequestDto queryParameters,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/stocks - Retrieving stocks");

        var parameters = queryParameters ?? new StockQueryRequestDto();

        var result = await _mediator.Send(new GetStocksQuery(parameters), cancellationToken);
        var response = ApiResponse<PagedResult<GetStockResponseDto>>.SuccessResponse(result, "Stocks retrieved successfully");
        return Ok(response);
    }

    /// <summary>
    /// Create a new stock
    /// </summary>
    /// <param name="request">Stock creation request data</param>
    /// <returns>Created stock with ID</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CreateStockResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateStockResponseDto>>> CreateStock(
        [FromBody] CreateStockRequestDto createStockRequestDto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/stocks - Creating new stock: {Symbol}", createStockRequestDto.Symbol);

        // Convert DTO to Command for MediatR
        var command = new CreateStockCommand(
            createStockRequestDto.Symbol,
            createStockRequestDto.CompanyName,
            createStockRequestDto.CurrentPrice,
            createStockRequestDto.Sector,
            createStockRequestDto.MarketCap
        );

        var result = await _mediator.Send(command, cancellationToken);

        var response = ApiResponse<CreateStockResponseDto>.SuccessResponse(result, "Stock created successfully");
        return CreatedAtAction(nameof(GetStockById), new { id = result.Id }, response);
    }

    /// <summary>
    /// Get a stock by ID
    /// </summary>
    /// <param name="id">Stock ID</param>
    /// <returns>Stock details</returns>
    [HttpGet("{id:int:min(1)}")]
    [ProducesResponseType(typeof(ApiResponse<GetStockResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetStockResponseDto>>> GetStockById(
        int id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/stocks/{Id} - Retrieving stock with ID: {StockId}", id, id);

        var result = await _mediator.Send(new GetStockByIdQuery(id), cancellationToken);
        var response = ApiResponse<GetStockResponseDto>.SuccessResponse(result, "Stock retrieved successfully");
        return Ok(response);
    }

    /// <summary>
    /// Get a stock quote from Financial Modeling Prep by symbol
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    [HttpGet("quote/{symbol}")]
    [ProducesResponseType(typeof(ApiResponse<GetStockResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetStockResponseDto>>> GetStockQuoteFromFmp(
        string symbol,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/stocks/quote/{Symbol} - Retrieving FMP quote", symbol);

        var result = await _mediator.Send(new GetStockQuoteFromFmpQuery(symbol), cancellationToken);
        var response = ApiResponse<GetStockResponseDto>.SuccessResponse(result, "Stock quote retrieved from FMP.");
        return Ok(response);
    }

    /// <summary>
    /// Get full FMP company profile for a stock symbol
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    [HttpGet("quote/full/{symbol}")]
    [ProducesResponseType(typeof(ApiResponse<GetFullStockQuoteResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<GetFullStockQuoteResponseDto>>> GetFullStockQuoteFromFmp(
        string symbol,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/stocks/quote/full/{Symbol} - Retrieving full FMP profile", symbol);

        var result = await _mediator.Send(new GetFullStockQuoteFromFmpQuery(symbol), cancellationToken);
        var response = ApiResponse<GetFullStockQuoteResponseDto>.SuccessResponse(result, "Full FMP stock profile retrieved.");
        return Ok(response);
    }

    /// <summary>
    /// Update an existing stock
    /// </summary>
    /// <param name="id">Stock ID to update</param>
    /// <param name="request">Stock update data - only provided fields will be updated</param>
    /// <returns>Success response</returns>
    [HttpPatch("{id:int:min(1)}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStock(
        int id,
        [FromBody] UpdateStockRequestDto updateStockRequestDto,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("PATCH /api/stocks/{Id} - Updating stock with ID: {StockId}", id, id);

        // Convert DTO to Command for MediatR
        var command = new UpdateStockCommand(
            Id: id,
            Symbol: updateStockRequestDto.Symbol,
            CompanyName: updateStockRequestDto.CompanyName,
            CurrentPrice: updateStockRequestDto.CurrentPrice,
            Sector: updateStockRequestDto.Sector,
            MarketCap: updateStockRequestDto.MarketCap
        );

        var result = await _mediator.Send(command, cancellationToken);

        var response = ApiResponse<bool>.SuccessResponse(result, "Stock updated successfully");
        return Ok(response);
    }

    /// <summary>
    /// Delete a stock
    /// </summary>
    /// <param name="id">Stock ID to delete</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id:int:min(1)}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteStock(
        int id,
        CancellationToken cancellationToken,
        [FromQuery] bool hardDelete = false)
    {
        _logger.LogInformation("DELETE /api/stocks/{Id} - Deleting stock with ID: {StockId}, hardDelete={HardDelete}", id, id, hardDelete);

        if (hardDelete && !User.IsInRole("Admin"))
        {
            _logger.LogWarning("Hard delete attempt by non-admin for stock {StockId}", id);
            return Forbid();
        }

        var command = new DeleteStockCommand(id, HardDelete: hardDelete);
        var result = await _mediator.Send(command, cancellationToken);

        var response = ApiResponse<bool>.SuccessResponse(result, hardDelete ? "Stock hard deleted successfully" : "Stock soft deleted successfully");
        return Ok(response);
    }
}
