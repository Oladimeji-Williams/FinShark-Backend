namespace FinShark.Application.Dtos;

/// <summary>
/// Response DTO for stock creation
/// Returns the newly created stock's ID
/// </summary>
public sealed class CreateStockResponseDto
{
    public required int Id { get; init; }
}
