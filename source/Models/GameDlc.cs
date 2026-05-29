using CommonPluginsShared.Collections;
using System.Linq;

namespace CheckDlc.Models
{
    public class GameDlc : PluginGameCollection<Dlc>
    {
        public bool PriceNotification { get; set; }
        public bool HasAllDlc => Items?.Where(x => !x.IsOwned)?.Count() == 0;

        public bool IsManual { get; set; }
        public uint AppId { get; set; }
    }
}
