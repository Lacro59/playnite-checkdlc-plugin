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
            GogPanel.StoreApi = CheckDlc.GogApi;

            // List features
            PART_FeatureDlc.ItemsSource = API.Instance.Database.Features.OrderBy(x => x.Name);

            // List GOG currencies
            List<StoreCurrency> dataGog = CheckDlc.GogApi.GetCurrencies();
            PART_GogCurrency.ItemsSource = dataGog.OrderBy(x => x.currency).ToList();

            try
            {
                int idx = ((List<StoreCurrency>)PART_GogCurrency.ItemsSource).FindIndex(x => x.currency == PluginDatabase.PluginSettings.GogCurrency.currency);
                PART_GogCurrency.SelectedIndex = idx;
            }
            catch { }

            // List Origin currencies
            List<StoreCurrency> dataOrigin = new List<StoreCurrency>
            {
                new StoreCurrency { country = "US", currency = "USD", symbol = "$" },
                new StoreCurrency { country = "GB", currency = "GBP", symbol = "£" },
                new StoreCurrency { country = "FR", currency = "EUR", symbol = "€" },
                new StoreCurrency { country = "DE", currency = "EUR", symbol = "€" }
            };
            PART_OriginCurrency.ItemsSource = dataOrigin.OrderBy(x => x.currency).ToList();

            try
            {
                int idx = ((List<StoreCurrency>)PART_OriginCurrency.ItemsSource).FindIndex(x => x.country == PluginDatabase.PluginSettings.OriginCurrency.country);
                PART_OriginCurrency.SelectedIndex = idx;
            }
            catch { }

            SteamPanel.Visibility = PluginDatabase.PluginSettings.PluginState.SteamIsEnabled ? Visibility.Visible : Visibility.Collapsed;
            EpicPanel.Visibility = PluginDatabase.PluginSettings.PluginState.EpicIsEnabled ? Visibility.Visible : Visibility.Collapsed;
            GogPanel.Visibility = PluginDatabase.PluginSettings.PluginState.GogIsEnabled ? Visibility.Visible : Visibility.Collapsed;
        }


        #region Tag
        private void ButtonAddTag_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.AddTagAllGames();
        }

        private void ButtonRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.RemoveTagAllGames();
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

        private void Button_Click_Remove2(object sender, RoutedEventArgs e)
        {
            try
            {
                int index = int.Parse(((FrameworkElement)sender).Tag.ToString());
                ((ObservableCollection<string>)PART_ManuallyOwnedList.ItemsSource).RemoveAt(index);
                PART_ManuallyOwnedList.Items.Refresh();
            }
            catch (Exception ex)
            {
                Common.LogError(ex, true);
            }
        }
    }
}