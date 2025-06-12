using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using static SaveManager;

public class MainMenuUI : MonoBehaviour
{
    public GameObject mapSelectPanel;
    public Button map2Button;
    public GameObject newbieGuidePanel;  

    void Start()
    {
        
        bool unlocked = CheckLevelCompletion("Map1_Completed");
        map2Button.interactable = unlocked;
        mapSelectPanel.SetActive(false);

        newbieGuidePanel.SetActive(false);  
    }

    private bool CheckLevelCompletion(string mapName)
    {
        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "saveData.json");

        if (File.Exists(filePath))
        {
            try
            {
               
                string encryptedData = File.ReadAllText(filePath);

               
                string decryptedData = SaveManager.Decrypt(encryptedData);
                Debug.Log(decryptedData);
               
                SaveData data = JsonUtility.FromJson<SaveData>(decryptedData);

                if (data == null)
                {
                    Debug.LogError("Failed to parse save data. The file may be corrupted.");
                    return false;
                }

                
                if (mapName == "Map1_Completed" && data.Map1_Completed == 1)
                    return true;
                if (mapName == "Map2_Completed" && data.Map2_Completed == 1)
                    return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error reading save file: " + ex.Message);
            }
        }
        return false;
    }

    public void OnBackToMainMenu()
    {
        
        mapSelectPanel.SetActive(false);
    }
    public void OnStartGame()
    {
        mapSelectPanel.SetActive(true);
    }

    public void OnQuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit");
    }

    public void LoadMap1()
    {
       
        PlayerPrefs.SetString("SelectedMap", "Map1");
        PlayerPrefs.Save();  
        SceneManager.LoadScene("CarSelectScene");
    }

    public void LoadMap2()
    {
        
        PlayerPrefs.SetString("SelectedMap", "Map2");
        PlayerPrefs.Save();  
        SceneManager.LoadScene("CarSelectScene");
    }

   
    private void SaveSelectedMap(string mapName)
    {
        SaveManager.SaveProgress(mapName); 
    }

   
    public void ShowNewbieGuide()
    {
        newbieGuidePanel.SetActive(true); 
    }

   
    public void CloseNewbieGuide()
    {
        newbieGuidePanel.SetActive(false);  
    }
}
