using CheckDlc.Services;
using System;
using System.Collections.Generic;
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
    public partial class CheckDlcSettingsView : UserControl
    {
        private CheckDlcDatabase PluginDatabase = CheckDlc.PluginDatabase;


        public CheckDlcSettingsView()
        {
            InitializeComponent();
        }


        #region Tag
        private void ButtonAddTag_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.AddTagAllGame();
        }

        private void ButtonRemoveTag_Click(object sender, RoutedEventArgs e)
        {
            PluginDatabase.RemoveTagAllGame();
        }
        #endregion
    }
}