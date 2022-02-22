using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckDlc.Models
{
    class EpicEntitledOfferItems
    {
        public OfferItemsData data { get; set; }
    }

    public class EntitledOfferItems
    {
        public string @namespace { get; set; }
        public string offerId { get; set; }
        public bool entitledToAllItemsInOffer { get; set; }
        public bool entitledToAnyItemInOffer { get; set; }
    }

    public class Launcher
    {
        public EntitledOfferItems entitledOfferItems { get; set; }
    }

    public class OfferItemsData
    {
        public Launcher Launcher { get; set; }
    }
}
