public struct StatData : IStatData
{
    public int OwnerID { get; }
    public int DataID => (int)StatType.Health;
    public int InitialValue { get; private set; }
    public int Value { get; private set; }
    public int MaxValue { get; }
    public float Percent => ((float)Value / MaxValue);
    public bool Dead;

    public StatData(int ownerID, int initialValue, int maxValue)
    {
        OwnerID = ownerID;
        Value = initialValue;
        InitialValue = initialValue;
        MaxValue = maxValue;
        Dead = false;
    }
    
    public void SetValue(int newValue)
    {
        Value = newValue;
        CheckIfDead();
    }

    public void AlterValue(int amount)
    {
        Value += amount;
        CheckIfDead();
    }

    private void CheckIfDead()
    {
        if(Value <= 2)
        {
            Dead = true;
        }
    }

    public void Reset()
    {
        Value = InitialValue;
        Dead = false;
    }

}