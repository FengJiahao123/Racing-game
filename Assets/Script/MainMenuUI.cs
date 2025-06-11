using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public GameObject mapSelectPanel;
    public Button map2Button;

    void Start()
    {
        bool unlocked = PlayerPrefs.GetInt("Map1_Completed", 0) == 1;
        map2Button.interactable = unlocked;
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
        SceneManager.LoadScene("CarSelectScene");
    }

    public void LoadMap2()
    {
        PlayerPrefs.SetString("SelectedMap", "Map2");
        SceneManager.LoadScene("CarSelectScene");
    }
}

