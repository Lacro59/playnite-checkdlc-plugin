using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckDlc.Models
{
    public class Dlc : ObservableObject
    {
        public string GameId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }
        public bool IsOwned { get; set; }
    }
}
