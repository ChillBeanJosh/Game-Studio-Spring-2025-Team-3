using Unity.VisualScripting;
using UnityEngine;

public class TurretController : MonoBehaviour
{
    Transform _Player;
    float distanceFromPlayer;

    public float turretRange;
    public float turretRotationSpeed;
    public Transform turretHead;
    public Transform fireLocation;
    [Space]
    public bool ShootPositiveProjectile;
    public GameObject positiveProjectile;
    public GameObject negativeProjectile;
    [Space]
    public float fireRate;
    private float nextFire;
    public float projectileSpeed;


    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            _Player = playerObj.transform;
            Debug.Log("PLAYER FOUND");
        }
        else
        {
            Debug.LogError("PLAYER NOT FOUND!!!");
        }
    }

    private void Update()
    {
        distanceFromPlayer = Vector3.Distance(_Player.position, transform.position);

        if (distanceFromPlayer <= turretRange)
        {
            Quaternion lookRotation = Quaternion.LookRotation(_Player.position - turretHead.position);
            turretHead.rotation = Quaternion.Slerp(turretHead.rotation, lookRotation, Time.deltaTime * turretRotationSpeed);

            if(Time.time  >= nextFire)
            {
                nextFire = Time.time + 1f / fireRate ;
                FireProjectile();
            }
        }
    }

    void FireProjectile()
    {
        GameObject projectileInstance;

        if (ShootPositiveProjectile)
        {
            projectileInstance = Instantiate(positiveProjectile, fireLocation.position, turretHead.rotation);
        }
        else
        {
            projectileInstance = Instantiate(negativeProjectile, fireLocation.position, turretHead.rotation);
        }


        projectileMotion projectileScript = projectileInstance.GetComponent<projectileMotion>();
        if (projectileScript != null)
        {
            projectileScript.speed = projectileSpeed;
        }

        Destroy(projectileInstance, 5f);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; 
        Gizmos.DrawWireSphere(transform.position, turretRange); 
    }

}
