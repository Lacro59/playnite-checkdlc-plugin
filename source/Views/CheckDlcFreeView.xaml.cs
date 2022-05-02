using CheckDlc.Services;
using CommonPluginsShared;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

            List<lvDlc> lvDlcs = PluginDatabase.Database.Items
                .SelectMany(x => x.Value.Items.Where(y => y.IsFree && !y.IsOwned)
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


        private void Button_Click_Refresh(object sender, RoutedEventArgs e)
        {
            List<lvDlc> data = (List<lvDlc>)PART_ListviewDlc.ItemsSource;
            if (data.Count > 0)
            {
                List<Guid> dataId = data.Select(x => x.Id).Distinct().ToList();
                PluginDatabase.Refresh(dataId);
            }

            PART_ListviewDlc.ItemsSource = null;
            List<lvDlc> lvDlcs = PluginDatabase.Database.Items
                .SelectMany(x => x.Value.Items.Where(y => y.IsFree && !y.IsOwned)
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
    }


    public class lvDlc
    {
        private CheckDlcDatabase PluginDatabase = CheckDlc.PluginDatabase;


        public Guid Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string NameDlc { get; set; }
        public string Link { get; set; }

        public string SourceName => PlayniteTools.GetSourceName(Id);

        public string SourceIcon => TransformIcon.Get(PlayniteTools.GetSourceName(Id));

        public RelayCommand<Guid> GoToGame => PluginDatabase.GoToGame;

        public bool GameExist => PluginDatabase.PlayniteApi.Database.Games.Get(Id) != null;
    }
}
