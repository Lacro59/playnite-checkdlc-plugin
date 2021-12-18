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
    abstract class GenericDlc
    {
        internal static readonly ILogger logger = LogManager.GetLogger();
        internal static readonly IResourceProvider resources = new ResourceProvider();

        internal static CheckDlcDatabase PluginDatabase = CheckDlc.PluginDatabase;

        protected string ClientName { get; }
        protected string LocalLang { get; }

        protected string LastErrorId { get; set; }
        protected string LastErrorMessage { get; set; }


        public GenericDlc(string ClientName, string LocalLang = "")
        {
            this.ClientName = ClientName;
            this.LocalLang = LocalLang;
        }


        public abstract List<Dlc> GetGameDlc(Game game);

        public List<Dlc> GetGameDlc (Guid Id)
        {
            Game game = PluginDatabase.PlayniteApi.Database.Games.Get(Id);

            if (game == null)
            {
                return new List<Dlc>();
            }

            return GetGameDlc(game);
        }


        #region Errors
        public virtual void ShowNotificationPluginError(Exception ex)
        {
            Common.LogError(ex, false, $"{ClientName}", true, "CheckDlc");
        }

        public virtual void ShowNotificationPluginError(string message)
        {
            logger.Error($"{ClientName}: {message}");

            PluginDatabase.PlayniteApi.Notifications.Add(new NotificationMessage(
                $"checkdlc-{ClientName.RemoveWhiteSpace().ToLower()}-noconfig",
                $"CheckDlc" + Environment.NewLine + $"{ClientName}: {message}",
                NotificationType.Error
            ));
        }

        public virtual void ShowNotificationPluginNoConfiguration(string Message)
        {
            LastErrorId = $"checkdlc-{ClientName.RemoveWhiteSpace().ToLower()}-noconfig";
            LastErrorMessage = Message;
            logger.Warn($"{ClientName} is not configured");

            PluginDatabase.PlayniteApi.Notifications.Add(new NotificationMessage(
                $"checkdlc-{ClientName.RemoveWhiteSpace().ToLower()}-noconfig",
                $"CheckDlc" + Environment.NewLine + $"{Message}",
                NotificationType.Error
            ));
        }

        public virtual void ShowNotificationPluginNoAuthenticate(string Message)
        {
            ShowNotificationPluginNoAuthenticate(Message, ExternalPlugin.None);
        }

        public virtual void ShowNotificationPluginNoAuthenticate(string Message, ExternalPlugin externalPlugin)
        {
            LastErrorId = $"checkdlc-{ClientName.RemoveWhiteSpace().ToLower()}-noauthenticate";
            LastErrorMessage = Message;
            logger.Warn($"{ClientName} user is not authenticated");

            PluginDatabase.PlayniteApi.Notifications.Add(new NotificationMessage(
                $"checkdlc-{ClientName.RemoveWhiteSpace().ToLower()}-noauthenticate",
                $"CheckDlc" + Environment.NewLine + $"{Message}",
                NotificationType.Error,
                () =>
                {
                    GogDlc.SettingsOpen = true;
                    SteamDlc.SettingsOpen = true;
                    PlayniteTools.ShowPluginSettings(externalPlugin);
                }
            ));
        }
        #endregion
    }
}
