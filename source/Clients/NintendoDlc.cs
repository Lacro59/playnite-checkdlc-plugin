using CheckDlc.Models;
using CheckDlc.Models.Nintendo;
using CommonPlayniteShared.PluginLibrary.NintendoLibrary.Models;
using CommonPlayniteShared.PluginLibrary.NintendoLibrary.Services;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using CommonPluginsStores.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static CommonPluginsShared.PlayniteTools;

namespace CheckDlc.Clients
{
    // https://github.com/favna/nintendo-switch-eshop
    public class NintendoDlc : GenericDlc
    {
        protected static readonly Lazy<NintendoAccountClient> _nintendoAccountClient = new Lazy<NintendoAccountClient>(() => new NintendoAccountClient(Path.Combine(API.Instance.Paths.ExtensionsDataPath, GetPluginId(ExternalPlugin.NintendoLibrary).ToString())));
        internal static NintendoAccountClient NintendoAccountClient => _nintendoAccountClient.Value;


        private static bool _settingsOpen = false;
        public static bool SettingsOpen
        {
            get => _settingsOpen;

            set
            {
                _settingsOpen = value;
                if (_settingsOpen)
                {
                    isUserLoggedIn = null;
                }
            }
        }

        protected static bool? isUserLoggedIn;
        public bool IsUserLoggedIn
        {
            get
            {
                if (isUserLoggedIn == null)
                {
                    try
                    {
                        NintendoAccountClient.CheckAuthentication().GetAwaiter().GetResult();
                        isUserLoggedIn = NintendoAccountClient.GetIsUserLoggedIn().GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        isUserLoggedIn = false;
                    }
                }
                return (bool)isUserLoggedIn;
            }

            set => SetValue(ref isUserLoggedIn, value);
        }


        public NintendoDlc() : base("Nintendo", CodeLang.GetEpicLangCountry(API.Instance.ApplicationSettings.Language))
        {

        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            Logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> gameDlcs = PluginDatabase.Get(game)?.Items ?? new List<Dlc>();

            try
            {
                if (IsUserLoggedIn)
                {
                    List<Dlc> newDlcs = new List<Dlc>();
                    List<PurchasedList.Transaction> purchasedList = NintendoAccountClient.GetPurchasedList().GetAwaiter().GetResult();
                    List<Doc> search = Search(game.Name);

                    search?.ForEach(x =>
                    {
                        Dlc dlc = new Dlc
                        {
                            DlcId = x.FsId,
                            Name = x.Title,
                            Description = x.Excerpt,
                            Image = x.ImageUrl,
                            Link = "https://www.nintendo.com" + x.Url,
                            IsOwned = purchasedList.FirstOrDefault(y => x.TitleExtrasTxt.FirstOrDefault(z => z.IsEqual(y.title)) != null) != null,
                            Price = "",//x.PriceLowestF.ToString(),
                            PriceBase = "",//x.PriceRegularF.ToString()
                        };

                        newDlcs.Add(dlc);
                    });

                    Logger.Info($"Find {newDlcs?.Count} dlc(s)");
                    return newDlcs?.Count > 0 ? newDlcs : gameDlcs;
                }
                else
                {
                    ShowNotificationPluginNoAuthenticate(string.Format(ResourceProvider.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.NintendoLibrary);
                }
            }
            catch (Exception ex)
            {
                ShowNotificationPluginError(ex);
            }

            return null;
        }


        private List<Doc> Search(string searchTerm)
        {
            try
            {
                string urlEurope = "https://searching.nintendo-europe.com/{0}";
                //string NintendoUSUrl = "http://www.nintendo.com";
                //string NintendoJPUrl = "https://www.nintendo.co.jp";

                string url = string.Format(urlEurope, LocalLang) + "/select?q=" + searchTerm + "t&fq=type%3ADLC%20AND%20sorting_title%3A*%20AND%20*%3A*&sort=related_game_title_s%20asc&start=0&rows=250&wt=json";
                string response = Web.DownloadStringData(url).GetAwaiter().GetResult();
                _ = Serialization.TryFromJson(response, out SearchResult searchResult, out Exception ex);
                if (ex != null)
                {
                    throw ex;
                }

                if (searchResult?.Response?.NumFound > 0)
                {
                    return searchResult.Response.Docs.Where(x => FixGameName(x.RelatedGameTitleS).IsEqual(searchTerm)).ToList();
                }
                else {
                    Logger.Warn($"No data found for {searchResult}");
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false, false, PluginDatabase.PluginName);
            }

            return null;
        }

        private string FixGameName(string name)
        {
            var gameName = name.
                RemoveTrademarks(" ").
                NormalizeGameName().
                Replace("full game", "", StringComparison.OrdinalIgnoreCase).
                Trim();
            return Regex.Replace(gameName, @"\s+", " ");
        }
    }
}
