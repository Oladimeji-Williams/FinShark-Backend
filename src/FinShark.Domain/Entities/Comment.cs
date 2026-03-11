namespace FinShark.Domain.Entities;

public class Comment: BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int? StockId { get; set; }
    public Stock? Stock { get; set; }
}
