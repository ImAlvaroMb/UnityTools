using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProfilePlacedObjectsTracker", menuName = "Tools/PrefabPlacer/ProfilePlacedObjectsTracker")]
public class ProfilePlacedObjectsTracker : MonoBehaviour
{
    public List<PlacementData> placedPrefabs = new List<PlacementData>();

    public void AddPlacement(PlacementData data)
    {
        if(!placedPrefabs.Exists(p => p.UniqueID == data.UniqueID))
            placedPrefabs.Add(data);
    }

    public void RemoveByUniqueID(string id)
    {
        placedPrefabs.RemoveAll(p => p.UniqueID == id); // remove all just as a safeguard
    }
}
