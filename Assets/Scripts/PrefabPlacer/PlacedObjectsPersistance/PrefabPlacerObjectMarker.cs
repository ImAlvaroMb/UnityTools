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
        if(!EditorApplication.isPlayingOrWillChangePlaymode && trackingSO != null)
        {
            trackingSO.RemoveByUniqueID(uniqueID);
            EditorUtility.SetDirty(trackingSO); // save the SO so this actually persists
        }
    }
}

#endif