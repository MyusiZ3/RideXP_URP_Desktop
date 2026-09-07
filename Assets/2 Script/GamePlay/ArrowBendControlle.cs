using UnityEngine;

[ExecuteAlways] // Biar jalan di editor juga
public class ArrowBendController : MonoBehaviour
{
    [Header("Bend Settings")]
    [Tooltip("Sudut kelengkungan dalam derajat (misal 45 untuk belokan kanan)")]
    public float bendAngle = 45f;

    [Tooltip("Radius lengkungan. Makin kecil = makin tajam belokannya")]
    public float bendRadius = 5f;

    [Tooltip("Update otomatis saat nilai berubah di editor")]
    public bool autoUpdate = true;

    void OnValidate()
    {
        if (autoUpdate)
        {
            BendArrow();
        }
    }

    void Reset()
    {
        BendArrow();
    }

    public void BendArrow()
    {
        int childCount = transform.childCount;

        if (childCount <= 1) return;

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            float t = i / (float)(childCount - 1); // nilai dari 0 ke 1
            float angle = Mathf.Lerp(0, bendAngle, t);
            float rad = Mathf.Deg2Rad * angle;

            float x = Mathf.Sin(rad) * bendRadius;
            float z = Mathf.Cos(rad) * bendRadius;

            // Posisikan anak
            child.localPosition = new Vector3(x, child.localPosition.y, z);

            // Rotasi menghadap ke belokan
            child.localRotation = Quaternion.Euler(0, angle, 0);
        }
    }
}
