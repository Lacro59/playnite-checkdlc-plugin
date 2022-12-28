using CheckDlc.Models;
using CommonPluginsShared;
using CommonPluginsStores.Epic;
using CommonPluginsStores.Models;
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
    class EpicDlc : GenericDlc
    {
        private static bool _SettingsOpen = false;
        public static bool SettingsOpen
        {
            get => _SettingsOpen;

            set
            {
                _SettingsOpen = value;
                if (_SettingsOpen)
                {
                    EpicAPI.ResetIsUserLoggedIn();
                }
            }
        }

        private static EpicApi _EpicAPI;
        private static EpicApi EpicAPI
        {
            get
            {
                if (_EpicAPI == null)
                {
                    _EpicAPI = new EpicApi(PluginDatabase.PluginName);
                }
                return _EpicAPI;
            }

            set => _EpicAPI = value;
        }


        public EpicDlc() : base("Epic", CodeLang.GetEpicLang(PluginDatabase.PlayniteApi.ApplicationSettings.Language))
        {
            EpicAPI.SetLanguage(PluginDatabase.PlayniteApi.ApplicationSettings.Language);
        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> GameDlc = new List<Dlc>();

            try
            {
                if (EpicAPI.IsUserLoggedIn)
                {
                    string productNameSpace = EpicAPI.GetNameSpace(PlayniteTools.NormalizeGameName(game.Name));
                    ObservableCollection<DlcInfos> dlcs = EpicAPI.GetDlcInfos(productNameSpace, EpicAPI.CurrentAccountInfos);
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

                    logger.Info($"Find {GameDlc?.Count} dlc");
                    return GameDlc;
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

            return null;
        }
    }
}
