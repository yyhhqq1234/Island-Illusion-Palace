using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using IIPUI;

public class SummonWheelUI : MonoBehaviour
{
    [Header("轮盘设置")]
    public float wheelRadius = 150f;
    public float slotSize = 80f;
    public Color normalColor = IIPUIStyle.BarBackgroundNeutral;
    public Color highlightColor = IIPUIStyle.SummonWheelHighlight;
    public Color emptyColor = IIPUIStyle.SummonWheelEmpty;

    [Header("UI组件")]
    public GameObject wheelBackground;
    public Transform slotsParent;
    public GameObject slotPrefab;
    public Text centerText;

    [Header("槽位图标")]
    public Sprite defaultSlotIcon;

    private SummonSystem summonSystem;
    private SummonWheelSlot[] slots = new SummonWheelSlot[3];
    private int selectedSlotIndex = -1;
    private bool isWheelOpen = false;
    private Camera mainCamera;
    private Vector2 wheelCenter;

    private readonly Vector2[] slotPositions = new Vector2[]
    {
        new Vector2(0, 1),
        new Vector2(-0.866f, -0.5f),
        new Vector2(0.866f, -0.5f)
    };

    void Awake()
    {
        mainCamera = Camera.main;
        CreateWheelSlots();
        HideWheel();
    }

    void Start()
    {
        summonSystem = FindObjectOfType<SummonSystem>();
        if (summonSystem == null)
        {
            Debug.LogError("找不到SummonSystem！");
        }
    }

    void Update()
    {
        if (isWheelOpen)
        {
            UpdateWheelSelection();
        }
    }

    void CreateWheelSlots()
    {
        if (slotsParent == null)
        {
            GameObject parent = new GameObject("SlotsParent");
            parent.transform.SetParent(transform);
            parent.transform.localPosition = Vector3.zero;
            slotsParent = parent.transform;
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject slotObj = slotPrefab != null 
                ? Instantiate(slotPrefab, slotsParent) 
                : CreateDefaultSlot();

            slotObj.name = $"Slot_{i}";
            slotObj.transform.localPosition = slotPositions[i] * wheelRadius;
            slotObj.transform.localScale = Vector3.one;

            slots[i] = new SummonWheelSlot
            {
                gameObject = slotObj,
                image = slotObj.GetComponent<Image>(),
                text = slotObj.GetComponentInChildren<Text>(),
                index = i
            };

            if (slots[i].image != null)
            {
                slots[i].image.rectTransform.sizeDelta = new Vector2(slotSize, slotSize);
            }
        }
    }

    GameObject CreateDefaultSlot()
    {
        // 圆角底 + 边框 + 中央槽位号（统一外观）
        GameObject slot = IIPUIFactory.MakeSlot("Slot", slotsParent,
            new Vector2(slotSize, slotSize), normalColor,
            centerLabel: null, keyLabel: null, border: true);

        // 中央槽位文本（雅黑）
        IIPUIFactory.CreateLabel("Text", slot.transform, "", IIPUIStyle.FontSizeBody, Color.white);
        return slot;
    }

    public void ShowWheel()
    {
        isWheelOpen = true;
        gameObject.SetActive(true);

        if (wheelBackground != null)
        {
            wheelBackground.SetActive(true);
        }

        wheelCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        UpdateSlotDisplay();

        if (centerText != null)
        {
            centerText.text = "选择召唤物";
        }

        Debug.Log("召唤轮盘已打开");
    }

    public void HideWheel()
    {
        isWheelOpen = false;
        gameObject.SetActive(false);

        if (wheelBackground != null)
        {
            wheelBackground.SetActive(false);
        }

        selectedSlotIndex = -1;
    }

    void UpdateWheelSelection()
    {
        if (mainCamera == null) return;

        Vector2 mousePos = Input.mousePosition;
        Vector2 direction = mousePos - wheelCenter;

        if (direction.magnitude < slotSize * 0.5f)
        {
            selectedSlotIndex = -1;
            UpdateSlotHighlight(-1);
            if (centerText != null)
            {
                centerText.text = "取消";
            }
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        int newSelection = GetSlotIndexFromAngle(angle);

        if (newSelection != selectedSlotIndex)
        {
            selectedSlotIndex = newSelection;
            UpdateSlotHighlight(selectedSlotIndex);
            UpdateCenterText(selectedSlotIndex);
        }
    }

    int GetSlotIndexFromAngle(float angle)
    {
        if (angle >= 330 || angle < 90)
            return 0;
        else if (angle >= 150 && angle < 270)
            return 1;
        else
            return 2;
    }

    void UpdateSlotHighlight(int highlightedIndex)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].image != null)
            {
                if (summonSystem != null && i < summonSystem.battleSummons.Count && summonSystem.battleSummons[i] != null)
                {
                    slots[i].image.color = (i == highlightedIndex) ? highlightColor : normalColor;
                }
                else
                {
                    slots[i].image.color = emptyColor;
                }
            }
        }
    }

    void UpdateSlotDisplay()
    {
        if (summonSystem == null) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].text != null)
            {
                if (i < summonSystem.battleSummons.Count && summonSystem.battleSummons[i] != null)
                {
                    var core = summonSystem.battleSummons[i];
                    slots[i].text.text = $"{core.enemyType}\nLv.{core.level}";
                }
                else
                {
                    slots[i].text.text = "空槽位";
                }
            }
        }
    }

    void UpdateCenterText(int slotIndex)
    {
        if (centerText == null) return;

        if (slotIndex < 0)
        {
            centerText.text = "取消";
            return;
        }

        if (summonSystem != null && slotIndex < summonSystem.battleSummons.Count && summonSystem.battleSummons[slotIndex] != null)
        {
            var core = summonSystem.battleSummons[slotIndex];
            centerText.text = $"召唤\n{core.enemyType}";
        }
        else
        {
            centerText.text = "空槽位\n无法召唤";
        }
    }

    public int GetSelectedSlot()
    {
        return selectedSlotIndex;
    }

    public bool IsSlotEmpty(int slotIndex)
    {
        if (summonSystem == null) return true;

        if (slotIndex < 0 || slotIndex >= summonSystem.battleSummons.Count)
            return true;

        return summonSystem.battleSummons[slotIndex] == null;
    }

    public bool IsWheelOpen()
    {
        return isWheelOpen;
    }

    public void RefreshDisplay()
    {
        UpdateSlotDisplay();
    }
}

public class SummonWheelSlot
{
    public GameObject gameObject;
    public Image image;
    public Text text;
    public int index;
}
