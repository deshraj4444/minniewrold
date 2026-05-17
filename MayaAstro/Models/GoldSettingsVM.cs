using System;

namespace MayaAstro.Models
{
    public class GoldSettingsVM
    {
        public int Id { get; set; }

        // Gold Settings
        public decimal GoldPremiumPerGram { get; set; }
        public decimal GoldGstAdjustment { get; set; }
        public decimal GoldDealerMargin { get; set; }

        // Silver Settings
        public decimal SilverPremiumPerKg { get; set; }
        public decimal SilverGstAdjustment { get; set; }
        public decimal SilverDealerMargin { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}