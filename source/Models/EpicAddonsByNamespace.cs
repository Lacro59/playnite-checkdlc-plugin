using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckDlc.Models
{
    class EpicAddonsByNamespace
    {
        public Data data { get; set; }
        public Extensions extensions { get; set; }
    }

    public class Element
    {
        public object countriesBlacklist { get; set; }
        public List<CustomAttribute> customAttributes { get; set; }
        public string description { get; set; }
        public object developer { get; set; }
        public DateTime effectiveDate { get; set; }
        public string id { get; set; }
        public bool isFeatured { get; set; }
        public List<KeyImage> keyImages { get; set; }
        public DateTime lastModifiedDate { get; set; }
        public string longDescription { get; set; }
        public string @namespace { get; set; }
        public string offerType { get; set; }
        public object productSlug { get; set; }
        public DateTime releaseDate { get; set; }
        public string status { get; set; }
        public object technicalDetails { get; set; }
        public string title { get; set; }
        public string urlSlug { get; set; }
        public Price price { get; set; }
    }

    public class CatalogOffers
    {
        public List<Element> elements { get; set; }
    }

    public class EpicCatalog
    {
        public CatalogOffers catalogOffers { get; set; }
    }

    public class Data
    {
        public EpicCatalog Catalog { get; set; }
    }

    public class Extensions
    {
    }
}
