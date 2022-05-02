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
using AngleSharp.Parser.Html;
using AngleSharp.Dom.Html;
using CommonPlayniteShared.Common;
using CommonPluginsShared.Converters;
using System.Globalization;
using AngleSharp.Dom;

namespace CheckDlc.Clients
{
    class SteamDlc : GenericDlc
    {
        public static bool SettingsOpen = false;

        private string UrlSteamAppDetails = "https://store.steampowered.com/api/appdetails?appids={0}&l={1}";
        private static string UrlSteamUserData = "https://store.steampowered.com/dynamicstore/userdata/";
        private static string UrlSteamGame = "https://store.steampowered.com/app/{0}/?l={1}";

        private string SteamDbDlc = "https://steamdb.info/app/{0}/dlc/";

        private static string FileUserData = Path.Combine(PluginDatabase.Paths.PluginUserDataPath, "SteamUserData.json");

        private static SteamUserData _UserData = null;
        private static SteamUserData UserData
        {
            get
            {
                if (_UserData == null || SettingsOpen)
                {
                    try
                    {
                        _UserData = LoadUserData(true);
                        if (_UserData == null)
                        {
                            using (IWebView WebViewOffscreen = API.Instance.WebViews.CreateOffscreenView())
                            {
                                WebViewOffscreen.NavigateAndWait(UrlSteamUserData);
                                WebViewOffscreen.NavigateAndWait(UrlSteamUserData);
                                string data = WebViewOffscreen.GetPageText();
                                Serialization.TryFromJson<SteamUserData>(data, out _UserData);

                                if (_UserData?.rgOwnedApps?.Count > 0)
                                {
                                    SaveUserData(_UserData);
                                }
                                else
                                {
                                    SteamUserData loadedData = LoadUserData();
                                    _UserData = loadedData ?? _UserData;
                                }
                            }
                        }

                        SettingsOpen = false;
                    }
                    catch (Exception ex)
                    {
                        Common.LogError(ex, false, $"Steam", true, PluginDatabase.PluginName);
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
            logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> GameDlc = new List<Dlc>();

            try
            {
                string GameId = game.GameId;
                List<int> DlcsIdSteam = GetFromSteamWebApi(GameId);
                List<int> DlcsIdSteamDb = GetFromSteamDb(GameId);

                List<int> DlcsId = (DlcsIdSteam.Count >= DlcsIdSteamDb.Count) ? DlcsIdSteam : DlcsIdSteamDb;
                foreach (int DlcId in DlcsId)
                {
                    try
                    {
                        string dataDlc = Web.DownloadStringData(string.Format(UrlSteamAppDetails, DlcId, LocalLang)).GetAwaiter().GetResult();
                        if (dataDlc == "\u001f�\b\0\0\0\0\0\0\u0003�+��\u0001\0O��%\u0004\0\0\0")
                        {
                            logger.Warn($"No data for {game.Name} - {DlcId}");
                            continue;
                        }

                        Serialization.TryFromJson<Dictionary<string, StoreAppDetailsResult>>(dataDlc, out Dictionary<string, StoreAppDetailsResult> parsedDataDlc);
                        if (parsedDataDlc == null)
                        {
                            logger.Warn($"No parsed data for {game.Name} - {DlcId}");
                            return GameDlc;
                        }

                        StoreAppDetailsResult storeAppDetailsResultDlc = parsedDataDlc[DlcId.ToString()];
                        if (storeAppDetailsResultDlc?.data == null)
                        {
                            logger.Warn($"No data for {game.Name} - {DlcId}");
                            return GameDlc;
                        }

                        Dlc dlc = new Dlc
                        {
                            DlcId = DlcId.ToString(),
                            Name = storeAppDetailsResultDlc.data.name,
                            Description = storeAppDetailsResultDlc.data.detailed_description,
                            Image = storeAppDetailsResultDlc.data.header_image,
                            Link = string.Format(UrlSteamGame, DlcId, LocalLang),
                            IsOwned = IsOwned(DlcId),
                            Price = storeAppDetailsResultDlc.data.is_free ? "0" : storeAppDetailsResultDlc.data.price_overview?.final_formatted,
                            PriceBase = storeAppDetailsResultDlc.data.is_free ? "0" : storeAppDetailsResultDlc.data.price_overview?.initial_formatted
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

            logger.Info($"Find {GameDlc?.Count} dlc");
            return GameDlc;
        }


        private bool IsOwned(int GameId)
        {
            if (UserData?.rgOwnedApps?.Count > 0)
            {
                int? finded = UserData?.rgOwnedApps?.Find(x => x == GameId);
                return finded != null && finded != 0;
            }
            else
            {
                ShowNotificationPluginNoAuthenticate(string.Format(resources.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.SteamLibrary);
                return false;
            }
        }


        private List<int> GetFromSteamWebApi(string AppId)
        {
            List<int> Dlcs = new List<int>();

            try
            {
                string data = Web.DownloadStringData(string.Format(UrlSteamAppDetails, AppId, LocalLang)).GetAwaiter().GetResult();
                if (data.IsNullOrEmpty() || data == "\u001f�\b\0\0\0\0\0\0\u0003�+��\u0001\0O��%\u0004\0\0\0")
                {
                    logger.Warn($"No data for {AppId}");
                    return Dlcs;
                }

                Serialization.TryFromJson<Dictionary<string, StoreAppDetailsResult>>(data, out Dictionary<string, StoreAppDetailsResult> parsedData);
                if (parsedData == null)
                {
                    logger.Warn($"No parsed data for {AppId}");
                    return Dlcs;
                }
                if (!parsedData.ContainsKey(AppId))
                {
                    logger.Info($"No dlc for {AppId}");
                    return Dlcs;
                }

                StoreAppDetailsResult storeAppDetailsResult = parsedData[AppId];
                foreach (int el in storeAppDetailsResult.data.dlc)
                {
                    Dlcs.Add(el);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }

            return Dlcs;
        }

        private List<int> GetFromSteamDb(string AppId)
        {
            List<int> Dlcs = new List<int>();

            try
            {
                using (IWebView WebViewOffScreen = API.Instance.WebViews.CreateOffscreenView())
                {
                    //string data = Web.DownloadStringData(string.Format(SteamDbDlc, AppId)).GetAwaiter().GetResult();
                    WebViewOffScreen.NavigateAndWait(string.Format(SteamDbDlc, AppId));
                    string data = WebViewOffScreen.GetPageSource();
                    IHtmlDocument htmlDocument = new HtmlParser().Parse(data);

                    IHtmlCollection<IElement> SectionDlcs = htmlDocument.QuerySelectorAll("#dlc tr.app");
                    if (SectionDlcs != null)
                    {
                        foreach (IElement el in SectionDlcs)
                        {
                            string DlcIdString = el.QuerySelector("td a")?.InnerHtml;
                            int.TryParse(DlcIdString, out int DlcId);

                            if (DlcId != 0)
                            {
                                Dlcs.Add(DlcId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, true, PluginDatabase.PluginName);
            }

            return Dlcs;
        }
        

        private static SteamUserData LoadUserData(bool OnlyNow = false)
        {
            if (File.Exists(FileUserData))
            {
                try
                {
                    DateTime DateLastWrite = File.GetLastWriteTime(FileUserData);
                    if (OnlyNow && !(DateLastWrite.ToString("yyyy-MM-dd") == DateTime.Now.ToString("yyyy-MM-dd")))
                    {
                        return null;
                    }

                    if (!OnlyNow)
                    {
                        LocalDateTimeConverter localDateTimeConverter = new LocalDateTimeConverter();
                        string formatedDateLastWrite = localDateTimeConverter.Convert(DateLastWrite, null, null, CultureInfo.CurrentCulture).ToString();
                        logger.Warn($"Use saved UserData - {formatedDateLastWrite}");
                        API.Instance.Notifications.Add(new NotificationMessage(                        
                            $"{PluginDatabase.PluginName }-steam-saveddata",
                            $"{PluginDatabase.PluginName}" + Environment.NewLine 
                                + string.Format(resources.GetString("LOCCheckDlcUseSavedUserData"), "Steam", formatedDateLastWrite),
                            NotificationType.Info,
                            () =>
                            {
                                GogDlc.SettingsOpen = true;
                                SteamDlc.SettingsOpen = true;
                                EpicDlc.SettingsOpen = true;
                                OriginDlc.SettingsOpen = true;
                                PlayniteTools.ShowPluginSettings(ExternalPlugin.SteamLibrary);
                            }
                        ));
                    }

                    return Serialization.FromJsonFile<SteamUserData>(FileUserData);
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false, true, PluginDatabase.PluginName);
                }
            }

            return null;
        }

        private static void SaveUserData(SteamUserData UserData)
        {
            FileSystem.PrepareSaveFile(FileUserData);
            File.WriteAllText(FileUserData, Serialization.ToJson(UserData));
        }
    }
}
