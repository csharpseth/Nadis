public interface IWeapon : IItem
{
    float Damage { get; set; }
    float Range { get; set; }
    UnityEngine.AudioSource Source { get; set; }
}
