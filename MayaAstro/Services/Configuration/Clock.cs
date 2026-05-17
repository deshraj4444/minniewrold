namespace MayaAstro.Services.Configuration
{
    public class Clock : IClock
    {
        public DateTime CurrentDateTime()
        {
            return DateTime.UtcNow;
        }
    }
}
