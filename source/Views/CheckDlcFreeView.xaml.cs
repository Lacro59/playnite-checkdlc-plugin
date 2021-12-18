using CheckDlc.Services;
using CommonPluginsShared;
using Playnite.SDK;
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
    /// Logique d'interaction pour CheckDlcFreeView.xaml
    /// </summary>
    public partial class CheckDlcFreeView : UserControl
    {
        private CheckDlcDatabase PluginDatabase = CheckDlc.PluginDatabase;


        public CheckDlcFreeView()
        {
            InitializeComponent();

            var lvDlcs = PluginDatabase.Database.Items.SelectMany(x => x.Value.Items.Where(y => y.IsFree && !y.IsOwned)
                .Select(z => new lvDlc
                {
                    Icon = x.Value.Icon,
                    Id = x.Key,
                    Name = x.Value.Name,
                    NameDlc = z.Name,
                    Link = z.Link
                })).ToList();

            PART_ListviewDlc.ItemsSource = lvDlcs;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!((string)((FrameworkElement)sender).Tag).IsNullOrEmpty())
            {
                Process.Start((string)((FrameworkElement)sender).Tag);
            }
        }
    }


    public class lvDlc
    {
        private CheckDlcDatabase PluginDatabase = CheckDlc.PluginDatabase;


        public Guid Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string NameDlc { get; set; }
        public string Link { get; set; }

        public string SourceName
        {
            get
            {
                return PlayniteTools.GetSourceName(Id);
            }
        }

        public string SourceIcon
        {
            get
            {
                return TransformIcon.Get(PlayniteTools.GetSourceName(Id));
            }
        }

        public RelayCommand<Guid> GoToGame
        {
            get
            {
                return PluginDatabase.GoToGame;
            }
        }

        public bool GameExist
        {
            get
            {
                return PluginDatabase.PlayniteApi.Database.Games.Get(Id) != null;
            }
        }
    }
}
