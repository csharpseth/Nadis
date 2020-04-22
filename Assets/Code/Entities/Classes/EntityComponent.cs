using UnityEngine;
using System.Collections.Generic;
using System;

public class EntityComponent : MonoBehaviour, IIdentification
{
    //Variables
    private List<IComponent> _components = new List<IComponent>();

    public Action<float> OnUpdate;
    public Action OnAdd;
    public Action OnComponentDestroy;

    //Properties
    public int ID { get; set; }
    private void Awake()
    {
        ID = gameObject.GetInstanceID();
        EntityManager.Register(this);
    }
    

    public void Remove(IComponent component)
    {
        if (_components.Contains(component) == false) return;
        _components.Remove(component);
        OnUpdate -= component.OnUpdate;
    }
    public void Add(IComponent component)
    {
        if (_components.Contains(component) == true) return;
        _components.Add(component);
        OnUpdate += component.OnUpdate;
    }
    public IComponent Get(Type type)
    {
        IComponent component = null;

        for (int i = 0; i < _components.Count; i++)
        {
            if(_components[i].GetType().Equals(type))
            {
                component = _components[i];
                break;
            }
        }

        return component;
    }

    private void OnDestroy()
    {
        OnComponentDestroy?.Invoke();
        EntityManager.DeRegister(this);
    }
}
