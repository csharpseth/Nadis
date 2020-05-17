using UnityEngine;

[CreateAssetMenu(fileName = "New Ballistics Model", menuName = "Items/Ballistics Data", order = 0)]
public class Ballistics : ScriptableObject
{
    [Range(1, 1000)]
    public int range;
    [Range(5, 25)]
    public int damage;
}