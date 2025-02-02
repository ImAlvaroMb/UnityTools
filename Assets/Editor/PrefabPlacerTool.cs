using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class PrefabPlacerTool : EditorWindow
{
    private List<string> prefabProfiles = new List<string>(); // List of profile names
    private int selectedProfileIndex = 0;
    private GameObject[] prefabs;
    private int selectedPrefabIndex = 0;
    private GameObject previewInstance;

    private const string PROFILE_FOLDER_PATH = "Assets/PrefabProfiles/";
    private const string PREFAB_PROFILES_KEY = "PrefabPlacerProfiles";

    [MenuItem("Tools/Prefab Placer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPlacerTool>("Prefab Placer");
    }

    private void OnEnable()
    {
        LoadPrefabProfiles();
        LoadPrefabs();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Prefab Profiles", EditorStyles.boldLabel);

        if (prefabProfiles.Count > 0)
        {
            selectedProfileIndex = EditorGUILayout.Popup("Select Profile", selectedProfileIndex, prefabProfiles.ToArray());
            if (GUILayout.Button("Load Prefabs"))
            {
                LoadPrefabs();
            }
        }
        else
        {
            EditorGUILayout.LabelField("No profiles available.");
        }

        if (GUILayout.Button("Add New Profile"))
        {
            string newProfileName = EditorUtility.SaveFilePanel("Enter Profile Name", PROFILE_FOLDER_PATH, "NewProfile", "");
            if (!string.IsNullOrEmpty(newProfileName))
            {
                newProfileName = Path.GetFileNameWithoutExtension(newProfileName);
                string newProfilePath = Path.Combine(PROFILE_FOLDER_PATH, newProfileName);
                if (!Directory.Exists(newProfilePath))
                {
                    Directory.CreateDirectory(newProfilePath);
                    prefabProfiles.Add(newProfileName);
                    SavePrefabProfiles();
                }
            }
        }

        if (prefabProfiles.Count > 0 && GUILayout.Button("Remove Selected Profile"))
        {
            string profilePath = Path.Combine(PROFILE_FOLDER_PATH, prefabProfiles[selectedProfileIndex]);
            if (Directory.Exists(profilePath))
            {
                Directory.Delete(profilePath, true);
            }
            prefabProfiles.RemoveAt(selectedProfileIndex);
            SavePrefabProfiles();
        }

        EditorGUILayout.Space();

        if (prefabs.Length > 0)
        {
            selectedPrefabIndex = EditorGUILayout.Popup("Select Prefab", selectedPrefabIndex, System.Array.ConvertAll(prefabs, prefab => prefab.name));
        }
        else
        {
            EditorGUILayout.LabelField("No prefabs found.");
        }

        if (GUILayout.Button("Add Prefab to Profile"))
        {
            string prefabPath = EditorUtility.OpenFilePanel("Select Prefab", "Assets", "prefab");
            if (!string.IsNullOrEmpty(prefabPath))
            {
                string destinationPath = Path.Combine(PROFILE_FOLDER_PATH, prefabProfiles[selectedProfileIndex], Path.GetFileName(prefabPath));
                File.Copy(prefabPath, destinationPath, true);
                LoadPrefabs();
            }
        }

        if (GUILayout.Button("Place Prefab in Scene"))
        {
            StartPlacingPrefab();
        }
    }

    private void LoadPrefabs()
    {
        if (prefabProfiles.Count == 0) return;
        string selectedProfilePath = Path.Combine(PROFILE_FOLDER_PATH, prefabProfiles[selectedProfileIndex]);
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { selectedProfilePath });

        prefabs = new GameObject[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            prefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }

    private void SavePrefabProfiles()
    {
        EditorPrefs.SetString(PREFAB_PROFILES_KEY, string.Join(";", prefabProfiles));
    }

    private void LoadPrefabProfiles()
    {
        string savedData = EditorPrefs.GetString(PREFAB_PROFILES_KEY, "");
        if (!string.IsNullOrEmpty(savedData))
        {
            prefabProfiles = new List<string>(savedData.Split(';'));
        }
    }

    private void StartPlacingPrefab()
    {
        if (prefabs.Length == 0 || selectedPrefabIndex < 0 || selectedPrefabIndex >= prefabs.Length)
            return;

        previewInstance = Instantiate(prefabs[selectedPrefabIndex]);
        previewInstance.hideFlags = HideFlags.HideAndDontSave;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (previewInstance == null) return;

        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            previewInstance.transform.position = hit.point;
            previewInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

            if (e.type == EventType.MouseDrag && e.button == 1)
            {
                previewInstance.transform.Rotate(Vector3.up, e.delta.x * 0.5f, Space.World);
            }
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            PlacePrefab();
            SceneView.duringSceneGui -= OnSceneGUI;
            e.Use();
        }
        if (e.type == EventType.MouseDown && e.button == 1 || e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            CancelPlacingPrefab();
            SceneView.duringSceneGui -= OnSceneGUI;
            e.Use();
        }

        sceneView.Repaint();
    }

    private void PlacePrefab()
    {
        if (previewInstance == null) return;

        GameObject finalInstance = PrefabUtility.InstantiatePrefab(prefabs[selectedPrefabIndex]) as GameObject;
        finalInstance.transform.position = previewInstance.transform.position;
        finalInstance.transform.rotation = previewInstance.transform.rotation;

        DestroyImmediate(previewInstance);
        previewInstance = null;
    }

    private void CancelPlacingPrefab()
    {
        if (previewInstance != null)
        {
            DestroyImmediate(previewInstance);
            previewInstance = null;
        }
    }
}


