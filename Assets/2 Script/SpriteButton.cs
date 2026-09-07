using UnityEngine;

public class SpriteButton : MonoBehaviour
{
    public Sprite defaultSprite;   // Gambar default (misalnya gambar sprite normal)
    public Sprite clickedSprite;   // Gambar saat button diklik

    private SpriteRenderer spriteRenderer;  // Komponen SpriteRenderer untuk mengubah sprite

    void Start()
    {
        // Ambil komponen SpriteRenderer dari objek
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set sprite awal (default)
        spriteRenderer.sprite = defaultSprite;
    }

    void Update()
    {
        // Deteksi klik mouse
        if (Input.GetMouseButtonDown(0))  // Klik kiri mouse
        {
            // Buat ray dari posisi klik
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            // Cek apakah ray mengenai objek ini
            if (hit.collider != null && hit.collider.transform == transform)
            {
                OnButtonClicked();  // Jalankan fungsi saat button diklik
            }
        }
    }

    // Fungsi yang dijalankan saat button diklik
    void OnButtonClicked()
    {
        // Ganti sprite saat button diklik
        spriteRenderer.sprite = clickedSprite;
        Debug.Log("Button clicked!");
        
        // Tambahkan aksi lain yang ingin dijalankan setelah klik, misalnya:
        // Perform some action, like opening a menu, starting a game, etc.
    }
}
