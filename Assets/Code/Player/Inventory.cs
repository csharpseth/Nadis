using UnityEngine;

public class Inventory : MonoBehaviour
{
    public string SelectInputAxis = "Mouse ScrollWheel";
    public Transform slotContainer;
    public GameObject slotPrefab;

    private PhysicalItem[] _content;
    private int _activeIndex = 0;
    private int _netID;
    
    public PhysicalItem ActiveItem { get { return _content[_activeIndex]; } }
    public PhysicalItem[] Content { get { return _content; } }
    
    public void Init(int playerID, int size)
    {
        _netID = playerID;
        _content = new PhysicalItem[size];

        if(slotContainer != null && slotPrefab != null)
        {
            for (int i = 0; i < size; i++)
            {
                InventorySlot s = Instantiate(slotPrefab, slotContainer).GetComponent<InventorySlot>();
                s.Init(i, playerID);
            }
        }

        Subscribe();
    }
    private void Update()
    {
        if (_netID != NetworkManager.LocalPlayer.ID) return;

        float val = Input.GetAxisRaw(SelectInputAxis);
        if(val > 0f)
        {
            _activeIndex++;
            if (_activeIndex > (_content.Length - 1)) _activeIndex = (_content.Length - 1);
            Events.Inventory.OnActive(_activeIndex);
        }else if(val < 0f)
        {
            _activeIndex--;
            if (_activeIndex < 0) _activeIndex = 0;
            Events.Inventory.OnActive(_activeIndex);
        }
    }

    private int GetFirstOpenIndex()
    {
        if (ActiveItem == null)
            return _activeIndex;

        for (int i = 0; i < _content.Length; i++)
        {
            if (_content[i] == null)
                return i;
        }

        return -1;
    }
    private bool Contains(int instanceID)
    {
        for (int i = 0; i < _content.Length; i++)
        {
            if (_content[i] == null || _content[i].InstanceID != instanceID) continue;
            return true;
        }
        return false;

    }
    private int GetIndexOf(int instanceID)
    {
        if (Contains(instanceID) == false) return -1;
        for (int i = 0; i < _content.Length; i++)
        {
            if (_content[i] == null) continue;

            if (_content[i].InstanceID == instanceID) return i;
        }

        return -1;
    }

    public void AddItem(int instanceID, int netID, bool send)
    {
        if (netID != _netID) return;

        int addIndex = GetFirstOpenIndex();
        PhysicalItem item = Events.Item.GetItem(instanceID);
        if (item == null) return;
        Events.Item.Interact?.Invoke(instanceID, netID, Side.Right, false);
        _content[addIndex] = item;
        Events.Inventory.OnAddItem?.Invoke(addIndex, item, netID, send);
    }

    public void RemoveItem(int instanceID, int netID, bool send)
    {
        if (netID != _netID) return;

        int indexOf = GetIndexOf(instanceID);
        if (indexOf == -1) return;

        _content[indexOf] = null;
        Events.Item.Reset?.Invoke(instanceID, false);
        Events.Inventory.OnRemoveItem?.Invoke(indexOf, netID, send);
    }

    public void RemoveAtCurrentIndex(int netID, bool send)
    {
        if (_content[_activeIndex] == null) return;
        RemoveItem(_content[_activeIndex].InstanceID, netID, send);
    }

    public void DropAllItems(int netID, bool send)
    {
        if (netID != _netID) return;

        for (int i = 0; i < _content.Length; i++)
        {
            _activeIndex = i;
            Events.Inventory.RemoveActiveItem(netID, send);
        }

        _activeIndex = 0;
    }

    //Don't Forget To Unsubscribe
    private void Subscribe()
    {
        Events.Inventory.AddItem += AddItem;
        Events.Inventory.RemoveItem += RemoveItem;
        Events.Inventory.RemoveActiveItem += RemoveAtCurrentIndex;
        Events.Inventory.DropAllItems += DropAllItems;
        Events.Player.UnSubscribe += UnSubscribe;
    }
    private void UnSubscribe(int netID)
    {
        if (_netID != netID) return;

        Events.Inventory.AddItem -= AddItem;
        Events.Inventory.RemoveItem -= RemoveItem;
        Events.Inventory.RemoveActiveItem -= RemoveAtCurrentIndex;
        Events.Inventory.DropAllItems -= DropAllItems;
        Events.Player.UnSubscribe -= UnSubscribe;
    }
}
