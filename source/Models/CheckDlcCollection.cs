using Playnite.SDK;
using Playnite.SDK.Models;
using CommonPluginsShared.Collections;
using CommonPlayniteShared.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonPluginsShared;

namespace CheckDlc.Models
{
    public class CheckDlcCollection : PluginItemCollection<GameDlc>
    {
        public CheckDlcCollection(string path, GameDatabaseCollection type = GameDatabaseCollection.Uknown) : base(path, type)
        {
        }
    }
}
