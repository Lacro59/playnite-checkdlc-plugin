using CheckDlc.Models;
using CommonPluginsStores.Gog.Models;
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

        public StoreCurrency GogCurrency { get; set; } = new StoreCurrency { country = "US", currency = "USD", symbol = "$" };
        public StoreCurrency OriginCurrency { get; set; } = new StoreCurrency { country = "US", currency = "USD", symbol = "$" };

        public ObservableCollection<string> IgnoredList { get; set; } = new ObservableCollection<string>();

        public GameFeature DlcFeature { get; set; } = null;

        private bool _EnableIntegrationButton { get; set; } = true;
        public bool EnableIntegrationButton
        {
            get => _EnableIntegrationButton;
            set
            {
                _EnableIntegrationButton = value;
                OnPropertyChanged();
            }
        }

        private bool _EnableIntegrationListDlcAll { get; set; } = true;
        public bool EnableIntegrationListDlcAll
        {
            get => _EnableIntegrationListDlcAll;
            set
            {
                _EnableIntegrationListDlcAll = value;
                OnPropertyChanged();
            }
        }

        private bool _EnableIntegrationListDlcOwned { get; set; } = true;
        public bool EnableIntegrationListDlcOwned
        {
            get => _EnableIntegrationListDlcOwned;
            set
            {
                _EnableIntegrationListDlcOwned = value;
                OnPropertyChanged();
            }
        }

        private bool _EnableIntegrationListDlcNotOwned { get; set; } = true;
        public bool EnableIntegrationListDlcNotOwned
        {
            get => _EnableIntegrationListDlcNotOwned;
            set
            {
                _EnableIntegrationListDlcNotOwned = value;
                OnPropertyChanged();
            }
        }

        private bool _EnableIntegrationButtonDetails { get; set; } = false;
        public bool EnableIntegrationButtonDetails
        {
            get => _EnableIntegrationButtonDetails;
            set
            {
                _EnableIntegrationButtonDetails = value;
                OnPropertyChanged();
            }
        }
        #endregion

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        #region Variables exposed

        #endregion  
    }

    public class CheckDlcSettingsViewModel : ObservableObject, ISettings
    {
        private readonly CheckDlc Plugin;
        private CheckDlcSettings EditingClone { get; set; }

        private CheckDlcSettings settings;
        public CheckDlcSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public CheckDlcSettingsViewModel(CheckDlc plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            Plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<CheckDlcSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new CheckDlcSettings();
            }
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
}