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
                        _UserData = null;
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
        private static string UrlGogAppDetails = "https://api.gog.com/products/{0}?expand=description";
        private static string UrlGogGame = "https://www.gog.com/game/{0}";

        private static GogUserData _UserData = null;
        private static GogUserData UserData
        {
            get
            {
                if (_UserData == null)
                {
                    try
                    {
                        string AccessToken = AccountInfo.accessToken;
                        string UserId = AccountInfo.userId;
                        string UserName = AccountInfo.username;

                        string data = Web.DownloadStringData(UrlGogOwned, AccessToken).GetAwaiter().GetResult();
                        _UserData = Serialization.FromJson<GogUserData>(data);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, $"GOG", true, "CheckDlc");
                    }
                }
                return _UserData;
            }
        }


        public GogDlc() : base("GOG", CodeLang.GetGogLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language))
        {

        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            List<Dlc> GameDlc = new List<Dlc>();
            string UrlLang = string.Format(UrlGogLang, LocalLang.ToLower());

            try
            {
                if (IsConnected)
                {
                    string data = Web.DownloadStringDataWithUrlBefore(string.Format(UrlGogAppDetails, game.GameId), UrlLang).GetAwaiter().GetResult();
                    Models.ProductApiDetail productApiDetail = Serialization.FromJson<Models.ProductApiDetail>(data);

                    string stringDlcs = Serialization.ToJson(productApiDetail?.dlcs);
                    if (stringDlcs.IsNullOrEmpty())
                    {
                        return GameDlc;
                    }

                    var Dlcs = Serialization.FromJson<Models.GogDlcs>(stringDlcs);
                    foreach (var el in Dlcs?.products)
                    {
                        try
                        {
                            string dataDlc = Web.DownloadStringDataWithUrlBefore(string.Format(UrlGogAppDetails, el.id), UrlLang).GetAwaiter().GetResult();
                            Models.ProductApiDetail productApiDetailDlc = Serialization.FromJson<Models.ProductApiDetail>(dataDlc);

                            Dlc dlc = new Dlc
                            {
                                GameId = el.id.ToString(),
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
                            if (!ex.Message.Contains("404(Not Found)"))
                            {
                                Common.LogError(ex, false);
                            }
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
                ShowNotificationPluginError(ex);
            }

            return GameDlc;
        }


        private bool IsOwned(int GameId)
        {
            if (UserData?.owned?.Count > 0)
            {
                var finded = UserData?.owned?.Find(x => x == GameId);
                return (finded != null && finded != 0);
            }
            else
            {
                ShowNotificationPluginNoAuthenticate(string.Format(resources.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.GogLibrary);
                return false;
            }
        }
    }
}
