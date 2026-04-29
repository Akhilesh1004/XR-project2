using UnityEngine;

public class PlacedNote : MonoBehaviour
{
    public NoteCompletionManager manager;

    private bool counted = false;

    public void SetPlacedCorrectly()
    {
        if (counted) return;

        counted = true;
        Debug.Log(gameObject.name + " 放置成功");

        if (manager != null)
        {
            manager.AddCompletedCount();
        }
        else
        {
            Debug.LogWarning("PlacedNote 的 manager 沒有指定");
        }
    }

    public void ResetPlacedState()
    {
        counted = false;
    }
}