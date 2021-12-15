using CheckDlc.Services;
using System.Windows;
using System.Windows.Controls;

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