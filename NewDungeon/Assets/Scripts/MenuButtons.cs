using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtons : MonoBehaviour
{
    public GameObject loadingBar;
    public Image bar;
    public Material wallMat;

    List<AsyncOperation> scenesLoading = new List<AsyncOperation>();

    private void Awake()
    {
        wallMat.mainTextureScale = new Vector2(1, 1);
    }

    public void StartButton()
    {
        loadingBar.gameObject.SetActive(true);


        scenesLoading.Add(SceneManager.LoadSceneAsync("DungeonScene"));

        StartCoroutine(GetSceneLoadProgress());
    }

    public IEnumerator GetSceneLoadProgress()
    {
        for (int i = 0; i < scenesLoading.Count; i++)
        {
            while (!scenesLoading[i].isDone)
            {
                foreach (AsyncOperation operation in scenesLoading)
                {
                    bar.fillAmount = operation.progress;
                }

                yield return null;
            }
        }

        loadingBar.gameObject.SetActive(false);
    }

    public void GuideButton()
    {

    }

    public void QuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
