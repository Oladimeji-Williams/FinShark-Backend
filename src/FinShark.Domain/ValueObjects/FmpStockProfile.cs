namespace FinShark.Domain.ValueObjects;

public sealed class FmpStockProfile
{
    public string Symbol { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Beta { get; set; }
    public long VolAvg { get; set; }
    public long MktCap { get; set; }
    public decimal LastDiv { get; set; }
    public string Range { get; set; } = string.Empty;
    public decimal Changes { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string Cik { get; set; } = string.Empty;
    public string Isin { get; set; } = string.Empty;
    public string Cusip { get; set; } = string.Empty;
    public string Exchange { get; set; } = string.Empty;
    public string ExchangeShortName { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Ceo { get; set; } = string.Empty;
    public string Sector { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string FullTimeEmployees { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Zip { get; set; } = string.Empty;
    public decimal DcfDiff { get; set; }
    public decimal Dcf { get; set; }
    public string Image { get; set; } = string.Empty;
    public string IpoDate { get; set; } = string.Empty;
    public bool DefaultImage { get; set; }
    public bool IsEtf { get; set; }
    public bool IsActivelyTrading { get; set; }
    public bool IsAdr { get; set; }
    public bool IsFund { get; set; }
}
