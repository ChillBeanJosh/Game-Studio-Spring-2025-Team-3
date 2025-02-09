using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController; //needed for custom functions for character physics.
using UnityEngine;


//NOTE: Structures (struct) are used to organized related variables.
public struct CharacterInput
{
    public Quaternion Rotation;
    public Vector2 Move;
    public bool Jump;

}

public class PlayerCharacter : MonoBehaviour, ICharacterController
{
    [SerializeField] private KinematicCharacterMotor motor;
    [SerializeField] private Transform cameraTarget;
    [Space]
    [SerializeField] private float walkSpeed = 20f;
    [Space]
    [SerializeField] private float jumpSpeed = 20f;
    [SerializeField] private float gravity = -90f;

    private Quaternion _requestedRotation;
    private Vector3 _requestedMovement;
    private bool _requestedJump;


    public void Initialize()
    {
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


            currentVelocity = groundedMovement * walkSpeed;
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
            motor.ForceUnground(time: 0f);

            
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
    



    public void BeforeCharacterUpdate(float deltaTime)
    {
    }

    public void PostGroundingUpdate(float deltaTime)
    {
    }

    public void AfterCharacterUpdate(float deltaTime)
    {
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
