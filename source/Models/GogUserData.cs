using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckDlc.Models
{
    public class GogUserData
    {
        public string country { get; set; }
        public List<GogCurrency> currencies { get; set; }
        public SelectedCurrency selectedCurrency { get; set; }
        public PreferredLanguage preferredLanguage { get; set; }
        public string ratingBrand { get; set; }
        public bool isLoggedIn { get; set; }
        public Checksum checksum { get; set; }
        public Updates updates { get; set; }
        public string userId { get; set; }
        public string username { get; set; }
        public string galaxyUserId { get; set; }
        public string email { get; set; }
        public string avatar { get; set; }
        public WalletBalance walletBalance { get; set; }
        public PurchasedItems purchasedItems { get; set; }
        public int wishlistedItems { get; set; }
        public List<Friend> friends { get; set; }
        public List<object> personalizedProductPrices { get; set; }
        public List<object> personalizedSeriesPrices { get; set; }
    }

    public class GogCurrency
    {
        public string code { get; set; }
        public string symbol { get; set; }
    }

    public class SelectedCurrency
    {
        public string code { get; set; }
        public string symbol { get; set; }
    }

    public class PreferredLanguage
    {
        public string code { get; set; }
        public string name { get; set; }
    }

    public class Checksum
    {
        public object cart { get; set; }
        public string games { get; set; }
        public string wishlist { get; set; }
        public object reviews_votes { get; set; }
        public object games_rating { get; set; }
    }

    public class Updates
    {
        public int messages { get; set; }
        public int pendingFriendRequests { get; set; }
        public int unreadChatMessages { get; set; }
        public int products { get; set; }
        public int total { get; set; }
    }

    public class WalletBalance
    {
        public string currency { get; set; }
        public int amount { get; set; }
    }

    public class PurchasedItems
    {
        public int games { get; set; }
        public int movies { get; set; }
    }

    public class Friend
    {
        public string username { get; set; }
        public int userSince { get; set; }
        public string galaxyId { get; set; }
        public string avatar { get; set; }
    }
}
