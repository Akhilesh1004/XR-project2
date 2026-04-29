using Oculus.Interaction;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayLoopSoundOnGrabOrNear : MonoBehaviour
{
    [Header("Grab Detection")]
    public Grabbable grabbable;

    [Header("Near Detection")]
    public Transform targetObject;
    public float nearDistance = 0.2f;

    [Header("Audio")]
    public AudioSource audioSource;

    [Tooltip("若有指定，播放這個 AudioClip；否則使用 AudioSource 原本的 clip")]
    public AudioClip loopClip;

    [Tooltip("是否在開始時自動找 Grabbable")]
    public bool autoFindGrabbable = true;

    [Tooltip("是否用 2D 距離忽略高度差")]
    public bool ignoreYDistance = false;

    public NoteCompletionManager noteCompletionManager;

    bool count = false;

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (autoFindGrabbable && grabbable == null)
        {
            grabbable = GetComponent<Grabbable>();
        }

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.playOnAwake = false;

            if (loopClip != null)
            {
                audioSource.clip = loopClip;
            }
        }

        

    }

    private void Update()
    {
        Debug.Log(audioSource.clip.loadState);
        bool grabbed = IsGrabbed();
        bool near = IsNearTarget();

        if (grabbed || near)
        {
            Debug.Log("START");
            StartLoopSound();
        }
        else
        {
            Debug.Log("STOP");
            StopLoopSound();
        }
        if (near && !count)
        {
            count = true;
            noteCompletionManager.AddCompletedCount();
        }
        if(!near && count)
        {
            count = false;
            noteCompletionManager.SubCompletedCount();
        }
    }

    private bool IsGrabbed()
    {
        if (grabbable == null) return false;

        return grabbable.SelectingPointsCount > 0;
    }

    private bool IsNearTarget()
    {
        if (targetObject == null) return false;

        Vector3 a = transform.position;
        Vector3 b = targetObject.position;

        if (ignoreYDistance)
        {
            a.y = 0f;
            b.y = 0f;
        }

        float dist = Vector3.Distance(a, b);
        Debug.Log(dist);
        return dist <= nearDistance;
    }

    private void StartLoopSound()
    {
        if (audioSource == null) return;
        if (audioSource.clip == null) return;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            Debug.Log(audioSource.isPlaying);
        }
    }

    private void StopLoopSound()
    {
        if (audioSource == null) return;

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (targetObject == null) return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(targetObject.position, nearDistance);
    }
}