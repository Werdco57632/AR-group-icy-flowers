# Cat's Curios — independent shop module

This shop can be finished and reviewed before the updated game is pushed. No existing game scripts, scenes, build settings, or packages were changed. The gesture effects are deliberately undecided.

## Try it now

1. Open this project in Unity 6 (the project specifies **6000.3.22f1**).
2. Open `Assets/Scenes/ShopDemo.unity` and press **Play**.
3. Start with **40 palettes**. Buy Pointing (15), Peace sign (20), or Open palm (25). Owned items and unaffordable items cannot be purchased again.
4. Click **Continue adventure**. The preview shows the point where the real next level will load. **Finish demo level** adds 10 test palettes and reopens the shop.
5. Stop and play again to check persistence. **Reset demo wallet & unlocks** resets only the demo's separate save.

The cat, palette, and gesture art are the supplied PNGs, copied without modification. The UI uses bright sky blue, smiling green hills, grass, flowers, outlined paper signs, and a striped cat stand to match the game. Decorations are lightweight native UI vectors. The bundled Patrick Hand font is licensed under the SIL Open Font License; its license is included under Resources/GestureShop/Fonts. UI supports mouse/touch, landscape, portrait scrolling, and safe-area insets. The demo has no camera or hand-tracking dependency.

## What can be done while waiting

- Play through the purchase flow and choose gesture prices. Prices live in `Assets/Scripts/Shop/Progress/GestureCatalog.cs`.
- Review the cat, art placement, text, and palette currency with the team.
- Decide gesture effects and whether gestures are all usable at once or need equipping. The current shop permanently unlocks poses and has no equipment limit.
- Share the new module files with the teammate. Existing gameplay files can stay on her branch until she pushes them.

## Connect it when the game arrives

Add **ShopIntermission** to the level manager. In the Inspector:

- Wire **On Opened** to disable gameplay input/gesture actions.
- Wire **On Closed** to restore input. Physics pauses while shopping by default and the previous time scale is restored, including if the component is disabled. Time scale alone does not stop `Update` input handlers.
- Wire **On Continue** to the existing next-level loader.
- Call `OpenShop()` from the existing level-completion code, after awarding that level's palettes. Opening the shop does not automatically award currency.

Use one wallet as the source of truth. The module exposes `ShopProgress.Wallet` for gameplay and a separate `ShopWallet` for the demo. If the new game already has a wallet, agree on migration/an adapter before connecting its coin script; do not run two independent balances.

```csharp
using IcyFlowers.Shop;

// When a palette is collected (give each pickup a stable, unique ID):
bool collected = ShopProgress.Wallet.TryCollectCoin("level-01/palette-03", 1);
// Hide on success. On spawn, use HasCollectedCoin(id) to hide saved pickups.

// Optional one-time level bonus, then intermission:
ShopProgress.Wallet.AwardLevel("level-01", 10);
shopIntermission.OpenShop();

// Gate the eventual gesture action in the teammate's recognition/controller code:
if (ShopProgress.Wallet.Owns(HandGesture.Peace))
{
    // Invoke the agreed Peace-sign effect here.
}
```

`ShopIntermission.AddCoins(int)` is also available for UnityEvent-based pickups. It intentionally adds each time it is called; use `TryCollectCoin` if the game needs persistent pickup IDs. `Changed` fires after a successful wallet mutation, so a HUD can refresh its balance. Use `HasCollectedCoin(id)` and `HasLevelReward(id)` for saved reward history. `CloseShop()` dismisses the shop without advancing the level.

The scene is a preview of the intermission only. It does not implement real levels, a coin pickup system, hand recognition, or gesture abilities. Those stay with the incoming gameplay code.

## Files to bring over

- `Assets/Scripts/Shop/` and `Assets/Scripts/Shop.meta` — UI, integration component, demo, wallet/catalog.
- `Assets/Resources/GestureShop/` and its `.meta` — original supplied art with sprite import settings.
- `Assets/Scenes/ShopDemo.unity` and its `.meta` — standalone preview, not added to Build Settings.
- `Assets/Tests/Editor/ShopWalletTests.cs`, test assembly definition, and accompanying `.meta` files — wallet tests.
- This guide.

Preserve the `.meta` files when copying so Unity keeps scene and script references. Use a branch created from the team's real repository when it is available; this downloaded folder has no `.git` history.

## Save behavior

Progress uses local `PlayerPrefs`, saved after successful mutations. Purchases persist across level loads and app restarts. This is a local game save, not an account/cloud wallet. The gameplay and demo use distinct versioned keys. Stable pickup/level IDs prevent awarding the same reward twice; retries/respawns should reuse the same IDs.

## Validation in this workspace

All six runtime C# sources compile without warnings/errors against the installed Unity **6000.3.10f1** engine and UGUI assemblies. Scene-to-script GUIDs and new asset metadata were checked. The wallet has **11 EditMode test cases**; run them from **Window → General → Test Runner → EditMode** in Unity.

The installed editor's licensing client could not initialize for the isolated validation project. Play-mode behavior, visual layout, and the EditMode suite therefore still need to be checked in a working Unity editor. The original project's requested editor version, dependencies, settings, and gameplay files were left intact.
