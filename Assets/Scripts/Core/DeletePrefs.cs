using UnityEngine;

public class DeletePrefs : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("PlayerPrefs 已全部刪除");
        }
    }
}