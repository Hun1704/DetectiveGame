using UnityEngine;
using System.Collections;

public class ChapterEndPanel : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    void OnEnable()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        canvasGroup.blocksRaycasts = true;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = t;
            yield return null;
        }
        canvasGroup.alpha = 1;
    }
}
