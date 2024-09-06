using CheckDlc.Services;
using CommonPluginsShared;
using Playnite.SDK;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using CheckDlc.Clients;
using CheckDlc.Models;
using System.Collections.Generic;
using CommonPluginsStores.Gog;
using CommonPluginsStores.Gog.Models;
using CommonPluginsStores.Models;
using CommonPluginsStores.Origin;
using CommonPluginsStores.Steam;

namespace CheckDlc.Views
{
    public partial class CheckDlcSettingsView : UserControl
    {
        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;


        public CheckDlcSettingsView()
        {
            InitializeComponent();

            SteamPanel.StoreApi = CheckDlc.SteamApi;
            EpicPanel.StoreApi = CheckDlc.EpicApi;

            // List features
            PART_FeatureDlc.ItemsSource = API.Instance.Database.Features.OrderBy(x => x.Name);

            // List GOG currencies
            GogApi gogApi = new GogApi(PluginDatabase.PluginName);
            List<StoreCurrency> dataGog = gogApi.GetCurrencies();   
            PART_GogCurrency.ItemsSource = dataGog.OrderBy(x => x.currency).ToList();

            try
            {
                int idx = ((List<StoreCurrency>)PART_GogCurrency.ItemsSource).FindIndex(x => x.currency == PluginDatabase.PluginSettings.Settings.GogCurrency.currency);
                PART_GogCurrency.SelectedIndex = idx;
            }
            catch { }

            // List Origin currencies
            OriginApi originApi = new OriginApi(PluginDatabase.PluginName);
            List<StoreCurrency> dataOrigin = originApi.GetCurrencies();            
            PART_OriginCurrency.ItemsSource = dataOrigin.OrderBy(x => x.currency).ToList();

            try
            {
                int idx = ((List<StoreCurrency>)PART_OriginCurrency.ItemsSource).FindIndex(x => x.country == PluginDatabase.PluginSettings.Settings.OriginCurrency.country);
                PART_OriginCurrency.SelectedIndex = idx;
            }
            catch { }

            SteamPanel.Visibility = PluginDatabase.PluginSettings.Settings.PluginState.SteamIsEnabled ? Visibility.Visible : Visibility.Collapsed;
            EpicPanel.Visibility = PluginDatabase.PluginSettings.Settings.PluginState.EpicIsEnabled ? Visibility.Visible : Visibility.Collapsed;
        }


        #region Tag
        private void ButtonAddTag_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.AddTagAllGame();
        }

        private void ButtonRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.RemoveTagAllGame();
        }
        #endregion


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _ = Process.Start((string)((FrameworkElement)sender).Tag);
        }


        private void Button_Click_Remove(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = int.Parse(((FrameworkElement)sender).Tag.ToString());
                ((ObservableCollection<string>)PART_IgnoredList.ItemsSource).RemoveAt(index);
                PART_IgnoredList.Items.Refresh();
            }
            catch (Exception ex)
            {
                Common.LogError(ex, true);
            }
        }

        private void Button_Click_Add(object sender, RoutedEventArgs e)
        {
            StringSelectionDialogResult item = API.Instance.Dialogs.SelectString(ResourceProvider.GetString("LOCCommonInputItemIgnore"), ResourceProvider.GetString("LOCCheckDlc"), string.Empty);
            if (!item.SelectedString.IsNullOrEmpty())
            {
                ((ObservableCollection<string>)PART_IgnoredList.ItemsSource).Add(item.SelectedString);
            }
        }
    }
}