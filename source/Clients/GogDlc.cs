﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CheckDlc.Models;
using Playnite.SDK.Models;
using CommonPluginsShared;
using static CommonPluginsShared.PlayniteTools;
using CommonPluginsStores.Gog;
using System.Collections.ObjectModel;
using CommonPluginsStores.Models;
using Playnite.SDK;

namespace CheckDlc.Clients
{
    public class GogDlc : GenericDlc
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
                    GogApi?.ResetIsUserLoggedIn();
                }
            }
        }

        private static GogApi GogApi => CheckDlc.GogApi;


        public GogDlc() : base("GOG", CodeLang.GetGogLang(API.Instance.ApplicationSettings.Language))
        {
            GogApi.SetLanguage(API.Instance.ApplicationSettings.Language);
        }


        public override List<Dlc> GetGameDlc(Game game)
        {
            Logger.Info($"Get dlc for {game.Name} with {ClientName}");
            List<Dlc> gameDlcs = PluginDatabase.Get(game)?.Items ?? new List<Dlc>();

            try
            {
                if (GogApi.IsUserLoggedIn)
                {
                    GogApi.SetCurrency(PluginDatabase.PluginSettings.Settings.GogCurrency);

                    List<Dlc> newDlcs = new List<Dlc>();
                    ObservableCollection<DlcInfos> dlcs = GogApi.GetDlcInfos(game.GameId, GogApi.CurrentAccountInfos);
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
                    ShowNotificationPluginNoAuthenticate(string.Format(ResourceProvider.GetString("LOCCommonStoresNoAuthenticate"), ClientName), ExternalPlugin.GogLibrary);
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
