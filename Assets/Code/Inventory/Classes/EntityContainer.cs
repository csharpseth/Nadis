using System.Collections.Generic;

public class EntityContainer<T> where T : Item
{
    private T[] _content;
    private int _size;

    public T[] Content { get { return _content; } }
    public int Size { get { return _content.Length; } }


    public EntityContainer(int size)
    {
        _content = new T[size];
        _size = size;
    }

    public int AddItem(T item)
    {
        int index = -1;

        for (int i = 0; i < _size; i++)
        {
            if(_content[i] == null)
            {
                _content[i] = item;
                index = i;
                break;
            }
        }

        return index;
    }
    public T Remove(int index)
    {
        T item = null;
        
        if(index >= 0 && index < _size)
        {
            if(_content[index] != null)
            {
                item = _content[index];
                _content[index] = null;
            }
        }

        return item;
    }
    public int Remove(T item)
    {
        int index = -1;

        for (int i = 0; i < _size; i++)
        {
            if (_content[i] == item)
            {
                _content[i] = null;
                index = i;
                break;
            }
        }

        return index;
    }
    public T GetItem(int index)
    {
        T item = null;

        if(index >= 0 && index < _size)
        {
            if(_content[index] != null)
            {
                item = _content[index];
            }
        }

        return item;
    }
    public T GetItem(ulong networkID)
    {
        T item = null;

        for (int i = 0; i < _size; i++)
        {
            if(_content[i].NetworkID == networkID)
            {
                item = _content[i];
                break;
            }
        }

        return item;
    }
    public T[] GetItems(int id, int max = 0, int min = 0)
    {
        List<T> items = new List<T>();

        for (int i = 0; i < _size; i++)
        {
            if(_content[i].ID == id)
            {
                if (max <= 0 || items.Count < max)
                {
                    items.Add(_content[i]);
                }
                else if (max > 0 && items.Count >= max)
                    break;
            }
        }

        T[] itemArray = items.ToArray();

        if(min > 0 && items.Count < min)
        {
            itemArray = null;
        }

        return itemArray;
    }

    public void DisableAllExcept(int index)
    {
        for (int i = 0; i < _size; i++)
        {
            if(_content[i] != null)
            {
                _content[i].Hide(true);
            }
        }

        if (_content[index] != null) _content[index].Hide(false);
    }
}
