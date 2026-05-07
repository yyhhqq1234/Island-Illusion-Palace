using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class InventoryItemUI : MonoBehaviour
{
    [Header("UI元素")]
    public Image itemIcon;
    public Text quantityText;
    public Text itemNameText;
    public Button useButton;
    public Button equipButton;
    public Button dropButton;

    [Header("事件")]
    public UnityEvent<InventoryItem> onItemClicked;
    public UnityEvent<InventoryItem> onUseItem;
    public UnityEvent<InventoryItem> onEquipItem;
    public UnityEvent<InventoryItem> onDropItem;

    [Header("引用")]
    private InventoryItem currentItem;
    private InventorySystem inventorySystem;

    void Start()
    {
        InitializeButtons();
        FindInventorySystem();
    }

    void InitializeButtons()
    {
        if (useButton != null)
        {
            useButton.onClick.AddListener(UseItem);
        }
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(EquipItem);
        }
        if (dropButton != null)
        {
            dropButton.onClick.AddListener(DropItem);
        }

        // 添加物品点击事件
        Button itemButton = GetComponent<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(ItemClicked);
        }
    }

    void FindInventorySystem()
    {
        inventorySystem = FindObjectOfType<InventorySystem>();
    }

    public void SetItem(InventoryItem item)
    {
        currentItem = item;
        UpdateItemDisplay();
        UpdateButtonStates();
    }

    void UpdateItemDisplay()
    {
        if (currentItem == null)
        {
            // 清空显示
            if (itemIcon != null) itemIcon.gameObject.SetActive(false);
            if (quantityText != null) quantityText.text = "";
            if (itemNameText != null) itemNameText.text = "";
            return;
        }

        // 显示物品信息
        if (itemIcon != null)
        {
            itemIcon.gameObject.SetActive(true);
            if (currentItem.icon != null)
            {
                itemIcon.sprite = currentItem.icon;
            }
            else
            {
                // 使用默认图标
                itemIcon.color = GetItemTypeColor(currentItem.itemType);
            }
        }

        if (quantityText != null)
        {
            if (currentItem.CanStack && currentItem.quantity > 1)
            {
                quantityText.text = currentItem.quantity.ToString();
                quantityText.gameObject.SetActive(true);
            }
            else
            {
                quantityText.gameObject.SetActive(false);
            }
        }

        if (itemNameText != null)
        {
            itemNameText.text = currentItem.itemName;
        }
    }

    void UpdateButtonStates()
    {
        if (currentItem == null)
        {
            // 禁用所有按钮
            if (useButton != null) useButton.gameObject.SetActive(false);
            if (equipButton != null) equipButton.gameObject.SetActive(false);
            if (dropButton != null) dropButton.gameObject.SetActive(false);
            return;
        }

        // 根据物品类型显示不同的按钮
        if (useButton != null)
        {
            useButton.gameObject.SetActive(currentItem.itemType == ItemType.Consumable);
        }

        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(currentItem.itemType == ItemType.Weapon);
            if (equipButton.gameObject.activeSelf)
            {
                equipButton.GetComponentInChildren<Text>().text = currentItem.isEquipped ? "卸下" : "装备";
            }
        }

        if (dropButton != null)
        {
            dropButton.gameObject.SetActive(true);
        }
    }

    void ItemClicked()
    {
        if (currentItem != null && onItemClicked != null)
        {
            onItemClicked.Invoke(currentItem);
        }
    }

    void UseItem()
    {
        if (currentItem != null && inventorySystem != null)
        {
            inventorySystem.UseItem(currentItem);
            if (onUseItem != null)
            {
                onUseItem.Invoke(currentItem);
            }
            // 更新显示
            UpdateItemDisplay();
        }
    }

    void EquipItem()
    {
        if (currentItem != null && inventorySystem != null)
        {
            if (currentItem.isEquipped)
            {
                inventorySystem.UnequipItem(currentItem);
            }
            else
            {
                inventorySystem.EquipItem(currentItem);
            }
            if (onEquipItem != null)
            {
                onEquipItem.Invoke(currentItem);
            }
            // 更新显示
            UpdateButtonStates();
        }
    }

    void DropItem()
    {
        if (currentItem != null && inventorySystem != null)
        {
            inventorySystem.RemoveItem(currentItem.itemName, 1);
            if (onDropItem != null)
            {
                onDropItem.Invoke(currentItem);
            }
            // 更新显示
            UpdateItemDisplay();
        }
    }

    Color GetItemTypeColor(ItemType type)
    {
        return type switch
        {
            ItemType.Consumable => Color.green,
            ItemType.Material => Color.yellow,
            ItemType.Weapon => Color.blue,
            ItemType.MemoryFragment => Color.red,
            _ => Color.gray
        };
    }

    public InventoryItem GetCurrentItem()
    {
        return currentItem;
    }

    public void ClearItem()
    {
        currentItem = null;
        UpdateItemDisplay();
        UpdateButtonStates();
    }
}
