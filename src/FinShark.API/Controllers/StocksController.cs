using FinShark.Application.Dtos;
using FinShark.Application.Stocks.Commands.CreateStock;
using FinShark.Application.Stocks.Commands.DeleteStock;
using FinShark.Application.Stocks.Commands.UpdateStock;
using FinShark.Application.Stocks.Queries.GetStocks;
using FinShark.Application.Stocks.Queries.GetStockById;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FinShark.API.Controllers;

/// <summary>
/// Stocks API Controller
/// Handles HTTP requests for stock operations
/// Implements CQRS pattern through MediatR
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class StocksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateStockRequestDto> _createStockValidator;
    private readonly IValidator<UpdateStockRequestDto> _updateStockValidator;
    private readonly ILogger<StocksController> _logger;

    public StocksController(
        IMediator mediator,
        IValidator<CreateStockRequestDto> createStockValidator,
        IValidator<UpdateStockRequestDto> updateStockValidator,
        ILogger<StocksController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _createStockValidator = createStockValidator ?? throw new ArgumentNullException(nameof(createStockValidator));
        _updateStockValidator = updateStockValidator ?? throw new ArgumentNullException(nameof(updateStockValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Get all stocks
    /// </summary>
    /// <returns>List of all stocks</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<StockDto>>>> GetAllStocks(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/stocks - Retrieving all stocks");

        try
        {
            var result = await _mediator.Send(new GetStocksQuery(), cancellationToken);
            var response = ApiResponse<IEnumerable<StockDto>>.SuccessResponse(result, "Stocks retrieved successfully");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stocks");
            throw;
        }
    }

    /// <summary>
    /// Create a new stock
    /// </summary>
    /// <param name="request">Stock creation request data</param>
    /// <returns>Created stock ID</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<int>>> CreateStock(
        [FromBody] CreateStockRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("POST /api/stocks - Creating new stock: {Symbol}", request.Symbol);

        // Validate request DTO
        var validationResult = await _createStockValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            var errorResponse = ApiResponse<int>.FailureResponse(errors);
            return BadRequest(errorResponse);
        }

        try
        {
            // Convert DTO to Command for MediatR
            var command = new CreateStockCommand(
                request.Symbol,
                request.CompanyName,
                request.CurrentPrice,
                request.Industry,
                request.MarketCap
            );

            var stockId = await _mediator.Send(command, cancellationToken);

            var response = ApiResponse<int>.SuccessResponse(stockId, "Stock created successfully");
            return CreatedAtAction(nameof(GetAllStocks), new { id = stockId }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating stock");
            throw;
        }
    }

    /// <summary>
    /// Get a stock by ID
    /// </summary>
    /// <param name="id">Stock ID</param>
    /// <returns>Stock details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<StockDto>>> GetStockById(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(ApiResponse<StockDto>.FailureResponse("Invalid stock ID"));

        _logger.LogInformation("GET /api/stocks/{Id} - Retrieving stock with ID: {StockId}", id, id);

        try
        {
            var result = await _mediator.Send(new GetStockByIdQuery(id), cancellationToken);
            var response = ApiResponse<StockDto>.SuccessResponse(result, "Stock retrieved successfully");
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Stock not found with ID: {StockId}", id);
            var errorResponse = ApiResponse<StockDto>.FailureResponse(ex.Message);
            return NotFound(errorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving stock with ID: {StockId}", id);
            throw;
        }
    }

    /// <summary>
    /// Update an existing stock
    /// </summary>
    /// <param name="id">Stock ID to update</param>
    /// <param name="request">Stock update data</param>
    /// <returns>Success response</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateStock(
        int id,
        [FromBody] UpdateStockRequestDto request,
        CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(ApiResponse<bool>.FailureResponse("Invalid stock ID"));
        if (request == null) throw new ArgumentNullException(nameof(request));

        _logger.LogInformation("PUT /api/stocks/{Id} - Updating stock with ID: {StockId}, Symbol: {Symbol}", id, id, request.Symbol);

        // Validate request DTO
        var validationResult = await _updateStockValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            var errorResponse = ApiResponse<bool>.FailureResponse(errors);
            return BadRequest(errorResponse);
        }

        try
        {
            // Convert DTO to Command for MediatR
            var command = new UpdateStockCommand(
                id,
                request.Symbol,
                request.CompanyName,
                request.CurrentPrice,
                request.Industry,
                request.MarketCap
            );

            var result = await _mediator.Send(command, cancellationToken);

            var response = ApiResponse<bool>.SuccessResponse(result, "Stock updated successfully");
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Stock not found for update with ID: {StockId}", id);
            var errorResponse = ApiResponse<bool>.FailureResponse(ex.Message);
            return NotFound(errorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock with ID: {StockId}", id);
            throw;
        }
    }

    /// <summary>
    /// Delete a stock
    /// </summary>
    /// <param name="id">Stock ID to delete</param>
    /// <returns>Success response</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteStock(
        int id,
        CancellationToken cancellationToken)
    {
        if (id <= 0) return BadRequest(ApiResponse<bool>.FailureResponse("Invalid stock ID"));

        _logger.LogInformation("DELETE /api/stocks/{Id} - Deleting stock with ID: {StockId}", id, id);

        try
        {
            var command = new DeleteStockCommand(id);
            var result = await _mediator.Send(command, cancellationToken);

            var response = ApiResponse<bool>.SuccessResponse(result, "Stock deleted successfully");
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Stock not found for deletion with ID: {StockId}", id);
            var errorResponse = ApiResponse<bool>.FailureResponse(ex.Message);
            return NotFound(errorResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting stock with ID: {StockId}", id);
            throw;
        }
    }
}