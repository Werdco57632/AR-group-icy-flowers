using UnityEngine;
using UnityEngine.Events;

namespace IcyFlowers.Shop
{
    /// <summary>Add to a level manager; call OpenShop at level completion and wire onContinue.</summary>
    public sealed class ShopIntermission : MonoBehaviour
    {
        [SerializeField] bool pauseWhileShopping = true;
        [SerializeField] UnityEvent onOpened = new UnityEvent();
        [SerializeField] UnityEvent onClosed = new UnityEvent();
        [SerializeField] UnityEvent onContinue = new UnityEvent();
        GestureShopView view;
        bool paused;
        bool opened;
        float previousTimeScale;

        public ShopWallet Wallet => ShopProgress.Wallet;
        public bool IsOpen => view != null && view.IsOpen;
        public UnityEvent OnContinue => onContinue;
        public UnityEvent OnOpened => onOpened;
        public UnityEvent OnClosed => onClosed;

        public void OpenShop()
        {
            if (IsOpen) return;
            if (view == null)
            {
                var ui = new GameObject("Gesture shop", typeof(RectTransform));
                ui.transform.SetParent(transform, false);
                view = ui.AddComponent<GestureShopView>();
                view.Closed += RestoreTime;
            }
            if (pauseWhileShopping)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0;
                paused = true;
            }
            view.Show(Wallet, Continue);
            opened = true;
            onOpened.Invoke();
        }

        // For an existing coin script's UnityEvent. Use Wallet.TryCollectCoin for stable pickup IDs.
        public void AddCoins(int amount) { Wallet.AddCoins(amount); }
        public bool IsGestureUnlocked(HandGesture gesture) { return Wallet.Owns(gesture); }
        public void CloseShop()
        {
            if (view != null) view.Hide();
            RestoreTime();
        }

        void Continue()
        {
            RestoreTime();
            onContinue.Invoke();
        }
        void RestoreTime()
        {
            if (paused)
            {
                Time.timeScale = previousTimeScale;
                paused = false;
            }
            if (opened)
            {
                opened = false;
                onClosed.Invoke();
            }
        }
        void OnDisable()
        {
            if (view != null) view.Hide();
            RestoreTime();
        }
        void OnDestroy() { RestoreTime(); }
    }
}
