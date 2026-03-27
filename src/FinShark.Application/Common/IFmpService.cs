using FinShark.Domain.Entities;
using FinShark.Domain.ValueObjects;

namespace FinShark.Application.Common;

public interface IFmpService
{
    Task<Stock?> GetStockQuoteAsync(string symbol, CancellationToken cancellationToken = default);
    Task<FmpStockProfile> GetFullStockProfileAsync(string symbol, CancellationToken cancellationToken = default);
}
