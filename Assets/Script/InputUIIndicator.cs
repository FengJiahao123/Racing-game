using UnityEngine;
using UnityEngine.UI;

public class InputUIIndicator : MonoBehaviour
{
    public Image leftKey;
    public Image rightKey;
    public Image driftKey;

    public Color defaultColor = Color.gray;
    public Color activeColor = Color.cyan;

    void Update()
    {
        leftKey.color = Input.GetKey(KeyCode.A) ? activeColor : defaultColor;
        rightKey.color = Input.GetKey(KeyCode.D) ? activeColor : defaultColor;
        driftKey.color = Input.GetKey(KeyCode.LeftShift) ? activeColor : defaultColor;
    }
}

