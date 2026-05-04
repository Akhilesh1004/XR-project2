using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AdditiveSceneLoader : MonoBehaviour
{
    public string[] scenesToLoad =
    {
        "SubScene_01",
        "SubScene_02",
        "SubScene_03"
    };

    private IEnumerator Start()
    {
        Debug.Log("AdditiveSceneLoader 啟動");

        foreach (string sceneName in scenesToLoad)
        {
            Debug.Log("準備載入：" + sceneName);

            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                Debug.Log(sceneName + " 已經載入，跳過");
                continue;
            }

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            if (op == null)
            {
                Debug.LogError("載入失敗：" + sceneName + "，請確認名稱與 Build Settings");
                continue;
            }

            while (!op.isDone)
            {
                yield return null;
            }

            Debug.Log("載入完成：" + sceneName);
        }

        Debug.Log("三個 SubScene 都載入完成");
    }
}