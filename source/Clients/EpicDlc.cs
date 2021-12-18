using CheckDlc.Models;
using CommonPlayniteShared.PluginLibrary.EpicLibrary;
using CommonPlayniteShared.PluginLibrary.EpicLibrary.Services;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
                        Common.LogError(ex, false, $"Epic", true, "CheckDlc");
                    }
                }
                return (bool)_IsConnected;
            }
        }


        private string UrlBase = "https://www.epicgames.com";
        private string UrlGameDetails = "https://store-content.ak.epicgames.com/api/{0}/content/products/{1}";
        private string UrlStore = "https://www.epicgames.com/store/{0}/p/{1}";


        public EpicDlc() : base("Epic", CodeLang.GetEpicLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language))
        {

        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            logger.Info($"Get Dlc for {game.Name} with {ClientName}");
            List<Dlc> GameDlc = new List<Dlc>();
            string LocalLangFinal = LocalLang;

            try
            {
                if (IsConnected)
                {
                    var tokens = EpicAPI.loadTokens();

                    // Product code
                    string ProductSlug = GetProductSlug(game.Name);
                    if (ProductSlug.IsNullOrEmpty())
                    {
                        logger.Warn($"ProductSlug not find for {game.Name}");
                        return GameDlc;
                    }

                    // Game data 
                    string Url = string.Format(UrlStore, LocalLangFinal, ProductSlug);

                    List<HttpCookie> Cookies = new List<HttpCookie>
                    {
                        new Playnite.SDK.HttpCookie
                        {
                            Domain = ".www.epicgames.com",
                            Name = "EPIC_LOCALE_COOKIE",
                            Value = CodeLang.GetGogLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language)
                        },
                        new Playnite.SDK.HttpCookie
                        {
                            Domain = ".www.epicgames.com",
                            Name = "EPIC_EG1",
                            Value = tokens.access_token
                        }
                    };

                    string ResultWeb = Web.DownloadStringData(Url, Cookies).GetAwaiter().GetResult();
                    if (!ResultWeb.Contains("lang=\"" + LocalLang + "\"", StringComparison.InvariantCultureIgnoreCase))
                    {
                        LocalLangFinal = CodeLang.GetGogLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language);
                        Url = string.Format(UrlStore, LocalLangFinal, ProductSlug);
                        ResultWeb = Web.DownloadStringData(Url, Cookies).GetAwaiter().GetResult();
                    }

                    // Extract data
                    int indexStart = ResultWeb.IndexOf("window.__REACT_QUERY_INITIAL_QUERIES__ =");
                    int indexEnd = ResultWeb.IndexOf("window.server_rendered");

                    string stringStart = ResultWeb.Substring(0, indexStart + "window.__REACT_QUERY_INITIAL_QUERIES__ =".Length);
                    string stringEnd = ResultWeb.Substring(indexEnd);

                    int length = ResultWeb.Length - stringStart.Length - stringEnd.Length;

                    string JsonDataString = ResultWeb.Substring(
                        indexStart + "window.__REACT_QUERY_INITIAL_QUERIES__ =".Length,
                        length
                    );

                    indexEnd = JsonDataString.IndexOf("}]};");
                    length = JsonDataString.Length - (JsonDataString.Length - indexEnd - 3);
                    JsonDataString = JsonDataString.Substring(0, length);

                    // Serialize data 
                    EpicData epicData = Serialization.FromJson<EpicData>(JsonDataString);
                    var AppData = epicData.queries
                        .Where(x => (Serialization.ToJson(x.queryHash)).Contains("PDP_PREFETCH_PRODUCT_DATA", StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault();

                    string dataString = Serialization.ToJson(AppData.state.data);
                    dynamic DataObject = Serialization.FromJson<dynamic>(dataString);

                    // Dlcs
                    List<EpicDlcData> epicDlcDatas = new List<EpicDlcData>();
                    string epicDlcDataString = Serialization.ToJson(DataObject["pages"][0]["data"]["dlc"]["dlc"]);
                    if (!epicDlcDataString.IsNullOrEmpty() && !epicDlcDataString.IsEqual("null"))
                    {
                        epicDlcDatas = Serialization.FromJson<List<EpicDlcData>>(epicDlcDataString);
                    }
                    
                    foreach (var el in epicDlcDatas)
                    {
                        if (el.offerId.IsNullOrEmpty())
                        {
                            continue;
                        }

                        try
                        {
                            var DlcData = epicData.queries
                                .Where(x => x.queryHash.Contains("getEntitledOfferItems", StringComparison.InvariantCultureIgnoreCase) && x.queryHash.Contains(el.offerId, StringComparison.InvariantCultureIgnoreCase))
                                .FirstOrDefault();

                            Dlc dlc = new Dlc
                            {
                                DlcId = el.offerId,
                                Name = el.title,
                                Description = el.description,
                                Image = el.image.src,
                                Link = UrlBase + "/" + el.slug,
                                IsOwned = IsOwned(DlcData, el.offerId)
                            };

                            GameDlc.Add(dlc);
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false);
                        }
                    }


                    // Catalog data
                    var CatalogData = epicData.queries
                        .Where(x => (Serialization.ToJson(x.queryHash)).Contains("getCatalogOffer", StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                    foreach (var el in CatalogData)
                    {
                        try
                        {
                            string epicCatalogOfferString = Serialization.ToJson(el.state.data);
                            EpicCatalogOffer epicCatalogOffer = Serialization.FromJson<EpicCatalogOffer>(epicCatalogOfferString);

                            var DlcData = epicData.queries
                                .Where(x => x.queryHash.Contains("getEntitledOfferItems", StringComparison.InvariantCultureIgnoreCase) && x.queryHash.Contains(epicCatalogOffer.Catalog.catalogOffer.id, StringComparison.InvariantCultureIgnoreCase))
                                .FirstOrDefault();

                            // already exist? => replace beacause some dlc is incorrect (ex: Football Manager 2021
                            int idx = GameDlc.FindIndex(x => x.DlcId.IsEqual(epicCatalogOffer.Catalog.catalogOffer.id));
                            if (idx > -1)
                            {
                                GameDlc[idx] = new Dlc
                                {
                                    DlcId = epicCatalogOffer.Catalog.catalogOffer.id,
                                    Name = epicCatalogOffer.Catalog.catalogOffer.title,
                                    Description = epicCatalogOffer.Catalog.catalogOffer.description,
                                    Image = epicCatalogOffer.Catalog.catalogOffer.keyImages.Find(x => x.type.IsEqual("OfferImageWide")).url.Replace("\u002F", "/"),
                                    Link = string.Format(UrlStore, LocalLangFinal, epicCatalogOffer.Catalog.catalogOffer.urlSlug),
                                    IsOwned = IsOwned(DlcData, epicCatalogOffer.Catalog.catalogOffer.id),
                                    Price = epicCatalogOffer.Catalog.catalogOffer.price.totalPrice.fmtPrice.discountPrice
                                };
                            }
                            else if (epicCatalogOffer.Catalog.catalogOffer.categories.Find(x => x.path.IsEqual("addons")) != null)
                            {
                                GameDlc.Add(new Dlc
                                {
                                    DlcId = epicCatalogOffer.Catalog.catalogOffer.id,
                                    Name = epicCatalogOffer.Catalog.catalogOffer.title,
                                    Description = epicCatalogOffer.Catalog.catalogOffer.description,
                                    Image = epicCatalogOffer.Catalog.catalogOffer.keyImages.Find(x => x.type.IsEqual("OfferImageWide")).url.Replace("\u002F", "/"),
                                    Link = string.Format(UrlStore, LocalLangFinal, epicCatalogOffer.Catalog.catalogOffer.urlSlug),
                                    IsOwned = IsOwned(DlcData, epicCatalogOffer.Catalog.catalogOffer.id),
                                    Price = epicCatalogOffer.Catalog.catalogOffer.price.totalPrice.fmtPrice.discountPrice
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false);
                        }
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

            return GameDlc;
        }


        private bool IsOwned(Query dlcInfos, string Id)
        {
            if (dlcInfos == null)
            {
                logger.Warn("No dlc find");
                return false;
            }

            try
            {
                string data = Serialization.ToJson(dlcInfos.state.data);
                dynamic epicData = Serialization.FromJson<dynamic>(data);

                return (bool)epicData["Launcher"]["entitledOfferItems"]["entitledToAllItemsInOffer"];
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, "CheckDlc");
            }

            return false;
        }


        private string GetProductSlug(string Name)
        {
            string ProductSlug = string.Empty;
            using (var client = new WebStoreClient())
            {
                var catalogs = client.QuerySearch(Name).GetAwaiter().GetResult();
                if (catalogs.HasItems())
                {
                    var catalog = catalogs.FirstOrDefault(a => a.title.Equals(Name, StringComparison.InvariantCultureIgnoreCase));
                    if (catalog == null)
                    {
                        catalog = catalogs[0];
                    }

                    ProductSlug = catalog?.productSlug?.Replace("/home", string.Empty);
                }
            }
            return ProductSlug;
        }
    }
}
