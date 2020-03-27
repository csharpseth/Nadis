using UnityEngine;
using System.Collections;

public class InventoryUIManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform container;

    private void Start()
    {
        if (InteractionController.ins == null)
            return;

        for (int i = 0; i < InteractionController.ins.inventorySize; i++)
        {
            InventorySlot slot = Instantiate(slotPrefab, container).GetComponent<InventorySlot>();
            slot.Init(i);
            InteractionController.ins.OnInventoryAdd += slot.AddItem;
            InteractionController.ins.OnInventoryRemove += slot.Clear;
            InteractionController.ins.OnInventorySelect += slot.Select;

            slot.Select(0);
        }
    }
}
