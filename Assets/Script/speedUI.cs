using UnityEngine;
using TMPro;

public class VehicleUI : MonoBehaviour
{
    public CarController car; // 拖入车辆控制器
    public TMP_Text speedText;          // 拖入 TextMeshPro 文本组件

    void Update()
    {
        if (car != null && speedText != null)
        {
            int speed = Mathf.RoundToInt(car.KPH);
            speedText.text = speed + " KM/H";
        }
    }
}
