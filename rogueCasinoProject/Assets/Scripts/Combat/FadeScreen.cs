using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeScreen : MonoBehaviour
{
    public static FadeScreen Instance;

    public Image blackImage;
    private bool isFading;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void FadeToBlack(float duration = 1.5f)
    {
        if (isFading) return;
        StartCoroutine(FadeRoutine(duration));
    }

    public void FadeToBlackAndLoadMenu(float duration = 1.5f)
    {
        if (isFading) return;
        StartCoroutine(FadeAndLoadMenuRoutine(duration));
    }

    IEnumerator FadeRoutine(float duration)
    {
        isFading = true;
        blackImage.gameObject.SetActive(true);

        float elapsed = 0f;
        Color c = blackImage.color;
        c.a = 0f;
        blackImage.color = c;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            blackImage.color = c;
            yield return null;
        }

        isFading = false;
    }

    IEnumerator FadeAndLoadMenuRoutine(float duration)
    {
        isFading = true;
        blackImage.gameObject.SetActive(true);

        float elapsed = 0f;
        Color c = blackImage.color;
        c.a = 0f;
        blackImage.color = c;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Clamp01(elapsed / duration);
            blackImage.color = c;
            yield return null;
        }

        MapState.ClearPendingEncounter();
        SceneManager.LoadScene("Menu");
    }
}
