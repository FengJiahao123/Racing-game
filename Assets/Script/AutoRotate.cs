using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 30, 0);
    public bool isSelected = false;

    void Update()
    {
        if (isSelected)
            transform.Rotate(rotationSpeed * Time.deltaTime);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }
}
