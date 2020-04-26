[System.Serializable]
public struct ProceduralAnimationData
{
    public ProceduralStepData stepData;
    public ProceduralMoveData moveData;
}

[System.Serializable]
public struct ProceduralMoveData
{
    public float handFootMatchStrength;
    public float handMoveSpeed;
    public float chestMotionStrength;
    public float maxRootForwardAngle;
    public float maxRootSideAngle;
}

[System.Serializable]
public struct ProceduralStepData
{
    public float footMoveSpeed;
    public float stepSize;
    public float sideStepSize;
    public float stepHeight;
    public float maxDistBeforeNextStep;
    public float stepRayLength;
    public UnityEngine.Vector3 rightFootRayOffset;
    public UnityEngine.Vector3 leftFootRayOffset;
}
