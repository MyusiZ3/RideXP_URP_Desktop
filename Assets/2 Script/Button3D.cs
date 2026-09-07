using UnityEngine;
using UnityEngine.Events;

public class Button3D : MonoBehaviour
{
    public UnityEvent OnButtonClick;  // Event yang akan dipanggil saat button diklik
    private Renderer buttonRenderer;  // Renderer untuk mengubah warna button
    private Camera mainCamera;

    // Warna untuk efek hover, klik, dan press
    public Color hoverColor = Color.yellow;
    public Color clickColor = Color.green;
    public Color pressColor = Color.blue;
    public Color defaultColor = Color.white;

    void Start()
    {
        mainCamera = Camera.main;  // Ambil kamera utama
        buttonRenderer = GetComponent<Renderer>();  // Ambil komponen Renderer
        buttonRenderer.material.color = defaultColor;  // Set warna awal (default)
    }

    void Update()
    {
        // Deteksi apakah mouse berada di atas button (hover)
        if (IsMouseOver())
        {
            buttonRenderer.material.color = hoverColor;  // Set warna saat hover
        }
        else if (Input.GetMouseButton(0) && IsMouseOver())  // Deteksi klik (mouse ditekan)
        {
            buttonRenderer.material.color = pressColor;  // Set warna saat button ditekan
        }
        else
        {
            buttonRenderer.material.color = defaultColor;  // Set warna default
        }

        // Deteksi klik kiri pada mouse untuk men-trigger event
        if (Input.GetMouseButtonDown(0) && IsMouseOver())
        {
            OnButtonClicked();  // Jalankan event saat button diklik
        }
    }

    // Cek apakah mouse berada di atas objek ini
    bool IsMouseOver()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);  // Membuat ray dari posisi mouse
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))  // Deteksi apakah ray mengenai objek ini
        {
            return hit.transform == transform;  // Cek apakah objek yang diklik adalah button ini
        }

        return false;
    }

    // Fungsi yang dijalankan saat button 3D diklik
    void OnButtonClicked()
    {
        Debug.Log("3D Button Clicked!");  // Log untuk memastikan klik terdeteksi

        // Panggil event yang terhubung di UnityEvent
        OnButtonClick.Invoke();

        // Fungsi untuk membuka main menu (UI)
        OpenMainMenu();
    }

    // Fungsi untuk membuka main menu (UI)
    void OpenMainMenu()
    {
        GameObject mainMenu = GameObject.Find("MainMenu");  // Ganti dengan nama panel main menu di scene kamu
        if (mainMenu != null)
        {
            mainMenu.SetActive(true);  // Menampilkan main menu
        }
    }
}
