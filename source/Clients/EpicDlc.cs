using CheckDlc.Models;
using CommonPlayniteShared.PluginLibrary.EpicLibrary;
using CommonPlayniteShared.PluginLibrary.EpicLibrary.Models;
using CommonPlayniteShared.PluginLibrary.EpicLibrary.Services;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static CommonPluginsShared.PlayniteTools;

namespace CheckDlc.Clients
{
    class EpicDlc : GenericDlc
    {
        public static bool SettingsOpen = false;

        private static EpicAccountClient _EpicAPI;
        private static EpicAccountClient EpicAPI
        {
            get
            {
                if (_EpicAPI == null)
                {
                    _EpicAPI = new EpicAccountClient(
                        PluginDatabase.PlayniteApi,
                        PluginDatabase.Paths.PluginUserDataPath + "\\..\\00000002-DBD1-46C6-B5D0-B1BA559D10E4\\tokens.json"
                    );
                }
                return _EpicAPI;
            }

            set
            {
                _EpicAPI = value;
            }
        }

        private static bool? _IsConnected = null;
        private static bool IsConnected
        {
            get
            {
                if (_IsConnected == null || SettingsOpen)
                {
                    try
                    {
                        _IsConnected = EpicAPI.GetIsUserLoggedIn();
                        SettingsOpen = false;
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, $"Epic", true, PluginDatabase.PluginName);
                    }
                }
                return (bool)_IsConnected;
            }
        }


        private string UrlBase = "https://www.epicgames.com";
        private string UrlGameDetails = "https://store-content.ak.epicgames.com/api/{0}/content/products/{1}";
        private string UrlStore = "https://www.epicgames.com/store/{0}/p/{1}";
        private string GraphQLEndpoint = @"https://graphql.epicgames.com/graphql";


