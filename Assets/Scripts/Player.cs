using UnityEngine;

public class Player : MonoBehaviour
{

    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;

    private PlayerInputActions _inputActions;

    void Start()
    {
        //Removes Default Cursor Visability.
        Cursor.lockState = CursorLockMode.Locked;

        //Enabling the Unity Input System.
        _inputActions = new PlayerInputActions();
        _inputActions.Enable();

        //Function called from PlayerCharacter class.
        //Gets Refrence to the Kinematic Character Motor.
        playerCharacter.Initialize();

        //Function called from PlayerCamera Class.
        //Gets refrence to the GetCameraTarget() Transform.
        playerCamera.Initialize(playerCharacter.GetCameraTarget());
    }

    void OnDestroy()
    {
        _inputActions.Dispose();
    }

    void Update()
    {
        //Gets the refrence to the specified Input System Map.
        var input = _inputActions.Gameplay;

        //Gets refence to the structure (cameraInput) within PlayerCamera.cs
        //Declaring the Look variable in the structure to be the value for the "Look" Action within the Input System Map.
        //Currently controlled by the Mouse and will be used to UpdateRotation.
        var cameraInput = new CameraInput { Look = input.Look.ReadValue<Vector2>() };

        playerCamera.UpdateRotation(cameraInput);
        playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());
    }
}
