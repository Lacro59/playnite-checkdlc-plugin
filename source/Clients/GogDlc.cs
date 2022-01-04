using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckDlc.Models;
using Playnite.SDK.Models;
using Playnite.SDK.Data;
using CommonPluginsShared;
using Playnite.SDK;
using CommonPlayniteShared.PluginLibrary.Services.GogLibrary;
using CommonPlayniteShared.PluginLibrary.GogLibrary.Models;
using static CommonPluginsShared.PlayniteTools;
using CommonPluginsShared.Extensions;
using System.Net;

namespace CheckDlc.Clients
{
    class GogDlc : GenericDlc
    {
        public static bool SettingsOpen = false;

        private static GogAccountClient _GogAPI;
        private static GogAccountClient GogAPI
        {
            get
            {
                if (_GogAPI == null)
                {
                    _GogAPI = new GogAccountClient(API.Instance.WebViews.CreateOffscreenView());
                }
                return _GogAPI;
            }

            set
            {
                _GogAPI = value;
            }
        }

        private static AccountBasicRespose _AccountInfo;
        private static AccountBasicRespose AccountInfo
        {
            get
            {
                if (_AccountInfo == null)
                {
                    _AccountInfo = GogAPI.GetAccountInfo();
                }
                return _AccountInfo;
            }

            set
            {
                _AccountInfo = value;
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
                        _IsConnected = GogAPI.GetIsUserLoggedIn();
                        SettingsOpen = false;
                        _UserDataOwned = null;
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, $"GOG", true, "CheckDlc");
                    }
                }
                return (bool)_IsConnected;
            }
        }

        private string UrlGogLang = @"https://www.gog.com/user/changeLanguage/{0}";

        private static string UrlGogOwned = "https://embed.gog.com/user/data/games";
        private static string UrlGogUserData = "https://www.gog.com/userData.json";

        private static string UrlGogAppDetails = "https://api.gog.com/products/{0}?expand=description";
        private static string UrlGogGame = "https://www.gog.com/game/{0}";
        private static string UrlGogPrice = "https://api.gog.com/products/prices?ids={0}&countryCode={1}&currency={2}";

        private static GogUserDataOwned _UserDataOwned = null;
        private static GogUserDataOwned UserDataOwned
        {
            get
            {
                if (_UserDataOwned == null)
                {
                    try
                    {
                        string AccessToken = AccountInfo.accessToken;
                        string UserId = AccountInfo.userId;
                        string UserName = AccountInfo.username;

                        string data = Web.DownloadStringData(UrlGogOwned, AccessToken).GetAwaiter().GetResult();
                        _UserDataOwned = Serialization.FromJson<GogUserDataOwned>(data);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, $"GOG", true, "CheckDlc");
                    }
                }
                return _UserDataOwned;
            }
        }


        public GogDlc() : base("GOG", CodeLang.GetGogLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language))
        {

        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            logger.Info($"Get Dlc for {game.Name} with {ClientName}");

            List<Dlc> GameDlc = new List<Dlc>();
            string UrlLang = string.Format(UrlGogLang, LocalLang.ToLower());

            try
            {
                if (IsConnected)
                {
                    string data = Web.DownloadStringDataWithUrlBefore(string.Format(UrlGogAppDetails, game.GameId), UrlLang).GetAwaiter().GetResult();
                    Models.ProductApiDetail productApiDetail = Serialization.FromJson<Models.ProductApiDetail>(data);

                    string stringDlcs = Serialization.ToJson(productApiDetail?.dlcs);
                    if (stringDlcs.IsNullOrEmpty() || stringDlcs.IsEqual("[]"))
                    {
                        return GameDlc;
                    }

                    var Dlcs = Serialization.FromJson<GogDlcs>(stringDlcs);
                    foreach (var el in Dlcs?.products)
                    {
                        try
                        {
                            string dataDlc = Web.DownloadStringDataWithUrlBefore(string.Format(UrlGogAppDetails, el.id), UrlLang).GetAwaiter().GetResult();
                            Models.ProductApiDetail productApiDetailDlc = Serialization.FromJson<Models.ProductApiDetail>(dataDlc);

                            Dlc dlc = new Dlc
                            {
                                DlcId = el.id.ToString(),
                                Name = productApiDetailDlc.title,
                                Description = productApiDetailDlc.description.full,
                                Image = "https:" + productApiDetailDlc.images.logo2x,
                                Link = string.Format(UrlGogGame, productApiDetailDlc.slug),
                                IsOwned = IsOwned(el.id)
                            };

                            GameDlc.Add(dlc);
                        }
                        catch (Exception ex)
                        {
                            if (!ex.Message.Contains("404"))
                            {
                                Common.LogError(ex, false);
                            }
                        }
                    }

                    // Price
                    if (GameDlc.Count > 0)
                    {
                        try
                        {
                            string priceCountry = CodeLang.GetOriginLangCountry(PluginDatabase.PlayniteApi.ApplicationSettings.Language);
                            var ids = GameDlc.Select(x => x.DlcId);
                            string joined = string.Join(",", ids);
                            string UrlPrice = string.Format(UrlGogPrice, joined, (priceCountry.IsEqual("en") ? "us" : priceCountry), PluginDatabase.PluginSettings.Settings.GogCurrencySelected.code.ToUpper());
                            dynamic dataObj = GetPrice(UrlPrice);

                            string CodeCurrency = PluginDatabase.PluginSettings.Settings.GogCurrencySelected.code;
                            string SymbolCurrency = PluginDatabase.PluginSettings.Settings.GogCurrencySelected.symbol;

                            if (dataObj["message"] != null && ((string)dataObj["message"]).Contains("is not supported in", StringComparison.InvariantCultureIgnoreCase))
                            {
                                UrlPrice = string.Format(UrlGogPrice, joined, (priceCountry.IsEqual("en") ? "us" : priceCountry), "USD");
                                dataObj = GetPrice(UrlPrice);
                                CodeCurrency = "USD";
                                SymbolCurrency = "$";
                            }

                            if (dataObj["message"] != null)
                            {
                                logger.Info($"{ClientName}: {dataObj["message"]}");
                                return GameDlc;
                            }

                            string dataObjString = Serialization.ToJson(dataObj["_embedded"]);
                            GogPriceResult gogPriceResult = Serialization.FromJson<GogPriceResult>(dataObjString);

                            foreach (var el in gogPriceResult.items)
                            {
                                int idx = GameDlc.FindIndex(x => x.DlcId.IsEqual(el._embedded.product.id.ToString()));
                                if (idx > -1)
                                {
                                    double.TryParse(el._embedded.prices[0].finalPrice.Replace(CodeCurrency, string.Empty, StringComparison.InvariantCultureIgnoreCase), out double Price);
                                    double.TryParse(el._embedded.prices[0].basePrice.Replace(CodeCurrency, string.Empty, StringComparison.InvariantCultureIgnoreCase), out double PriceBase);

                                    Price = Price * 0.01;
                                    PriceBase = PriceBase * 0.01;

                                    GameDlc[idx].Price = Price + " " + SymbolCurrency;
                                    GameDlc[idx].PriceBase = PriceBase + " " + SymbolCurrency;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Common.LogError(ex, false, true, "CheckDlc", resources.GetString("LOCCheckDlcGogErrorCurrency"));
                        }
                    }
                }
                else
                {
                    ShowNotificationPluginNoAuthenticate(string.Format(resources.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.GogLibrary);
                }
            }
            catch (Exception ex)
            {
                if (!ex.Message.Contains("404"))
                {
                    ShowNotificationPluginError(ex);
                }
                else
                {
                    logger.Warn($"No data for {game.Name}");
                }
            }

            return GameDlc;
        }


        private dynamic GetPrice(string UrlPrice)
        {
            string DataPrice = Web.DownloadStringData(UrlPrice).GetAwaiter().GetResult();
            return Serialization.FromJson<dynamic>(DataPrice);
        }


        private bool IsOwned(int GameId)
        {
            if (UserDataOwned?.owned?.Count > 0)
            {
                var finded = UserDataOwned?.owned?.Find(x => x == GameId);
                return (finded != null && finded != 0);
            }
            else
            {
                ShowNotificationPluginNoAuthenticate(string.Format(resources.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.GogLibrary);
                return false;
            }
        }


        public List<GogCurrency> GetGogCurrencies()
        {
            try
            {
                if (IsConnected)
                {
                    string webData = Web.DownloadStringData(UrlGogUserData).GetAwaiter().GetResult();
                    var userData = Serialization.FromJson<GogUserData>(webData);
                    return userData.currencies;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, "CheckDlc");
            }

            return new List<GogCurrency>
            {
                new GogCurrency { code = "USD", symbol = "$" }
            };
        }
    }
}
