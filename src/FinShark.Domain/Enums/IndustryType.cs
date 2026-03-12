namespace FinShark.Domain.Enums;

/// <summary>
/// Industry classification for stocks
/// Represents the sector/industry in which a stock operates
/// </summary>
public enum IndustryType
{
    /// <summary>Technology and software companies</summary>
    Technology = 1,

    /// <summary>Financial services and banking</summary>
    Finance = 2,

    /// <summary>Healthcare and pharmaceutical companies</summary>
    Healthcare = 3,

    /// <summary>Energy, oil, and utilities</summary>
    Energy = 4,

    /// <summary>Retail and consumer goods</summary>
    Retail = 5,

    /// <summary>Manufacturing and industrial</summary>
    Manufacturing = 6,

    /// <summary>Real estate and property</summary>
    RealEstate = 7,

    /// <summary>Telecommunications</summary>
    Telecommunications = 8,

    /// <summary>Transportation and logistics</summary>
    Transportation = 9,

    /// <summary>Consumer discretionary and entertainment</summary>
    Entertainment = 10,

    /// <summary>Other or unclassified industry</summary>
    Other = 11
}
