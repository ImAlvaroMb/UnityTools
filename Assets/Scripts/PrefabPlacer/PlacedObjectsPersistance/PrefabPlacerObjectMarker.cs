using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
public class PrefabPlacerObjectMarker : MonoBehaviour
{
    public ScriptableObject trackingSO;
    public string prefabGUID;
    public string instanceID;

    private void OnDestroy()
    {
        if(!EditorApplication.isPlayingOrWillChangePlaymode && trackingSO != null)
        {
            
        }
    }
}

#endif