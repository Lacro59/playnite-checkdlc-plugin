using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckDlc.Models
{
    class SteamUserData
    {
        public List<int> rgWishlist { get; set; }
        public List<int> rgOwnedPackages { get; set; }
        public List<int> rgOwnedApps { get; set; }
        public List<int> rgFollowedApps { get; set; }
    }
}
