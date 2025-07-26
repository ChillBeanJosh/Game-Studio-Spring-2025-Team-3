using NUnit.Framework;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.UI.Image;
using System.Collections.Generic;

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
    public Lens lens;
    public LayerMask lensLayer;
    public bool lensHit;
    public float lazerOffset;

    Vector3 ImagePoint = Vector3.zero;

    [Header("Debug Visualization")]
    public GameObject laserPointMarkerPrefab;
    private List<GameObject> laserPointMarkers = new List<GameObject>();


    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }


    private void Update()
    {
        //Clear Hitmarkers:
        foreach (var marker in laserPointMarkers)
        {
            Destroy(marker);
        }
        laserPointMarkers.Clear();


        //Laser Setup:
        Vector3 ObjectPostion = transform.position;
        Vector3 ObjectDirection = transform.up;
        float remainingLazerDistance = lazerDistance;


        laserPoints = new List<Vector3>();
        laserPoints.Add(ObjectPostion);

        HashSet<Collider> lensesHit = new HashSet<Collider>();

        while (remainingLazerDistance > 0f)
        {
            Ray ray = new Ray(ObjectPostion, ObjectDirection);
            hits = Physics.RaycastAll(ray, remainingLazerDistance, lensLayer);

            foreach (var h in hits)
            {
                Debug.Log($"Raycast hit {h.collider.name} at distance {h.distance}");
            }

            RaycastHit? closestTarget = null;
            float minDistance = Mathf.Infinity;

            //Ensure Raycast Hits can only hit the Same Target Once:
            foreach(var h in hits)
            {
                if (lensesHit.Contains(h.collider)) continue;
           
                if (h.distance < minDistance)
                {
                    closestTarget = h;
                    minDistance = h.distance;
                }
            }

            if (!closestTarget.HasValue)
            {
                laserPoints.Add(ObjectPostion + ObjectDirection * remainingLazerDistance);
                break;
            }

            //Lens Collison:
            RaycastHit hit = closestTarget.Value;
            lens = hit.collider.GetComponent<Lens>() ?? hit.collider.GetComponentInParent<Lens>();
            if (lens == null) break;


            lensesHit.Add(hit.collider);
            laserPoints.Add(hit.point);


            //Focal Length:     [Convex = Positive]     [Concave = Negative]
            float f = lens.isConvex ? Mathf.Abs(lens.focalLength) : -Mathf.Abs(lens.focalLength);

            //Object Distance:
            float p = Vector3.Distance(ObjectPostion, hit.point);


            //Case to avoid 0:
            if (Mathf.Abs(p) < 0.001f && Mathf.Abs(f - p) < 0.001f) break;


            //Image Distance:
            float i = 1f / ((1f / f) - (1f / p));

            //Intial Object Height [Based on Hit Point]
            float initialHeight = ObjectPostion.y - hit.point.y;

            //Magnification:
            float magnification = i / p;

            //Image Height:
            float finalHeight = magnification * initialHeight;


            //Final Image Position:
            Vector3 imageDirection = (i >= 0) ? ObjectDirection : -ObjectDirection;
            Vector3 baseImagePoint = hit.point + imageDirection * Mathf.Abs(i);
            Vector3 tentativeImagePoint = new Vector3(baseImagePoint.x, hit.point.y + finalHeight, baseImagePoint.z);


            //Check For Any Additional Lens Positions Between Hit and Image Positions:
            Vector3 toImage = tentativeImagePoint - hit.point;
            float toImageDistance = toImage.magnitude;
            Ray obstructionRay = new Ray(hit.point, toImage.normalized);


            //If Another Lens is Obstructing the Ray to the Image Position:
            if (Physics.Raycast(obstructionRay, out RaycastHit obstructionHit, toImageDistance, lensLayer))
            {
                laserPoints.Add(obstructionHit.point);

                ObjectDirection = (obstructionHit.point - hit.point).normalized;
                ObjectPostion = obstructionHit.point + ObjectDirection * lazerOffset;
                remainingLazerDistance -= Vector3.Distance(ObjectPostion, obstructionHit.point);
                continue;
            }

            ImagePoint = tentativeImagePoint;
            laserPoints.Add(ImagePoint);

            remainingLazerDistance -= Vector3.Distance(ObjectPostion, ImagePoint);
            ObjectDirection = (ImagePoint - hit.point).normalized;
            ObjectPostion = ImagePoint + ObjectDirection * lazerOffset;
        }

        //Visualization of HitPoints:
        lineRenderer.positionCount = laserPoints.Count;
        lineRenderer.SetPositions(laserPoints.ToArray());

        if (laserPointMarkerPrefab != null)
        {
            foreach (var point in laserPoints)
            {
                GameObject marker = Instantiate(laserPointMarkerPrefab, point, Quaternion.identity);
                laserPointMarkers.Add(marker);
            }
        }
    }
}
