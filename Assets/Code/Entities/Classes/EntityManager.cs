using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    private static Action<float> OnUpdate;


    private static Dictionary<int, EntityComponent> _entities = new Dictionary<int, EntityComponent>();
    public static bool Contains(int id)
    {
        return _entities.ContainsKey(id);
    }
    public static void Register(EntityComponent component)
    {
        if (Contains(component.ID) == false) return;

        _entities.Add(component.ID, component);
        OnUpdate += component.OnUpdate;
    }
    public static void DeRegister(EntityComponent component)
    {
        if (Contains(component.ID) == false) return;

        _entities.Remove(component.ID);
    }

    private void Update()
    {
        if (_entities.Count == 0) return;

        OnUpdate?.Invoke(Time.deltaTime);
    }

}
