using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler 
{
    private string dataDirPath = "";
    private string dataFileName = "";
    private bool useEncryption = false;

    private readonly string encryptionCodeWord = "word";
    private readonly string backupExtension = ".bak";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public GameData Load(string profileId, bool allowRestoreFromBackup = true)
    {
        if(profileId == null) // return right away since theres no data to find, avoid throw errors
        {
            return null;
        }

        //gets the Operating Sys default saving path
        string fullPath = Path.Combine(dataDirPath, profileId,  dataFileName);
        GameData loadedData = null;
        if(File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }
                if(useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);

            } catch (Exception e) 
            {

                // try to reread data in case rollback succes but data is failing to load
                if(allowRestoreFromBackup)
                {
                    Debug.Log(e);
                    bool rollbackSucces = AttemptRollback(fullPath);
                    if (rollbackSucces)
                    {
                        //try to load data again
                        loadedData = Load(profileId, false);
                    }
                } else
                {
                    Debug.Log("Eror when trying tyo laod file" + fullPath + "and backup failed" + e);
                }
                
            }
        }

        return loadedData;
    }

    public void Save(GameData data, string profileId)
    {

        if(profileId == null)
        {
            return;
        }
        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        string backupFilePath = fullPath + backupExtension;
        Debug.Log(fullPath);
        try
        {
            //create directory if it doesnt exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // serialize data to Json
            string dataToStore = JsonUtility.ToJson(data, true);


            if(useEncryption) 
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }
            //write the serialized data to the file 
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);  
                }
            }
            //verify the saved file can be loaded
            GameData verifiedGameData = Load(profileId);

            if(verifiedGameData != null)
            {
                File.Copy(fullPath, backupFilePath, true);
            } else
            {
                throw new Exception("Save file cant be verified");
            }
        } catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    public Dictionary<string, GameData> LoadAllProfiles()
    {
        Dictionary<string, GameData> profileDictionary = new Dictionary<string, GameData>();
        //loop over all directory names in the data directory path
        IEnumerable<DirectoryInfo> dirInfos = new DirectoryInfo(dataDirPath).EnumerateDirectories();

        foreach (DirectoryInfo dirInfo in dirInfos)
        {
            string profileId = dirInfo.Name;
            //check if data file exist, and if doesnt then folder isnt a profile and should be skipped
            string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
            if(!File.Exists(fullPath))
            {
                Debug.Log("Skipping direcroty when loading profiles because it doesnt contain data");
                continue; // used to immediately continue to next dirInfo
            }

            GameData profileData = Load(profileId); // load Gamedata for this profile and put it in the dictionary
            if(profileData != null)
            {
                profileDictionary.Add(profileId, profileData);
            } else
            {
                Debug.LogError("Tried to load profile but something went wrong. ProfileId: " + profileId);
            } 
        }

        return profileDictionary;
    }

    public string getMostRecentlyUpdatedPorfile()
    {

        string mostRecentProfileId = null;
        Dictionary<string, GameData> profilesGamdeData = LoadAllProfiles();
        foreach(KeyValuePair<string, GameData> pair in profilesGamdeData)
        {
            string profileId = pair.Key;
            GameData gameData = pair.Value;

            if(gameData == null)
            {
                continue;
            }
            //first data we come acroos its the most recent
            if(mostRecentProfileId == null)
            {
                mostRecentProfileId = profileId;
            } else // comprate to see which date is most recent
            {
                DateTime mostRecentDateTime = DateTime.FromBinary(profilesGamdeData[mostRecentProfileId].lastUpdated);
                DateTime newDateTime = DateTime.FromBinary(gameData.lastUpdated);

                if(newDateTime > mostRecentDateTime)
                {
                    mostRecentProfileId = profileId;
                }
            }

            
        }
        return mostRecentProfileId;
    }

    public void Delete(string profileId)
    {
        // base case if the profileId is null, return right away
        if (profileId == null)
        {
            return;
        }

        string fullPath = Path.Combine(dataDirPath, profileId, dataFileName);
        try
        {
            // ensure the data file exists at this path before deleting the directory
            if (File.Exists(fullPath))
            {
                // delete the profile folder and everything within it
                Directory.Delete(Path.GetDirectoryName(fullPath), true);
            }
            else
            {
                Debug.LogWarning("Tried to delete profile data, but data was not found at path: " + fullPath);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to delete profile data for profileId: "
                + profileId + " at path: " + fullPath + "\n" + e);
        }
    }
    private bool AttemptRollback(string fullPath)
    {
        bool success = false;
        string backupFilePath = fullPath + backupExtension;
        try
        {
            // if the file exists, attempt to roll back to it by overwriting the original file
            if (File.Exists(backupFilePath))
            {
                File.Copy(backupFilePath, fullPath, true);
                success = true;
                Debug.LogWarning("Had to roll back to backup file at: " + backupFilePath);
            }
            // otherwise, we don't yet have a backup file - so there'InputController nothing to roll back to
            else
            {
                throw new Exception("Tried to roll back, but no backup file exists to roll back to.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error occured when trying to roll back to backup file at: "
                + backupFilePath + "\n" + e);
        }

        return success;
    }



    // XOR encryption
    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";

        for(int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ encryptionCodeWord[i] % encryptionCodeWord.Length);
        }

        return modifiedData;
    }


}
