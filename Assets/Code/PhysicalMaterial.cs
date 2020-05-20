using UnityEngine;

public class PhysicalMaterial : MonoBehaviour, IMaterialProperty
{
    public MaterialProperty Material { get { return _material; } }
    [SerializeField]
    private MaterialProperty _material;
}
