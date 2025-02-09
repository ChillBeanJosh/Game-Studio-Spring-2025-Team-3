using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController; //needed for custom functions for character physics.
using UnityEngine;

//NOTE: Enumerators (enum) are being used for different state handlings.
public enum CrouchInput
{
    None, Toggle
}

public enum Stance
{
    Stand, Crouch
}


//NOTE: Structures (struct) are used to organized related variables.
public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public CrouchInput Crouch;

}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float crouchSpeed = 7f;
    [Space]
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float gravity = -90f;
    [Space]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [Range (0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;

    private Stance _stance;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;
    private bool _requestedCrouch;

    private Collider[] _uncrouchOverlapResults;


    public void Initialize()
    {
        _stance = Stance.Stand;
        _uncrouchOverlapResults = new Collider[8];

        motor.CharacterController = this;
    }

    //UpdateInput() is called EVERY FRAME
    //NOTE: Could be called multiple times between physics ticks if applicable.
    //SO, will be used to queue requests (inputs) FOR physics ticks.
    public void UpdateInput(CharacterInput input)
    {
        _requestedRotation = input.Rotation;


        //Given the 2D Vector Inputs -> Projects onto a 3D setting (x,y) = (x,z).
        _requestedMovement = new Vector3(input.Move.x, 0f, input.Move.y);
        //Clamps the Vectors into unit vector to normalize movement.
        _requestedMovement = Vector3.ClampMagnitude(_requestedMovement, 1f);
        //Adjusts orientation to match the direction faced.
        _requestedMovement = input.Rotation * _requestedMovement;


        //True if -> Already = true OR when jump input is pressed.
        _requestedJump = _requestedJump || input.Jump;

        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.None => _requestedCrouch,
            _ => _requestedCrouch

            //IN THE CASE OF IMPLEMENTING HOLD.
            //CrouchInput.Crouch => true,
            //CrouchInput.UnCrouch => false
            //
        };
   
    }

    //Function used to adjust the Character Body (Root/Mesh).
    public void UpdateBody()
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;

        //Depending of the Stance (state) -> changes the specified camera height.
        var cameraTargetHeight = currentHeight *
        (
            _stance is Stance.Stand
                ? standCameraTargetHeight
                : crouchCameraTargetHeight
        );

        var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);

        cameraTarget.localPosition = new Vector3(0f, cameraTargetHeight, 0f);
        root.localScale = rootTargetScale;
    }

    //NOTE: UpdateVelocity() is called is physics tick, BY THE "KinematicCharacterMotor".
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //If the Character is grounded.
        if (motor.GroundingStatus.IsStableOnGround)
        {
            //NOTE: By default will give a unit vector -> multiply (_requestedMovement) to keep speed.
            var groundedMovement = motor.GetDirectionTangentToSurface
            (

                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            //If stance = Stand -> walkspeed, Else -> crouchSpeed.
            var speed = _stance is Stance.Stand
                ? walkSpeed
                : crouchSpeed;

            currentVelocity = groundedMovement * speed;
        }
        //If the Character is in the air.
        else
        {
            currentVelocity += motor.CharacterUp * gravity * deltaTime;
        }

        if(_requestedJump)
        {
            _requestedJump = false;

            //Unsticks player from the ground.
            motor.ForceUnground(time: 0.1f);

            
            var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
            var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

            currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
        }

        
    }

    //NOTE: UpdateRotation() is called is physics tick, BY THE "KinematicCharacterMotor".
    //This function is going to be used to match the players rotation -> camera rotation.
    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        //Creates a vector using the _requestedRotation (Exact Camera Rotation), and locking it to a plane (flat) foward.
        //Note: CharacterUp is forward (W), same as Input System Mapping.
        var forward = Vector3.ProjectOnPlane(_requestedRotation * Vector3.forward, motor.CharacterUp);

        //In the case of looking straight Up or Down.
        if (forward != Vector3.zero)
        {
            currentRotation = Quaternion.LookRotation(forward, motor.CharacterUp);
        }
    }
    


    //NOTE: BeforeCharacterUpdate() is called before the "KinematicCharacterController" Updates.
    public void BeforeCharacterUpdate(float deltaTime)
    {
        //Enabling Player Crouch.
        if (_requestedCrouch && _stance is Stance.Stand)
        {
            _stance = Stance.Crouch;
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: crouchHeight * 0.5f
            );
        }
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    //NOTE: AfterCharacterUpdate() is called after the "KinematicCharacterController" Updates.
    public void AfterCharacterUpdate(float deltaTime)
    {
        //Disabling Player Crouch
        if (!_requestedCrouch && _stance is not Stance.Stand)
        {
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: standHeight,
                yOffset: standHeight * 0.5f
            );

            var pos = motor.TransientPosition;
            var rot = motor.TransientRotation;
            var mask = motor.CollidableLayers;

            //IF there are detected Overlaps when Moving Up to Uncrouch.
            if (motor.CharacterOverlap(pos, rot, _uncrouchOverlapResults, mask, QueryTriggerInteraction.Ignore) > 0)
            {
                _requestedCrouch = true;

                motor.SetCapsuleDimensions
                (
                    radius: motor.Capsule.radius,
                    height: crouchHeight,
                    yOffset: standHeight * 0.5f
                );
            }
            else
            {
                _stance = Stance.Stand;
            }
        }
    }




    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {
    }




    public bool IsColliderValidForCollisions(Collider coll) => true;
  
    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {
    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {
    }


    public Transform GetCameraTarget() => cameraTarget;
}
