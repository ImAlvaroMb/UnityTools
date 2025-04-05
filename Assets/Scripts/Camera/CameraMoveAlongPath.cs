using Cinemachine;
using UnityEngine;

public class CameraMoveAlongPath : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private CinemachinePathBase path; // Assign your CinemachinePath/SmoothPath

    [Header("Settings")]
    [SerializeField] private float speed = 0.5f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool easeBetweenPoints = true;

    private CinemachineTrackedDolly dolly;
    private float currentWaypoint = 0;
    private float segmentProgress = 0f; // Progress between current and next waypoint

    void Start()
    {
        if (virtualCamera == null || path == null)
        {
            Debug.LogError("Assign Virtual Camera and Path!");
            return;
        }

        dolly = virtualCamera.GetCinemachineComponent<CinemachineTrackedDolly>();
        if (dolly == null)
        {
            Debug.LogError("Virtual Camera needs Tracked Dolly!");
            return;
        }

        dolly.m_Path = path; // Force-assign path (optional)
        currentWaypoint = 0;
        segmentProgress = 0f;
    }

    void Update()
    {
        if (dolly == null || path == null) return;

        // Calculate total waypoints (excludes closing loop point if path is closed)
        float waypointCount = path.PathLength;
        if (waypointCount < 2) return; // Need at least 2 points

        // Get current and next waypoint indices
        float nextWaypoint = (currentWaypoint + 1) % waypointCount;

        // Move progress between current and next waypoint
        float adjustedSpeed = speed;
        if (easeBetweenPoints)
        {
            // Smooth speed (slow in/out between points)
            adjustedSpeed = Mathf.Lerp(speed * 0.3f, speed, Mathf.PingPong(segmentProgress, 0.5f));
        }

        segmentProgress += adjustedSpeed * Time.deltaTime;

        // Check if we reached the next waypoint
        if (segmentProgress >= 1f)
        {
            segmentProgress = 0f;
            currentWaypoint = nextWaypoint;

            // Handle path end (loop or stop)
            if (!loop && currentWaypoint == 0)
            {
                enabled = false; // Stop script at last point
                return;
            }
        }

        // Calculate the actual path position (0-1 normalized)
        float normalizedPosition = (currentWaypoint + segmentProgress) / waypointCount;
        dolly.m_PathPosition = normalizedPosition;
    }
}
