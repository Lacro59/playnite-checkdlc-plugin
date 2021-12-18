using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckDlc.Models
{
    public class GogPriceResult
    {
        public List<GogItem> items { get; set; }
    }

    public class Self
    {
        public string href { get; set; }
    }

    public class Links
    {
        public Self self { get; set; }
    }

    public class Currency
    {
        public string code { get; set; }
    }

    public class GogPrice
    {
        public Currency currency { get; set; }
        public string basePrice { get; set; }
        public string finalPrice { get; set; }
        public string bonusWalletFunds { get; set; }
    }

    public class GogProduct
    {
        public int id { get; set; }
    }

    public class Embedded
    {
        public List<GogPrice> prices { get; set; }
        public GogProduct product { get; set; }
    }

    public class GogItem
    {
        public Links _links { get; set; }
        public Embedded _embedded { get; set; }
    }
}
