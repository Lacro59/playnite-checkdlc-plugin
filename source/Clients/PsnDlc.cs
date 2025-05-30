﻿using CheckDlc.Models;
using CommonPluginsShared;
using CommonPluginsStores.Models;
using CommonPluginsStores.Psn;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonPluginsShared.PlayniteTools;

namespace CheckDlc.Clients
{
    public class PsnDlc : GenericDlc
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
                    PsnApi?.ResetIsUserLoggedIn();
                }
            }
        }

        private static PsnApi PsnApi => new PsnApi("CheckDlc");


        public PsnDlc() : base("PSN", CodeLang.GetOriginLang(API.Instance.ApplicationSettings.Language))
        {

        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            Logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> gameDlcs = PluginDatabase.Get(game)?.Items ?? new List<Dlc>();

            try
            {
                if (PsnApi.IsUserLoggedIn)
                {
                    List<Dlc> newDlcs = new List<Dlc>();
                    ObservableCollection<DlcInfos> dlcs = PsnApi.GetDlcInfos(game.GameId, PsnApi.CurrentAccountInfos);
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
                    ShowNotificationPluginNoAuthenticate(string.Format(ResourceProvider.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.PSNLibrary);
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
