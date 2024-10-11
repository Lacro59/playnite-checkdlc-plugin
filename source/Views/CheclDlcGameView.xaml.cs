using CheckDlc.Models;
using CheckDlc.Services;
using Playnite.SDK.Models;
using System;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using CommonPluginsShared;
using CommonPluginsShared.Converters;
using System.Globalization;

namespace CheckDlc.Views
{
    /// <summary>
    /// Logique d'interaction pour CheclDlcGameView.xaml
    /// </summary>
    public partial class CheclDlcGameView : UserControl
    {
        private readonly CheckDlc Plugin;
        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;

        private Game GameContext { get; set; }


        public CheclDlcGameView(CheckDlc plugin, Game GameContext)
        {
            Plugin = plugin;

            InitializeComponent();
   
            this.GameContext = GameContext;
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked, PART_LimitPrice.Text);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!((string)((FrameworkElement)sender).Tag).IsNullOrEmpty())
            {
                _ = Process.Start((string)((FrameworkElement)sender).Tag);
            }
        }


        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            PART_Dlcs.ItemsSource = null;
            PluginDatabase.Refresh(GameContext.Id);
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked, PART_LimitPrice.Text);
        }


        private void ToggleButtonPriceNotification_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            GameDlc data = PluginDatabase.GetOnlyCache(GameContext);
            data.PriceNotification = (bool)tb.IsChecked;
            PluginDatabase.Update(data);
        }


        #region Filter
        private void PART_TgHide_Click(object sender, RoutedEventArgs e)
        {
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked, PART_LimitPrice.Text);
        }

        private void PART_TgFree_Click(object sender, RoutedEventArgs e)
        {
            PART_TgHide.IsChecked = false;
            PART_TgHidden.IsChecked = false;
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked, PART_LimitPrice.Text);
        }

        private void PART_TgHidden_Click(object sender, RoutedEventArgs e)
        {
            PART_TgFree.IsChecked = false;
            PART_TgHide.IsChecked = false;
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked, PART_LimitPrice.Text);
        }

        private void PART_LimitPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked, PART_LimitPrice.Text);
        }


        private void Filter(bool hiddenOwned, bool onlyFree, bool showHidden, string price)
        {
            PART_Dlcs.ItemsSource = null;

            GameDlc gameDlc = PluginDatabase.Get(GameContext, true);
            if (gameDlc?.Count == 0)
            {
                return;
            }

            PART_PriceNotification.IsChecked = gameDlc.PriceNotification;
            List<Dlc> data = new List<Dlc>();

            _ = double.TryParse(price, out double PriceLimit);
            if (PriceLimit == 0)
            {
                PriceLimit = 1000000000;
            }

            data = gameDlc.Items.Where(x => (!hiddenOwned || !x.IsOwned) && (showHidden || !x.IsHidden) && x.PriceNumeric <= PriceLimit).OrderBy(x => x.Name).ToList();
            if (onlyFree)
            {
                data = data.Where(x => x.IsFree).ToList();
            }
            if (showHidden)
            {
                data = data.Where(x => x.IsHidden).ToList();
            }

            PART_Dlcs.ItemsSource = data;
            PART_TotalFoundCount.Text = data.Count.ToString();
            PART_TotalOwnedCount.Text = gameDlc.Items.Where(x => x.IsOwned).Count().ToString();
            PART_TotalHiddenCount.Text = gameDlc.Items.Where(x => x.IsHidden).Count().ToString();
            PART_DataDate.Text = new LocalDateTimeConverter().Convert(gameDlc.DateLastRefresh, null, null, CultureInfo.CurrentCulture).ToString();
        }
        #endregion


        private void Part_Ignore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string id = ((Button)sender).Tag.ToString();
                if (PluginDatabase.PluginSettings.Settings.IgnoredList.Contains(id))
                {
                    _ = PluginDatabase.PluginSettings.Settings.IgnoredList.Remove(id);
                }
                else
                {
                    PluginDatabase.PluginSettings.Settings.IgnoredList.Add(id);
                }
                Plugin.SavePluginSettings(PluginDatabase.PluginSettings.Settings);
                Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked, PART_LimitPrice.Text);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }

        private void Part_Owned_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string id = ((Button)sender).Tag.ToString();
                if (PluginDatabase.PluginSettings.Settings.ManuallyOwneds.Contains(id))
                {
                    _ = PluginDatabase.PluginSettings.Settings.ManuallyOwneds.Remove(id);
                }
                else
                {
                    PluginDatabase.PluginSettings.Settings.ManuallyOwneds.Add(id);
                }
                Plugin.SavePluginSettings(PluginDatabase.PluginSettings.Settings);
                Filter((bool)PART_TgHide.IsChecked, (bool)PART_TgFree.IsChecked, (bool)PART_TgHidden.IsChecked, PART_LimitPrice.Text);
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }
    }
}
