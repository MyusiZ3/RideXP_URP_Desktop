// using UnityEngine;
// using System.Collections.Generic;

// public class WaypointManager : MonoBehaviour
// {
//     public RamSpline ramSpline; // Assign dari Inspector
//     public List<Vector3> waypoints = new List<Vector3>();

//     void Awake()
//     {
//         GenerateWaypointsFromSpline();
//     }

//     void GenerateWaypointsFromSpline()
//     {
//         if (ramSpline == null)
//         {
//             Debug.LogError("RAM Spline belum di-assign!");
//             return;
//         }

//         waypoints.Clear();
//         for (int i = 0; i < ramSpline.points.Count; i++)
//         {
//             waypoints.Add(ramSpline.points[i]);
//         }
//     }
// }
// =============================
// ✅ WaypointManager.cs (Versi Lengkap)
// =============================

using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    [Header("Spline yang dipakai sebagai jalur")]
    public RamSpline ramSpline; // Assign dari Inspector

    [Header("Waypoint hasil generate dari spline")]
    public List<Vector3> waypoints = new List<Vector3>();

    void Awake()
    {
        GenerateWaypointsFromSpline();
    }

    public void GenerateWaypointsFromSpline()
    {
        if (ramSpline == null)
        {
            Debug.LogError("RAM Spline belum di-assign!");
            return;
        }

        waypoints.Clear();
        for (int i = 0; i < ramSpline.points.Count; i++)
        {
            waypoints.Add(ramSpline.points[i]);
        }
    }

    // Bisa dipanggil ulang kalau spline berubah
    public void RefreshWaypoints() => GenerateWaypointsFromSpline();
}
