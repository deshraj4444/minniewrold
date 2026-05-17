using System;

namespace MayaAstro.DatabaseEntities
{
    public class GoldSettings
    {
        public int Id { get; set; }

        // Gold
        public decimal? GoldPremiumPerGram { get; set; }
        public decimal? GoldGstAdjustment { get; set; }
        public decimal? GoldDealerMargin { get; set; }

        // Silver
        public decimal? SilverPremiumPerKg { get; set; }
        public decimal? SilverGstAdjustment { get; set; }
        public decimal? SilverDealerMargin { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}