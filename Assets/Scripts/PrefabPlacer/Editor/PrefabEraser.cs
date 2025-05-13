using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PrefabEraser : IPrefabPlacerMode
{
    private ProfilePlacedObjectsTrackerSO activeTrackerSO;
    public float eraserRadius;
    private bool isErasing;
    private bool doLogic = true;
    public void OnModeActivated()
    {
        throw new System.NotImplementedException();
    }

    public void OnModeDeactivated()
    {
        throw new System.NotImplementedException();
    }

    public void StartErasing(ProfilePlacedObjectsTrackerSO trackerSO)
    {
        activeTrackerSO = trackerSO;
        isErasing = true;
        doLogic = true;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    public void StopErasing()
    {
        isErasing = false;
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!doLogic) return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Handles.color = Color.red;
            Handles.DrawWireDisc(hit.point, hit.normal, eraserRadius);

            if (e.type == EventType.MouseDown && e.button == 0) // Left Mouse Button Click
            {
                EraseObjects(hit.point);
                e.Use(); // Mark the event as used to prevent propagation
            }
        }

        sceneView.Repaint();

        if(!isErasing)
            doLogic = false;
    }

    private void EraseObjects(Vector3 position)
    {
        if (activeTrackerSO == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(position, eraserRadius);
        List<GameObject> objectsToRemove = new List<GameObject>();

        foreach (var hit in hitColliders)
        {
            if(hit.gameObject.TryGetComponent(out PrefabPlacerObjectMarker marker))
            {
                if(activeTrackerSO.ContainsByUniqueID(marker.uniqueID))
                {
                    objectsToRemove.Add(hit.gameObject);
                }
            }
        }

        foreach (var obj in objectsToRemove)
        {
            activeTrackerSO.RemoveByUniqueID(obj.GetComponent<PrefabPlacerObjectMarker>().uniqueID);
            Undo.DestroyObjectImmediate(obj); // use Undo for better editor undo functionality
        }
    }
}
