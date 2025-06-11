using UnityEngine;

public class PlayerInput : MonoBehaviour, IDriverInput
{
    public float Throttle => Input.GetAxis("Vertical");
    public float Steer => Input.GetAxis("Horizontal");
    public bool Brake => Input.GetKey(KeyCode.Space);
    public bool Handbrake => Input.GetKey(KeyCode.LeftShift)
                          && Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f;
}

