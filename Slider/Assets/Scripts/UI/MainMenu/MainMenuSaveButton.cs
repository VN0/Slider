using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuSaveButton : MonoBehaviour
{
    public TextMeshProUGUI profileNameText;
    public TextMeshProUGUI completionText;
    public TextMeshProUGUI timeText;
    public Image catSticker;

    [SerializeField] private int profileIndex = -1;
    private SaveProfile profile;

    public static bool deleteMode;

    public MainMenuManager mainMenuManager;

    private void OnEnable() 
    {
        ReadProfileFromSave();
        UpdateButton();
    }

    public void UpdateButton()
    {

        if (profile != null)
        {
            completionText.gameObject.SetActive(true);
            timeText.gameObject.SetActive(true);

            // name based on delete mode
            if (!deleteMode)
                profileNameText.text = profile.GetProfileName();
            else
                profileNameText.text = "Delete?";
            completionText.text = string.Format("{0}/9", GetNumAreasCompleted(profile));
            float seconds = profile.GetPlayTimeInSeconds();
            int minutes = (int)seconds / 60;
            timeText.text = string.Format("{0}h{1:D2}", minutes / 60, minutes % 60);
            catSticker.enabled = profile.GetCompletionStatus();
        }
        else
        {
            profileNameText.text = "[ Empty ]";
            completionText.gameObject.SetActive(false);
            timeText.gameObject.SetActive(false);
            catSticker.enabled = false;
        }
    }

    private int GetNumAreasCompleted(SaveProfile profile)
    {
        int count = 0;
        foreach (Area area in Area.GetValues(typeof(Area)))
        {
            if (area == Area.None) continue;

            SGridData data = profile.GetSGridData(area);
            if (data == null) 
            {
                Debug.LogError("SGridData was null when trying to read number of complete areas!");
                continue;
            }

            if (data.completionColor == ArtifactWorldMapArea.AreaStatus.color)
            {
                count += 1;
            }
        }
        return count;
    }
    
    public void ReadProfileFromSave()
    {
        SerializableSaveProfile ssp = SaveSystem.GetSerializableSaveProfile(profileIndex);
        if (ssp != null)
            profile = ssp.ToSaveProfile();
        else
            profile = null;
        SaveSystem.SetProfile(profileIndex, profile);
    }

    public void OnClick()
    {
        if (profile == null)
        {
            // create new profile
            mainMenuManager.OpenNewSave(profileIndex);
        }
        else
        {
            if (deleteMode)
            {
                DeleteThisProfile();
            }
            else
            {
                // load my profile
                LoadThisProfile();
            }
        }
    }

    private void LoadThisProfile()
    {
        SaveSystem.LoadSaveProfile(profileIndex);
    }

    public void DeleteThisProfile()
    {
        if (profile != null)
        {
            // TODO: seek confirmation
            SaveSystem.DeleteSaveProfile(profileIndex);
            profile = null;
            SaveSystem.SetProfile(profileIndex, profile);
            UpdateButton();
        }
    }
}
