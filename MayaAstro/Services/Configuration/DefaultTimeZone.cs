using Microsoft.Extensions.Options;
using System;

namespace MayaAstro.Services.Configuration
{
    public class DefaultTimeZone : IDefaultTimeZone
    {
        private readonly IOptions<App> _appDefaultTimeZone;
        public DefaultTimeZone(IOptions<App> appDefaultTimeZone)
        {
            _appDefaultTimeZone = appDefaultTimeZone;
        }
        public DateTime ConvertToDefaultTimeZone(DateTime dateValue)
        {
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(_appDefaultTimeZone.Value.DefaultTimeZone);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(dateValue, DateTimeKind.Utc), timeZoneInfo);
        }
    }
}
