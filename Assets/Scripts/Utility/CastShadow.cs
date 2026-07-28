using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;

public class CastShadow : MonoBehaviour
{
    private DecalProjector decalProjector;
    [SerializeField] private CastShadowData castShadowData;
    
    

// Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       decalProjector = GetComponent<DecalProjector>();
       if (castShadowData == null)
       {
           Debug.LogWarning("CastShadowData is missing");
       }
    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.parent.position, Vector3.down, out hit, castShadowData.shadowDistance, castShadowData.shadowLayerMask))
        {
            decalProjector.enabled = true;
            float distanceToSurface = Vector3.Distance(transform.parent.position, hit.point);
            float sizeMultiplier = Mathf.Lerp(1f, castShadowData.minSizeMultiplier, distanceToSurface / castShadowData.shadowDistance);
            decalProjector.size = new Vector3(castShadowData.shadowSize * sizeMultiplier, castShadowData.shadowSize * sizeMultiplier, decalProjector.size.z);
            decalProjector.fadeFactor = Mathf.Lerp(1f, castShadowData.minFadeMultiplier, distanceToSurface / castShadowData.shadowDistance);
        }
        else
        {
            decalProjector.enabled = false;
        }
    }
}
