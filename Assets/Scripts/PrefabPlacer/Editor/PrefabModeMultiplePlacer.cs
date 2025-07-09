using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;
using Unity.Mathematics;

public class PrefabModeMultiplePlacer : MonoBehaviour, IPrefabPlacerMode
{
    private ProfilePlacedObjectsTrackerSO activeTrackerSO;
    private bool isPlacing;
    private List<GameObject> targetPrefabsList = new List<GameObject>();
    public float placingRadius;
    public float density;
    public Vector2 scaleValues = Vector2.one;

    public void OnModeActivated(ProfilePlacedObjectsTrackerSO trackerSO)
    {
        activeTrackerSO = trackerSO;
        isPlacing = true;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    public void OnModeDeactivated()
    {
        isPlacing = false;
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public void StartPlacing(List<GameObject> prefabs, ProfilePlacedObjectsTrackerSO trackerSO)
    {
        if (prefabs == null) return;

        activeTrackerSO = trackerSO;
        isPlacing = true;
        targetPrefabsList.Clear();
        targetPrefabsList = prefabs;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    public void StopPlacing()
    {
        isPlacing = false;
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public void UpdatePrefabSelection(List<GameObject> prefabs, ProfilePlacedObjectsTrackerSO trackerSO)
    {
        if(prefabs == null)
        {
            targetPrefabsList.Clear();
            return;
        }
        activeTrackerSO = trackerSO; 
        targetPrefabsList = prefabs;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPlacing) return;

        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlID);

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Handles.color = new Color(0, 1, 0, 0.1f); 
            Handles.DrawSolidDisc(hit.point, hit.normal, placingRadius);
            Handles.color = Color.green;
            Handles.DrawWireDisc(hit.point, hit.normal, placingRadius);

 
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PlaceObjects(hit.point, hit.normal);
                e.Use(); 
            }
        }

        sceneView.Repaint();
    }

    private void PlaceObjects(Vector3 centerPosition, Vector3 surfaceNormal)
    {
        if (activeTrackerSO == null || targetPrefabsList == null || targetPrefabsList.Count == 0)
        {
            Debug.LogWarning("No hay prefabs seleccionados para colocar.");
            return;
        }

        // decide how many objects are goiong to be instantiated
        int objectCount = Mathf.RoundToInt(density * placingRadius / 5f);

        for (int i = 0; i < objectCount; i++)
        {
            // 
            GameObject prefabToPlace = targetPrefabsList[UnityEngine.Random.Range(0, targetPrefabsList.Count)];

            // random position while checking if it is possible to instantiate it somewhere
            Vector2 randomPointInCircle = UnityEngine.Random.insideUnitCircle * placingRadius;
            Vector3 spawnPosition = centerPosition + new Vector3(randomPointInCircle.x, 0, randomPointInCircle.y);

            RaycastHit placementHit;
            if (Physics.Raycast(spawnPosition + surfaceNormal * 5f, -surfaceNormal, out placementHit))
            {
                spawnPosition = placementHit.point;
            }
            else
            {
                continue;
            }

            Quaternion spawnRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0);

            GameObject newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPlace);
            newObject.transform.position = spawnPosition;
            newObject.transform.rotation = spawnRotation;
            newObject.transform.localScale = Vector3.one * UnityEngine.Random.Range(scaleValues.x, scaleValues.y);

            Undo.RegisterCreatedObjectUndo(newObject, "Place Multiple Prefabs");

            PrefabPlacerObjectMarker marker = newObject.AddComponent<PrefabPlacerObjectMarker>();
            marker.uniqueID = Guid.NewGuid().ToString();
            activeTrackerSO.AddPlacement(new PlacementData
            {
                UniqueID = marker.uniqueID,
                prefab = newObject,
                position = newObject.transform.position,
                rotation = newObject.transform.rotation,
                scale = newObject.transform.localScale
            });

            EditorUtility.SetDirty(activeTrackerSO);
            
        }
        //StopPlacing();
    }
}
