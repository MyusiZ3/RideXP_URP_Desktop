using UnityEngine;

public class CameraPanning : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public float duration = 3f;
    public bool autoStart = true;

    private float timer;
    private bool isMoving;

    void Start()
    {
        if (autoStart) StartPanning();
    }

    public void StartPanning()
    {
        timer = 0f;
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / duration);
        transform.position = Vector3.Lerp(startPoint.position, endPoint.position, t);

        if (t >= 1f) isMoving = false;
    }
}
