using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlanetZone : MonoBehaviour
{
    public float gravityStrength = -90f;

    [Tooltip("Local offset for gravity center if needed")]
    public Vector3 gravityCenterOffset = Vector3.zero;



    public Vector3 GetGravityDirection(Vector3 characterPosition)
    {
        Vector3 gravityCenter = transform.position + gravityCenterOffset;
        return (gravityCenter - characterPosition).normalized;
    }


    public float GetGravityStrength()
    {
        return gravityStrength;
    }



    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + gravityCenterOffset, 0.5f);
    }

}
