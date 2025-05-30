using System;
using UnityEngine;


//NOTE: Structures (struct) are used to organized related variables.
public struct CameraInput
{
    public Vector2 Look;
}

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 0.1f;
    private Vector3 _eulerAngles;

    public void Initialize(Transform target)
    {
        transform.position = target.position;
        transform.eulerAngles = _eulerAngles = target.eulerAngles;
    }


    public void UpdatePosition(Transform target)
    {
        transform.position = target.position;
    }

    public void UpdateRotation(CameraInput input)
    {
        _eulerAngles.x += -input.Look.y * sensitivity;
        _eulerAngles.y += input.Look.x * sensitivity;

        _eulerAngles.x = Mathf.Clamp(_eulerAngles.x, -89f, 89f); //clamp y axis for no flipping

        transform.eulerAngles = _eulerAngles;
    }


}
