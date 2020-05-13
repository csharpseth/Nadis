using UnityEngine;

public class PlayerMouseController : MonoBehaviour, IDisableIfRemotePlayer, INetworkInitialized
{
    public static PlayerMouseController Instance;

    public float lookSpeed = 5f;
    public float maxHorizontalAngle = 40f;

    private Transform playerBody;
    [SerializeField]
    private Camera playerCamera;
    [SerializeField]
    private SkinnedMeshRenderer meshRenderer;
    private Transform playerCameraTransform;
    private bool execute = true;
    private bool disabled = false;

    private Vector2 centerScreen = Vector2.zero;
    public Vector2 CenterScreen
    {
        get
        {
            centerScreen.x = Screen.width / 2f;
            centerScreen.y = Screen.height / 2f;

            return centerScreen;
        }
    }

    public Ray CenterScreenRay
    {
        get
        {
            return playerCamera.ScreenPointToRay(CenterScreen);
        }
    }

    public int NetID { get; private set; }

    public float HorizontalLookPercent = 0f;
    
    private void Update()
    {
        if (disabled) return;

        if (Inp.Interface.Pause) execute = !execute;

        if(execute)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            return;
        }
        
        float h = lookSpeed * Inp.Move.LookDir.x;
        float v = -lookSpeed * Inp.Move.LookDir.y;

        Vector3 bodyRot = playerBody.eulerAngles;
        bodyRot.y += h;
        playerBody.eulerAngles = bodyRot;
        
        Vector3 camRot = playerCameraTransform.localEulerAngles;
        
        camRot.x += v;

        if (camRot.x > 0f && camRot.x < 180f && camRot.x > maxHorizontalAngle)
        {
            camRot.x = maxHorizontalAngle;
        }
        if (camRot.x > 0f && camRot.x > 180f && camRot.x < (360f - maxHorizontalAngle))
            camRot.x = (360f - maxHorizontalAngle);

        playerCameraTransform.localEulerAngles = camRot;

        HorizontalLookPercent = HorizontalAnglePercent();
    }

    public float HorizontalAnglePercent(float offset = 0f)
    {
        float percent = 0f;
        float camRot = playerCameraTransform.localEulerAngles.x + offset;

        if (camRot > 0f && camRot < 180f)
        {
            percent = -(camRot / maxHorizontalAngle);
        }
        if (camRot > 0f && camRot > 180f)
            percent = ((360f - camRot) / maxHorizontalAngle);

        return percent;
    }

    public void Disable(bool disabled)
    {
        this.disabled = disabled;
        playerCamera.enabled = !disabled;
        if (meshRenderer != null && disabled)
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    public void InitFromNetwork(int netID)
    {
        NetID = netID;
        Instance = this;

        playerBody = transform;
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();

        playerCameraTransform = playerCamera.transform;
        if (meshRenderer != null)
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
