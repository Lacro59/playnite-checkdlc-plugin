using CheckDlc.Models;
using CommonPluginsShared;
using CommonPluginsShared.Interfaces;
using CommonPluginsShared.Plugins;
using CommonPluginsStores;
using CommonPluginsStores.Models;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CheckDlc
{
    public class CheckDlcSettings : PluginSettings
    {
        /// <summary>
        /// Normalized source names included in CheckDlc library operations (see <see cref="PlayniteTools.GetSourceName"/>).
        /// Matches <c>source/Clients</c> store coverage — fixed plugin policy, not exposed in the settings UI.
        /// Includes legacy Playnite source names (e.g. <c>Origin</c> before EA app rebranding).
        /// </summary>
        private static readonly IReadOnlyList<string> FixedSupportedSources = new List<string>
        {
            "Steam",
            "Epic",
            "GOG",
            "EA app",
            "Origin",
            "Playstation",
            "Nintendo"
        };

        public CheckDlcSettings()
        {
            ApplyFixedLibraryFilterPolicy();
        }

        /// <summary>
        /// Applies fixed library filter values for this plugin (not user-configurable).
        /// </summary>
        public void ApplyFixedLibraryFilterPolicy()
        {
            IncludeEmulatedGames = false;
            LibrarySourceFilterMode = SourceFilterMode.Whitelist;
            EnabledSources = new List<string>(FixedSupportedSources);
            ExcludedSources = new List<string>();
        }

        #region Settings variables
        public bool EnableTagAllDlc { get; set; } = true;

        public bool PriceNotification { get; set; } = false;

        public StoreCurrency GogCurrency { get; set; } = new StoreCurrency { country = "US", currency = "USD", symbol = "$" };
        public StoreCurrency OriginCurrency { get; set; } = new StoreCurrency { country = "US", currency = "USD", symbol = "$" };

        public ObservableCollection<string> IgnoredList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> ManuallyOwneds { get; set; } = new ObservableCollection<string>();

        public GameFeature DlcFeature { get; set; } = null;

        private bool _enableIntegrationButton = true;
        public bool EnableIntegrationButton { get => _enableIntegrationButton; set => SetValue(ref _enableIntegrationButton, value); }

        private bool _enableIntegrationListDlcAll = true;
        public bool EnableIntegrationListDlcAll { get => _enableIntegrationListDlcAll; set => SetValue(ref _enableIntegrationListDlcAll, value); }

        private bool _enableIntegrationListDlcOwned = true;
        public bool EnableIntegrationListDlcOwned { get => _enableIntegrationListDlcOwned; set => SetValue(ref _enableIntegrationListDlcOwned, value); }

        private bool _enableIntegrationListDlcNotOwned = true;
        public bool EnableIntegrationListDlcNotOwned { get => _enableIntegrationListDlcNotOwned; set => SetValue(ref _enableIntegrationListDlcNotOwned, value); }

        private bool _enableIntegrationButtonDetails = false;
        public bool EnableIntegrationButtonDetails { get => _enableIntegrationButtonDetails; set => SetValue(ref _enableIntegrationButtonDetails, value); }

        // TODO TEMP
        public SteamSettings SteamApiSettings { get; set; } = new SteamSettings();
        [DontSerialize]
        public EpicSettings EpicSettings { get; set; } = new EpicSettings();

        public StoreSettings SteamStoreSettings { get; set; } = new StoreSettings { ForceAuth = true, UseAuth = true, UseApi = false };
        public StoreSettings EpicStoreSettings { get; set; } = new StoreSettings { ForceAuth = true, UseAuth = true };
        public StoreSettings GogStoreSettings { get; set; } = new StoreSettings { ForceAuth = true, UseAuth = true };


        // TODO TEMP
        public bool IsConverted { get; set; } = false;
        #endregion

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        #region Variables exposed for custom themes
        private List<Dlc> _listDlcs = new List<Dlc>();
        [DontSerialize]
        public List<Dlc> ListDlcs { get => _listDlcs; set => SetValue(ref _listDlcs, value); }
        #endregion  
    }

    public class CheckDlcSettingsViewModel : PluginSettingsViewModel, IPluginSettingsViewModel
    {
        private readonly CheckDlc Plugin;
        private CheckDlcSettings EditingClone { get; set; }

        private CheckDlcSettings _settings;
        public CheckDlcSettings Settings { get => _settings; set => SetValue(ref _settings, value); }
        IPluginSettings IPluginSettingsViewModel.Settings => Settings;

        private List<GameFeature> _features = new List<GameFeature>();
        /// <summary>
        /// Playnite features available for automatic DLC tagging.
        /// </summary>
        public List<GameFeature> Features
        {
            get => _features;
            private set => SetValue(ref _features, value);
        }

        /// <summary>Gets whether the GOG library integration is enabled in Playnite.</summary>
        public bool IsGogStoreEnabled => Settings?.PluginState.GogIsEnabled ?? false;

        /// <summary>Gets whether the Origin/EA library integration is enabled in Playnite.</summary>
        public bool IsOriginStoreEnabled => Settings?.PluginState.OriginIsEnabled ?? false;

        /// <summary>Gets whether any store currency selector should be shown.</summary>
        public bool ShowCurrencySection => IsGogStoreEnabled || IsOriginStoreEnabled;

        public CheckDlcSettingsViewModel(CheckDlc plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            Plugin = plugin;

            // Load saved settings.
            CheckDlcSettings savedSettings = plugin.LoadPluginSettings<CheckDlcSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            Settings = savedSettings ?? new CheckDlcSettings();

            if (Settings.EpicSettings == null)
            {
                Settings.EpicSettings = new EpicSettings();
            }

            // TODO temp
            if (Settings.SteamStoreSettings == null)
            {
                Settings.SteamStoreSettings = new StoreSettings
                {
                    ForceAuth = true,
                    UseApi = false,
                    UseAuth = true
                };
            }
            else
            {
                Settings.SteamStoreSettings.ForceAuth = true;
                Settings.SteamStoreSettings.UseApi = false;
                Settings.SteamStoreSettings.UseAuth = true;
            }
            if (Settings.EpicStoreSettings == null)
            {
                Settings.EpicStoreSettings = new StoreSettings
                {
                    UseAuth = Settings.EpicSettings.UseAuth
                };
            }

            Settings.ApplyFixedLibraryFilterPolicy();
        }

        /// <summary>
        /// Refreshes store availability flags used to show or hide currency selectors in settings.
        /// </summary>
        public void RefreshStoreAvailability()
        {
            OnPropertyChanged(nameof(IsGogStoreEnabled));
            OnPropertyChanged(nameof(IsOriginStoreEnabled));
            OnPropertyChanged(nameof(ShowCurrencySection));
        }

        /// <summary>
        /// Loads Playnite features for the DLC metadata combo box.
        /// Must run when settings are opened; the database is not populated at plugin startup.
        /// </summary>
        public void RefreshFeatures()
        {
            if (API.Instance?.Database?.Features == null)
            {
                Features = new List<GameFeature>();
                return;
            }

            List<GameFeature> features = API.Instance.Database.Features.OrderBy(x => x.Name).ToList();
            Features = features;

            if (Settings?.DlcFeature == null)
            {
                return;
            }

            GameFeature match = features.FirstOrDefault(x => x.Id == Settings.DlcFeature.Id);
            if (match != null && !ReferenceEquals(Settings.DlcFeature, match))
            {
                Settings.DlcFeature = match;
            }
        }

        // Code executed when settings view is opened and user starts editing values.
        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
            RefreshFeatures();
            RefreshStoreAvailability();
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
            Settings.ApplyFixedLibraryFilterPolicy();

            CheckDlc.PluginDatabase.EnsureStoreApis(Settings, reloadAccountInfos: true);

            Plugin.SavePluginSettings(Settings);
            CheckDlc.PluginDatabase.PluginSettings = Settings;
            OnPropertyChanged();
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

    // TODO TEMP
    public class SteamSettings
    {
        public bool UseApi { get; set; } = false;
        public bool UseAuth { get; set; } = true;
    }

    public class EpicSettings
    {
        public bool UseAuth { get; set; } = true;
    }
}
