using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;

public class DataPersistanceManager : MonoBehaviour
{

    [Header("Debugging")]
    [SerializeField] private bool initDataIfNull = false;
    [SerializeField] private bool disableDataPersitance = false;
    [SerializeField] private bool overrideSelectedProfileId = false;
    [SerializeField] private string testSelectedProfileId = "test";

    [Header("File Storage Config")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    public static DataPersistanceManager instance { get; private set; }

    private GameData gameData;

    private List<IDataPersistance> dataPersistanceObject;

    private FileDataHandler dataHandler;
    private string selectedProfileId = "";

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log("More than 1 dataPersistance in the scene, destroying the newest one");
            Destroy(this.gameObject);
            return;
        }
        instance = this;

        DontDestroyOnLoad(this.gameObject);

        if (disableDataPersitance)
        {
            Debug.LogWarning("Data persistancew is currently disabled");
        }
        AssertFolderExistance();
        this.dataHandler = new FileDataHandler(GetApplicationDataPath(), fileName, useEncryption);// gets the operating sys standard directory for persisting data in unity proj
        InitializeSelectedProfileId();
    }


    private string GetApplicationDataPath()
    {
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData) + "/" + Application.companyName + "/" + Application.productName;

    }
    private void AssertFolderExistance()
    {
        if (!Directory.Exists(GetApplicationDataPath()))
        {
            Directory.CreateDirectory(GetApplicationDataPath());
        }
    }

    private void OnEnable() // should be called before start so that we save or load properly
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        this.dataPersistanceObject = FindAllDataPersistanceObjects();
        LoadGame();
    }

    public void OnSceneUnloaded(Scene scene)
    {
        SaveGame(); // change this method of saving to a different one, recommended saving before changing a scene, since if we use this method it ill probably trigger null reference exepcitons
    }

    public void ChangeSelectedProfile(string newProfileId)
    {
        this.selectedProfileId = newProfileId;
        LoadGame(); // ensure that the game data is updated 
    }

    public void NewGame()
    {
        this.gameData = new GameData();
    }

    public void LoadGame()
    {
        if (disableDataPersitance)
        {
            return;
        }

        this.gameData = dataHandler.Load(selectedProfileId);

        if (this.gameData == null && initDataIfNull)
        {
            NewGame();
        }

        if (this.gameData == null)
        {
            Debug.Log("No data was found, a new game needs to be started");
            return;
        }

        //push the loaded data to the objects that need it
        foreach (IDataPersistance dataPersistanceObj in dataPersistanceObject)
        {
            dataPersistanceObj.LoadData(gameData);
        }
    }

    public void SaveGame()
    {
        if (disableDataPersitance)
        {
            return;
        }

        if (this.gameData == null)
        {
            Debug.Log("No data was found, a new game needs to be created");
            return;
        }

        //pass the data to other script to update & update the saving time
        foreach (IDataPersistance dataPersistanceObj in dataPersistanceObject)
        {
            dataPersistanceObj.SaveData(gameData);
        }
        gameData.lastUpdated = System.DateTime.Now.ToBinary();
        dataHandler.Save(gameData, selectedProfileId);
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private List<IDataPersistance> FindAllDataPersistanceObjects()
    {
        IEnumerable<IDataPersistance> dataPersistacenObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistance>();

        return new List<IDataPersistance>(dataPersistacenObjects);
    }
    public void ChangeSelectedProfileId(string newProfileId)
    {
        // update the profile to use for saving and loading
        this.selectedProfileId = newProfileId;
        // load the game, which will use that profile, updating our game data accordingly
        LoadGame();
    }


    public void DeleteProfileData(string profileId)
    {
        dataHandler.Delete(profileId);

        InitializeSelectedProfileId();
        LoadGame();
    }

    private void InitializeSelectedProfileId()
    {
        this.selectedProfileId = dataHandler.getMostRecentlyUpdatedPorfile();
        if (overrideSelectedProfileId)
        {
            this.selectedProfileId = testSelectedProfileId;
            Debug.LogWarning("Overrode selected profile");
        }
    }

    public bool HasGameData()
    {
        return this.gameData != null;
    }

    public Dictionary<string, GameData> GetAllProfilesData()
    {
        return dataHandler.LoadAllProfiles(); // todo create save slots on menu
    }

    public int getActiveScene()
    {
        return gameData.currentScene;
    }
}
