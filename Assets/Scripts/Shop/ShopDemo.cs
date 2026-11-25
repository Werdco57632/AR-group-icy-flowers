using UnityEngine;
using UnityEngine.UI;

namespace IcyFlowers.Shop
{
    /// <summary>Standalone preview; its save never changes the real game's balance or unlocks.</summary>
    public sealed class ShopDemo : MonoBehaviour
    {
        public const string DemoSaveKey = "icyflowers.shop.demo.v1";
        ShopWallet wallet;
        GestureShopView view;
        GameObject menu;
        Text status;
        CanvasScaler demoScaler;
        RectTransform demoPanel;
        int level = 1;

        void Start()
        {
            var cameraObject = new GameObject("Shop preview camera", typeof(Camera));
            cameraObject.transform.SetParent(transform, false);
            var previewCamera = cameraObject.GetComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(.31f, .76f, .93f);
            previewCamera.cullingMask = 0;
            wallet = new ShopWallet(DemoSaveKey, 40);
            var ui = new GameObject("Gesture shop", typeof(RectTransform));
            ui.transform.SetParent(transform, false);
            view = ui.AddComponent<GestureShopView>();
            BuildMenu();
            Open();
        }

        void Open()
        {
            menu.SetActive(false);
            view.Show(wallet, Continued, "LEVEL " + level + " COMPLETE  /  TAKE A BREATHER");
        }

        void Update()
        {
            if (demoScaler == null) return;
            bool portrait = Screen.width < Screen.height;
            demoScaler.referenceResolution = portrait ? new Vector2(720, 1280) : new Vector2(1280, 800);
            demoPanel.sizeDelta = portrait ? new Vector2(640, 900) : new Vector2(760, 650);
        }

        void Continued()
        {
            menu.SetActive(true);
            status.text = "Ready for level " + (level + 1) + "\n\n" + wallet.Balance + " palettes left in your pocket.\nYour purchased gestures are saved.\n\nThis is the shop preview. Your game's next level\nwill connect to the Continue event here.";
        }

        void BuildMenu()
        {
            menu = new GameObject("Demo controls", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            menu.transform.SetParent(transform, false);
            var canvas = menu.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = menu.GetComponent<CanvasScaler>();
            demoScaler = scaler;
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            menu.AddComponent<Image>().color = new Color(.31f, .76f, .93f);
            view.Scenery(menu.transform);
            var panel = view.Panel("Demo panel", menu.transform, new Color(1f, .985f, .89f));
            demoPanel = panel;
            panel.anchorMin = panel.anchorMax = new Vector2(.5f, .5f);
            panel.sizeDelta = new Vector2(760, 650);
            var title = view.Label("Demo title", panel, "Off we grow!", 34, new Color(.09f,.23f,.17f), FontStyle.Bold);
            title.alignment = TextAnchor.MiddleCenter;
            GestureShopView.Anchor(title.rectTransform, 0, .83f, 1, 1, 0, 0, 0, 0);
            status = view.Label("Status", panel, "", 22, new Color(.16f,.31f,.20f));
            status.alignment = TextAnchor.MiddleCenter;
            GestureShopView.Anchor(status.rectTransform, 0, .35f, 1, .83f, 0, 0, 0, 0);
            var finish = view.Button("Finish next demo level", panel, "Finish demo level  (+10 palettes)", new Color(.13f,.44f,.22f), Color.white, () =>
            {
                level++;
                wallet.AddCoins(10);
                Open();
            });
            GestureShopView.Anchor((RectTransform)finish.transform, .12f, .22f, .88f, .32f, 0, 0, 0, 0);
            var back = view.Button("Reopen shop", panel, "Reopen shop", new Color(.13f,.44f,.22f), Color.white, Open);
            GestureShopView.Anchor((RectTransform)back.transform, .12f, .10f, .88f, .20f, 0, 0, 0, 0);
            var reset = view.Button("Reset demo", panel, "Reset demo wallet & unlocks", new Color(.75f,.86f,.57f), new Color(.09f,.23f,.17f), () =>
            {
                PlayerPrefs.DeleteKey(DemoSaveKey);
                PlayerPrefs.Save();
                wallet = new ShopWallet(DemoSaveKey, 40);
                level = 1;
                Open();
            });
            GestureShopView.Anchor((RectTransform)reset.transform, .12f, -.02f, .88f, .08f, 0, 0, 0, 0);
        }
    }
}
