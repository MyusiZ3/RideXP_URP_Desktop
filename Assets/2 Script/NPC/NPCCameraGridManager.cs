// NPCCameraGridManager.cs
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Mengatur beberapa kamera untuk ditampilkan dalam format grid di layar.
/// Ideal untuk menampilkan view dari beberapa NPC secara bersamaan.
/// Pasang skrip ini pada satu GameObject kosong di scene (misalnya, "CameraGridManager").
/// </summary>
public class NPCCameraGridManager : MonoBehaviour
{
    [Header("Referensi Kamera")]
    [Tooltip("Daftar semua komponen Kamera dari NPC yang ingin ditampilkan di grid. Seret dari Hierarchy.")]
    public List<Camera> npcCameras;

    [Tooltip("Kamera utama Player. Jika kosong, akan mencoba mencari kamera dengan tag 'MainCamera'.")]
    public Camera mainPlayerCamera;

    [Header("Pengaturan Grid Layout")]
    [Tooltip("Jumlah kolom untuk grid kamera.")]
    [Range(1, 5)]
    public int gridColumns = 2;

    [Tooltip("Ukuran setiap view kamera dalam persentase layar (0.0 - 1.0).")]
    public Vector2 cameraViewSize = new Vector2(0.2f, 0.2f);

    [Tooltip("Jarak antar view kamera dalam persentase layar.")]
    public Vector2 padding = new Vector2(0.01f, 0.01f);

    [Tooltip("Posisi jangkar grid di layar.")]
    public ScreenAnchor anchor = ScreenAnchor.TopRight;

    [Header("Kontrol Tampilan")]
    [Tooltip("Centang untuk menampilkan atau menyembunyikan grid kamera NPC.")]
    public bool showNpcGrid = true;

    // Enum untuk pilihan posisi jangkar di layar
    public enum ScreenAnchor
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    void Start()
    {
        // Cari kamera utama jika belum di-assign
        if (mainPlayerCamera == null)
        {
            mainPlayerCamera = Camera.main;
            if (mainPlayerCamera == null)
            {
                Debug.LogError("[NPCCameraGridManager] Kamera utama (MainCamera) tidak ditemukan! Pastikan ada kamera dengan tag 'MainCamera'.", this);
                enabled = false;
                return;
            }
        }
        
        // Atur layout kamera saat game dimulai
        UpdateCameraLayout();
    }

    // Dipanggil di Editor setiap kali nilai di Inspector diubah
    private void OnValidate()
    {
        // Memastikan nilai tidak negatif
        cameraViewSize.x = Mathf.Max(0, cameraViewSize.x);
        cameraViewSize.y = Mathf.Max(0, cameraViewSize.y);
        padding.x = Mathf.Max(0, padding.x);
        padding.y = Mathf.Max(0, padding.y);
        
        // Panggil UpdateCameraLayout agar perubahan di Inspector bisa langsung terlihat di Game view
        // Ini sangat membantu saat tuning posisi dan ukuran.
        UpdateCameraLayout();
    }

    /// <summary>
    /// Fungsi utama untuk menghitung dan menerapkan posisi serta ukuran viewport untuk setiap kamera NPC.
    /// </summary>
    public void UpdateCameraLayout()
    {
        if (mainPlayerCamera == null || npcCameras == null || npcCameras.Count == 0)
        {
            return;
        }

        // Pastikan kamera utama render di paling bawah (depth rendah)
        mainPlayerCamera.depth = -1; // Menggunakan -1 agar lebih aman

        Vector2 startPosition = GetAnchorPosition();

        for (int i = 0; i < npcCameras.Count; i++)
        {
            Camera cam = npcCameras[i];
            if (cam == null)
            {
                Debug.LogWarning($"[NPCCameraGridManager] Ada kamera null di daftar pada indeks {i}.");
                continue;
            }

            // Jika grid tidak ditampilkan, nonaktifkan kamera NPC
            if (!showNpcGrid)
            {
                // Cukup nonaktifkan komponen Kamera, bukan seluruh GameObject NPC
                cam.enabled = false;
                continue;
            }

            cam.enabled = true;

            // Pastikan depth kamera NPC lebih tinggi dari kamera utama agar render di atasnya
            cam.depth = mainPlayerCamera.depth + 1 + i; // Beri depth berbeda agar urutan render konsisten

            // Hitung posisi baris dan kolom
            int row = i / gridColumns;
            int col = i % gridColumns;

            // Hitung posisi x dan y untuk rect viewport
            // (x, y) di Rect adalah pojok kiri bawah
            float xPos, yPos;

            if (anchor == ScreenAnchor.TopRight || anchor == ScreenAnchor.BottomRight)
            {
                // Jajar ke kanan
                xPos = startPosition.x - (col * (cameraViewSize.x + padding.x)) - cameraViewSize.x;
            }
            else // Jajar ke kiri
            {
                xPos = startPosition.x + (col * (cameraViewSize.x + padding.x));
            }

            if (anchor == ScreenAnchor.TopRight || anchor == ScreenAnchor.TopLeft)
            {
                // Jajar ke atas
                yPos = startPosition.y - (row * (cameraViewSize.y + padding.y)) - cameraViewSize.y;
            }
            else // Jajar ke bawah
            {
                yPos = startPosition.y + (row * (cameraViewSize.y + padding.y));
            }
            
            // Terapkan viewport rect baru ke kamera
            cam.rect = new Rect(xPos, yPos, cameraViewSize.x, cameraViewSize.y);
        }
    }

    /// <summary>
    /// Mendapatkan posisi awal (pojok) untuk grid berdasarkan pilihan anchor.
    /// </summary>
    private Vector2 GetAnchorPosition()
    {
        switch (anchor)
        {
            case ScreenAnchor.TopLeft:
                return new Vector2(0, 1);
            case ScreenAnchor.TopRight:
                return new Vector2(1, 1);
            case ScreenAnchor.BottomLeft:
                return new Vector2(0, 0);
            case ScreenAnchor.BottomRight:
                return new Vector2(1, 0);
            default:
                return new Vector2(1, 1); // Default ke TopRight
        }
    }

    /// <summary>
    /// Metode publik untuk menyembunyikan atau menampilkan grid kamera.
    /// Bisa dipanggil dari UI Button atau skrip lain.
    /// </summary>
    public void ToggleGridVisibility(bool isVisible)
    {
        showNpcGrid = isVisible;
        UpdateCameraLayout();
    }
}

