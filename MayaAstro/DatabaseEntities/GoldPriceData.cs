using System;
namespace MayaAstro.DatabaseEntities
{
    public class GoldPriceData
    {
        public int Id { get; set; }

        // Spot Rates
        public decimal GoldSpot { get; set; }
        public decimal SilverSpot { get; set; }
        public decimal InrSpot { get; set; }

        // Dealer Gold Rates
        public decimal Gold995InclGstBuy { get; set; }
        public decimal Gold995InclGstSell { get; set; }

        public decimal Gold995Below50Buy { get; set; }
        public decimal Gold995Below50Sell { get; set; }

        public decimal Gold995Below10Buy { get; set; }
        public decimal Gold995Below10Sell { get; set; }

        // Silver Dealer Rates
        public decimal Silver30KgImportedBuy { get; set; }
        public decimal Silver30KgImportedSell { get; set; }

        public decimal SilverBelow30KgBuy { get; set; }
        public decimal SilverBelow30KgSell { get; set; }

        // Futures
        public decimal GoldFutureBuy { get; set; }
        public decimal GoldFutureSell { get; set; }

        public decimal SilverFutureBuy { get; set; }
        public decimal SilverFutureSell { get; set; }

        // Next Contract
        public decimal GoldNextBuy { get; set; }
        public decimal GoldNextSell { get; set; }

        public decimal SilverNextBuy { get; set; }
        public decimal SilverNextSell { get; set; }

        public DateTime UpdatedAt { get; set; }

    }
}

