using UnityEngine;

public class BezierPath : MonoBehaviour
{
    public Transform pointA; // Titik awal
    public Transform pointB; // Titik akhir
    public Transform controlPointA; // Titik kontrol pertama
    public Transform controlPointB; // Titik kontrol kedua

    public int numberOfPoints = 10; // Jumlah titik sepanjang path

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pointA.position, controlPointA.position);
        Gizmos.DrawLine(controlPointA.position, controlPointB.position);
        Gizmos.DrawLine(controlPointB.position, pointB.position);

        // Gambar titik-titik pada path Bezier
        for (float t = 0; t <= 1; t += 1f / numberOfPoints)
        {
            Vector3 p = CalculateBezierPoint(t);
            Gizmos.DrawSphere(p, 0.1f);
        }
    }

    public Vector3 CalculateBezierPoint(float t)
    {
        // Kurva Bezier kuadrat
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * pointA.position; // Pertama, PointA
        p += 2 * u * t * controlPointA.position; // Kontrol pertama
        p += tt * controlPointB.position; // Kontrol kedua
        p += tt * pointB.position; // Terakhir, PointB

        return p;
    }
}
