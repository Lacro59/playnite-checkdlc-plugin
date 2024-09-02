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
        private CheckDlc Plugin { get; }
        private CheckDlcDatabase PluginDatabase => CheckDlc.PluginDatabase;


        public CheckDlcFreeView(CheckDlc plugin)
        {
            Plugin = plugin;

            InitializeComponent();

            List<LvDlc> lvDlcs = PluginDatabase.Database.Items
                .SelectMany(x => x.Value.Items.Where(y => y.IsFree && !y.IsOwned)
                .Select(z => new LvDlc
                {
                    Icon = x.Value.Icon,
                    Id = x.Key,
                    Name = x.Value.Name,
                    NameDlc = z.Name,
                    NameHide = x.Value.Name + "##" + z.Name,
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
            List<LvDlc> data = (List<LvDlc>)PART_ListviewDlc.ItemsSource;
            if (data.Count > 0)
            {
                List<Guid> dataId = data.Select(x => x.Id).Distinct().ToList();
                PluginDatabase.Refresh(dataId);
            }

            PART_ListviewDlc.ItemsSource = null;
            List<LvDlc> lvDlcs = PluginDatabase.Database.Items
                .SelectMany(x => x.Value.Items.Where(y => y.IsFree && !y.IsOwned)
                .Select(z => new LvDlc
                {
                    Icon = x.Value.Icon,
                    Id = x.Key,
                    Name = x.Value.Name,
                    NameDlc = z.Name,
                    NameHide = x.Value.Name + "##" + z.Name,
                    Link = z.Link
                })).ToList();

            PART_ListviewDlc.ItemsSource = lvDlcs;
        }

        private void Part_Ignore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string name = ((Button)sender).Tag.ToString();
                Guid Id = Guid.Parse(UI.FindParent<Grid>(((Button)sender)).Tag.ToString());
                PluginDatabase.PluginSettings.Settings.IgnoredList.Add(name);
                Plugin.SavePluginSettings(PluginDatabase.PluginSettings.Settings);
                PluginDatabase.Refresh(Id);

                PART_ListviewDlc.ItemsSource = null;
                List<LvDlc> lvDlcs = PluginDatabase.Database.Items
                    .SelectMany(x => x.Value.Items.Where(y => y.IsFree && !y.IsOwned)
                    .Select(z => new LvDlc
                    {
                        Icon = x.Value.Icon,
                        Id = x.Key,
                        Name = x.Value.Name,
                        NameDlc = z.Name,
                        NameHide = x.Value.Name + "##" + z.Name,
                        Link = z.Link
                    })).ToList();

                PART_ListviewDlc.ItemsSource = lvDlcs;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }

        private void Part_Refresh_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Guid Id = Guid.Parse(((Button)sender).Tag.ToString());
                PluginDatabase.Refresh(Id);

                PART_ListviewDlc.ItemsSource = null;
                List<LvDlc> lvDlcs = PluginDatabase.Database.Items
                    .SelectMany(x => x.Value.Items.Where(y => y.IsFree && !y.IsOwned)
                    .Select(z => new LvDlc
                    {
                        Icon = x.Value.Icon,
                        Id = x.Key,
                        Name = x.Value.Name,
                        NameDlc = z.Name,
                        NameHide = x.Value.Name + "##" + z.Name,
                        Link = z.Link
                    })).ToList();

                PART_ListviewDlc.ItemsSource = lvDlcs;
            }
            catch (Exception ex)
            {
                Common.LogError(ex, false);
            }
        }
    }


    public class LvDlc
    {
        public Guid Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string NameDlc { get; set; }
        public string NameHide { get; set; }
        public string Link { get; set; }

        public string SourceName => PlayniteTools.GetSourceName(Id);

        public string SourceIcon => TransformIcon.Get(PlayniteTools.GetSourceName(Id));

        public bool GameExist => API.Instance.Database.Games.Get(Id) != null;
    }
}
