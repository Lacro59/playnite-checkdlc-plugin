using CheckDlc.Models;
using CheckDlc.Services;
using CommonPluginsShared.Collections;
using CommonPluginsShared.Controls;
using CommonPluginsShared.Interfaces;
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
        private CheckDlcDatabase PluginDatabase = CheckDlc.PluginDatabase;
        internal override IPluginDatabase _PluginDatabase
        {
            get => PluginDatabase;
            set => PluginDatabase = (CheckDlcDatabase)_PluginDatabase;
        }

        private PPluginListDlcDataContext ControlDataContext = new PPluginListDlcDataContext();
        internal override IDataContext _ControlDataContext
        {
            get => ControlDataContext;
            set => ControlDataContext = (PPluginListDlcDataContext)_ControlDataContext;
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
                    PluginDatabase.PlayniteApi.Database.Games.ItemUpdated += Games_ItemUpdated;

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
        private bool _IsActivated;
        public bool IsActivated { get => _IsActivated; set => SetValue(ref _IsActivated, value); }

        private ObservableCollection<Dlc> _ItemsSource;
        public ObservableCollection<Dlc> ItemsSource { get => _ItemsSource; set => SetValue(ref _ItemsSource, value); }
    }


    public enum ListDlcType
    {
        All, Owned, NotOwned
    }
}
