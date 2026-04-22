using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PortalPulse : MonoBehaviour
{
    public float pulseSpeed = 1.5f;
    public float minScale = 1.0f;
    public float maxScale = 1.08f;
    public float emissionMin = 1.2f;
    public float emissionMax = 2.0f;

    private Material mat;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float scale = Mathf.Lerp(minScale, maxScale, t);

        // 以中心縮放
        Vector2 tiling = new Vector2(scale, scale);
        Vector2 offset = (Vector2.one - tiling) * 0.5f;

        mat.SetTextureScale("_BaseMap", tiling);
        mat.SetTextureOffset("_BaseMap", offset);

        if (mat.HasProperty("_EmissionColor"))
        {
            float e = Mathf.Lerp(emissionMin, emissionMax, t);
            mat.SetColor("_EmissionColor", Color.white * e);
        }
    }
}