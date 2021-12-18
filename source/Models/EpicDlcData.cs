using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckDlc.Models
{
    public class EpicDlcData
    {
        public RegionRestrictions regionRestrictions { get; set; }
        public Image image { get; set; }
        public string _type { get; set; }
        public string @namespace { get; set; }
        public string description { get; set; }
        public string offerId { get; set; }
        public string tag { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string slug { get; set; }
    }

    public class RegionRestrictions
    {
        public string _type { get; set; }
    }

    public class Image
    {
        public string src { get; set; }
        public string _type { get; set; }
    }
}
