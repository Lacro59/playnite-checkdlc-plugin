using CheckDlc.Models;
using CheckDlc.Services;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CheckDlc.Views
{
    /// <summary>
    /// Logique d'interaction pour CheclDlcGameView.xaml
    /// </summary>
    public partial class CheclDlcGameView : UserControl
    {
        private CheckDlcDatabase PluginDatabase = CheckDlc.PluginDatabase;


        public CheclDlcGameView(Game GameContext)
        {
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
    }
}
