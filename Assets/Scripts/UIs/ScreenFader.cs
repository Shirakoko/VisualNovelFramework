using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/** 可淡入淡出的黑幕 */
[RequireComponent(typeof(Image))]
public class ScreenFader : MonoBehaviour
{
    private Image blackScreen;

    private void Awake()
    {
        blackScreen = GetComponent<Image>();
        blackScreen.gameObject.SetActive(false);
        blackScreen.color = new Color(0, 0, 0, 0);
    }

    public IEnumerator FadeOutAndIn(System.Action callback, float duration)
    {
        this.gameObject.SetActive(true);

        yield return StartCoroutine(FadeTo(1f, duration));
        callback?.Invoke();
        yield return StartCoroutine(FadeTo(0f, duration));

        this.gameObject.SetActive(false);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        Color color = blackScreen.color;
        float startAlpha = color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            blackScreen.color = color;
            yield return null;
        }

        // 确保最终达到目标值
        color.a = targetAlpha;
        blackScreen.color = color;
    }
}