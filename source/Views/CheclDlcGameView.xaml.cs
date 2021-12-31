using CheckDlc.Models;
using CheckDlc.Services;
using Playnite.SDK.Models;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace CheckDlc.Views
{
    /// <summary>
    /// Logique d'interaction pour CheclDlcGameView.xaml
    /// </summary>
    public partial class CheclDlcGameView : UserControl
    {
        private CheckDlcDatabase PluginDatabase = CheckDlc.PluginDatabase;

        private Game GameContext;


        public CheclDlcGameView(Game GameContext)
        {
            this.GameContext = GameContext;

            InitializeComponent();

            GameDlc gameDlc = PluginDatabase.Get(GameContext, true);
            gameDlc.Items.Sort((x, y) => (y.Name).CompareTo(x.Name));
            PART_Dlcs.ItemsSource = gameDlc.Items;
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
            gameDlc.Items.Sort((x, y) => (y.Name).CompareTo(x.Name));
            PART_Dlcs.ItemsSource = gameDlc.Items;
        }
    }
}
