using UnityEngine;

public class PlacedNote : MonoBehaviour
{
    public NoteCompletionManager manager;

    private bool counted = false;

    public void SetPlacedCorrectly()
    {
        if (counted) return;

        counted = true;
        Debug.Log(gameObject.name + " ��m���");

        if (manager != null)
        {
            manager.AddCompletedCount();
        }
        else
        {
            Debug.LogWarning("PlacedNote �� manager �S�����w");
        }
    }

    public void ResetPlacedState()
    {
        counted = false;
    }
}