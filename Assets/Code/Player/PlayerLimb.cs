using UnityEngine;

public class PlayerLimb : MonoBehaviour, IMaterialProperty
{
    public MaterialProperty Material { get { return _material; } }
    [SerializeField]
    private MaterialProperty _material;
    public PlayerAppendage appendage;
}