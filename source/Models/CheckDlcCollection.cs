using Playnite.SDK;
using CommonPluginsShared.Collections;

namespace CheckDlc.Models
{
    public class CheckDlcCollection : PluginItemCollection<GameDlc>
    {
        public CheckDlcCollection(string path, GameDatabaseCollection type = GameDatabaseCollection.Uknown) : base(path, type)
        {
        }
    }
}
