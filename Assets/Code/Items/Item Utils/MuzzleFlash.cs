using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public Sprite[] muzzleFlashSprites;
    [Range(0f, 0.5f)]
    public float muzzleFlashDuration;
    private SpriteRenderer[] spriteRenderers;
    private Light muzzleFlashLight;

    private bool doFlash = false;
    private float timeActivated;

    private void Awake()
    {
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        muzzleFlashLight = GetComponentInChildren<Light>();

        Trigger();
    }

    private void Update()
    {
        //Test
        if (Input.GetKeyDown(KeyCode.Y))
            Trigger();

        if (doFlash == false) return;

        float timeDiff = (Time.realtimeSinceStartup - timeActivated);
        if(timeDiff >= muzzleFlashDuration)
        {
            if(spriteRenderers != null)
            {
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    spriteRenderers[i].enabled = false;
                }
            }

            if(muzzleFlashLight != null)
            {
                muzzleFlashLight.enabled = false;
            }

            doFlash = false;
        }

    }


    public void Trigger()
    {
        timeActivated = Time.realtimeSinceStartup;
        doFlash = true;
        Sprite sprite = muzzleFlashSprites[Random.Range(0, muzzleFlashSprites.Length)];
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = sprite;
            spriteRenderers[i].enabled = true;
        }


        if (muzzleFlashLight != null)
        {
            muzzleFlashLight.enabled = true;
        }
    }
}
