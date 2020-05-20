using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXController : MonoBehaviour
{
    private static FXController instance;

    public int maxPrefabs = 500;
    public ImpactData[] impacts;
    private Dictionary<MaterialProperty, GameObjectPool> impactPools;
    private void Awake()
    {
        if (instance != null) Destroy(this);
        instance = this;
        impactPools = new Dictionary<MaterialProperty, GameObjectPool>();
        PopulateImpactPools();
    }

    private void PopulateImpactPools()
    {
        int individualPoolSize = maxPrefabs / impacts.Length;
        for (int i = 0; i < impacts.Length; i++)
        {
            if (impactPools.ContainsKey(impacts[i].material)) continue;

            GameObjectPool pool = new GameObjectPool(impacts[i].particlePrefab, individualPoolSize, true);
            impactPools.Add(impacts[i].material, pool);
        }
    }


    public static void HitAt(Vector3 position, IMaterialProperty properties)
    {
        if (properties == null || instance.impactPools.ContainsKey(properties.Material) == false) return;

        instance.impactPools[properties.Material].SpawnAt(position);
    }



    [System.Serializable]
    public struct ImpactData
    {
        public GameObject particlePrefab;
        public MaterialProperty material;
    }
    

}