        public EpicDlc() : base("Epic", CodeLang.GetEpicLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language))
        {

        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> GameDlc = new List<Dlc>();
            string LocalLangFinal = LocalLang;

            try
            {
                if (IsConnected)
                {
                    OauthResponse tokens = EpicAPI.loadTokens();
                    
                    string gameNamespace = GetNameSpace(game.Name);
                    if (gameNamespace.IsNullOrEmpty())
                    {
                        logger.Warn($"namespace is empty for {game.Name}");
                        return null;
                    }

                    // List DLC
                    EpicAddonsByNamespace dataDLC = GetAddonsByNamespace(gameNamespace).GetAwaiter().GetResult();
                    foreach(Element el in dataDLC?.data?.Catalog?.catalogOffers?.elements)
                    {
                        EpicEntitledOfferItems ownedDLC = GetEntitledOfferItems(gameNamespace, el.id, tokens.access_token).GetAwaiter().GetResult();
                        Dlc dlc = new Dlc
                        {
                            DlcId = el.id,
                            Name = el.title,
                            Description = el.description,
                            Image = el.keyImages.Find(x => x.type.IsEqual("OfferImageWide")).url.Replace("\u002F", "/"),
                            Link = string.Format(UrlStore, LocalLangFinal, el.urlSlug),
                            IsOwned = ownedDLC.data.Launcher.entitledOfferItems.entitledToAllItemsInOffer,
                            Price = el.price.totalPrice.fmtPrice.discountPrice,
                            PriceBase = el.price.totalPrice.fmtPrice.originalPrice,
                        };

                        GameDlc.Add(dlc);
                    }
                }
                else
                {
                    ShowNotificationPluginNoAuthenticate(string.Format(resources.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.EpicLibrary);
                }
            }
            catch (Exception ex)
            {
                ShowNotificationPluginError(ex);
            }

            logger.Info($"Find {GameDlc?.Count} dlc");
            return GameDlc;
        }


        private string GetProductSlug(string Name)
        {
            string ProductSlug = string.Empty;
            using (WebStoreClient client = new WebStoreClient())
            {
                List<WebStoreModels.QuerySearchResponse.SearchStoreElement> catalogs = client.QuerySearch(Name).GetAwaiter().GetResult();
                if (catalogs.HasItems())
                {
                    catalogs = catalogs.OrderBy(x => x.title.Length).ToList();
                    WebStoreModels.QuerySearchResponse.SearchStoreElement catalog = catalogs.FirstOrDefault(a => a.title.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
                    if (catalog == null)
                    {
                        catalog = catalogs[0];
                    }

                    ProductSlug = catalog?.productSlug?.Replace("/home", string.Empty);
                }
            }
            return ProductSlug;
        }

        private string GetNameSpace(string Name)
        {
            string NameSpace = string.Empty;
            using (WebStoreClient client = new WebStoreClient())
            {
                List<WebStoreModels.QuerySearchResponse.SearchStoreElement> catalogs = client.QuerySearch(Name).GetAwaiter().GetResult();
                if (catalogs.HasItems())
                {
                    catalogs = catalogs.OrderBy(x => x.title.Length).ToList();
                    WebStoreModels.QuerySearchResponse.SearchStoreElement catalog = catalogs.FirstOrDefault(a => a.title.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
                    if (catalog == null)
                    {
                        catalog = catalogs[0];
                    }

                    NameSpace = catalog?.epicNamespace;
                }
            }
            return NameSpace;
        }


        private async Task<EpicAddonsByNamespace> GetAddonsByNamespace(string epic_namespace)
        {
            var query = new QueryAddonsByNamespace();
            query.variables.epic_namespace = epic_namespace;
            query.variables.locale = LocalLang;
            query.variables.country = CodeLang.GetOriginLangCountry(LocalLang);
            var content = new StringContent(Serialization.ToJson(query), Encoding.UTF8, "application/json");
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.PostAsync(GraphQLEndpoint, content);
            var str = await response.Content.ReadAsStringAsync();
            var data = Serialization.FromJson<EpicAddonsByNamespace>(str);
            return data;
        }

        private async Task<EpicEntitledOfferItems> GetEntitledOfferItems(string productNameSpace, string offerId, string token)
        {
            var query = new QueryEntitledOfferItems();
            query.variables.productNameSpace = productNameSpace;
            query.variables.offerId = offerId;
            var content = new StringContent(Serialization.ToJson(query), Encoding.UTF8, "application/json");
            string str = await Web.PostStringData(GraphQLEndpoint, token, content);
            var data = Serialization.FromJson<EpicEntitledOfferItems>(str);
            return data;
        }
    }


    public class QueryAddonsByNamespace
    {
        public class Variables
        {
            public string locale = "en-US";
            public string country = "US";
            public string epic_namespace = "";
            public string sortBy = "releaseDate";
            public string sortDir = "asc";
            public string categories = "addons|digitalextras";
            public int count = 50;
        }

        public Variables variables = new Variables();
        public string query = @"query getAddonsByNamespace($categories: String!, $count: Int!, $country: String!, $locale: String!, $epic_namespace: String!, $sortBy: String!, $sortDir: String!) {    Catalog {        catalogOffers(namespace: $epic_namespace, locale: $locale, params: {            category: $categories,            count: $count,            country: $country,            sortBy: $sortBy,            sortDir: $sortDir        }) {            elements {                countriesBlacklist                customAttributes {                    key                    value                }                description                developer                effectiveDate                id                isFeatured                keyImages {                    type                    url                }                lastModifiedDate                longDescription                namespace                offerType                productSlug                releaseDate                status                technicalDetails                title                urlSlug                price(country: $country) {                    totalPrice {                        discountPrice                        originalPrice                        voucherDiscount                        discount                        currencyCode                        currencyInfo {                            decimals                        }                        fmtPrice(locale: $locale) {                            originalPrice                            discountPrice                            intermediatePrice                        }                    }                }            }        }    }}";
    }


    public class QueryEntitledOfferItems
    {
        public class Variables
        {
            public string productNameSpace = "";
            public string offerId = "";
        }

        public Variables variables = new Variables();
        public string query = @"query getEntitledOfferItems($productNameSpace: String!, $offerId: String!) {    Launcher {        entitledOfferItems(namespace: $productNameSpace, offerId: $offerId) {            namespace            offerId            entitledToAllItemsInOffer            entitledToAnyItemInOffer        }    }}";
    }
}
