using UnityEngine;

public class PerspectiveController : MonoBehaviour
{
    public static PerspectiveController ins;
    public KeyCode toggleKey = KeyCode.Tab;
    public Camera firstPersonCam;
    public Camera thirdPersonCam;
    public SkinnedMeshRenderer headRenderer;
    private bool firstPerson = false;

    public bool noDisplay = false;

    public Camera ActiveCamera { get; private set; }

    private void Awake()
    {
        if (ins != null)
            return;

        ins = this;
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
        if (Input.GetKeyDown(toggleKey))
        {
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

}
