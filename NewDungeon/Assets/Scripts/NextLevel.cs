using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextLevel : MonoBehaviour
{
    private Image fadeImage;

    private void Start()
    {
        fadeImage = GameObject.FindGameObjectWithTag("FadeTag").GetComponent<Image>();
    }

    private void OnTriggerEnter(Collider other)
    {
        StartCoroutine(FadeToNew());
    }

    private IEnumerator FadeToNew()
    {
        for (float i = 0; i <= 1; i += Time.deltaTime)
        {
            fadeImage.color = new Color(1, 1, 1, i);
            yield return null;
        }

        DungeonCreator.current.CreateDungeon(true);
    }
}
