using UnityEngine;

public class LightTurnOnWhenPlayerNear : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Distance")]
    public float triggerDistance = 3f;

    [Header("Light")]
    public Light targetLight;

    [Tooltip("是否同時開啟本物件上的 Renderer（如果有的話）")]
    public bool enableRendererToo = false;

    private bool hasTurnedOn = false;
    private Renderer[] renderers;

    private void Awake()
    {
        if (targetLight == null)
        {
            targetLight = GetComponent<Light>();
        }

        if (player == null)
        {
            OVRCameraRig rig = FindObjectOfType<OVRCameraRig>();
            if (rig != null)
            {
                player = rig.centerEyeAnchor;
            }
        }

        if (enableRendererToo)
        {
            renderers = GetComponentsInChildren<Renderer>(true);
        }

        if (targetLight != null)
        {
            targetLight.enabled = false;
        }

        if (enableRendererToo && renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }
    }

    private void Update()
    {
        if (hasTurnedOn) return;
        if (player == null || targetLight == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= triggerDistance)
        {
            TurnOnLight();
        }
    }

    private void TurnOnLight()
    {
        hasTurnedOn = true;
        targetLight.enabled = true;

        if (enableRendererToo && renderers != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = true;
            }
        }

        Debug.Log("Light turned on permanently.");
    }
}