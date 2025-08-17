using NUnit.Framework;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabModeSpline : MonoBehaviour, IPrefabPlacerMode
{
    public GameObject prefabToPlace;

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
        
    }

    private void OnSceneGUI(SceneView sceneView)
    {

    }
}
