using CheckDlc.Models;
using CommonPluginsShared;
using CommonPluginsStores.Epic;
using CommonPluginsStores.Models;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using static CommonPluginsShared.PlayniteTools;

namespace CheckDlc.Clients
{
    public class EpicDlc : GenericDlc
    {
        private static bool settingsOpen = false;
        public static bool SettingsOpen
        {
            get => settingsOpen;

            set
            {
                settingsOpen = value;
                if (settingsOpen)
                {
                    EpicApi.ResetIsUserLoggedIn();
                }
            }
        }

        private static EpicApi epicApi;
        private static EpicApi EpicApi
        {
            get
            {
                if (epicApi == null)
                {
                    epicApi = new EpicApi(PluginDatabase.PluginName);
                }
                return epicApi;
            }

            set => epicApi = value;
        }


        public EpicDlc() : base("Epic", CodeLang.GetEpicLang(API.Instance.ApplicationSettings.Language))
        {
            EpicApi.SetLanguage(API.Instance.ApplicationSettings.Language);
        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            Logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> GameDlc = new List<Dlc>();

            try
            {
                if (EpicApi.IsUserLoggedIn)
                {
                    string productNameSpace = EpicApi.GetNameSpace(PlayniteTools.NormalizeGameName(game.Name));
                    ObservableCollection<DlcInfos> dlcs = EpicApi.GetDlcInfos(productNameSpace, EpicApi.CurrentAccountInfos);
                    dlcs?.ForEach(x =>
                    {
                        Dlc dlc = new Dlc
                        {
                            DlcId = x.Id,
                            Name = x.Name,
                            Description = x.Description,
                            Image = x.Image,
                            Link = x.Link,
                            IsOwned = x.IsOwned,
                            Price = x.Price,
                            PriceBase = x.PriceBase
                        };

                        GameDlc.Add(dlc);
                    });

                    Logger.Info($"Find {GameDlc?.Count} dlc");
                    return GameDlc;
                }
                else
                {
                    ShowNotificationPluginNoAuthenticate(string.Format(ResourceProvider.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.EpicLibrary);
                }
            }
            catch (Exception ex)
            {
                ShowNotificationPluginError(ex);
            }

            return null;
        }
    }
}
