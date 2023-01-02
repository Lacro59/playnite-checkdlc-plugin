using CommonPluginsShared.Collections;
using System.Collections.Generic;

namespace CheckDlc.Models
{
    public class GameDlc : PluginDataBaseGame<Dlc>
    {
        private List<Dlc> _Items = new List<Dlc>();
        public override List<Dlc> Items { get => _Items; set => SetValue(ref _Items, value); }

        public bool PriceNotification { get; set; }
    }
}
