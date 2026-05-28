using UnityEngine;

namespace IIP.UI
{
    public interface IUIProvider
    {
        void ShowPanel(UIPanelType type);
        void HidePanel(UIPanelType type);
        void ShowTooltip(string text, Vector2 position);
        void HideTooltip();
        void ShowNotification(string message, float duration = 3f);
    }

    public interface IAlchemyUIProvider
    {
        bool IsAlchemyPanelOpen();
        void ShowAlchemyPanel();
        void HideAlchemyPanel(float delay = 0f);
        void SetPlayerInCauldronRange(bool inRange);
        void UpdateProduceDisplay();
    }

    public interface IInventoryUIProvider
    {
        bool IsInventoryOpen();
        void ToggleInventory();
        void RefreshInventory();
    }
}
