using CheckDlc.Models;
using CheckDlc.Services;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace CheckDlc.Controls
{
    /// <summary>
    /// Interaction logic for PluginListDlc.xaml
    /// </summary>
    public partial class PluginListDlc : PluginUserControlExtend
    {
        private static CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;
        protected override IPluginDatabase pluginDatabase => PluginDatabase;

        private PPluginListDlcDataContext ControlDataContext = new PPluginListDlcDataContext();
        protected override IDataContext controlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PPluginListDlcDataContext)value;
        }

        public ListDlcType ListType { get; set; } = ListDlcType.All;

        public PluginListDlc()
        {
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
            switch (ListType)
            {
                case ListDlcType.All:
                    ControlDataContext.IsActivated = PluginDatabase.PluginSettings.EnableIntegrationListDlcAll;
                    break;

                case ListDlcType.Owned:
                    ControlDataContext.IsActivated = PluginDatabase.PluginSettings.EnableIntegrationListDlcOwned;
                    break;

                case ListDlcType.NotOwned:
                    ControlDataContext.IsActivated = PluginDatabase.PluginSettings.EnableIntegrationListDlcNotOwned;
                    break;
            }

            ControlDataContext.ItemsSource = new ObservableCollection<Dlc>();
        }

        public override void SetData(Game newContext, PluginGameEntry pluginGameData)
        {
            GameDlc gameDlc = (GameDlc)pluginGameData;
            switch (ListType)
            {
                case ListDlcType.All:
                    ControlDataContext.ItemsSource = gameDlc.Items.ToObservable();
                    break;

                case ListDlcType.Owned:
                    ControlDataContext.ItemsSource = gameDlc.Items.Where(x => x.IsOwned).ToObservable();
                    break;

                case ListDlcType.NotOwned:
                    ControlDataContext.ItemsSource = gameDlc.Items.Where(x => !x.IsOwned).ToObservable();
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string link = (string)((FrameworkElement)sender).Tag;
            if (!string.IsNullOrEmpty(link))
            {
                _ = Process.Start(link);
            }
        }
    }


    public class PPluginListDlcDataContext : ObservableObject, IDataContext
    {
        private bool isActivated;
        public bool IsActivated { get => isActivated; set => SetValue(ref isActivated, value); }

        private ObservableCollection<Dlc> itemsSource;
        public ObservableCollection<Dlc> ItemsSource { get => itemsSource; set => SetValue(ref itemsSource, value); }
    }


    public enum ListDlcType
    {
        All, Owned, NotOwned
    }
}
