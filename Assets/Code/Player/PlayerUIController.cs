using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIController : MonoBehaviour
{
    [Header("Inventory-UI Data:")]
    public Transform slotContainer;
    public GameObject slotPrefab;

    private InventorySlot[] slots;

    private void Awake()
    {
        Subscribe();
    }

    private void Subscribe()
    {
        Events.Inventory.OnInventoryCreated += OnInventoryCreated;
        Events.Inventory.OnItemAddedToInventory += OnItemAddedToInventory;
        Events.Inventory.OnItemRemovedFromInventory += OnItemRemovedFromInventory;
    }

    private void UnSubscribe()
    {
        Events.Inventory.OnInventoryCreated -= OnInventoryCreated;
        Events.Inventory.OnItemAddedToInventory -= OnItemAddedToInventory;
        Events.Inventory.OnItemRemovedFromInventory -= OnItemRemovedFromInventory;
    }


    public void OnInventoryCreated(int size)
    {
        if(slots != null && slots.Length > 0)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if(slots[i] != null)
                {
                    Destroy(slots[i].background.gameObject);
                }
            }
        }
        
        slots = new InventorySlot[size];
        for (int i = 0; i < size; i++)
        {
            GameObject temp = Instantiate(slotPrefab, slotContainer);
            GameObject text = temp.transform.GetChild(0).gameObject;
            text.transform.SetParent(temp.transform);

            InventorySlot slot = new InventorySlot()
            {
                background = temp.GetComponent<Image>(),
                nameText = text.GetComponent<TextMeshProUGUI>()
            };

            slots[i] = slot;
            slots[i].Update();
        }
    }
    public void OnItemAddedToInventory(Item ent, int index)
    {
        slots[index].content = ent;
        slots[index].Update();
    }
    public void OnItemRemovedFromInventory(int index)
    {
        slots[index].content = null;
        slots[index].Update();
    }
}

public class InventorySlot
{
    public Color Empty = new Color(0.3f, 0.3f, 0.3f);
    public Color Active = new Color(0.5f, 0.5f, 0.5f);

    public Image background;
    public TextMeshProUGUI nameText;
    public Item content;

    public void Update()
    {
        if(content == null || content.Active == false)
        {
            background.color = Empty;
            nameText.text = "";
        }else if(content.Active == true)
        {
            background.color = Active;
            nameText.text = content.Name;
        }
    }
}
