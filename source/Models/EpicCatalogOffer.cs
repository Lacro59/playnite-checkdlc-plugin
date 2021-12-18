using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckDlc.Models
{
    public class EpicCatalogOffer
    {
        public Catalog Catalog { get; set; }
    }

    public class KeyImage
    {
        public string type { get; set; }
        public string url { get; set; }
    }

    public class Seller
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class Tag
    {
        public string id { get; set; }
        public string name { get; set; }
        public string groupName { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public string @namespace { get; set; }
        public object releaseInfo { get; set; }
    }

    public class CustomAttribute
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class Category
    {
        public string path { get; set; }
    }

    public class AgeGating
    {
        public int? ageControl { get; set; }
        public object descriptor { get; set; }
        public object elements { get; set; }
        public string gameRating { get; set; }
        public string ratingImage { get; set; }
        public string ratingSystem { get; set; }
        public string title { get; set; }
    }

    public class Mappings
    {
        public string cmsSlug { get; set; }
        public string offerId { get; set; }
        public DateTime createdDate { get; set; }
        public object deletedDate { get; set; }
        public Mappings mappings { get; set; }
        public string pageSlug { get; set; }
        public string pageType { get; set; }
        public string productId { get; set; }
        public string sandboxId { get; set; }
        public DateTime updatedDate { get; set; }
    }

    public class CatalogNs
    {
        public List<AgeGating> ageGatings { get; set; }
        public string displayName { get; set; }
        //public List<Mapping> mappings { get; set; }
        public string store { get; set; }
    }

    public class OfferMapping
    {
        public DateTime createdDate { get; set; }
        public object deletedDate { get; set; }
        public Mappings mappings { get; set; }
        public string pageSlug { get; set; }
        public string pageType { get; set; }
        public string productId { get; set; }
        public string sandboxId { get; set; }
        public DateTime updatedDate { get; set; }
    }

    public class CurrencyInfo
    {
        public int decimals { get; set; }
    }

    public class FmtPrice
    {
        public string originalPrice { get; set; }
        public string discountPrice { get; set; }
        public string intermediatePrice { get; set; }
    }

    public class TotalPrice
    {
        public int discountPrice { get; set; }
        public int originalPrice { get; set; }
        public int voucherDiscount { get; set; }
        public int discount { get; set; }
        public string currencyCode { get; set; }
        public CurrencyInfo currencyInfo { get; set; }
        public FmtPrice fmtPrice { get; set; }
    }

    public class LineOffer
    {
        public List<object> appliedRules { get; set; }
    }

    public class Price
    {
        public TotalPrice totalPrice { get; set; }
        public List<LineOffer> lineOffers { get; set; }
    }

    public class CatalogOffer
    {
        public string title { get; set; }
        public string id { get; set; }
        public string @namespace { get; set; }
        public List<string> countriesBlacklist { get; set; }
        public object countriesWhitelist { get; set; }
        public object developerDisplayName { get; set; }
        public string description { get; set; }
        public DateTime effectiveDate { get; set; }
        public DateTime? expiryDate { get; set; }
        public bool allowPurchaseForPartialOwned { get; set; }
        public object externalLinks { get; set; }
        public bool isCodeRedemptionOnly { get; set; }
        public List<KeyImage> keyImages { get; set; }
        public object longDescription { get; set; }
        public Seller seller { get; set; }
        public string productSlug { get; set; }
        public object publisherDisplayName { get; set; }
        public DateTime? releaseDate { get; set; }
        public string urlSlug { get; set; }
        public object url { get; set; }
        public List<Tag> tags { get; set; }
        public List<Item> items { get; set; }
        public List<CustomAttribute> customAttributes { get; set; }
        public List<Category> categories { get; set; }
        public CatalogNs catalogNs { get; set; }
        public List<OfferMapping> offerMappings { get; set; }
        public DateTime? pcReleaseDate { get; set; }
        public bool? prePurchase { get; set; }
        public Price price { get; set; }
        public object allDependNsOfferIds { get; set; }
        public List<object> majorNsOffers { get; set; }
        public List<object> subNsOffers { get; set; }
        public string status { get; set; }
    }

    public class Catalog
    {
        public CatalogOffer catalogOffer { get; set; }
    }
}
