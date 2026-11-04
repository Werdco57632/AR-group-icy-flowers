using System;
using IcyFlowers.Shop;
using NUnit.Framework;
using UnityEngine;

public class ShopWalletTests
{
    private string saveKey;

    [SetUp]
    public void SetUp() { saveKey = "icyflowers.shop.tests." + Guid.NewGuid().ToString("N"); }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey(saveKey);
        PlayerPrefs.DeleteKey(saveKey + ".other");
        PlayerPrefs.Save();
    }

    [Test]
    public void StartingAllowanceIsGrantedOnlyOnce()
    {
        var first = new ShopWallet(saveKey, 40);
        Assert.That(first.Balance, Is.EqualTo(40));
        Assert.That(first.TryPurchase(HandGesture.Point), Is.EqualTo(PurchaseResult.Purchased));
        Assert.That(new ShopWallet(saveKey, 40).Balance, Is.EqualTo(25));
    }

    [Test]
    public void PurchaseDeductsExactPriceAndPersistsOwnership()
    {
        var wallet = new ShopWallet(saveKey, 20);
        Assert.That(wallet.TryPurchase(HandGesture.Peace), Is.EqualTo(PurchaseResult.Purchased));
        var reloaded = new ShopWallet(saveKey);
        Assert.That(reloaded.Balance, Is.Zero);
        Assert.That(reloaded.Owns(HandGesture.Peace), Is.True);
        Assert.That(reloaded.Owns(HandGesture.Point), Is.False);
    }

    [Test]
    public void DuplicatePurchaseNeverChargesAgain()
    {
        var wallet = new ShopWallet(saveKey, 40);
        wallet.TryPurchase(HandGesture.Point);
        Assert.That(wallet.TryPurchase(HandGesture.Point), Is.EqualTo(PurchaseResult.AlreadyOwned));
        Assert.That(wallet.Balance, Is.EqualTo(25));
    }

    [Test]
    public void UnaffordableAndUnknownGesturesDoNotChangeWallet()
    {
        var wallet = new ShopWallet(saveKey, 14);
        Assert.That(wallet.TryPurchase(HandGesture.Point), Is.EqualTo(PurchaseResult.NotEnoughCoins));
        Assert.That(wallet.TryPurchase(HandGesture.None), Is.EqualTo(PurchaseResult.NotForSale));
        Assert.That(wallet.TryPurchase((HandGesture)999), Is.EqualTo(PurchaseResult.NotForSale));
        Assert.That(wallet.Owns(HandGesture.None), Is.False);
        Assert.That(wallet.Balance, Is.EqualTo(14));
    }

    [Test]
    public void SeparateKeysKeepDemoAndGameplayBalancesIndependent()
    {
        var game = new ShopWallet(saveKey);
        var demo = new ShopWallet(saveKey + ".other", 40);
        demo.TryPurchase(HandGesture.Point);
        Assert.That(game.Balance, Is.Zero);
        Assert.That(game.Owns(HandGesture.Point), Is.False);
        Assert.That(new ShopWallet(saveKey).Balance, Is.Zero);
    }

    [Test]
    public void PickupAndLevelRewardsStayExactlyOnceAfterReload()
    {
        var wallet = new ShopWallet(saveKey);
        Assert.That(wallet.TryCollectCoin("level1/pickup1", 5), Is.True);
        Assert.That(wallet.AwardLevel("level1"), Is.True);
        var reloaded = new ShopWallet(saveKey);
        Assert.That(reloaded.TryCollectCoin("level1/pickup1", 5), Is.False);
        Assert.That(reloaded.AwardLevel("level1"), Is.False);
        Assert.That(reloaded.HasCollectedCoin("level1/pickup1"), Is.True);
        Assert.That(reloaded.HasLevelReward("level1"), Is.True);
        Assert.That(reloaded.HasCollectedCoin("level1/pickup2"), Is.False);
        Assert.That(reloaded.HasLevelReward("level2"), Is.False);
        Assert.That(reloaded.Balance, Is.EqualTo(15));
    }

    [Test]
    public void InvalidRewardsAndOverflowNeverCreateCurrencyOrConsumeIds()
    {
        var wallet = new ShopWallet(saveKey, int.MaxValue);
        Assert.That(wallet.AddCoins(1), Is.False);
        Assert.That(wallet.AddCoins(-1), Is.False);
        Assert.That(wallet.TryCollectCoin("coin", 1), Is.False);
        wallet.TryPurchase(HandGesture.Point);
        Assert.That(wallet.TryCollectCoin("coin", 1), Is.True);
        Assert.That(wallet.AwardLevel(" "), Is.False);
        Assert.That(wallet.TryCollectCoin(null, 1), Is.False);
    }

    [TestCase("broken json")]
    [TestCase("{\"version\":42,\"balance\":500}")]
    [TestCase("{\"version\":1,\"balance\":-500,\"owned\":null,\"collected\":null,\"rewarded\":null}")]
    public void DamagedSavesLoadSafelyWithoutRegrantingStartingAllowance(string json)
    {
        PlayerPrefs.SetString(saveKey, json);
        var wallet = new ShopWallet(saveKey, 40);
        Assert.That(wallet.Balance, Is.Zero);
        Assert.That(wallet.Owns(HandGesture.None), Is.False);
        Assert.That(wallet.AddCoins(1), Is.True);
        Assert.That(new ShopWallet(saveKey).Balance, Is.EqualTo(1));
    }

    [Test]
    public void ChangeEventFiresOnlyForSuccessfulMutations()
    {
        var wallet = new ShopWallet(saveKey);
        int changes = 0;
        wallet.Changed += () => changes++;
        wallet.TryPurchase(HandGesture.Point);
        wallet.AddCoins(20);
        wallet.TryPurchase(HandGesture.Point);
        wallet.TryPurchase(HandGesture.Point);
        wallet.AddCoins(0);
        Assert.That(changes, Is.EqualTo(2));
    }
}
