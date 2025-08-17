using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabModeSpline : MonoBehaviour, IPrefabPlacerMode
{
    public GameObject prefabToPlace;
    // create spline class that will contain node points, handles angles, line info...
    // own SO to contain all created splines so rthat already placed spline can be edited...

    private ProfilePlacedObjectsTrackerSO trackerSO;

    private List<Vector3> points = new List<Vector3>();
    private int selectedNodeIndex = -1;
    private bool isPlacing = false;

    private const float NODE_SIZE = 0.15f;
    private const float HANDLE_SIZE = 0.1f;
    public void OnModeActivated(ProfilePlacedObjectsTrackerSO trackerSO)
    {
        isPlacing = true;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    public void OnModeDeactivated()
    {
        isPlacing = false;
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPlacing) return;
    }
}
