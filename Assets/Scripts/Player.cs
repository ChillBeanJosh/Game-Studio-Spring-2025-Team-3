using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private PlayerCamera playerCamera;
    [Space]
    [SerializeField] private CameraSpring cameraSpring;
    [SerializeField] private CameraLean cameraLean;

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

        //Function called from CameraSpring Class.
        cameraSpring.Initialize();

        //Function called from CameraLean Class.
        cameraLean.Initialize();
    }

    void OnDestroy()
    {
        _inputActions.Gameplay.Disable();

        _inputActions.Dispose();
    }

    void Update()
    {
        //Gets the refrence to the specified Input System Map.
        var input = _inputActions.Gameplay;

        var deltaTime = Time.deltaTime;


        //Gets refence to the structure (CameraInput) within PlayerCamera.cs
        //Declaring the Look variable in the structure to be the value for the "Look" Action within the Input System Map.
        var cameraInput = new CameraInput
        {
            Look = input.Look.ReadValue<Vector2>()
        };

        playerCamera.UpdateRotation(cameraInput);
        playerCamera.UpdatePosition(playerCharacter.GetCameraTarget());


        //Gets refence to the structure (CharacterInput) within PlayerCharacter.cs
        //Declaring the Move variable in the structure to be the value for the "Move" Action within the Input System Map.
        var characterInput = new CharacterInput
        {
            Rotation = playerCamera.transform.rotation,
            Move = input.Move.ReadValue<Vector2>(),
            Jump = input.Jump.WasPerformedThisFrame(),
            JumpSustain = input.Jump.IsPressed(),
            Crouch = input.Crouch.WasPressedThisFrame()
                ? CrouchInput.Toggle //if
                : CrouchInput.None //else
        };

        playerCharacter.UpdateInput(characterInput);
        playerCharacter.UpdateBody(deltaTime);


        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                Teleport(hit.point);
            }
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            playerCharacter.ActivateChargeToggle();
        }

        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            playerCharacter.ChargeToggle();
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            playerCharacter.ToggleAntiGravity();
        }
    }

    private void LateUpdate()
    {
        var deltaTime = Time.deltaTime;
        var state = playerCharacter.GetState();

        cameraSpring.UpdateSpring(deltaTime, playerCharacter.GetCameraTarget().up);

        cameraLean.UpdateLean
        (
            deltaTime, 
            state.Stance is Stance.Slide, 
            state.Acceleration, 
            playerCharacter.GetCameraTarget().up
        );


    }


    //TESTING 
    public void Teleport(Vector3 position)
    {
        playerCharacter.SetPosition(position);
    }
}
