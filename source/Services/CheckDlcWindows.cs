using CheckDlc.Models;
using CheckDlc.Views;
using CommonPluginsShared;
using CommonPluginsShared.Interfaces;
using CommonPluginsShared.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System.Windows;

namespace CheckDlc.Services
{
    public class CheckDlcWindows : PluginWindows
    {
        private CheckDlcDatabase Database => (CheckDlcDatabase)PluginDatabase;

        public CheckDlcWindows(string pluginName, IPluginDatabase pluginDatabase) : base(pluginName, pluginDatabase)
        {
        }

        public override void ShowPluginGameDataWindow(GenericPlugin plugin)
        {
            Game gameContext = Database.GameContext;
            if (gameContext == null)
            {
                return;
            }

            ShowGameView((CheckDlc)plugin, gameContext);
        }

        public override void ShowPluginGameDataWindow(Game gameContext)
        {
            if (gameContext == null)
            {
                return;
            }

            ShowGameView(Database.Plugin, gameContext);
        }

        public void ShowFreeDlcWindow(CheckDlc plugin)
        {
            WindowOptions windowOptions = new WindowOptions
            {
                CanBeResizable = false,
                Height = 720,
                Width = 1080,
                ShowMaximizeButton = false
            };

            CheckDlcFreeView viewExtension = new CheckDlcFreeView(plugin);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(
                BuildWindowTitle(ResourceProvider.GetString("LOCCheckDlcViewFreeDlcNoOwned")),
                viewExtension,
                windowOptions);
            windowExtension.ShowDialog();
        }

        private void ShowGameView(CheckDlc plugin, Game gameContext)
        {
            if (plugin == null || gameContext == null)
            {
                return;
            }

            GameDlc gameDlc = Database.Get(gameContext, true);
            if (gameDlc?.HasData != true)
            {
                return;
            }

            WindowOptions windowOptions = new WindowOptions
            {
                CanBeResizable = false,
                Height = 720,
                Width = 1080,
                ShowMaximizeButton = false
            };

            CheclDlcGameView viewExtension = new CheclDlcGameView(plugin, gameContext);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(
                BuildWindowTitle(gameContext.Name),
                viewExtension,
                windowOptions);
            windowExtension.ShowDialog();
        }

        private static string BuildWindowTitle(string context)
        {
            return string.Format(
                ResourceProvider.GetString("LOCCheckDlcWindowTitleFormat"),
                ResourceProvider.GetString("LOCCheckDlc"),
                context);
        }
    }
}
