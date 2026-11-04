using System;
using System.Collections.Generic;
using UnityEngine;

namespace IcyFlowers.Shop
{
    public enum PurchaseResult
    {
        Purchased,
        AlreadyOwned,
        NotEnoughCoins,
        NotForSale
    }

    /// <summary>A saved palette balance and gesture collection, isolated by save key.</summary>
    public sealed class ShopWallet
    {
        [Serializable]
        private sealed class Profile
        {
            public int version = 1;
            public int balance;
            public List<string> owned = new List<string>();
            public List<string> collected = new List<string>();
            public List<string> rewarded = new List<string>();
        }

        private readonly string saveKey;
        private readonly HashSet<string> owned = new HashSet<string>();
        private readonly HashSet<string> collected = new HashSet<string>();
        private readonly HashSet<string> rewarded = new HashSet<string>();

        public int Balance { get; private set; }
        public event Action Changed;

        public ShopWallet(string saveKey, int startingBalance = 0)
        {
            if (string.IsNullOrWhiteSpace(saveKey))
                throw new ArgumentException("A wallet needs a unique save key.", nameof(saveKey));
            this.saveKey = saveKey;

            if (!PlayerPrefs.HasKey(saveKey))
            {
                Balance = Math.Max(0, startingBalance);
                Save();
                return;
            }

            // Corrupt saves start empty, never re-granting the starting allowance.
            try
            {
                var profile = JsonUtility.FromJson<Profile>(PlayerPrefs.GetString(saveKey));
                if (profile == null || profile.version != 1) return;
                Balance = Math.Max(0, profile.balance);
                foreach (var item in GestureCatalog.Items)
                    if (profile.owned != null && profile.owned.Contains(item.Id)) owned.Add(item.Id);
                LoadIds(profile.collected, collected);
                LoadIds(profile.rewarded, rewarded);
            }
            catch (ArgumentException)
            {
                Balance = 0;
            }
        }

        public bool Owns(HandGesture gesture)
        {
            var item = GestureCatalog.Find(gesture);
            return item != null && owned.Contains(item.Id);
        }

        /// <summary>Check a stable pickup ID at spawn to hide a previously collected pickup.</summary>
        public bool HasCollectedCoin(string id) => ValidId(id) && collected.Contains(id);

        /// <summary>Check whether a stable level ID has already paid its completion reward.</summary>
        public bool HasLevelReward(string id) => ValidId(id) && rewarded.Contains(id);

        public PurchaseResult TryPurchase(HandGesture gesture)
        {
            var item = GestureCatalog.Find(gesture);
            if (item == null) return PurchaseResult.NotForSale;
            if (owned.Contains(item.Id)) return PurchaseResult.AlreadyOwned;
            if (Balance < item.Price) return PurchaseResult.NotEnoughCoins;

            Balance -= item.Price;
            owned.Add(item.Id);
            Commit();
            return PurchaseResult.Purchased;
        }

        public bool AddCoins(int amount)
        {
            if (!CanAdd(amount)) return false;
            Balance += amount;
            Commit();
            return true;
        }

        /// <summary>Use a stable scene/level/pickup ID to prevent repeat payouts after reloads.</summary>
        public bool TryCollectCoin(string id, int amount)
        {
            if (!ValidId(id) || collected.Contains(id) || !CanAdd(amount)) return false;
            collected.Add(id);
            Balance += amount;
            Commit();
            return true;
        }

        /// <summary>Grants a level completion reward once for each stable level ID.</summary>
        public bool AwardLevel(string id, int amount = 10)
        {
            if (!ValidId(id) || rewarded.Contains(id) || !CanAdd(amount)) return false;
            rewarded.Add(id);
            Balance += amount;
            Commit();
            return true;
        }

        private bool CanAdd(int amount) => amount > 0 && Balance <= int.MaxValue - amount;
        private static bool ValidId(string id) => !string.IsNullOrWhiteSpace(id);

        private static void LoadIds(List<string> source, HashSet<string> target)
        {
            if (source == null) return;
            foreach (var id in source)
                if (ValidId(id)) target.Add(id);
        }

        private void Commit()
        {
            Save();
            Changed?.Invoke();
        }

        private void Save()
        {
            var profile = new Profile
            {
                balance = Balance,
                owned = new List<string>(owned),
                collected = new List<string>(collected),
                rewarded = new List<string>(rewarded)
            };
            profile.owned.Sort(StringComparer.Ordinal);
            profile.collected.Sort(StringComparer.Ordinal);
            profile.rewarded.Sort(StringComparer.Ordinal);
            PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(profile));
            PlayerPrefs.Save();
        }
    }

    public static class ShopProgress
    {
        public const string SaveKey = "icyflowers.shop.progress.v1";
        private static ShopWallet wallet;
        public static ShopWallet Wallet => wallet ?? (wallet = new ShopWallet(SaveKey));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetCachedWallet() { wallet = null; }
    }
}
