using UnityEngine;

public class PerspectiveController : MonoBehaviour, IDisableIfRemotePlayer
{
    public static PerspectiveController ins;
    public KeyCode toggleKey = KeyCode.Tab;
    public Camera firstPersonCam;
    public AudioListener listener;
    public Camera thirdPersonCam;
    public SkinnedMeshRenderer headRenderer;
    private bool firstPerson = false;
    public bool debugAllowed = true;
    private GameObject debugObject;
    private bool debugView = false;

    public bool noDisplay = false;

    public Camera ActiveCamera { get; private set; }

    private void Awake()
    {
        if (ins != null)
            return;

        ins = this;

        if (debugObject == null) debugAllowed = false;

        if(debugAllowed)
        {
            debugObject = GameObject.FindGameObjectWithTag("Debug View");
            debugObject.SetActive(false);
        }

        listener = firstPersonCam.GetComponent<AudioListener>();

        if (noDisplay)
        {
            firstPersonCam.gameObject.SetActive(false);
            thirdPersonCam.gameObject.SetActive(false);
        }
        else
        {
            TogglePerspective();
        }
    }

    private void Update()
    {
        if (noDisplay) return;

        if (Input.GetKeyDown(toggleKey))
        {
            TogglePerspective();
        }

        if(debugAllowed && debugObject != null && Input.GetKeyDown(KeyCode.I))
        {
            debugView = !debugView;
            debugObject.SetActive(debugView);
            firstPersonCam.enabled = !debugView;
            thirdPersonCam.enabled = !debugView;

            if(!debugView)
                TogglePerspective();
        }
    }

    public void TogglePerspective()
    {
        firstPerson = !firstPerson;

        firstPersonCam.enabled = firstPerson;
        thirdPersonCam.enabled = !firstPerson;
        if(headRenderer != null)
            headRenderer.shadowCastingMode = firstPerson == true ? UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly : UnityEngine.Rendering.ShadowCastingMode.On;

        if (firstPerson == true)
            ActiveCamera = firstPersonCam;
        else
            ActiveCamera = thirdPersonCam;

    }

    public void Disable(bool disabled)
    {
        headRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;

        firstPersonCam.enabled = !disabled;
        thirdPersonCam.enabled = !disabled;
        if (listener == null) listener = firstPersonCam.GetComponent<AudioListener>();
        if (listener == null) listener = thirdPersonCam.GetComponent<AudioListener>();
        
        listener.enabled = !disabled;

        noDisplay = disabled;
    }
}
