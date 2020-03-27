using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalItem : MonoBehaviour
{
    public ItemMetaData meta;
    public float grabSpeed = 5f;
    public float grabbedThreshold = 0.1f;
    public Vector3 heldEulerOffset;
    
    internal Rigidbody rb;
    private Collider col;
    private Transform parent;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        if(col == null)
        {
            col = GetComponentInChildren<Collider>();
        }
    }

    public virtual void PrimaryUse()
    {
        Debug.Log(meta.name + " Primary Use");
    }

    public virtual void SecondaryUse()
    {
        Debug.Log(meta.name + " Secondary Use");
    }

    public void Interact(Transform hand)
    {
        rb.isKinematic = true;
        col.enabled = false;
        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = heldEulerOffset;
        parent = hand;
    }

    public void ResetObjct()
    {
        transform.parent = null;
        parent = null;
        rb.isKinematic = false;
        col.enabled = true;
    }

    public void Hide(bool val)
    {
        gameObject.SetActive(!val);
    }

}

[System.Serializable]
public struct ItemMetaData
{
    public string name;
    public int id;
}
