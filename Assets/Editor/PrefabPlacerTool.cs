using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class PrefabPlacerTool : EditorWindow
{
    private List<string> prefabProfiles = new List<string>();
    private int selectedProfileIndex = 0;
    private GameObject[] prefabs;
    private int selectedPrefabIndex = 0;
    private GameObject previewInstance;

    private const string PROFILE_FOLDER_PATH = "Assets/PrefabProfiles/"; //where all the profiles / prefabs data will be saved
    private const string PREFAB_PROFILES_KEY = "PrefabPlacerProfiles"; //editor prefs persistance key

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
            selectedProfileIndex = Mathf.Clamp(selectedProfileIndex, 0, prefabProfiles.Count - 1);
            int newSelectedIndex = EditorGUILayout.Popup("Select Profile", selectedProfileIndex, prefabProfiles.ToArray());
            if (newSelectedIndex != selectedProfileIndex)
            {
                selectedProfileIndex = newSelectedIndex;
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
                    AssetDatabase.Refresh();
                }
            }
        }

        if (prefabProfiles.Count > 0 && GUILayout.Button("Remove Selected Profile"))
        {
            string profilePath = Path.Combine(PROFILE_FOLDER_PATH, prefabProfiles[selectedProfileIndex]);
            if (EditorUtility.DisplayDialog("Confirm Profile Deletion",
        $"Are you sure you want to delete the profile \"{prefabProfiles[selectedProfileIndex]}\" and all its prefabs?",
        "Delete", "Cancel"))
            {
                if (Directory.Exists(profilePath))
                {
                    Directory.Delete(profilePath, true);
                    string metaFilePath = profilePath + ".meta";
                    if (File.Exists(metaFilePath))
                    {
                        File.Delete(metaFilePath);
                    }
                }
                prefabProfiles.RemoveAt(selectedProfileIndex);
                SavePrefabProfiles();
                AssetDatabase.Refresh();
            }
        }

        EditorGUILayout.Space();

        
        if(prefabProfiles.Count > 0)
        {
            if (prefabs.Length == 0 || prefabs == null)
            {
                EditorGUILayout.LabelField("No prefabs found.");
            }
            else
            {
                selectedPrefabIndex = EditorGUILayout.Popup("Select Prefab", selectedPrefabIndex, System.Array.ConvertAll(prefabs, prefab => prefab.name));
            }

            if (GUILayout.Button("Add Prefab to Profile"))
            {
                string prefabPath = EditorUtility.OpenFilePanel("Select Prefab", Application.dataPath, "prefab");
                if (!string.IsNullOrEmpty(prefabPath))
                {
                    string relativePath = "Assets" + prefabPath.Replace(Application.dataPath, "").Replace("\\", "/");

                    //ensure the profile directory exists
                    string profilePath = Path.Combine(PROFILE_FOLDER_PATH, prefabProfiles[selectedProfileIndex]);
                    if (!Directory.Exists(profilePath))
                    {
                        Directory.CreateDirectory(profilePath);
                    }

                    //save reference to a text file
                    string profileFilePath = Path.Combine(profilePath, "prefabs.txt");
                    File.AppendAllLines(profileFilePath, new[] { relativePath });

                    LoadPrefabs(); //refresh the prefab list
                    AssetDatabase.Refresh();
                }
            }

            if (GUILayout.Button("Delete Prefab from Selected Profile"))
            {
                if (prefabs.Length > 0 && selectedPrefabIndex >= 0 && selectedPrefabIndex < prefabs.Length)
                {
                    string prefabPath = AssetDatabase.GetAssetPath(prefabs[selectedPrefabIndex]);
                    if (!string.IsNullOrEmpty(prefabPath))
                    {
                        if (EditorUtility.DisplayDialog("Confirm Deletion",
                            $"Are you sure you want to remove the prefab \"{Path.GetFileName(prefabPath)}\" from this profile?",
                            "Remove", "Cancel"))
                        {
                            string profilePath = Path.Combine(PROFILE_FOLDER_PATH, prefabProfiles[selectedProfileIndex]);
                            string profileFilePath = Path.Combine(profilePath, "prefabs.txt");

                            if (File.Exists(profileFilePath))
                            {
                                var lines = new List<string>(File.ReadAllLines(profileFilePath));
                                lines.Remove(prefabPath);
                                File.WriteAllLines(profileFilePath, lines);
                            }

                            LoadPrefabs(); // Refresh the list after deletion
                            AssetDatabase.Refresh();
                        }
                    }
                }
            }

            if (GUILayout.Button("Place Prefab in Scene"))
            {
                StartPlacingPrefab();
            }
        }
        

    }

    private void LoadPrefabs()
    {
        if (prefabProfiles.Count == 0) return;

        string profilePath = Path.Combine(PROFILE_FOLDER_PATH, prefabProfiles[selectedProfileIndex]);
        string profileFilePath = Path.Combine(profilePath, "prefabs.txt");

        if (!File.Exists(profileFilePath)) //if there are no prefabs initialize the prefabs array to avoid errors
        {
            prefabs = new GameObject[0];
            return;
        }

        string[] prefabPaths = File.ReadAllLines(profileFilePath);
        List<GameObject> loadedPrefabs = new List<GameObject>();

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                loadedPrefabs.Add(prefab);
            }
        }

        prefabs = loadedPrefabs.ToArray();
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
        else
        {
            prefabProfiles = new List<string>(); // Ensure it's initialized
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
        //HandleUtility.AddDefaultControl(0);  // Prevent Unity selection box from interfering

        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            previewInstance.transform.position = hit.point;
            previewInstance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
        }

        /*if (e.type == EventType.MouseDrag && e.button == 1) 
        {
            previewInstance.transform.Rotate(Vector3.up, e.delta.x * 0.5f, Space.World);
            e.Use();
        }*/

        if (e.type == EventType.MouseDown && e.button == 0) 
        {
            PlacePrefab();
            SceneView.duringSceneGui -= OnSceneGUI;
            e.Use();
        }

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            CancelPlacingPrefab();
            SceneView.duringSceneGui -= OnSceneGUI;
            e.Use();
        }

        sceneView.Repaint();
    }

    private void PlacePrefab()
    {
        if (prefabs.Length == 0 || selectedPrefabIndex < 0 || selectedPrefabIndex >= prefabs.Length)
            return;

        GameObject prefab = prefabs[selectedPrefabIndex];
        GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject; //intantiate a preview from the saved prefab path

        if (previewInstance != null)
        {
            instance.transform.position = previewInstance.transform.position;
            instance.transform.rotation = previewInstance.transform.rotation;
            DestroyImmediate(previewInstance);
            previewInstance = null;
        }
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



