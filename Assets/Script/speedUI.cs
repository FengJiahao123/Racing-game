using UnityEngine;
using TMPro;

public class VehicleUI : MonoBehaviour
{
    public CarController car; // ���복��������
    public TMP_Text speedText;          // ���� TextMeshPro �ı����

    void Update()
    {
        if (car != null && speedText != null)
        {
            int speed = Mathf.RoundToInt(car.KPH);
            speedText.text = speed + " KM/H";
        }
    }
}
