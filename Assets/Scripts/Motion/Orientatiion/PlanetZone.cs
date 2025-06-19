using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlanetZone : MonoBehaviour
{
    public float gravityStrength = -90f;
    public Vector3 gravityCenterOffset = Vector3.zero;
    [Space]

    public SphereCollider _gravityTrigger;
    public float GravityRadius => _gravityTrigger != null ? _gravityTrigger.radius * transform.lossyScale.x : 0f;


    public Vector3 GetGravityDirection(Vector3 characterPosition)
    {
        Vector3 gravityCenter = transform.position + gravityCenterOffset;
        return (gravityCenter - characterPosition).normalized;
    }


    public float GetGravityStrength()
    {
        return gravityStrength;
    }
}
