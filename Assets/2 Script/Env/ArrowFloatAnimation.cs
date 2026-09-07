using UnityEngine;

public class ArrowFloatAnimation : MonoBehaviour
{
    public float floatAmplitude = 0.2f; // seberapa tinggi geraknya
    public float floatSpeed = 2f;       // seberapa cepat geraknya
    public float scaleAmplitude = 0.05f; // seberapa besar scalingnya
    public float scaleSpeed = 2f;        // seberapa cepat scalingnya
    public float alphaSpeed = 2f;        // seberapa cepat alpha berubah

    private Vector3 initialPosition;
    private Vector3 initialScale;
    private SpriteRenderer sr;

    void Start()
    {
        initialPosition = transform.localPosition;
        initialScale = transform.localScale;
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Gerakan naik-turun
        float y = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = initialPosition + new Vector3(0f, y, 0f);

        // Scaling efek
        float scaleOffset = Mathf.Sin(Time.time * scaleSpeed) * scaleAmplitude;
        transform.localScale = initialScale + new Vector3(scaleOffset, scaleOffset, 0f);

        // Alpha transparan glow efek
        if (sr != null)
        {
            Color color = sr.color;
            color.a = 0.5f + 0.5f * Mathf.Sin(Time.time * alphaSpeed); // antara 0 dan 1
            sr.color = color;
        }
    }
}
