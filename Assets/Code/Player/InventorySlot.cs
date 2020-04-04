using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public const string DefaultText = "";

    private int _id;
    private PhysicalItem item;

    public TextMeshProUGUI nameText;
    public Image background;

    public Color normalColor;
    public Color selectColor;

    public void Init(int id)
    {
        _id = id;
        Clear(id);
        if (background == null)
            background = GetComponent<Image>();
        if (nameText == null)
            nameText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void Select(int id)
    {
        if(_id == id)
        {
            background.color = selectColor;
            if (item != null)
                Events.Item.OnItemHide(item.InstanceID, false, true);
        }
        else
        {
            background.color = normalColor;
            if(item != null)
                Events.Item.OnItemHide(item.InstanceID, true, true);
        }
    }

    public void AddItem(PhysicalItem item, int id)
    {
        if(this.item == null && _id == id)
        {
            this.item = item;
            nameText.text = item.meta.name;
            Debug.Log(item.meta.name);
        }
    }

    public void Clear(int id)
    {
        if(id == _id)
        {
            item = null;
            nameText.text = DefaultText;
        }
    }
}
