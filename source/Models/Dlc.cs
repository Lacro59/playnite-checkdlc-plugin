using CommonPlayniteShared;
using Playnite.SDK.Data;
using System.Collections.Generic;

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

        [DontSerialize]
        public string ImagePath
        {
            get
            {
                return ImageSourceManager.GetImagePath(Image);
            }
        }
    }
}
