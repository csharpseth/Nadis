using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXController : MonoBehaviour
{
    private static FXController instance;

    public int poolSize = 300;
    public bool autoEnque = true;
    public GameObject[] hitEffectPrefabs;
    public GameObject[] muzzleFlashPrefabs;

    private GameObjectPool hitEffectPool;
    private GameObjectPool muzzleFlashPool;

    private void Start()
    {
        if (instance != null) Destroy(this);

        instance = this;
        hitEffectPool = new GameObjectPool(hitEffectPrefabs, poolSize, autoEnque);
        //muzzleFlashPool = new GameObjectPool(muzzleFlashPrefabs, poolSize, autoEnque);
    }

    public static void HitAt(Vector3 pos, Vector3 rot = default)
    {
        instance.hitEffectPool.SpawnAt(pos, rot);
    }

    public static void MuzzleAt(Vector3 pos, Vector3 rot = default)
    {
        instance.muzzleFlashPool.SpawnAt(pos, rot);
    }
}
