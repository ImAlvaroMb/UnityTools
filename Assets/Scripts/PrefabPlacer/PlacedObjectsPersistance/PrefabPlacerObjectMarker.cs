using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class PrefabPlacerObjectMarker : MonoBehaviour
{
    public ProfilePlacedObjectsTrackerSO trackingSO;
    public string uniqueID;



    private void OnDestroy()
    {
        if (!EditorApplication.isPlayingOrWillChangePlaymode && trackingSO != null)
        {
            trackingSO.RemoveByUniqueID(uniqueID);
            EditorUtility.SetDirty(trackingSO); // save the SO so this actually persists
        }
    }
}


public class PrefabPlacerObjectBoundsDefinition : MonoBehaviour
{

    public Mesh meshToUseForBounds;
    public float boundsSphereRadius;

    public Bounds GetBounds()
    {
        if (meshToUseForBounds == null)
        {
            return new Bounds(transform.position, new Vector3(boundsSphereRadius, boundsSphereRadius, boundsSphereRadius));
        }
        return meshToUseForBounds.bounds; 
    }
}

#endif