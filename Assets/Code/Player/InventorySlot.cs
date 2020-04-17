using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public const string DefaultText = "";

    private int _slotID;
    private int _netID;
    private PhysicalItem item;
    private bool selected = false;

    public TextMeshProUGUI nameText;
    public Image background;

    public Color normalColor;
    public Color selectColor;
    public bool dontDraw = false;

    public void Init(int slotID, int netID)
    {
        _slotID = slotID;
        _netID = netID;

        if (background == null)
            background = GetComponent<Image>();
        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();

        //Subscribe Events
        Events.Inventory.OnAddItem += OnAddItem;
        Events.Inventory.OnRemoveItem += OnRemoveItem;
        Events.Inventory.OnActive += OnSelectSlot;
    }

    private void UpdateSlot()
    {
        if(item == null)
        {
            nameText.text = DefaultText;
        }else
        {
            nameText.text = item.meta.name;
        }

        background.color = (selected) ? selectColor : normalColor;

        if (dontDraw)
            background.color = Color.clear;
    }

    public void OnAddItem(int index, PhysicalItem addItem, int netID, bool send)
    {
        if (_netID != netID || item != null || _slotID != index) return;
        item = addItem;
        UpdateSlot();
    }
    public void OnRemoveItem(int index, int netID, bool send)
    {
        if (netID != _netID || index != _slotID) return;

        item = null;
        UpdateSlot();
    }
    public void OnSelectSlot(int slotIndex)
    {
        selected = (slotIndex == _slotID);
        UpdateSlot();
        if(item != null)
        {
            Events.Item.Hide(item.InstanceID, !selected, true);
        }
    }

}
