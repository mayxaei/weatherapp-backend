using Pogoda.Services;

namespace Pogoda.Clients
{
    public class MeteoResolver : IMeteoResolver
    {
        private readonly OpenMeteoClient _openMeteoClient;
        private readonly ArchiveOpenMeteoClient _archiveOpenMeteoClient;

        public MeteoResolver(
            OpenMeteoClient openMeteoClient,
            ArchiveOpenMeteoClient archiveOpenMeteoClient)
        {
            _openMeteoClient = openMeteoClient;
            _archiveOpenMeteoClient = archiveOpenMeteoClient;
        }

        public async Task<OpenMeteoResponse> GetDataAsync(
            double latitude,
            double longitude,
            string? startDate = null,
            string? endDate = null)
        {
            if (startDate == null && endDate == null)
            {
                return await _openMeteoClient.GetDataAsync(
                    latitude,
                    longitude);
            }

            return await _archiveOpenMeteoClient.GetDataAsync(
                latitude,
                longitude,
                startDate,
                endDate);
        }
    }
}