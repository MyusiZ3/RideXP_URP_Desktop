using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ParticlePathPlacer : MonoBehaviour
{
    // Referensi ke objek Ramspline yang telah diassign di scene
    public GameObject ramSpline;

    // Prefab untuk partikel debu dan daun
    public GameObject dustPrefab;
    public GameObject leafPrefab;
    
    // Pengaturan jumlah partikel
    public int particleCount = 50;

    // Menjaga agar partikel tetap ada setelah game berhenti
    private bool particlesPlaced = false;

    // Start is called before the first frame update
    void Start()
    {
        // Tidak menempatkan partikel langsung saat scene start
    }

    // Fungsi untuk menempatkan partikel ketika tombol di klik
    public void PlaceParticlesOnButtonClick()
    {
        if (particlesPlaced)
        {
            Debug.Log("Particles already placed.");
            return; // Partikel sudah ditempatkan, tidak melakukan apa-apa
        }

        // Mendapatkan komponen Ramspline dari objek
        var spline = ramSpline.GetComponent<RamSpline>();

        if (spline == null)
        {
            Debug.LogError("Ramspline not found on the assigned object.");
            return;
        }

        // Menempatkan partikel sepanjang jalur spline
        PlaceParticles(spline);
        particlesPlaced = true; // Menandai bahwa partikel sudah ditempatkan
    }

    void PlaceParticles(RamSpline spline)
    {
        // Cek apakah RamSpline memiliki array/titik spline
        if (spline.points.Count == 0)
        {
            Debug.LogError("No spline points found in Ramspline.");
            return;
        }

        // Menempatkan partikel sepanjang jalur spline
        for (int i = 0; i < particleCount; i++)
        {
            // Normalisasi nilai t (0 hingga 1) untuk mengambil posisi sepanjang spline
            float t = (i + 1) / (float)(particleCount + 1);

            // Menghitung index pada titik spline berdasarkan t
            int index = Mathf.FloorToInt(t * (spline.points.Count - 1));
            Vector3 position = spline.points[index]; // Mengakses titik spline berdasarkan index

            // Acak pemilihan antara debu atau daun
            GameObject particlePrefab = Random.value > 0.5f ? dustPrefab : leafPrefab;

            // Menempatkan prefab partikel pada posisi yang dihitung
            Instantiate(particlePrefab, position, Quaternion.identity, transform);
        }
    }

    #if UNITY_EDITOR
    // Menambahkan tombol ke Inspector
    [CustomEditor(typeof(ParticlePathPlacer))]
    public class ParticlePathPlacerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Memanggil default Inspector
            DrawDefaultInspector();

            ParticlePathPlacer script = (ParticlePathPlacer)target;

            // Menambahkan tombol "Generate Particles"
            if (GUILayout.Button("Generate Particles"))
            {
                script.PlaceParticlesOnButtonClick();
            }
        }
    }
    #endif
}
