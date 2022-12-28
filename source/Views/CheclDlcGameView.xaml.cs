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

namespace CheckDlc.Views
{
    /// <summary>
    /// Logique d'interaction pour CheclDlcGameView.xaml
    /// </summary>
    public partial class CheclDlcGameView : UserControl
    {
        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;

        private Game GameContext { get; set; }


        public CheclDlcGameView(Game GameContext)
        {           
            InitializeComponent();

            this.GameContext = GameContext;
            Filter((bool)PART_TgHide.IsChecked, PART_LimitPrice.Text);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!((string)((FrameworkElement)sender).Tag).IsNullOrEmpty())
            {
                Process.Start((string)((FrameworkElement)sender).Tag);
            }
        }


        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            PART_Dlcs.ItemsSource = null;
            PluginDatabase.Refresh(GameContext.Id);

            GameDlc gameDlc = PluginDatabase.Get(GameContext, true);
            gameDlc.Items.Sort((x, y) => y.Name.CompareTo(x.Name));
            PART_Dlcs.ItemsSource = gameDlc.Items;
            PART_TotalFoundCount.Text = gameDlc.Items.Count.ToString();
            PART_TotalOwnedCount.Text = gameDlc.Items.Where(x => x.IsOwned).Count().ToString();
        }


        private void ToggleButtonPriceNotification_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton tb = sender as ToggleButton;
            GameDlc data = PluginDatabase.GetOnlyCache(GameContext);
            data.PriceNotification = (bool)tb.IsChecked;
            PluginDatabase.Update(data);
        }


        #region Filter
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            Filter((bool)PART_TgHide.IsChecked, PART_LimitPrice.Text);
        }

        private void PART_LimitPrice_TextChanged(object sender, TextChangedEventArgs e)
        {
            Filter((bool)PART_TgHide.IsChecked, PART_LimitPrice.Text);
        }


        private void Filter(bool HideOwned, string Price)
        {
            PART_Dlcs.ItemsSource = null;

            GameDlc gameDlc = PluginDatabase.Get(GameContext, true);
            PART_PriceNotification.IsChecked = gameDlc.PriceNotification;
            List<Dlc> data = new List<Dlc>();

            double.TryParse(Price, out double PriceLimit);
            if (PriceLimit == 0)
            {
                PriceLimit = 1000000000;
            }

            if (HideOwned)
            {
                data = gameDlc.Items.Where(x => !x.IsOwned && x.PriceNumeric <= PriceLimit).OrderBy(x => x.Name).ToList();
            }
            else
            {
                data = gameDlc.Items.Where(x => x.PriceNumeric <= PriceLimit).OrderBy(x => x.Name).ToList();
            }

            PART_Dlcs.ItemsSource = data;
            PART_TotalFoundCount.Text = data.Count.ToString();
            PART_TotalOwnedCount.Text = data.Where(x => x.IsOwned).Count().ToString();
        }
        #endregion
    }
}
