using CheckDlc.Services;
using CommonPluginsControls.Stores;
using CommonPluginsControls.Stores.Models;
using CommonPluginsShared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommonPluginsStores.Gog;
using CommonPluginsStores.Models;

namespace CheckDlc.Views
{
    public partial class CheckDlcSettingsView : UserControl
    {
        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;


        public CheckDlcSettingsView()
        {
            InitializeComponent();
            Loaded += CheckDlcSettingsView_Loaded;

            StoreSettingsLog.Debug("CheckDlc settings view initializing store panels");

            SteamPanel.StoreApi = CheckDlc.SteamApi;
            EpicPanel.StoreApi = CheckDlc.EpicApi;
            GogPanel.StoreApi = CheckDlc.GogApi;

            RegisterStorePanels();
            InitializeCurrencyComboBoxes();
        }

        private void CheckDlcSettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is CheckDlcSettingsViewModel viewModel)
            {
                viewModel.RefreshFeatures();
            }
        }

        private void RegisterStorePanels()
        {
            StoresSettings.RegisterStore(new StoreSettingsEntry
            {
                Id = "Steam",
                NameResourceKey = "LOCCommonStoreSteam",
                CategoryResourceKey = "LOCCommonStoresLaunchers",
                Panel = SteamPanel,
                IsVisible = PluginDatabase.PluginSettings.PluginState.SteamIsEnabled,
                SortOrder = 0
            });

            StoresSettings.RegisterStore(new StoreSettingsEntry
            {
                Id = "Epic",
                NameResourceKey = "LOCCommonStoreEpic",
                CategoryResourceKey = "LOCCommonStoresLaunchers",
                Panel = EpicPanel,
                IsVisible = PluginDatabase.PluginSettings.PluginState.EpicIsEnabled,
                SortOrder = 1
            });

            StoresSettings.RegisterStore(new StoreSettingsEntry
            {
                Id = "Gog",
                NameResourceKey = "LOCCommonStoreGog",
                CategoryResourceKey = "LOCCommonStoresLaunchers",
                Panel = GogPanel,
                IsVisible = PluginDatabase.PluginSettings.PluginState.GogIsEnabled,
                SortOrder = 2
            });
        }

        private void InitializeCurrencyComboBoxes()
        {
            List<StoreCurrency> gogCurrencies = CheckDlc.GogApi.GetCurrencies()
                .OrderBy(x => x.currency)
                .ToList();
            PART_GogCurrency.ItemsSource = gogCurrencies;
            SelectCurrency(PART_GogCurrency, PluginDatabase.PluginSettings.GogCurrency, matchCountry: false);

            List<StoreCurrency> originCurrencies = new List<StoreCurrency>
            {
                new StoreCurrency { country = "US", currency = "USD", symbol = "$" },
                new StoreCurrency { country = "GB", currency = "GBP", symbol = "£" },
                new StoreCurrency { country = "FR", currency = "EUR", symbol = "€" },
                new StoreCurrency { country = "DE", currency = "EUR", symbol = "€" }
            }.OrderBy(x => x.currency).ToList();
            PART_OriginCurrency.ItemsSource = originCurrencies;
            SelectCurrency(PART_OriginCurrency, PluginDatabase.PluginSettings.OriginCurrency, matchCountry: true);
        }

        private static void SelectCurrency(ComboBox comboBox, StoreCurrency savedCurrency, bool matchCountry)
        {
            if (comboBox?.ItemsSource == null || savedCurrency == null)
            {
                return;
            }

            try
            {
                var currencies = (List<StoreCurrency>)comboBox.ItemsSource;
                StoreCurrency match = matchCountry
                    ? currencies.Find(x => x.country == savedCurrency.country)
                    : currencies.Find(x => x.currency == savedCurrency.currency);

                if (match != null)
                {
                    comboBox.SelectedItem = match;
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, true);
            }
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


        private void Button_RemoveIgnored_Click(object sender, RoutedEventArgs e)
        {
            RemoveListItem(PART_IgnoredList, sender);
        }

        private void Button_RemoveManuallyOwned_Click(object sender, RoutedEventArgs e)
        {
            RemoveListItem(PART_ManuallyOwnedList, sender);
        }

        private static void RemoveListItem(ListBox listBox, object sender)
        {
            try
            {
                string item = ((FrameworkElement)sender).Tag as string;
                if (string.IsNullOrEmpty(item))
                {
                    return;
                }

                var collection = listBox?.ItemsSource as ObservableCollection<string>;
                if (collection != null && collection.Contains(item))
                {
                    collection.Remove(item);
                }
            }
            catch (Exception ex)
            {
                Common.LogError(ex, true);
            }
        }
    }
}
