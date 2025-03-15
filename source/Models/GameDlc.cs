using CommonPluginsShared.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CheckDlc.Models
{
    public class GameDlc : PluginDataBaseGame<Dlc>
    {
        private List<Dlc> items = new List<Dlc>();
        public override List<Dlc> Items { get => items; set => SetValue(ref items, value); }

        public bool PriceNotification { get; set; }
        public bool HasAllDlc => Items?.Where(x => !x.IsOwned)?.Count() == 0;
    }
}
