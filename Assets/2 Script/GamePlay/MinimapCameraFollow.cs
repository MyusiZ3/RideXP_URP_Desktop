using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Controls top-down minimap camera tracking following the target player transform.
/// </summary>
public class MinimapCameraFollow : MonoBehaviour
{
    [Header("Target & Camera Settings")]
    public Transform player;
    public bool rotateWithPlayer = false;
    public float height = 20f;
    public float distanceBehind = 0f;

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0f, 0f, 0f, 0f);

            if (cam.TryGetComponent<UniversalAdditionalCameraData>(out var data))
            {
                data.renderPostProcessing = false;
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
        transform.rotation = rotateWithPlayer 
            ? Quaternion.Euler(90f, player.eulerAngles.y, 0f) 
            : Quaternion.Euler(90f, 0f, 0f);
    }
}
