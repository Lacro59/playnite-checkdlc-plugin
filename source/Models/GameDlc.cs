using CommonPluginsShared.Collections;
using System.Collections.Generic;

namespace CheckDlc.Models
{
    public class GameDlc : PluginDataBaseGame<Dlc>
    {
        private List<Dlc> items = new List<Dlc>();
        public override List<Dlc> Items { get => items; set => SetValue(ref items, value); }

        public bool PriceNotification { get; set; }
        public bool HasAllDlc => Items?.FindAll(x => !x.IsOwned)?.Count == 0;
    }
}
