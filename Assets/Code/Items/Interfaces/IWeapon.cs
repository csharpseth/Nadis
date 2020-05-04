public interface IWeapon : IItem
{
    int Damage { get; set; }
    float Range { get; set; }
    UnityEngine.AudioSource Source { get; set; }
}
