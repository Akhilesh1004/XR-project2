using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class SceneDistortionController : MonoBehaviour
{
    [Header("Effects settings")]
    public Volume globalVolume;
    
    [Header("Distortion settings")]
    public float maxDistortion = 0.5f;
    public float distortionSpeed = 0.05f;
    
    [Header("Recovery settings")]
    public bool instantRecover = true;
    public float recoverSpeed = 0.5f;

    private LensDistortion lensDistortion;
    private bool isDistorting = false;
    private bool isRecovering = false;

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void Start()
    {
        if (globalVolume != null)
        {
            globalVolume.profile.TryGet(out lensDistortion);
            if (lensDistortion != null)
            {
                lensDistortion.intensity.Override(0f);
            }
        }

        if (SceneManager.GetActiveScene() == gameObject.scene)
        {
            StartDistortion();
        }
    }

    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        if (next == gameObject.scene)
        {
            StartDistortion();
        }
    }

    private void StartDistortion()
    {
        Debug.Log("Scene is actived, distortion starting");
        isDistorting = true;
        isRecovering = false;
    }

    public void OnGrabbed()
    {
        isDistorting = false;

        if (instantRecover && lensDistortion != null)
        {
            lensDistortion.intensity.Override(0f);
            isRecovering = false;
            Debug.Log("Item been grabbed, scene recover instantly");
        }
        else
        {
            isRecovering = true;
            Debug.Log("Item been grabbed, scene recover slowly");
        }
    }

    void Update()
    {
        if (lensDistortion == null) return;

        float currentIntensity = lensDistortion.intensity.value;

        if (isDistorting && currentIntensity != maxDistortion)
        {
            float newValue = Mathf.MoveTowards(currentIntensity, maxDistortion, distortionSpeed * Time.deltaTime);
            lensDistortion.intensity.Override(newValue);
        }
        else if (isRecovering && currentIntensity != 0f)
        {
            float newValue = Mathf.MoveTowards(currentIntensity, 0f, recoverSpeed * Time.deltaTime);
            lensDistortion.intensity.Override(newValue);
            
            if (newValue == 0f) isRecovering = false;
        }
    }
}