using UnityEngine;
using System.Collections.Generic;

//Ensure the Object has a MeshFilter (THIS IS NEEDED TO ACCESS AND MODIFY THE MESH)
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class OceanWaves : MonoBehaviour
{
    //Object Information:
    Mesh surface;
    MeshCollider surfaceCollider;

    //(2) Vector3's which [1] = Default, [2] = Oscillated:
    Vector3[] originalPositions;
    Vector3[] displacedPositions;

    [System.Serializable]
    public class Wave
    {
        public float Amplitude;

        public float Wavelength;
        public float waveSpeed;

        public Vector3 direction;

        public float k => (2 * Mathf.PI) / Wavelength;
        public float w => (k * waveSpeed);
    }


    [Header("Wave Config:")]
    //# of Harmonic Waves, Used For SuperPosition When Combining Multiple Waves:
    public int waveCount = 5;
    private int previousWaveCount;
    [Space]

    //Varying Magnitude Range For Waves, Used For Unique Wave Generation:
    public float minAmplitude = 0.1f;
    public float baseAmplitude = 0.2f;
    public float maxAmplitude = 0.3f;
    [Space]

    //Varying Distance Between Repition of the Shape of the Wave, Used For Unique Wave Generation:
    public float minWaveLength = 1f;
    public float maxWaveLength = 10f;
    [Space]

    //Varying WaveSpeed's Determining the Velocity of the Wave, Used For Unique Wave Generation:
    public float minSpeed = 1f;
    public float maxSpeed = 4f;
    [Space]

    //Test Switch For 1D -> 2D Wave:
    public bool isOneDimension = false;
    private bool previousIsOneDimension;
    [Space]

    //List Holder, Used To Hold All Generated Unique Waves:
    List<Wave> waves;


    //Lerp Smoothing, Used To Easily Transition From Verticies:
    [Range(0f, 1f)]
    public float Smoothing = 0.15f;

    //Store Previous Frames of the Y-Positions:
    float[] previousHeights;


    void Start()
    {
        //Setup The Mesh Surface:
        surface = GetComponent<MeshFilter>().mesh;
        surfaceCollider = GetComponent<MeshCollider>();

        //Setup The Mesh Default Vector Positions (VERTICIES ARE STORED AS THE UNCHANGED MESH POSITION:
        originalPositions = surface.vertices;

        //Setup The Mesh Displacement Vector Positions (ALLOCATING MEMORY -> HAVE A VECTOR3 ARRAY ONLY DEFINING ITS SIZE TO BE THE SAME AS THE DEFAULT POSITION VERTICIES SIZE, NO VALUES SET YET):
        displacedPositions = new Vector3[originalPositions.Length];

        //Setup The Previous Positions, Empty Array in Equal Size to the Amount of Available points from all the vericies on the Y-Axis:
        previousHeights = new float[originalPositions.Length];

        previousWaveCount = waveCount;
        previousIsOneDimension = isOneDimension;
        GenerateWaveList();
    }


    void GenerateWaveList()
    {
        //Decleration of Wave List:
        waves = new List<Wave>();


        //For Each Wave:
        for (int i = 0; i < waveCount; i++)
        {
            Vector3 dir;

            if (isOneDimension)
            {
                //Strictly On X-Axis, Height on Y-Axis:
                dir = new Vector3(1f, 0f, 0f);
            }
            else
            {
                //On the XZ-Axis, Height on Y-Axis:
                float dx = Random.Range(-1f, 1f);
                float dz = Random.Range(-1f, 1f);

                dir = new Vector3(dx, 0f, dz).normalized;
            }

            //Adds the Current waveCount[i] to the Wave List With Unique Generation:
            waves.Add(new Wave
            {
                Amplitude = baseAmplitude * Random.Range(minAmplitude, maxAmplitude),

                Wavelength = Random.Range(minWaveLength, maxWaveLength),

                waveSpeed = Random.Range(minSpeed, maxSpeed),

                direction = dir
            });
        }


    }

    void Update()
    {
        if (previousWaveCount != waveCount)
        {
            previousWaveCount = waveCount;
            GenerateWaveList();
        }

        if (previousIsOneDimension != isOneDimension)
        {
            previousIsOneDimension = isOneDimension;
            GenerateWaveList();
        }

        //Formal Decleration of time:
        float time = Time.time;

        //Iterating For ALL Existing Verticies Withing The Default Surface:
        for (int i = 0; i < originalPositions.Length; i++)
        {
            //Temp Value -> Used To Hold The Information Of Current Vertice:
            Vector3 defaultPosition = originalPositions[i];
            Vector3 worldPosition = transform.TransformPoint(defaultPosition);

            //Initially Setting Y = 0, (NO WAVE YET, ONLY EQUILIBRIUM):
            float displacedHeight = 0f;

            //Summation of Multiple Sine Waves (WAVE SUPERPOSITION):
            foreach (var wave in waves)
            {
                float k = wave.k;
                float w = (wave.waveSpeed * k);
                float dot = Vector3.Dot(wave.direction, new Vector3(worldPosition.x, 0f, worldPosition.z));
                float phase = (k * dot) - (w * time);

                displacedHeight += wave.Amplitude * Mathf.Sin(phase);
            }

            //Smooth Lerping:
            float smoothHeight = Mathf.Lerp(previousHeights[i], displacedHeight, 1f - Mathf.Exp(-(Smoothing) * Time.deltaTime));
            previousHeights[i] = smoothHeight;

            //After Iterating Through All Waves in the Provided Length -> Apply The Calculated Height to Displacement Positions:
            displacedPositions[i] = new Vector3(defaultPosition.x, displacedHeight, defaultPosition.z);

            //  *[STEPS ABOVE REPEATED FOR ALL OTHER EXISITNG VERTICIES]*
        }


        //Update The Mesh With the Calculated displaced Positions:
        surface.vertices = displacedPositions;
        surface.RecalculateNormals();

        //IMPORTANT: Reassign the Mesh to the Collider so Collisions Match the Waves.
        //Force Unity to Refresh the Collider:
        surfaceCollider.sharedMesh = null;
        surfaceCollider.sharedMesh = surface;
        
    }
}
