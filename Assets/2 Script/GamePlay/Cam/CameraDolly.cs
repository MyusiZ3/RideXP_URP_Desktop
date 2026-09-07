using UnityEngine;

public class CameraDolly : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float dollyDuration = 4f;

    private float timer = 0f;
    private bool isDolly = true;

    void Start()
    {
        transform.position = startPoint.position;
    }

    void Update()
    {
        if (!isDolly) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / dollyDuration);
        transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);

        transform.LookAt(endPoint); // Biar tetap ngarah ke tujuan

        if (t >= 1f) isDolly = false;
    }
}
