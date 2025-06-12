using UnityEngine;
using TMPro;

public class VehicleUI : MonoBehaviour
{
    public CarController car; 
    public TMP_Text speedText;          

    void Update()
    {
        if (car != null && speedText != null)
        {
            int speed = Mathf.RoundToInt(car.KPH);
            speedText.text = speed + " KM/H";
        }
    }
}
