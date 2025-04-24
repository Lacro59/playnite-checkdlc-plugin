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
        private static bool _settingsOpen = false;
        public static bool SettingsOpen
        {
            get => _settingsOpen;

            set
            {
                _settingsOpen = value;
                if (_settingsOpen)
                {
                    EpicApi?.ResetIsUserLoggedIn();
                }
            }
        }

        private static EpicApi EpicApi => CheckDlc.EpicApi;


        public EpicDlc() : base("Epic", CodeLang.GetEpicLang(API.Instance.ApplicationSettings.Language))
        {
            EpicApi.SetLanguage(API.Instance.ApplicationSettings.Language);
        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            Logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> gameDlcs = PluginDatabase.Get(game)?.Items ?? new List<Dlc>();

            try
            {
                if (EpicApi.IsUserLoggedIn)
                {
                    List<Dlc> newDlcs = new List<Dlc>();
                    string productNameSpace = EpicApi.GetNameSpace(game);
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

                        newDlcs.Add(dlc);
                    });

                    Logger.Info($"Find {newDlcs?.Count} dlc(s)");
                    return newDlcs?.Count > 0 ? newDlcs : gameDlcs;
                }
                else
                {
                    ShowNotificationPluginNoAuthenticate(string.Format(ResourceProvider.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.CheckDlc);
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
