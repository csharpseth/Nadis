using System;
using System.Collections.Generic;

public struct Pool<T>
{
    private Queue<T> _content;
    private bool createNewOnOverflow;
    private int inUse;

    public Pool(int size = 100, bool overflow = true)
    {
        _content = new Queue<T>();
        for (int i = 0; i < size; i++)
        {
            T t = Activator.CreateInstance<T>();
            _content.Enqueue(t);
        }

        createNewOnOverflow = overflow;
        inUse = 0;
    }

    public T GetInstance()
    {
        if (inUse >= _content.Count && createNewOnOverflow)
        {
            T t = Activator.CreateInstance<T>();
            _content.Enqueue(t);
        }
        else if (createNewOnOverflow && inUse >= _content.Count)
            return default;
        inUse++;
        return _content.Dequeue();
    }
    public void Release(T obj)
    {
        inUse--;
        _content.Enqueue(obj);
    }

}