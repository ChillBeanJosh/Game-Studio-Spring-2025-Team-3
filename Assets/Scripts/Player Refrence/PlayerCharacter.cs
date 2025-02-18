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
    Stand, Crouch, Slide
}

public struct CharacterState
{
    public bool Grounded;
    public Stance Stance;
    public Vector3 Velocity;
}


//NOTE: Structures (struct) are used to organized related variables.
public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;
    public bool JumpSustain;
    public CrouchInput Crouch;

}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform root;
    [SerializeField] private Transform cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [SerializeField] private float walkResponse = 25f;
    [Space]
    [SerializeField] private float crouchSpeed = 7f;
    [SerializeField] private float crouchResponse = 20f;
    [Space]
    [SerializeField] private float airSpeed = 15f;
    [SerializeField] private float airAcceleration = 70f;
    [Space]
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float coyoteTime = 0.2f;
    [Range(0f, 1f)]
    [SerializeField] private float jumpSustainGravity = 0.4f;
    [SerializeField] private float gravity = -90f;
    [Space]
    [SerializeField] private float slideStartSpeed = 25f;
    [SerializeField] private float slideEndSpeed = 15f;
    [SerializeField] private float slideFriction = 0.8f;
    [SerializeField] private float slideSteerAcceleration = 5f;
    [SerializeField] private float slideGravity = -90f;
    [Space]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchHeightResponse = 15f;
    [Space]
    [Range (0f, 1f)]
    [SerializeField] private float standCameraTargetHeight = 0.9f;
    [Range(0f, 1f)]
    [SerializeField] private float crouchCameraTargetHeight = 0.7f;

    private CharacterState _state;
    private CharacterState _lastState;
    private CharacterState _tempState;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;

    private bool _requestedJump;
    private bool _requestedSustainJump;

    private bool _requestedCrouch;
    private bool _requestedCrouchInAir;

    private float _timeSinceUngrounded;
    private float _timeSinceJumpRequested;
    private bool _ungroundedDueToJump;

    private Collider[] _uncrouchOverlapResults;


    public void Initialize()
    {
        _state.Stance = Stance.Stand;
        _lastState = _state;

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

        var wasRequestingJump = _requestedJump;

        //True if -> Already = true OR when jump input is pressed.
        _requestedJump = _requestedJump || input.Jump;

        if (_requestedJump && !wasRequestingJump)
        {
            _timeSinceJumpRequested = 0f;
        }

        _requestedSustainJump = input.JumpSustain;

        var wasRequestingCrouch = _requestedCrouch;

        _requestedCrouch = input.Crouch switch
        {
            CrouchInput.Toggle => !_requestedCrouch,
            CrouchInput.None => _requestedCrouch,
            _ => _requestedCrouch
        };

        if (_requestedCrouch && !wasRequestingCrouch)
        {
            _requestedCrouchInAir = !_state.Grounded;
        }
        else if (!_requestedCrouch && wasRequestingCrouch)
        {
            _requestedCrouchInAir = false;
        }
   
    }

    //Function used to adjust the Character Root/Mesh.
    public void UpdateBody(float deltaTime)
    {
        var currentHeight = motor.Capsule.height;
        var normalizedHeight = currentHeight / standHeight;

        var cameraTargetHeight = currentHeight *
        (
            //IF Stance = stand -> 'standCameraHeight', ELSE -> 'crouchCameraHeight'
            _state.Stance is Stance.Stand
                ? standCameraTargetHeight
                : crouchCameraTargetHeight
        );

        //Adjusts the cameraTarget position according to the updated value.
        cameraTarget.localPosition = Vector3.Lerp
        (
            a: cameraTarget.localPosition,
            b: new Vector3(cameraTarget.localPosition.x, cameraTargetHeight, cameraTarget.localPosition.z),
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
        );



        //Using the normalized height -> adjusts the Root/Mesh to fit.
        var rootTargetScale = new Vector3(1f, normalizedHeight, 1f);

        //Scales the Root/Mesh to the specified target scale from above.
        root.localScale = Vector3.Lerp
        (
            a: root.localScale,
            b: rootTargetScale,
            t: 1f - Mathf.Exp(-crouchHeightResponse * deltaTime)
        );

        
    }

    //NOTE: UpdateVelocity() is called is physics tick, BY THE "KinematicCharacterMotor".
    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        //If the Character is grounded.
        if (motor.GroundingStatus.IsStableOnGround)
        {
            _timeSinceUngrounded = 0f;
            _ungroundedDueToJump = false;
 
            //NOTE: By default will give a unit vector -> multiply (_requestedMovement) to keep speed.
            var groundedMovement = motor.GetDirectionTangentToSurface
            (
                direction: _requestedMovement,
                surfaceNormal: motor.GroundingStatus.GroundNormal
            ) * _requestedMovement.magnitude;

            //Sliding Movement:
            {
                var moving = groundedMovement.sqrMagnitude > 0f;
                var crouching = _state.Stance is Stance.Crouch;
                var wasStanding = _lastState.Stance is Stance.Stand;
                var wasInAir = !_lastState.Grounded;

                if (moving && crouching && (wasStanding || wasInAir))
                {
                    _state.Stance = Stance.Slide;

                    Debug.DrawRay(transform.position, currentVelocity, Color.red, 5f);
                    Debug.DrawRay(transform.position, _lastState.Velocity, Color.green, 5f);

                    if (wasInAir)
                    {
                        currentVelocity = Vector3.ProjectOnPlane
                        (
                            vector: _lastState.Velocity,
                            planeNormal: motor.GroundingStatus.GroundNormal
                        );
                    }

                    var effectiveSlideStartSpeed = slideStartSpeed;
                    if (!_lastState.Grounded && !_requestedCrouchInAir)
                    {
                        effectiveSlideStartSpeed = 0f;
                    }

                    var slideSpeed = Mathf.Max(slideStartSpeed, currentVelocity.magnitude);

                    currentVelocity = motor.GetDirectionTangentToSurface
                    (
                        direction: currentVelocity,
                        surfaceNormal: motor.GroundingStatus.GroundNormal
                    ) * slideSpeed;
                }
            }

            //Normal Movement:
            if (_state.Stance is Stance.Stand or Stance.Crouch)
            {
                //If stance = Stand -> walkSpeed, Else -> crouchSpeed.
                var speed = _state.Stance is Stance.Stand
                    ? walkSpeed
                    : crouchSpeed;

                //If stance = Stand -> walkResponse, Else -> crouchResponse.
                var response = _state.Stance is Stance.Stand
                    ? walkResponse
                    : crouchResponse;

                //Smoothly transitions Speed.
                var targetVelocity = groundedMovement * speed;
                currentVelocity = Vector3.Lerp
                (
                    a: currentVelocity,
                    b: targetVelocity,
                    t: 1f - Mathf.Exp(-response * deltaTime)
                );
            }
            //Sliding Logic:
            else
            {
                //Friction:
                currentVelocity -= currentVelocity * (slideFriction * deltaTime);

                //Slopes:
                {
                    //DOWN force projected to the GROUND
                    //Based on the projected planes angle:
                    // slideGravity will be multiplied giving varying speeds on different angles.
                    var force = Vector3.ProjectOnPlane
                    (
                        vector: -motor.CharacterUp,
                        planeNormal: motor.GroundingStatus.GroundNormal
                    ) * slideGravity;

                    currentVelocity -= force * deltaTime;
                }



                //Steering:
                {
                    var currentSpeed = currentVelocity.magnitude;
                    var targetVelocity = groundedMovement * currentSpeed;

                    var steerForce = (targetVelocity - currentVelocity) * slideSteerAcceleration * deltaTime;
                    currentVelocity += steerForce;
                    currentVelocity = Vector3.ClampMagnitude(currentVelocity, currentSpeed);
                }

                //Stopping:
                if (currentVelocity.magnitude < slideEndSpeed)
                {
                    _state.Stance = Stance.Crouch;
                }

            }
        }
        //If the Character is in the air.
        else
        {
            _timeSinceUngrounded += deltaTime;

            if (_requestedMovement.sqrMagnitude > 0f)
            {
                //Movement along the XZ-Plane.
                var planarMovement = Vector3.ProjectOnPlane
                (
                    vector: _requestedMovement,
                    planeNormal: motor.CharacterUp
                ) * _requestedMovement.magnitude;

                //Velocity Calculation.
                var currentPlanarVelocity = Vector3.ProjectOnPlane
                (
                    vector: currentVelocity,
                    planeNormal: motor.CharacterUp
                );

                //Movement Force Application.
                var movementForce = planarMovement * airAcceleration * deltaTime;

                //IF slower than MAX AIR SPEED -> MovementForce = Steering Force.
                if (currentPlanarVelocity.magnitude < airSpeed)
                {
                    //Setting Restrictions to the velocity to Max Out at "airSpeed".
                    var targetPlanarVelocity = currentPlanarVelocity + movementForce;
                    targetPlanarVelocity = Vector3.ClampMagnitude(targetPlanarVelocity, airSpeed);

                    movementForce = targetPlanarVelocity - currentPlanarVelocity;
                }
                //ELSE IF movement force is in the SAME DIRECTION of currentPlanarVelocity -> LIMIT movement force.
                else if (Vector3.Dot(currentPlanarVelocity, movementForce) > 0f)
                {
                    var constrainedMovementForce = Vector3.ProjectOnPlane
                    (
                        vector: movementForce,
                        planeNormal: currentPlanarVelocity.normalized
                    );

                    movementForce = constrainedMovementForce;
                }

                //Air-Climbing Negation:
                if (motor.GroundingStatus.FoundAnyGround)
                {
                    //IF MOVING in the SAME DIRECTION of velocity.
                    if (Vector3.Dot(movementForce, currentVelocity + movementForce) > 0f)
                    {
                        //Allowing ONLY movement on the XZ-PLANE.
                        var obstructionNormal = Vector3.Cross
                        (
                            motor.CharacterUp,
                            Vector3.Cross
                            (
                                motor.CharacterUp,
                                motor.GroundingStatus.GroundNormal
                            )
                        ).normalized;

                        movementForce = Vector3.ProjectOnPlane(movementForce, obstructionNormal);
                    }
                }

                //Steering force.
                currentVelocity += movementForce;
            }

            var effectiveGravity = gravity;
            var verticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);

            //Adjusts the Gravity in the case the BOOL is TRUE.
            if (_requestedSustainJump && verticalSpeed > 0f)
                effectiveGravity *= jumpSustainGravity;

            currentVelocity += motor.CharacterUp * effectiveGravity * deltaTime;
        }

        if(_requestedJump)
        {
            //Ground Check.
            var grounded = motor.GroundingStatus.IsStableOnGround;

            var canCoyoteJump = _timeSinceUngrounded < coyoteTime && !_ungroundedDueToJump;

            if (grounded || canCoyoteJump)
            {
                _requestedJump = false;
                _requestedCrouch = false;
                _requestedCrouchInAir = false;

                //Unsticks player from the ground.
                motor.ForceUnground(time: 0.1f);
                _ungroundedDueToJump = true;

                var currentVerticalSpeed = Vector3.Dot(currentVelocity, motor.CharacterUp);
                var targetVerticalSpeed = Mathf.Max(currentVerticalSpeed, jumpSpeed);

                currentVelocity += motor.CharacterUp * (targetVerticalSpeed - currentVerticalSpeed);
            }
            else
            {
                _timeSinceJumpRequested += deltaTime;

                var canJumpLater = _timeSinceJumpRequested < coyoteTime;
                _requestedJump = false;
            }
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
        _tempState = _state;

        //Enabling Player Crouch (EFFECTS THE MOTOR).
        if (_requestedCrouch && _state.Stance is Stance.Stand)
        {
            _state.Stance = Stance.Crouch;
            motor.SetCapsuleDimensions
            (
                radius: motor.Capsule.radius,
                height: crouchHeight,
                yOffset: crouchHeight * 0.5f
            );
        }
    }

    //NOTE: PostGroundingUpdate() is called the FRAME AFTER the "motor" COLLIDES with the GROUND.
    public void PostGroundingUpdate(float deltaTime)
    {
        if (!motor.GroundingStatus.IsStableOnGround && _state.Stance is Stance.Slide)
        {
            _state.Stance = Stance.Crouch;
        }
    }

    //NOTE: AfterCharacterUpdate() is called after the "KinematicCharacterController" Updates.
    public void AfterCharacterUpdate(float deltaTime)
    {
        //Disabling Player Crouch (EFFECTS THE MOTOR).
        if (!_requestedCrouch && _state.Stance is not Stance.Stand)
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
                _state.Stance = Stance.Stand;
            }
        }

        //Update state for grounded.
        _state.Grounded = motor.GroundingStatus.IsStableOnGround;
        _state.Velocity = motor.Velocity;

        _lastState = _tempState;
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

    public void SetPosition(Vector3 position, bool killVelocity = true)
    {
        motor.SetPosition(position);
        if (killVelocity)
        {
            motor.BaseVelocity = Vector3.zero;
        }

    }
}
