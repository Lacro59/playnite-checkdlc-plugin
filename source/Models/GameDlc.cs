using CommonPluginsShared.Collections;
using System.Collections.Generic;

namespace CheckDlc.Models
{
    public class GameDlc : PluginDataBaseGame<Dlc>
    {
        private List<Dlc> _Items = new List<Dlc>();
        public override List<Dlc> Items
        {
            get
            {
                return _Items;
            }

            set
            {
                _Items = value;
                OnPropertyChanged();
            }
        }
    }
}
