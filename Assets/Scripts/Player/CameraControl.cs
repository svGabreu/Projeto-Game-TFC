using UnityEngine;
using UnityEngine.InputSystem;

public class CameraControl : MonoBehaviour
{
    public Transform playerTransform;
    public Vector3 offset = new Vector3(0, 2f, -4f);
    public float mouseSensitivity = 10f;

    private float yaw = 0f;
    private float pitch = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // LateUpdate é usado para garantir que a câmera seja atualizada após o movimento do jogador
    void LateUpdate()
    {
        if (playerTransform == null) return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * mouseSensitivity * Time.deltaTime;
        pitch -= mouseDelta.y * mouseSensitivity * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, -60f, 70f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 desiredPosition = playerTransform.position + (rotation * offset);

        Vector3 lookAtPoint = playerTransform.position + Vector3.up * 1.5f;

        RaycastHit hit;
        if (Physics.Linecast(lookAtPoint, desiredPosition, out hit))
        {
            
            transform.position = hit.point + hit.normal * 0.15f;
        }
        else
        {
            transform.position = desiredPosition;
        }

        transform.LookAt(lookAtPoint);
    }
}