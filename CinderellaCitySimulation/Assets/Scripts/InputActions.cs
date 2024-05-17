using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

/// <summary>
/// Responsible for handling input actions from peripherals like controllers
/// Must be attached to the FPSController in each scene
/// </summary>

public class InputActions : MonoBehaviour
{
    private InputMaster inputMaster;
    public static Vector2 rightStickLook;
    private readonly float lookSpeed = 5f;
    private Camera cam;
    private CharacterController controller;

    private Quaternion m_CharacterTargetRot;
    private Quaternion m_CameraTargetRot;

    Quaternion ClampRotationAroundXAxis(Quaternion q)
    {
        q.x /= q.w;
        q.y /= q.w;
        q.z /= q.w;
        q.w = 1.0f;

        float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

        angleX = Mathf.Clamp(angleX, -90F, 90f);

        q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

        return q;
    }

    private void Awake()
    {
        inputMaster = new InputMaster();
        inputMaster.Player.Jump.performed += ctx => Jump();
        inputMaster.Player.TimeTravelForward.performed += ctx => TimeTravel();
        inputMaster.Player.Look.performed += ctx => rightStickLook = ctx.ReadValue<Vector2>();
        inputMaster.Player.Look.canceled += ctx => rightStickLook = Vector2.zero;
    }

    private void OnEnable()
    {
        cam = GetComponentInChildren<Camera>();
        controller = GetComponentInChildren<CharacterController>();
        m_CharacterTargetRot = controller.transform.localRotation;
        m_CameraTargetRot = cam.transform.localRotation;
        inputMaster.Enable();
    }

    private void Update()
    {
        // ensure controller right stick inputs move the camera around
        if (rightStickLook.x != 0 || rightStickLook.y != 0 || ModeState.isGuidedTourPaused)
        {
            // initially set the target rotations to the existing so there's no sudden movement
            m_CharacterTargetRot = this.GetComponentInChildren<CharacterController>().transform.localRotation;
            m_CameraTargetRot = this.GetComponentInChildren<Camera>().transform.localRotation;

            float yRot = rightStickLook.x * lookSpeed;
            float xRot = rightStickLook.y * lookSpeed;

            m_CharacterTargetRot *= Quaternion.Euler(0f, yRot, 0f);
            m_CameraTargetRot *= Quaternion.Euler(xRot, 0f, 0f);

            m_CameraTargetRot = ClampRotationAroundXAxis(m_CameraTargetRot);

            cam.transform.localRotation = Quaternion.Slerp(cam.transform.localRotation, m_CameraTargetRot,
                    lookSpeed * Time.deltaTime);
            controller.transform.localRotation = Quaternion.Slerp(controller.transform.localRotation, m_CharacterTargetRot,
                    lookSpeed * Time.deltaTime);
        }
    }

    public void Jump()
    {
        CrossPlatformInputManager.SetButtonDown("Jump");
        Debug.Log("Jump action triggered");
    }

    public void TimeTravel()
    {
        Debug.Log("Time travel action triggered");
    }
}
