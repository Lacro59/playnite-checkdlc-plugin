using CheckDlc.Services;
using CommonPluginsControls.Stores;
using CommonPluginsControls.Stores.Models;
using CommonPluginsShared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using CommonPluginsStores.Models;

namespace CheckDlc.Views
{
    public partial class CheckDlcSettingsView : UserControl
    {
        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;

        private bool _storeUiInitialized;


        public CheckDlcSettingsView()
        {
            InitializeComponent();
            Loaded += CheckDlcSettingsView_Loaded;
        }

        private void CheckDlcSettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            if (DataContext is CheckDlcSettingsViewModel viewModel)
            {
                viewModel.RefreshFeatures();
                viewModel.RefreshStoreAvailability();
            }

            PluginDatabase.EnsureStoreApis(
                (DataContext as CheckDlcSettingsViewModel)?.Settings,
                reloadAccountInfos: false);

            if (_storeUiInitialized)
            {
                RefreshStorePanels();
                InitializeCurrencyComboBoxes();
                return;
            }

            _storeUiInitialized = true;
            StoreSettingsLog.Debug("CheckDlc settings view initializing store panels");
            RegisterStorePanels();
            InitializeStorePanels();
            InitializeCurrencyComboBoxes();
        }

        /// <summary>
        /// Binds store panels to API instances when the corresponding library integration is enabled.
        /// </summary>
        private void InitializeStorePanels()
        {
            RefreshStorePanels();
        }

        private void RefreshStorePanels()
        {
            CheckDlcSettings settings = PluginDatabase.PluginSettings;

            SteamPanel.StoreApi = settings.PluginState.SteamIsEnabled ? CheckDlc.SteamApi : null;
            EpicPanel.StoreApi = settings.PluginState.EpicIsEnabled ? CheckDlc.EpicApi : null;
            GogPanel.StoreApi = settings.PluginState.GogIsEnabled ? CheckDlc.GogApi : null;
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
            CheckDlcSettings settings = PluginDatabase.PluginSettings;

            if (settings.PluginState.GogIsEnabled)
            {
                List<StoreCurrency> gogCurrencies = GetGogCurrencyList();
                PART_GogCurrency.ItemsSource = gogCurrencies;
                SelectCurrency(PART_GogCurrency, settings.GogCurrency, matchCountry: false);
            }
            else
            {
                PART_GogCurrency.ItemsSource = null;
            }

            if (settings.PluginState.OriginIsEnabled)
            {
                List<StoreCurrency> originCurrencies = new List<StoreCurrency>
                {
                    new StoreCurrency { country = "US", currency = "USD", symbol = "$" },
                    new StoreCurrency { country = "GB", currency = "GBP", symbol = "£" },
                    new StoreCurrency { country = "FR", currency = "EUR", symbol = "€" },
                    new StoreCurrency { country = "DE", currency = "EUR", symbol = "€" }
                }.OrderBy(x => x.currency).ToList();
                PART_OriginCurrency.ItemsSource = originCurrencies;
                SelectCurrency(PART_OriginCurrency, settings.OriginCurrency, matchCountry: true);
            }
            else
            {
                PART_OriginCurrency.ItemsSource = null;
            }
        }

        private List<StoreCurrency> GetGogCurrencyList()
        {
            if (PluginDatabase.PluginSettings.PluginState.GogIsEnabled && CheckDlc.GogApi != null)
            {
                return CheckDlc.GogApi.GetCurrencies()
                    .OrderBy(x => x.currency)
                    .ToList();
            }

            StoreCurrency saved = PluginDatabase.PluginSettings.GogCurrency;
            if (saved != null && !string.IsNullOrEmpty(saved.currency))
            {
                return new List<StoreCurrency> { saved };
            }

            return GetDefaultGogCurrencies();
        }

        private static List<StoreCurrency> GetDefaultGogCurrencies()
        {
            return new List<StoreCurrency>
            {
                new StoreCurrency { country = "US", currency = "USD", symbol = "$" }
            };
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
