using UnityEngine;

public class Projectile : MonoBehaviour
{


    public GameObject positiveProjectilePrefab; 
    public GameObject negativeProjectilePrefab; 

    [SerializeField] private Transform spawnPosition; 
    [SerializeField] private GameObject targetObject; 

    [SerializeField] private Material positiveMaterial;
    [SerializeField] private Material negativeMaterial;

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

        if (Input.GetKeyDown(KeyCode.Q) && Time.time >= lastFireTime + cooldownTime)
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

        GameObject projectile = Instantiate(projectilePrefab, spawnPosition.position, spawnPosition.rotation);

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
