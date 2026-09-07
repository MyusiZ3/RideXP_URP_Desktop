using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;
    public float radius = 5f;
    public float height = 2f;
    public float orbitSpeed = 30f;
    public float orbitDuration = 3f;

    private float angle = 0f;
    private float timer = 0f;
    private bool isOrbiting = true;

    void Start()
    {
        // Hitung angle awal kamera relatif ke target (dari atas)
        Vector3 flatDirection = transform.position - target.position;
        flatDirection.y = 0;

        angle = Mathf.Atan2(flatDirection.x, flatDirection.z) * Mathf.Rad2Deg;

        // Optional: pastikan ketinggian kamera sesuai setting
        transform.position = new Vector3(transform.position.x, target.position.y + height, transform.position.z);
        transform.LookAt(target.position);
    }

    void Update()
    {
        if (!isOrbiting) return;

        timer += Time.deltaTime;
        angle += orbitSpeed * Time.deltaTime;

        float rad = angle * Mathf.Deg2Rad;
        float x = Mathf.Sin(rad) * radius;
        float z = Mathf.Cos(rad) * radius;

        transform.position = target.position + new Vector3(x, height, z);
        transform.LookAt(target.position);

        if (timer >= orbitDuration)
        {
            isOrbiting = false;
        }
    }
}
