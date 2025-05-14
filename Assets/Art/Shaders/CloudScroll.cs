using UnityEngine;
using UnityEngine.Rendering.Universal;

// [ExecuteAlways]
public class CloudScroll : MonoBehaviour
{
    UniversalAdditionalLightData lightData;
    public Vector2 speed;

    void Update()
    {
        // If null, assign lightData.

        if (!lightData)
        {
            lightData = GetComponent<UniversalAdditionalLightData>();
        }

        // Scroll UVs of the light cookie texture.

        lightData.lightCookieOffset = speed * Time.time;
    }
}
