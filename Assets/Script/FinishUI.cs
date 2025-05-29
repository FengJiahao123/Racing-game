using UnityEngine;
using UnityEngine.UI;

public class FinishUI : MonoBehaviour
{
    public GameObject finishPanel;

    void Start()
    {
        finishPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F)) // 按F显示面板
        {
            ShowFinishPanel("01:32.46", 3);
        }
    }

    public void ShowFinishPanel(string time, int rank)
    {
        finishPanel.SetActive(true);
        finishPanel.transform.Find("TimeText").GetComponent<Text>().text = "Your Time: " + time;
        finishPanel.transform.Find("RankText").GetComponent<Text>().text = "Your Rank: " + RankToString(rank);
    }

    string RankToString(int rank)
    {
        if (rank == 1) return "1st";
        if (rank == 2) return "2nd";
        if (rank == 3) return "3rd";
        return rank + "th";
    }

    public void OnContinueButton()
    {
        finishPanel.SetActive(false); // 隐藏面板
    }
}

