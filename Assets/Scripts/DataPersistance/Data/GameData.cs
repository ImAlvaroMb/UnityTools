using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[System.Serializable]
public class GameData 
{
    [Header("CheckPoints && respawn")]
    public long lastUpdated;
    public int currentScene;
    public int currentCheckPoint;
    public Vector3 currentPosition;
    public List<Vector3> levelCheckPoints;
    [Header("Collectables")]
    public SerializableDictionary<string, bool> gameColectables;
    [Header("Porgress")]
    public float currentProgress;

    public GameData()
    {
        this.currentScene = 1;
        this.currentCheckPoint = 0;
        gameColectables = new SerializableDictionary<string, bool>();
        levelCheckPoints = new List<Vector3>();
        currentProgress = 0;
    }

    public float getPercentageComplete()
    {
        float progress = 90 / 5 * currentScene;
        progress = progress + currentProgress;
        return progress;
    }
}
