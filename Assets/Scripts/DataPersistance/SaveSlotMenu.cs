using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

public class SaveSlotMenu : MonoBehaviour
{
    [Header("Confirmation Popup")]
    [SerializeField] private ConfirmationPopupMenu popupMenu;

    [Header("MenuButton")]
    [SerializeField] private Button backButton;

    private SaveSlot[] saveSlots;

    private bool isLoadingGame = false;

    public UnityEvent OnSaveSlotClikedEvent;

    private void Awake()
    {
        saveSlots = GetComponentsInChildren<SaveSlot>();
    }

    public void ActivateMenu(bool isLoadingGame)
    {

        this.gameObject.SetActive(true);

        this.isLoadingGame = isLoadingGame;
        Dictionary<string, GameData> profilesGameData = DataPersistanceManager.instance.GetAllProfilesData();

        backButton.interactable = true;

        GameObject firstSelected = backButton.gameObject;
        foreach(SaveSlot saveSlot in saveSlots)
        {
            GameData profileData = null;
            profilesGameData.TryGetValue(saveSlot.getProfileId(), out profileData);
            saveSlot.SetData(profileData);
            if(profileData == null && isLoadingGame)
            {
                saveSlot.setInteractable(true);
            } else
            {
                saveSlot.setInteractable(true);
                if(firstSelected.Equals(backButton.gameObject))
                {
                    firstSelected = saveSlot.gameObject;
                }
            }
        }
    }

    public void OnSaveSlotClicked(SaveSlot saveSlot)
    {

        DisableMenuButtons();
        if(isLoadingGame)
        {
            DataPersistanceManager.instance.ChangeSelectedProfileId(saveSlot.getProfileId());
            SaveGameAndLoadScene();
        } else if(saveSlot.hasData) // new game, but is has data
        {
            popupMenu.ActivateMenu("Starting a New Game will override the currenly saved data. Are you sure?",
                () => 
                {
                    DataPersistanceManager.instance.ChangeSelectedProfileId(saveSlot.getProfileId());
                    DataPersistanceManager.instance.NewGame();
                    SaveGameAndLoadScene();
                }, 
                () => 
                {
                    this.ActivateMenu(isLoadingGame);
                }
            );
        } else // new game but has no data
        {
            DataPersistanceManager.instance.ChangeSelectedProfileId(saveSlot.getProfileId());
            DataPersistanceManager.instance.NewGame();
            SaveGameAndLoadScene();
        }
    }

    public void OnClearClicked(SaveSlot saveSlot)
    {
        DisableMenuButtons();
        popupMenu.ActivateMenu("Are you sure you want to delete this saved data?",
            () =>
            {
                DataPersistanceManager.instance.DeleteProfileData(saveSlot.getProfileId());
                ActivateMenu(isLoadingGame);
            },
            () =>
            {
                ActivateMenu(isLoadingGame);
            }
        );
    }

    private void SaveGameAndLoadScene()
    {
        DataPersistanceManager.instance.SaveGame();
        OnSaveSlotClikedEvent.Invoke();
        //  SceneManager.LoadScene(DataPersistanceManager.instance.getActiveScene());
    }

    public void DeactivateMenu()
    {
        this.gameObject.SetActive(false);
    }

    private void DisableMenuButtons()
    {
        foreach(SaveSlot saveSlot in saveSlots)
        {
            saveSlot.setInteractable(false);
        }

        backButton.interactable = false;
    }

}
