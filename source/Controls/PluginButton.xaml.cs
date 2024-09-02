using CommonPluginsShared;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using CheckDlc.Models;
using CheckDlc.Services;
using CheckDlc.Views;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Playnite.SDK;

namespace CheckDlc.Controls
{
    /// <summary>
    /// Logique d'interaction pour PluginButton.xaml
    /// </summary>
    public partial class PluginButton : PluginUserControlExtend
    {
        private readonly CheckDlc Plugin;

        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase; 
        internal override IPluginDatabase pluginDatabase => PluginDatabase;

        private PluginButtonDataContext ControlDataContext =  new PluginButtonDataContext();
        internal override IDataContext controlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PluginButtonDataContext)controlDataContext;
        }

        public PluginButton(CheckDlc plugin)
        {
            Plugin = plugin;
            AlwaysShow = true;

            InitializeComponent();
            DataContext = ControlDataContext;

            _ = Task.Run(() =>
            {
                // Wait extension database are loaded
                _ = System.Threading.SpinWait.SpinUntil(() => PluginDatabase.IsLoaded, -1);
                _ = Dispatcher.BeginInvoke((Action)delegate
                {
                    PluginDatabase.PluginSettings.PropertyChanged += PluginSettings_PropertyChanged;
                    PluginDatabase.Database.ItemUpdated += Database_ItemUpdated;
                    PluginDatabase.Database.ItemCollectionChanged += Database_ItemCollectionChanged;
                    API.Instance.Database.Games.ItemUpdated += Games_ItemUpdated;

                    // Apply settings
                    PluginSettings_PropertyChanged(null, null);
                });
            });
        }


        public override void SetDefaultDataContext()
        {
            ControlDataContext.IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationButton;
            ControlDataContext.Text = "\ue91f";
        }


        public override void SetData(Game newContext, PluginDataBaseGameBase PluginGameData)
        {
            GameDlc gameDlc = (GameDlc)PluginGameData;
            MustDisplay = gameDlc.HasData;
        }


        #region Events
        private void PART_PluginButton_Click(object sender, RoutedEventArgs e)
        {
            WindowOptions windowOptions = new WindowOptions
            {
                CanBeResizable = false,
                Height = 720,
                Width = 1000,
                ShowMaximizeButton = false
            };
            CheclDlcGameView ViewExtension = new CheclDlcGameView(Plugin, GameContext);
            Window windowExtension = PlayniteUiHelper.CreateExtensionWindow(ResourceProvider.GetString("LOCCheckDlc"), ViewExtension, windowOptions);
            _ = windowExtension.ShowDialog();
        }
        #endregion
    }


    public class PluginButtonDataContext : ObservableObject, IDataContext
    {
        private bool isActivated;
        public bool IsActivated { get => isActivated; set => SetValue(ref isActivated, value); }

        private string text = "\ue91f";
        public string Text { get => text; set => SetValue(ref text, value); }
    }
}
