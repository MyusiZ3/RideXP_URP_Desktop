using UnityEngine;

public class PlaceLeaves : MonoBehaviour
{
    public GameObject leafPrefab; // Prefab partikel daun
    public Transform path; // Jalan yang ingin diikuti
    public float distanceBetweenLeaves = 1f; // Jarak antar daun

    void Start()
    {
        // Ambil total panjang jalan
        float pathLength = path.GetComponent<Collider>().bounds.size.z;
        
        // Tempatkan daun sepanjang jalan
        for (float i = 0; i < pathLength; i += distanceBetweenLeaves)
        {
            Vector3 position = path.position + new Vector3(0, 0, i);
            Instantiate(leafPrefab, position, Quaternion.identity);
        }
    }
}
