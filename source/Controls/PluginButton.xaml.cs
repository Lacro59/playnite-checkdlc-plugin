using CheckDlc.Models;
using CheckDlc.Services;
using CommonPluginsShared;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Windows;

namespace CheckDlc.Controls
{
    /// <summary>
    /// Interaction logic for PluginButton.xaml
    /// </summary>
    public partial class PluginButton : PluginUserControlExtend
    {
        private static CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;
        protected override IPluginDatabase pluginDatabase => PluginDatabase;

        private PluginButtonDataContext ControlDataContext = new PluginButtonDataContext();
        protected override IDataContext controlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PluginButtonDataContext)value;
        }

        public PluginButton()
        {
            AlwaysShow = false;

            InitializeComponent();
            DataContext = ControlDataContext;
            Loaded += OnLoaded;
        }

        protected override void AttachStaticEvents()
        {
            base.AttachStaticEvents();

            AttachPluginEvents(PluginDatabase.PluginName, () =>
            {
                PluginDatabase.PluginSettings.PropertyChanged += CreatePluginSettingsHandler();
                PluginDatabase.DatabaseItemUpdated += CreateDatabaseItemUpdatedHandler<GameDlc>();
                PluginDatabase.DatabaseItemCollectionChanged += CreateDatabaseCollectionChangedHandler<GameDlc>();
            });
        }

        public override void SetDefaultDataContext()
        {
            ControlDataContext.IsActivated = PluginDatabase.PluginSettings.EnableIntegrationButton;
            ControlDataContext.Text = "\ue91f";
        }

        public override void SetData(Game newContext, PluginGameEntry pluginGameData)
        {
            GameDlc gameDlc = (GameDlc)pluginGameData;
            MustDisplay = gameDlc.HasData;
        }

        private void PART_PluginButton_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.PluginWindows.ShowPluginGameDataWindow(CurrentGame);
        }
    }


    public class PluginButtonDataContext : ObservableObject, IDataContext
    {
        private bool isActivated;
        public bool IsActivated { get => isActivated; set => SetValue(ref isActivated, value); }

        private string text = "\ue91f";
        public string Text { get => text; set => SetValue(ref text, value); }
    }
}
