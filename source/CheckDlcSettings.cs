using CommonPluginsStores.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CheckDlc
{
    public class CheckDlcSettings : ObservableObject
    {
        #region Settings variables
        public bool MenuInExtensions { get; set; } = true;
        public DateTime LastAutoLibUpdateAssetsDownload { get; set; } = DateTime.Now;

        public bool EnableTag { get; set; } = false;
        public bool AutoImport { get; set; } = true;

        public bool PriceNotification { get; set; } = false;

        public StoreCurrency GogCurrency { get; set; } = new StoreCurrency { country = "US", currency = "USD", symbol = "$" };
        public StoreCurrency OriginCurrency { get; set; } = new StoreCurrency { country = "US", currency = "USD", symbol = "$" };

        public ObservableCollection<string> IgnoredList { get; set; } = new ObservableCollection<string>();

        public GameFeature DlcFeature { get; set; } = null;

        private bool enableIntegrationButton { get; set; } = true;
        public bool EnableIntegrationButton
        {
            get => enableIntegrationButton;
            set
            {
                enableIntegrationButton = value;
                OnPropertyChanged();
            }
        }

        private bool enableIntegrationListDlcAll { get; set; } = true;
        public bool EnableIntegrationListDlcAll
        {
            get => enableIntegrationListDlcAll;
            set
            {
                enableIntegrationListDlcAll = value;
                OnPropertyChanged();
            }
        }

        private bool enableIntegrationListDlcOwned { get; set; } = true;
        public bool EnableIntegrationListDlcOwned
        {
            get => enableIntegrationListDlcOwned;
            set
            {
                enableIntegrationListDlcOwned = value;
                OnPropertyChanged();
            }
        }

        private bool enableIntegrationListDlcNotOwned { get; set; } = true;
        public bool EnableIntegrationListDlcNotOwned
        {
            get => enableIntegrationListDlcNotOwned;
            set
            {
                enableIntegrationListDlcNotOwned = value;
                OnPropertyChanged();
            }
        }

        private bool enableIntegrationButtonDetails { get; set; } = false;
        public bool EnableIntegrationButtonDetails
        {
            get => enableIntegrationButtonDetails;
            set
            {
                enableIntegrationButtonDetails = value;
                OnPropertyChanged();
            }
        }

        public SteamApiSettings SteamApiSettings { get; set; } = new SteamApiSettings();
        #endregion

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        #region Variables exposed
        private bool _HasData { get; set; } = false;
        [DontSerialize]
        public bool HasData
        {
            get => _HasData;
            set
            {
                _HasData = value;
                OnPropertyChanged();
            }
        }
        #endregion  
    }

    public class CheckDlcSettingsViewModel : ObservableObject, ISettings
    {
        private readonly CheckDlc Plugin;
        private CheckDlcSettings EditingClone { get; set; }

        private CheckDlcSettings settings;
        public CheckDlcSettings Settings { get => settings; set => SetValue(ref settings, value); }

        public CheckDlcSettingsViewModel(CheckDlc plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            Plugin = plugin;

            // Load saved settings.
            CheckDlcSettings savedSettings = plugin.LoadPluginSettings<CheckDlcSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            Settings = savedSettings ?? new CheckDlcSettings();
        }

        // Code executed when settings view is opened and user starts editing values.
        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        // Code executed when user decides to cancel any changes made since BeginEdit was called.
        // This method should revert any changes made to Option1 and Option2.
        public void CancelEdit()
        {
            Settings = EditingClone;
        }

        // Code executed when user decides to confirm changes made since BeginEdit was called.
        // This method should save settings made to Option1 and Option2.
        public void EndEdit()
        {
            // StoreAPI intialization
            CheckDlc.SteamApi.SaveCurrentUser();
            CheckDlc.SteamApi.CurrentAccountInfos = null;
            _ = CheckDlc.SteamApi.CurrentAccountInfos;

            Plugin.SavePluginSettings(Settings);
            CheckDlc.PluginDatabase.PluginSettings = this;
            this.OnPropertyChanged();
        }

        // Code execute when user decides to confirm changes made since BeginEdit was called.
        // Executed before EndEdit is called and EndEdit is not called if false is returned.
        // List of errors is presented to user if verification fails.
        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }

    public class SteamApiSettings
    {
        public bool UseApi { get; set; } = false;
        public bool UseAuth { get; set; } = false;
    }
}
