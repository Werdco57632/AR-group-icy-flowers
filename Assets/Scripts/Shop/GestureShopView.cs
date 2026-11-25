using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IcyFlowers.Shop
{
    /// <summary>Self-contained shop UI. The caller owns the wallet and level transition.</summary>
    public sealed class GestureShopView : MonoBehaviour
    {
        static readonly Color Ink = Hex("183B2B");
        static readonly Color Accent = Hex("216F39");
        static readonly Color Sky = Hex("50C2ED");
        static readonly Color Muted = Hex("385D45");
        readonly List<Card> cards = new List<Card>();
        ShopWallet wallet;
        Action onContinue;
        RectTransform safeRoot;
        Text balanceText, messageText, stageText;
        Button continueButton;
        Font font;
        EventSystem ownedEventSystem;
        bool built, portrait;
        Vector2 lastSize;
        string stage;

        sealed class Card
        {
            public HandGesture Gesture;
            public int Price;
            public Button Button;
            public Text Label, State;
            public Graphic Background;
        }

        public bool IsOpen { get; private set; }
        public event Action Closed;

        public void Show(ShopWallet progress, Action continueAction, string stageLabel = "BETWEEN ADVENTURES")
        {
            if (progress == null) throw new ArgumentNullException(nameof(progress));
            Unsubscribe();
            wallet = progress;
            onContinue = continueAction;
            stage = stageLabel;
            wallet.Changed += Refresh;
            IsOpen = true;
            gameObject.SetActive(true);
            Build();
            Refresh();
        }

        public void Hide()
        {
            CloseState();
            gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            Unsubscribe();
            if (ownedEventSystem != null) Destroy(ownedEventSystem.gameObject);
        }
        void OnDisable() { CloseState(); }
        void CloseState()
        {
            bool wasOpen = IsOpen;
            IsOpen = false;
            Unsubscribe();
            onContinue = null;
            if (wasOpen) Closed?.Invoke();
        }
        void Unsubscribe() { if (wallet != null) wallet.Changed -= Refresh; }

        void Update()
        {
            if (!IsOpen) return;
            var size = new Vector2(Screen.width, Screen.height);
            bool nowPortrait = Screen.width < Screen.height;
            if (size != lastSize)
            {
                if (portrait != nowPortrait) { Build(); Refresh(); }
                ApplySafeArea();
                lastSize = size;
            }
        }

        void Build()
        {
            if (font == null) font = Resources.Load<Font>("GestureShop/Fonts/PatrickHand-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (!built)
            {
                var canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 200;
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1280, 800);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                gameObject.AddComponent<GraphicRaycaster>();
                if (FindFirstObjectByType<EventSystem>() == null)
                    ownedEventSystem = new GameObject("Shop EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)).GetComponent<EventSystem>();
                built = true;
            }
            // Keep the canvas and EventSystem; replace only the presentation when rotating.
            if (safeRoot != null) { safeRoot.gameObject.SetActive(false); Destroy(safeRoot.gameObject); }
            cards.Clear();
            portrait = Screen.width < Screen.height;
            GetComponent<CanvasScaler>().referenceResolution = portrait ? new Vector2(720, 1280) : new Vector2(1280, 800);
            var background = GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            background.color = Sky;
            background.raycastTarget = true;
            safeRoot = Rect("Safe area", transform);
            ApplySafeArea();
            Scenery(safeRoot);
            var content = Rect("Shop content", safeRoot);
            Stretch(content, new Vector2(40, 26), new Vector2(-40, -26));

            var header = Rect("Header", content);
            Anchor(header, 0, 1, 1, 1, 0, -106, 0, 0);
            stageText = Label("Stage", header, stage, 18, Ink, FontStyle.Bold);
            Anchor(stageText.rectTransform, 0, 1, 0.65f, 1, 0, -24, 0, 0);
            var title = Label("Title", header, "The Cat's Curios", portrait ? 36 : 46, Ink, FontStyle.Bold);
            Anchor(title.rectTransform, 0, 0, .72f, 1, 0, 0, 0, -28);
            var purse = Panel("Wallet", header, Hex("FFF5BE"));
            Anchor(purse, .76f, .2f, 1, .92f, 0, 0, 0, 0);
            Art("Palette currency", purse, "Palette", .02f, .05f, .34f, .95f);
            balanceText = Label("Balance", purse, "0", 29, Ink, FontStyle.Bold);
            Anchor(balanceText.rectTransform, .36f, .4f, 1, 1, 0, 0, -8, -4);
            var currency = Label("Currency name", purse, "PALETTES", 16, Accent, FontStyle.Bold);
            Anchor(currency.rectTransform, .36f, .04f, 1, .43f, 0, 0, -8, 0);

            var body = Rect("Shelves", content);
            Anchor(body, 0, 0, 1, 1, 0, portrait ? 246 : 144, 0, -126);
            if (portrait)
            {
                var seller = BuildSeller(body);
                Anchor(seller, 0, .68f, 1, 1, 0, 16, 0, 0);
                BuildInventory(body, true);
            }
            else
            {
                var seller = BuildSeller(body);
                Anchor(seller, 0, 0, .285f, 1, 0, 0, -22, 0);
                BuildInventory(body, false);
            }

            var footer = Rect("Footer", content);
            Anchor(footer, 0, 0, 1, 0, 0, 0, 0, portrait ? 220 : 120);
            var roadNote = Panel("A note for the road", footer, Hex("FFFCE5"));
            Anchor(roadNote, 0, portrait ? .38f : 0, portrait ? 1 : .64f, 1, -8, -2, 8, 3);
            // Both labels follow their paper, with room for its irregular outline and font descenders.
            messageText = Label("Shop message", roadNote, "", 24, Accent, FontStyle.Bold);
            Anchor(messageText.rectTransform, 0, .32f, 1, 1, 24, 0, -24, -12);
            var note = Label("Saved note", roadNote, "Find palettes. Learn gestures. Keep growing.", 18, Muted);
            Anchor(note.rectTransform, 0, 0, 1, .32f, 24, 10, -24, 0);
            continueButton = Button("Continue", footer, "On to the next level  >", Accent, Color.white, Continue);
            Anchor((RectTransform)continueButton.transform, portrait ? .15f : .68f, 0, 1, 0, 0, 8, 0, 96);
            lastSize = new Vector2(Screen.width, Screen.height);
        }

        RectTransform BuildSeller(Transform parent)
        {
            var seller = Rect("Cat's little stand", parent);
            var awning = Decoration(seller, "Striped canopy", ShopSketchGraphic.Sketch.Awning, Hex("9E61C9"), .01f, .84f, .99f, 1);
            var small = Label("Shopkeeper label", seller, "CAT'S CORNER", 25, Ink, FontStyle.Bold);
            small.alignment = TextAnchor.MiddleCenter;
            Anchor(small.rectTransform, .05f, .73f, .95f, .85f, 0, 0, 0, 0);
            var cat = Art("Shopkeeper", seller, "Shopkeeper", -.04f, .14f, 1.04f, .83f);
            var bubble = Panel("A note from cat", seller, Hex("FFF9DA"));
            Anchor(bubble, .03f, .01f, .97f, .18f, 0, 0, 0, 0);
            var speech = Label("Cat dialogue", bubble, "Got palettes?\nI've got new moves!", 23, Ink);
            speech.alignment = TextAnchor.MiddleCenter;
            Stretch(speech.rectTransform, new Vector2(8, 4), new Vector2(-8, -4));
            if (portrait)
            {
                Anchor(awning, 0, .76f, .4f, 1, 0, 0, 0, 0);
                Anchor(small.rectTransform, .4f, .64f, .98f, .98f, 0, 0, 0, 0);
                Anchor(bubble, .40f, .05f, .98f, .65f, 0, 0, 0, 0);
                Anchor(cat, -.02f, 0, .42f, .92f, 0, 0, 0, 0);
            }
            return seller;
        }

        internal void Scenery(Transform parent)
        {
            var scene = Rect("A little patch of the game", parent);
            Stretch(scene, Vector2.zero, Vector2.zero);
            Decoration(scene, "Cloud left", ShopSketchGraphic.Sketch.Cloud, Hex("F8FFF1"), .30f, .82f, .48f, .97f);
            Decoration(scene, "Cloud right", ShopSketchGraphic.Sketch.Cloud, Hex("F8FFF1"), .80f, .74f, .99f, .89f);
            Decoration(scene, "Smiling hill left", ShopSketchGraphic.Sketch.Hill, Hex("1C9F42"), -.07f, .08f, .31f, .54f);
            Decoration(scene, "Smiling hill right", ShopSketchGraphic.Sketch.Hill, Hex("24B34E"), .72f, .08f, 1.09f, .47f);
            Decoration(scene, "Grassy platform", ShopSketchGraphic.Sketch.Grass, Hex("69A91E"), 0, 0, 1, .14f);
            Decoration(scene, "Purple flower", ShopSketchGraphic.Sketch.Flower, Hex("BB4EBA"), .012f, .025f, .052f, .13f);
            Decoration(scene, "Pink flower", ShopSketchGraphic.Sketch.Flower, Hex("FF85B5"), .94f, .025f, .983f, .13f);
        }

        static RectTransform Decoration(Transform parent, string name, ShopSketchGraphic.Sketch style, Color color, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent);
            Anchor(rect, x0, y0, x1, y1, 0, 0, 0, 0);
            var sketch = rect.gameObject.AddComponent<ShopSketchGraphic>();
            sketch.Style = style;
            sketch.color = color;
            sketch.raycastTarget = false;
            return rect;
        }

        void BuildInventory(RectTransform parent, bool vertical)
        {
            var inventory = Rect("Gesture inventory", parent);
            Anchor(inventory, vertical ? 0 : .285f, 0, 1, vertical ? .68f : 1, 0, 0, 0, 0);
            var heading = Label("Inventory heading", inventory, "A FEW NEW TRICKS FOR THE ROAD", 16, Accent, FontStyle.Bold);
            Anchor(heading.rectTransform, 0, 1, 1, 1, 4, -30, 0, 0);
            var scrollObject = Rect("Gesture scroll", inventory);
            Anchor(scrollObject, 0, 0, 1, 1, 0, 0, 0, -42);
            var scroll = scrollObject.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = vertical;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            var viewport = Rect("Viewport", scrollObject);
            Stretch(viewport, Vector2.zero, Vector2.zero);
            viewport.gameObject.AddComponent<RectMask2D>();
            var shelf = Rect("Cards", viewport);
            scroll.viewport = viewport;
            scroll.content = shelf;
            if (vertical)
            {
                shelf.anchorMin = new Vector2(0, 1); shelf.anchorMax = Vector2.one;
                shelf.pivot = new Vector2(.5f, 1);
                shelf.sizeDelta = new Vector2(0, 3 * 300 + 2 * 16);
                shelf.anchoredPosition = Vector2.zero;
            }
            else Stretch(shelf, Vector2.zero, Vector2.zero);
            int i = 0;
            foreach (var item in GestureCatalog.Items)
            {
                var panel = Panel(item.Name, shelf, Hex("FFFCE5"));
                if (vertical) Anchor(panel, 0, 1, 1, 1, 0, -i * 316 - 300, 0, -i * 316);
                else Anchor(panel, i / 3f, 0, (i + 1) / 3f, 1, 6, 0, -6, 0);
                BuildCard(panel, item.Gesture, item.Name, item.Instruction, item.Price, vertical);
                i++;
            }
        }

        void BuildCard(RectTransform panel, HandGesture gesture, string name, string instruction, int price, bool compact)
        {
            var card = new Card { Gesture = gesture, Price = price, Background = panel.GetComponent<Graphic>() };
            Decoration(panel, "A little strip of tape", ShopSketchGraphic.Sketch.Wood, Hex("E8C976"), .36f, .955f, .64f, .998f);
            var state = Label("Ownership", panel, "A NEW LITTLE TRICK", 15, Muted, FontStyle.Bold);
            card.State = state;
            var imageName = gesture == HandGesture.OpenPalm ? "OpenPalm" : gesture.ToString();
            var art = Art("Gesture artwork", panel, imageName, .05f, .48f, .95f, .90f);
            var title = Label("Gesture title", panel, name, 30, Ink, FontStyle.Bold);
            var how = Label("Pose instruction", panel, instruction, 20, Muted);
            how.alignment = TextAnchor.UpperCenter;
            title.alignment = TextAnchor.MiddleCenter;
            state.alignment = TextAnchor.MiddleCenter;
            Anchor(state.rectTransform, .04f, .91f, .96f, .98f, 0, 0, 0, 0);
            Anchor(title.rectTransform, .04f, .38f, .96f, .48f, 0, 0, 0, 0);
            Anchor(how.rectTransform, .08f, .25f, .92f, .38f, 0, 0, 0, 0);
            card.Button = Button("Buy " + name, panel, "", Accent, Color.white, () => Buy(gesture, name));
            card.Label = card.Button.GetComponentInChildren<Text>();
            Anchor((RectTransform)card.Button.transform, .07f, 0, .93f, 0, 0, 14, 0, 102);
            if (compact)
            {
                Anchor(art, 0, .02f, .36f, .98f, 0, 0, 0, 0);
                Anchor(state.rectTransform, .38f, .78f, .97f, .95f, 0, 0, 0, 0);
                Anchor(title.rectTransform, .38f, .57f, .97f, .79f, 0, 0, 0, 0);
                Anchor(how.rectTransform, .38f, .36f, .97f, .56f, 0, 0, 0, 0);
                Anchor((RectTransform)card.Button.transform, .40f, 0, .95f, 0, 0, 14, 0, 102);
                how.fontSize = 24;
                title.fontSize = 30;
                state.fontSize = 14;
                card.Label.fontSize = 24;
            }
            cards.Add(card);
        }

        void Buy(HandGesture gesture, string name)
        {
            var result = wallet.TryPurchase(gesture);
            Refresh();
            if (result == PurchaseResult.Purchased) messageText.text = name + " is yours! Try it on your next adventure.";
            else if (result == PurchaseResult.NotEnoughCoins) messageText.text = "Collect more palettes, then come see me!";
            else if (result == PurchaseResult.AlreadyOwned) messageText.text = "Already in your collection. A fine choice.";
        }

        void Refresh()
        {
            if (!built || wallet == null) return;
            balanceText.text = wallet.Balance.ToString();
            messageText.text = "Pick a little magic for the road.";
            foreach (var card in cards)
            {
                bool owned = wallet.Owns(card.Gesture);
                bool affordable = wallet.Balance >= card.Price;
                card.Button.interactable = !owned && affordable;
                card.Label.text = owned ? "Yours!" : card.Price + " palettes";
                card.State.text = owned ? "YOURS TO KEEP" : affordable ? "TAKE ME WITH YOU" : "NEED " + (card.Price - wallet.Balance) + " MORE PALETTES";
                card.Background.color = owned ? Hex("D9F2AE") : Hex("FFFCE5");
            }
        }

        void Continue()
        {
            if (!IsOpen) return;
            var callback = onContinue;
            Hide();
            callback?.Invoke();
        }

        void ApplySafeArea()
        {
            if (safeRoot == null || Screen.width == 0 || Screen.height == 0) return;
            var area = Screen.safeArea;
            safeRoot.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safeRoot.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safeRoot.offsetMin = safeRoot.offsetMax = Vector2.zero;
        }

        static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var color); return color; }
        internal static RectTransform Rect(string name, Transform parent)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }
        internal static void Stretch(RectTransform rect, Vector2 min, Vector2 max) { Anchor(rect, 0, 0, 1, 1, min.x, min.y, max.x, max.y); }
        internal static void Anchor(RectTransform rect, float x0, float y0, float x1, float y1, float left, float bottom, float right, float top)
        {
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = new Vector2(left, bottom); rect.offsetMax = new Vector2(right, top);
        }
        internal RectTransform Panel(string name, Transform parent, Color color)
        {
            var rect = Rect(name, parent);
            var sketch = rect.gameObject.AddComponent<ShopSketchGraphic>();
            sketch.Style = ShopSketchGraphic.Sketch.Paper;
            sketch.color = color;
            return rect;
        }
        internal Text Label(string name, Transform parent, string value, int size, Color color, FontStyle style = FontStyle.Normal)
        {
            if (font == null) font = Resources.Load<Font>("GestureShop/Fonts/PatrickHand-Regular") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var text = Rect(name, parent).gameObject.AddComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size; text.color = color; text.fontStyle = style;
            text.alignment = TextAnchor.MiddleLeft; text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }
        RectTransform Art(string name, Transform parent, string resource, float x0, float y0, float x1, float y1)
        {
            var rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = Resources.Load<Sprite>("GestureShop/" + resource);
            image.preserveAspect = true; image.raycastTarget = false;
            Anchor(rect, x0, y0, x1, y1, 0, 0, 0, 0);
            return rect;
        }
        internal Button Button(string name, Transform parent, string value, Color background, Color foreground, Action action)
        {
            var rect = Panel(name, parent, background);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<ShopSketchGraphic>();
            rect.GetComponent<ShopSketchGraphic>().Style = ShopSketchGraphic.Sketch.Wood;
            var colors = button.colors;
            colors.highlightedColor = Hex("B9ED99"); colors.pressedColor = Hex("7BBD64");
            colors.disabledColor = Hex("A9B79A"); button.colors = colors;
            var label = Label("Label", rect, value, 25, foreground, FontStyle.Bold);
            label.alignment = TextAnchor.MiddleCenter;
            Stretch(label.rectTransform, new Vector2(8, 2), new Vector2(-8, -2));
            button.onClick.AddListener(() => action?.Invoke());
            return button;
        }
    }
}
