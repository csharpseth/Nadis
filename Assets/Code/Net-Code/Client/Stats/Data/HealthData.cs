public struct HealthData : IStatData
{
    public int OwnerID { get; }
    public int DataID => (int)StatType.Health;
    public int Value { get; private set; }
    public int MaxValue { get; }
    public float Percent => ((float)Value / MaxValue);

    public HealthData(int ownerID, int initialValue, int maxValue)
    {
        OwnerID = ownerID;
        Value = initialValue;
        MaxValue = maxValue;
    }
    
    public void SetValue(int newValue)
    {
        Value = newValue;
    }

    public void AlterValue(int amount)
    {
        Value += amount;
    }

}