using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarSelectionUI : MonoBehaviour
{
    public GameObject[] carDisplays; 
    public GameObject[] carDataPanels;
    private int currentIndex = 0;

    void Start()
    {
        UpdateCarDisplay();
    }

    public void NextCar()
    {
        currentIndex = (currentIndex + 1) % carDisplays.Length;
        UpdateCarDisplay();
    }

    public void PrevCar()
    {
        currentIndex = (currentIndex - 1 + carDisplays.Length) % carDisplays.Length;
        UpdateCarDisplay();
    }

    void UpdateCarDisplay()
    {
        for (int i = 0; i < carDisplays.Length; i++)
        {
            bool isCurrent = (i == currentIndex);
            carDisplays[i].SetActive(isCurrent);

            AutoRotate rotator = carDisplays[i].GetComponent<AutoRotate>();
            if (rotator != null)
                rotator.SetSelected(isCurrent);

            if (i < carDataPanels.Length)
                carDataPanels[i].SetActive(isCurrent);
        }
    }

    public void ConfirmSelection()
    {
        PlayerPrefs.SetInt("SelectedCarIndex", currentIndex);
        string map = PlayerPrefs.GetString("SelectedMap", "Map1");
        SceneManager.LoadScene(map);
    }
}
