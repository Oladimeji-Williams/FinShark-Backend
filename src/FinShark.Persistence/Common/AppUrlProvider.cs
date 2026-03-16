using FinShark.Application.Common;
using Microsoft.Extensions.Configuration;

namespace FinShark.Persistence.Common;

public sealed class AppUrlProvider(IConfiguration configuration) : IAppUrlProvider
{
    public string GetClientUrl()
    {
        return configuration["AppSettings:ClientUrl"]
            ?? configuration["AppUrl"]
            ?? "http://localhost:5000";
    }
}
