using System;

namespace MayaAstro.Services.Configuration
{
    public interface IDefaultTimeZone
    {
        DateTime ConvertToDefaultTimeZone(DateTime dateValue);
    }
}
