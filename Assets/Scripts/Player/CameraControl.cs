using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    public Transform playerTransform;
    public Vector3 offset = new Vector3(0, 2f, -4f);
    public float mouseSensitivity = 10f;

    private float yaw   = 0f;
    private float pitch = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
        SnapCameraToPlayer();
    }

    public void SnapCameraToPlayer()
    {
        if (playerTransform == null) return;
        yaw   = playerTransform.eulerAngles.y;
        pitch = 0f;
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        transform.position  = playerTransform.position + (rotation * ScaledOffset());
        transform.LookAt(LookAtPoint());
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();
        yaw   += mouseDelta.x * mouseSensitivity * Time.deltaTime;
        pitch -= mouseDelta.y * mouseSensitivity * Time.deltaTime;
        pitch  = Mathf.Clamp(pitch, -60f, 70f);

        Quaternion rotation   = Quaternion.Euler(pitch, yaw, 0);
        Vector3    desiredPos = playerTransform.position + (rotation * ScaledOffset());
        Vector3    lookAt     = LookAtPoint();

        RaycastHit hit;
        transform.position = Physics.Linecast(lookAt, desiredPos, out hit)
            ? hit.point + hit.normal * 0.2f
            : desiredPos;

        transform.LookAt(lookAt);
    }

    private Vector3 ScaledOffset()  => Vector3.Scale(offset, playerTransform.lossyScale);
    private Vector3 LookAtPoint()   => playerTransform.position + Vector3.up * 1.5f * playerTransform.lossyScale.y;
}
