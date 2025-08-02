using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.UI.Image;
using System.Collections.Generic;
using System.Linq;

//Visualized with a Line Render
[RequireComponent(typeof(LineRenderer))]
public class LaserSource : MonoBehaviour
{
    [Header("Lazer Parameters: ")]
    public List<Vector3> laserPoints;
    public RaycastHit[] hits;
    public float lazerDistance;
    private LineRenderer lineRenderer;

    [Header("Lens Collision: ")]
    private Lens lens;
    public LayerMask lensLayer;
    public bool lensHit;
    public float lazerOffset;

    Vector3 ImagePoint = Vector3.zero;

    [Header("Debug Visualization")]
    public GameObject obstructionPointMarkerPrefab;
    public GameObject imagePointMarkerPrefab;

    private List<GameObject> laserPointMarkers = new List<GameObject>();
    private List<Vector3> obstructionPoints = new List<Vector3>();
    private List<Vector3> imagePoints = new List<Vector3>();

    public float laserWidth;


    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }


    private void Update()
    {
        ClearMarkers();

        //Laser Setup:
        Vector3 ObjectPostion = transform.position;
        Vector3 ObjectDirection = transform.up;
        float remainingLazerDistance = lazerDistance;

        laserPoints.Add(ObjectPostion);
        List<Collider> lensesHit = new List<Collider>();

        Vector3? previousImage = null;

        //Setting a Distance To Avoid Infinite Looping:
        while (remainingLazerDistance > 0f)
        {
            //Ray Setup:
            Ray ray = new Ray(ObjectPostion, ObjectDirection);
            hits = Physics.RaycastAll(ray, remainingLazerDistance, lensLayer);

            //No Lens Collision:
            if (!ClosestValidHit(hits, lensesHit, out RaycastHit hit))
            {
                laserPoints.Add(ObjectPostion + ObjectDirection * remainingLazerDistance);
                break;
            }


            //Lens Collison:
            lens = hit.collider.GetComponent<Lens>() ?? hit.collider.GetComponentInParent<Lens>();
            if (lens == null) break;

            //Add Lens to a Hit Collection:
            lensesHit.Add(hit.collider);

            //Add Lens Posistion For Markers:
            laserPoints.Add(hit.point);
            obstructionPoints.Add(hit.point);


            //Use image point from previous lens as object:
            Vector3 objectPosForThisLens = previousImage ?? ObjectPostion;


            //When Calculating Image Location:
            if (CalculateImagePoint(objectPosForThisLens, hit.point, lens, out Vector3 calculatedImagePoint))
            {
                //Save Image Point to Use As Object Position For Obstruction:
                previousImage = calculatedImagePoint;

                //Setup Distance Line between hit Location and Image Location:
                Vector3 toImage = calculatedImagePoint - hit.point;
                float toImageDistance = toImage.magnitude;
                Vector3 toImageDir = toImage.normalized;

                //Check For Any Additional Lens Positions Between Hit and Image Positions:
                if (HandleObstructionRecursive(hit.point, toImageDir, toImageDistance, lensesHit, out Vector3 finalImagePoint, out Vector3 nextPosition, out Vector3 nextDirection, out float distanceUsed, previousImage))
                {
                    ImagePoint = finalImagePoint;

                    ObjectPostion = nextPosition;
                    ObjectDirection = nextDirection;

                    remainingLazerDistance -= distanceUsed;
                    continue;
                }

                //No obstruction, Single Lens:
                ImagePoint = calculatedImagePoint;
                imagePoints.Add(ImagePoint);
                laserPoints.Add(ImagePoint);

                ObjectDirection = (ImagePoint - hit.point).normalized;
                ObjectPostion = ImagePoint + ObjectDirection * lazerOffset;

                remainingLazerDistance -= Vector3.Distance(hit.point, ImagePoint);
            }
            else break;
        }

        //Function Used to Display Hit Points & Image Points:
        Visualize();
    }


    private void ClearMarkers()
    {
        foreach (var marker in laserPointMarkers) Destroy(marker);

        laserPointMarkers.Clear();
        obstructionPoints.Clear();
        imagePoints.Clear();
        laserPoints.Clear();
    }


    private bool ClosestValidHit(RaycastHit[] hitArray, List<Collider> lensesHit, out RaycastHit closestHit)
    {
        //Default Parameters:
        closestHit = default;
        float closestDist = Mathf.Infinity;
        bool found = false;

        //Ensure Raycast Hits can only hit the Same Target Once, But Can Hit Previous Lenses:
        foreach (var hit in hitArray)
        {
            if (lensesHit.Count > 0 && hit.collider == lensesHit[lensesHit.Count - 1]) continue;

            if (hit.distance < closestDist)
            {
                closestDist = hit.distance;
                closestHit = hit;
                found = true;
            }
        }

        return found;
    }

    private bool CalculateImagePoint(Vector3 objectPos, Vector3 hitPoint, Lens lens, out Vector3 imagePoint)
    {
        //Default Image Point:
        imagePoint = Vector3.zero;
        if (lens == null) return false;

        //Focal Length:     [Convex = Positive]     [Concave = Negative]
        float f = lens.isConvex ? Mathf.Abs(lens.focalLength) : -Mathf.Abs(lens.focalLength);

        //Object Distance:
        float p = Vector3.Distance(objectPos, hitPoint);

        //Case to avoid 0:
        if (Mathf.Abs(p) < 0.001f || Mathf.Abs(f - p) < 0.001f) return false;

        //Image Distance:
        float i = 1f / ((1f / f) - (1f / p));

        //Intial Object Height [Based on Hit Point]
        float initialHeight = objectPos.y - hitPoint.y;

        //Magnification:
        float magnification = i / p;

        //Image Height:
        float finalHeight = magnification * initialHeight;


        //Final Image Position:
        Vector3 imageDirection = (i >= 0) ? (hitPoint - objectPos).normalized : -(hitPoint - objectPos).normalized;
        Vector3 baseImagePoint = hitPoint + imageDirection * Mathf.Abs(i);

        imagePoint = new Vector3(baseImagePoint.x, hitPoint.y + finalHeight, baseImagePoint.z);

        return true;
    }


    private bool HandleObstructionRecursive(Vector3 currentHitPoint, Vector3 toImageDir, float toImageDistance, List<Collider> lensesHit, out Vector3 finalImagePoint, out Vector3 nextPosition, out Vector3 nextDirection, out float totalDistanceUsed, Vector3? incomingObjectPoint)
    {
        //Default Parameters for outputs:
        finalImagePoint = Vector3.zero;
        nextPosition = currentHitPoint;
        nextDirection = toImageDir;
        totalDistanceUsed = 0f;


        //Ray Setup:
        Ray obstructionRay = new Ray(currentHitPoint, toImageDir);
        RaycastHit[] obstructionHits = Physics.RaycastAll(obstructionRay, toImageDistance, lensLayer);


        //No Lens Collision: [Return False Since This Function is Used to Check Multiple Lens Collisions]
        if (!ClosestValidHit(obstructionHits, lensesHit, out RaycastHit obstructionHit)) return false;
        

        //Lens Collison:
        var nextLens = obstructionHit.collider.GetComponent<Lens>() ?? obstructionHit.collider.GetComponentInParent<Lens>();
        if (nextLens == null) return false;


        //Add Lens to a Hit Collection:
        lensesHit.Add(obstructionHit.collider);

        //Add Lens Posistion For Markers:
        obstructionPoints.Add(obstructionHit.point);
        laserPoints.Add(obstructionHit.point);

        Vector3 objectPos = incomingObjectPoint ?? currentHitPoint;

        //When Calculating Image Location:
        if (CalculateImagePoint(objectPos, obstructionHit.point, nextLens, out Vector3 newImagePoint))
        {
            imagePoints.Add(newImagePoint);
            laserPoints.Add(newImagePoint);

            //Set the laser path:
            Vector3 nextDir = (newImagePoint - obstructionHit.point).normalized;
            Vector3 nextPos = newImagePoint + nextDir * lazerOffset;

            //Set the laser remaining Distance:
            float nextDist = Vector3.Distance(obstructionHit.point, newImagePoint);

            //Recursively check for further obstructions down the new path of "newImagePoint":
            if (HandleObstructionRecursive(obstructionHit.point, nextDir, nextDist, lensesHit, out Vector3 deeperImage, out Vector3 deeperPos, out Vector3 deeperDir, out float deeperUsed, newImagePoint))
            {
                finalImagePoint = deeperImage;
                nextPosition = deeperPos;
                nextDirection = deeperDir;
                totalDistanceUsed = Vector3.Distance(currentHitPoint, obstructionHit.point) + deeperUsed;
                return true;
            }
            else
            {
                //No Recursive Checks, 1 Obstruction:
                finalImagePoint = newImagePoint;
                nextPosition = nextPos;
                nextDirection = nextDir;
                totalDistanceUsed = Vector3.Distance(currentHitPoint, newImagePoint);
                return true;
            }
        }

        return false;
    }


    private void Visualize()
    {
        //Visualization of Line Render For Light Source:
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;

        lineRenderer.positionCount = laserPoints.Count;
        lineRenderer.SetPositions(laserPoints.ToArray());


        //Marker Visualization For Obstruction Points:
        if (obstructionPointMarkerPrefab != null)
        {
            foreach (var point in obstructionPoints)
            {
                GameObject marker = Instantiate(obstructionPointMarkerPrefab, point, Quaternion.identity);
                laserPointMarkers.Add(marker);
            }
        }


        //Marker Visualization For Image Locations:
        if (imagePointMarkerPrefab != null)
        {
            foreach (var point in imagePoints)
            {
                GameObject marker = Instantiate(imagePointMarkerPrefab, point, Quaternion.identity);
                laserPointMarkers.Add(marker);
            }
        }
    }
}

