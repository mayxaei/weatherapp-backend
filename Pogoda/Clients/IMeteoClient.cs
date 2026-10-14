using Pogoda.Services;

namespace Pogoda.Clients
{
    public interface IMeteoClient
    {
        Task<OpenMeteoResponse> GetDataAsync(
        double latitude,
        double longitude,
        string? startDate = null,
        string? endDate = null);
    }
}