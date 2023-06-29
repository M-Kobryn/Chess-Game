using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeBehavior : MonoBehaviour
{
    public float fadeDuration = 1f;
    public Color initialColor = Color.white;

    public void Fade() 
    {
        StartCoroutine(FadeAsync());
    }
    private IEnumerator FadeAsync()
    {
        Image img = transform.GetComponent<Image>();
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            img.color = Color.Lerp(initialColor, targetColor, elapsedTime / fadeDuration);
            yield return null;
        }
    }
}
