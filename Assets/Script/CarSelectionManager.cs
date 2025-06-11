using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CarSelectionUI : MonoBehaviour
{
    public GameObject[] carDisplays; // 这两个是展示用车模型（不是用于比赛的 prefab）
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

            // 控制旋转
            AutoRotate rotator = carDisplays[i].GetComponent<AutoRotate>();
            if (rotator != null)
                rotator.SetSelected(isCurrent);

            // 控制 UI 面板
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
