using CommonPlayniteShared;
using CommonPluginsShared;
using CommonPluginsShared.Extensions;
using Playnite.SDK.Data;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CheckDlc.Models
{
    public class Dlc : ObservableObject
    {
        public string DlcId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Link { get; set; }
        public bool IsOwned { get; set; }

        public string Price { get; set; }
        public string PriceBase { get; set; }

        [DontSerialize]
        public bool IsFree
        {
            get
            {
                try
                {
                    if (Price.IsNullOrEmpty())
                    {
                        return true;
                    }

                    string temp = Regex.Split(Price, @"\s+").Where(s => s != string.Empty).First();
                    temp = Regex.Replace(temp, @"[^\d.-]", "");
                    temp = temp.Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator).Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                    double dPrice = double.Parse(temp);
                    return dPrice == 0;
                }
                catch (Exception ex)
                {
                    Common.LogError(ex, false);
                    return false;
                }
            }
        }

        [DontSerialize]
        public bool IsDiscount
        {
            get
            {
                return !Price.IsEqual(PriceBase);
            }
        }

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
