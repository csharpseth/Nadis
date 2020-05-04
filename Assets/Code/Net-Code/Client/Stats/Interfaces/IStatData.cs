public interface IStatData
{
    int OwnerID { get; }
    int DataID { get; }
    int Value { get; }
    int MaxValue { get; }
    float Percent { get; }

    void SetValue(int newValue);
    void AlterValue(int amount);

}