using System;

namespace MayaAstro.Services.Extensions
{
    public static class GuidExtention
    {
        public static Guid GetNewGuid()
        {
            return Guid.NewGuid();
        }

        public static Guid ConvertToGuid(this string value)
        {
            return new Guid(value); 
        }
        public static string ConvertGuidToString(this Guid value)
        {
            return Convert.ToString(value);
        }
    }
}