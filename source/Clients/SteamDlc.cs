using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CheckDlc.Models;
using Playnite.SDK.Models;
using Playnite.SDK.Data;
using CommonPlayniteShared.PluginLibrary.SteamLibrary.SteamShared;
using CommonPluginsShared;
using Playnite.SDK;
using static CommonPluginsShared.PlayniteTools;

namespace CheckDlc.Clients
{
    class SteamDlc : GenericDlc
    {
        public static bool SettingsOpen = false;

        private string UrlSteamAppDetails = "https://store.steampowered.com/api/appdetails?appids={0}&l={1}";
        private static string UrlSteamUserData = "https://store.steampowered.com/dynamicstore/userdata/";
        private static string UrlSteamGame = "https://store.steampowered.com/app/{0}/?l={1}";

        private static SteamUserData _UserData = null;
        private static SteamUserData UserData
        {
            get
            {
                if (_UserData == null || SettingsOpen)
                {
                    try
                    {
                        using (var WebViewOffscreen = API.Instance.WebViews.CreateOffscreenView())
                        {
                            WebViewOffscreen.NavigateAndWait(UrlSteamUserData);
                            WebViewOffscreen.NavigateAndWait(UrlSteamUserData);
                            string data = WebViewOffscreen.GetPageText();
                            _UserData = Serialization.FromJson<SteamUserData>(data);
                        }
                        SettingsOpen = false;
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, $"Steam", true, "CheckDlc");
                    }
                }
                return _UserData;
            }
        }


        public SteamDlc() : base("Steam", CodeLang.GetSteamLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language))
        {

        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            List<Dlc> GameDlc = new List<Dlc>();

            try
            {
                string data = Web.DownloadStringData(string.Format(UrlSteamAppDetails, game.GameId, LocalLang)).GetAwaiter().GetResult();
                if (data == "\u001f�\b\0\0\0\0\0\0\u0003�+��\u0001\0O��%\u0004\0\0\0")
                {
                    logger.Warn($"No data for {game.Name}");
                    return GameDlc;
                }

                var parsedData = Serialization.FromJson<Dictionary<string, StoreAppDetailsResult>>(data);
                StoreAppDetailsResult storeAppDetailsResult = parsedData[game.GameId.ToString()];

                if (storeAppDetailsResult?.data?.dlc == null)
                {
                    return GameDlc;
                }

                foreach (var el in storeAppDetailsResult?.data?.dlc)
                {
                    try
                    {
                        string dataDlc = Web.DownloadStringData(string.Format(UrlSteamAppDetails, el, LocalLang)).GetAwaiter().GetResult();
                        if (dataDlc == "\u001f�\b\0\0\0\0\0\0\u0003�+��\u0001\0O��%\u0004\0\0\0")
                        {
                            logger.Warn($"No data for {game.Name} - {el}");
                            continue;
                        }

                        var parsedDataDlc = Serialization.FromJson<Dictionary<string, StoreAppDetailsResult>>(dataDlc);
                        StoreAppDetailsResult storeAppDetailsResultDlc = parsedDataDlc[el.ToString()];

                        Dlc dlc = new Dlc
                        {
                            GameId = el.ToString(),
                            Name = storeAppDetailsResultDlc.data.name,
                            Description = storeAppDetailsResultDlc.data.detailed_description,
                            Image = storeAppDetailsResultDlc.data.header_image,
                            Link = string.Format(UrlSteamGame, el, LocalLang),
                            IsOwned = IsOwned(el)
                        };

                        GameDlc.Add(dlc);
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false);
                    }
                }
            }
            catch(Exception ex)
            {
                ShowNotificationPluginError(ex);
            }

            return GameDlc;
        }


        private bool IsOwned(int GameId)
        {
            if (UserData?.rgOwnedApps?.Count > 0)
            {
                var finded = UserData?.rgOwnedApps?.Find(x => x == GameId);
                return (finded != null && finded != 0);
            }
            else
            {
                ShowNotificationPluginNoAuthenticate(string.Format(resources.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.SteamLibrary);
                return false;
            }
        }
    }
}
