using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
public class ToggleCanvas : EditorWindow
{
    [MenuItem("Tools/Toggle Canvas")]
    public static void ToggleVolume()
    {
        Canvas[] canvas = FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (Canvas obj in canvas)
        {
            bool isActive = obj.gameObject.activeSelf;
            obj.gameObject.SetActive(!isActive);
            Debug.Log($"All canvas elements count: {canvas.Length} being set to {isActive}");
        }

    }
}
#endif