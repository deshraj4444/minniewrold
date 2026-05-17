using AstroPortal.Api.Contracts;

namespace AstroPortal.Api.Services;

public interface IHoroscopeService
{
    Task<HoroscopeResponse> GetDailyHoroscopeAsync(string sign, CancellationToken cancellationToken);
}
