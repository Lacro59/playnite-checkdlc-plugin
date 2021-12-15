using System.Collections.Generic;

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
