public interface IWeapon : IItem
{
    float Damage { get; set; }
    float Range { get; set; }
    string PrimaryUseAnimation { get; set; }
    string SecondaryUseAnimation { get; set; }
}
