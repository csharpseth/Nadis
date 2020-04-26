[System.Serializable]
public class AnimationConfig
{
    [System.Serializable]
    public struct ConfigData
    {
        public string identifier;
        public LerpPoint[] localPoints;
        public bool persistent;
        public AnimatorTarget target;
    }
    public ConfigData[] configs;

    public ConfigData GetAnimation(string identifier)
    {
        for (int i = 0; i < configs.Length; i++)
        {
            if (configs[i].identifier.ToLower() == identifier.ToLower())
            {
                return configs[i];
            }
        }

        return default(ConfigData);
    }
}
