using CheckDlc.Models;
using CheckDlc.Services;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CommonPluginsShared.PlayniteTools;

namespace CheckDlc.Clients
{
    public abstract class GenericDlc : ObservableObject
    {
        internal static ILogger Logger => LogManager.GetLogger();

        internal static CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;

        protected string ClientName { get; }
        protected string LocalLang { get; }

        protected string LastErrorId { get; set; }
        protected string LastErrorMessage { get; set; }


        public GenericDlc(string clientName, string localLang = "")
        {
            ClientName = clientName;
            LocalLang = localLang;
        }


        public abstract List<Dlc> GetGameDlc(Game game);

        public List<Dlc> GetGameDlc (Guid id)
        {
            Game game = API.Instance.Database.Games.Get(id);
            return game == null ? new List<Dlc>() : GetGameDlc(game);
        }


        #region Errors
        public virtual void ShowNotificationPluginError(Exception ex)
        {
            Common.LogError(ex, false, $"{ClientName}", true, PluginDatabase.PluginName);
        }

        public virtual void ShowNotificationPluginError(string message)
        {
            Logger.Error($"{ClientName}: {message}");

            API.Instance.Notifications.Add(new NotificationMessage(
                $"{PluginDatabase.PluginName}-{ClientName.RemoveWhiteSpace().ToLower()}-noconfig",
                $"{PluginDatabase.PluginName}" + Environment.NewLine + $"{ClientName}: {message}",
                NotificationType.Error
            ));
        }

        public virtual void ShowNotificationPluginNoConfiguration(string message)
        {
            LastErrorId = $"checkdlc-{ClientName.RemoveWhiteSpace().ToLower()}-noconfig";
            LastErrorMessage = message;
            Logger.Warn($"{ClientName} is not configured");

            API.Instance.Notifications.Add(new NotificationMessage(
                $"{PluginDatabase.PluginName }-{ClientName.RemoveWhiteSpace().ToLower()}-noconfig",
                $"{PluginDatabase.PluginName}" + Environment.NewLine + $"{message}",
                NotificationType.Error
            ));
        }

        public virtual void ShowNotificationPluginNoAuthenticate(string message)
        {
            ShowNotificationPluginNoAuthenticate(message, ExternalPlugin.None);
        }

        public virtual void ShowNotificationPluginNoAuthenticate(string message, ExternalPlugin externalPlugin)
        {
            LastErrorId = $"{PluginDatabase.PluginName }-{ClientName.RemoveWhiteSpace().ToLower()}-noauthenticate";
            LastErrorMessage = message;
            Logger.Warn($"{ClientName} user is not authenticated");

            API.Instance.Notifications.Add(new NotificationMessage(
                $"{PluginDatabase.PluginName }-{ClientName.RemoveWhiteSpace().ToLower()}-noauthenticate",
                $"{PluginDatabase.PluginName}" + Environment.NewLine + $"{message}",
                NotificationType.Error,
                () =>
                {
                    GogDlc.SettingsOpen = true;
                    SteamDlc.SettingsOpen = true;
                    EpicDlc.SettingsOpen = true;
                    OriginDlc.SettingsOpen = true;
                    PlayniteTools.ShowPluginSettings(externalPlugin);
                }
            ));
        }
        #endregion
    }
}
