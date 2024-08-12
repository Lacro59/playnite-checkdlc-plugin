using CheckDlc.Models;
using CheckDlc.Services;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CheckDlc.Controls
{
    /// <summary>
    /// Logique d'interaction pour PluginListDlc.xaml
    /// </summary>
    public partial class PluginListDlc : PluginUserControlExtend
    {
        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;
        internal override IPluginDatabase pluginDatabase => PluginDatabase;

        private PPluginListDlcDataContext ControlDataContext = new PPluginListDlcDataContext();
        internal override IDataContext controlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PPluginListDlcDataContext)controlDataContext;
        }


        #region Properties
        public static readonly DependencyProperty ListTypeProperty;
        public ListDlcType ListType { get; set; } = ListDlcType.All;
        #endregion


        public PluginListDlc()
        {
            InitializeComponent();
            this.DataContext = ControlDataContext;

            Task.Run(() =>
            {
                // Wait extension database are loaded
                System.Threading.SpinWait.SpinUntil(() => PluginDatabase.IsLoaded, -1);

                this.Dispatcher.BeginInvoke((Action)delegate
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
            switch (ListType)
            {
                case ListDlcType.All:
                    ControlDataContext.IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationListDlcAll;
                    break;

                case ListDlcType.Owned:
                    ControlDataContext.IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationListDlcOwned;
                    break;

                case ListDlcType.NotOwned:
                    ControlDataContext.IsActivated = PluginDatabase.PluginSettings.Settings.EnableIntegrationListDlcNotOwned;
                    break;
            }
            
            ControlDataContext.ItemsSource = new ObservableCollection<Dlc>();
        }


        public override void SetData(Game newContext, PluginDataBaseGameBase PluginGameData)
        {
            GameDlc gameDlc = (GameDlc)PluginGameData;

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


        #region Events
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!((string)((FrameworkElement)sender).Tag).IsNullOrEmpty())
            {
                Process.Start((string)((FrameworkElement)sender).Tag);
            }
        }
        #endregion  
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
