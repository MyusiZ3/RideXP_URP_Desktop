using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform player; // Drag player ke sini
    public bool rotateWithPlayer = false; // Centang ini kalau mau minimap ikut arah

    public float height = 20f;   // Ketinggian kamera dari player
    public float distanceBehind = 0f; // Optional, kalau mau agak di belakang player

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f); // Background transparan (Alpha = 0)

            if (cam.TryGetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>(out var data))
            {
                data.renderPostProcessing = false; // WAJIB OFF agar URP tidak mengubah Alpha 0 menjadi hitam padat
                data.requiresColorTexture = false;
                data.requiresDepthTexture = false;
            }
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 newPosition = player.position;
        newPosition.y += height;

        if (distanceBehind != 0f)
        {
            Vector3 offset = -player.forward * distanceBehind;
            newPosition += new Vector3(offset.x, 0f, offset.z);
        }

        transform.position = newPosition;

        if (rotateWithPlayer)
        {
            // Kamera muter sesuai arah player
            transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
        }
        else
        {
            // Kamera tetap top-down fix
            transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }
    }
}
