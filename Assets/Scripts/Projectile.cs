using UnityEngine;

public class Projectile : MonoBehaviour
{


    public GameObject positiveProjectilePrefab; 
    public GameObject negativeProjectilePrefab; 

    [SerializeField] private Transform spawnPosition; 
    [SerializeField] private GameObject targetObject; 

    [SerializeField] private Material positiveMaterial;
    [SerializeField] private Material negativeMaterial;

    public float spawnOffset = 1.5f;
    public float projectileSpeed = 10f;
    public bool projectileMotion = false; 

    public float destroyAfter = 5f; 
    public float cooldownTime = 1f; 

    private PlayerCharacter playerCharacter;
    private float lastFireTime;

    void Start()
    {
        playerCharacter = FindObjectOfType<PlayerCharacter>();
    }

    void Update()
    {
        UpdateMaterial();

        if (Input.GetKeyDown(KeyCode.Mouse1) && Time.time >= lastFireTime + cooldownTime)
        {
            SpawnProjectile();
            lastFireTime = Time.time;
        }
    }

    //Spawn Orbs:
    void SpawnProjectile()
    {
        if (playerCharacter == null) return;

        GameObject projectilePrefab = playerCharacter.positiveCharge ? positiveProjectilePrefab : negativeProjectilePrefab;
        if (projectilePrefab == null)
        {
            Debug.LogError("no prefab available!!!");
            return;
        }

        
        Transform cameraTransform = Camera.main.transform; //gets the player camera transform (1)

        Vector3 spawnPos = cameraTransform.position + cameraTransform.forward * spawnOffset; //sets camera transform position to be the spawn position for the projectile (2)

        Quaternion spawnRot = Quaternion.LookRotation(cameraTransform.forward); //ensure the projectile when fired will shoot forward (3)


        GameObject projectile = Instantiate(projectilePrefab, spawnPos, spawnRot); 

        //Apply Projectile Movement Logic is true:
        projectileMotion projectileMovement = projectile.GetComponent<projectileMotion>();
        if (projectileMovement != null)
        {
            projectileMovement.SetProjectileProperties(projectileSpeed, projectileMotion, projectileMotion ? 0.5f : 0f);
        }

        Destroy(projectile, destroyAfter);
    }

    //Color Changer:
    private void UpdateMaterial()
    {
        if (targetObject != null && positiveMaterial != null && negativeMaterial != null)
        {
            Renderer renderer = targetObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = playerCharacter.positiveCharge ? positiveMaterial : negativeMaterial;
            }
        }
    }

}
