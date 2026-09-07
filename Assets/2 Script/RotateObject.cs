using UnityEngine;

public class RotateObject : MonoBehaviour
{
    // Kecepatan rotasi (derajat per detik)
    public float rotationSpeed = 30f;

    // Arah rotasi dalam vektor (misalnya, (1, 0, 0) untuk rotasi sumbu X)
    public Vector3 rotationAxis = new Vector3(0, 1, 0); // Default rotasi di sekitar sumbu Y

    // Menentukan apakah rotasi dilakukan secara lokal atau global
    public bool useLocalRotation = true;

    void Update()
    {
        // Rotasi objek berdasarkan kecepatan rotasi dan arah rotasi
        if (useLocalRotation)
        {
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self); // Rotasi lokal
        }
        else
        {
            transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.World); // Rotasi global
        }
    }
}
