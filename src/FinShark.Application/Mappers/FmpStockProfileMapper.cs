using FinShark.Application.Dtos;
using FinShark.Domain.ValueObjects;

namespace FinShark.Application.Mappers;

public static class FmpStockProfileMapper
{
    public static GetFullStockQuoteResponseDto ToDto(FmpStockProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile, nameof(profile));

        return new GetFullStockQuoteResponseDto
        {
            Symbol = profile.Symbol,
            Price = profile.Price,
            Beta = profile.Beta,
            VolAvg = profile.VolAvg,
            MktCap = profile.MktCap,
            LastDiv = profile.LastDiv,
            Range = profile.Range,
            Changes = profile.Changes,
            CompanyName = profile.CompanyName,
            Currency = profile.Currency,
            Cik = profile.Cik,
            Isin = profile.Isin,
            Cusip = profile.Cusip,
            Exchange = profile.Exchange,
            ExchangeShortName = profile.ExchangeShortName,
            Sector = profile.Sector,
            Website = profile.Website,
            Description = profile.Description,
            Ceo = profile.Ceo,
            Country = profile.Country,
            FullTimeEmployees = profile.FullTimeEmployees,
            Phone = profile.Phone,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            Zip = profile.Zip,
            DcfDiff = profile.DcfDiff,
            Dcf = profile.Dcf,
            Image = profile.Image,
            IpoDate = profile.IpoDate,
            DefaultImage = profile.DefaultImage,
            IsEtf = profile.IsEtf,
            IsActivelyTrading = profile.IsActivelyTrading,
            IsAdr = profile.IsAdr,
            IsFund = profile.IsFund
        };
    }
}
