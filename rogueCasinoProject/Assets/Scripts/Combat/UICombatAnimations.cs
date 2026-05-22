using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class UICombatAnimations : MonoBehaviour
{
    public static UICombatAnimations Instance;

    [Header("Targets")]
    public RectTransform enemyShakeTarget;
    public RectTransform discardTarget;
    public RectTransform deckTarget;

    [Header("Layers")]
    public RectTransform animationLayer;

    [Header("Draw Visual")]
    public Image cardVisualPrefab;
    public Sprite[] cardBackSprites;

    void Awake()
    {
        Instance = this;
    }

    public IEnumerator AnimateDrawReveal(Card realCard, RectTransform slotTarget)
    {
        if (realCard == null || slotTarget == null || deckTarget == null || animationLayer == null || cardVisualPrefab == null)
            yield break;

        CanvasGroup realCg = realCard.GetComponent<CanvasGroup>();
        if (realCg == null) realCg = realCard.gameObject.AddComponent<CanvasGroup>();

        realCg.alpha = 0f;
        realCg.blocksRaycasts = false;
        realCg.interactable = false;

        Sprite frontSprite = GetFrontSprite(realCard);
        Sprite backSprite = GetRandomBackSprite();
        if (backSprite == null)
            yield break;

        Image fake = Instantiate(cardVisualPrefab, animationLayer);
        RectTransform fakeRt = fake.GetComponent<RectTransform>();

        fake.sprite = backSprite;
        fake.preserveAspect = true;
        fake.raycastTarget = false;

        fakeRt.position = deckTarget.position;
        fakeRt.rotation = Quaternion.identity;
        fakeRt.localScale = Vector3.one * 0.82f;
        fakeRt.SetAsLastSibling();

        Vector3 startPos = deckTarget.position;
        Vector3 endPos = slotTarget.position + new Vector3(0f, 18f, 0f);

        float moveDuration = 0.22f;
        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / moveDuration);
            float eased = EaseOutCubic(p);

            fakeRt.position = Vector3.Lerp(startPos, endPos, eased);
            fakeRt.localScale = Vector3.Lerp(Vector3.one * 0.82f, Vector3.one, eased);

            yield return null;
        }

        yield return StartCoroutine(FlipFakeCard(fakeRt, fake, frontSprite, 0.16f));

        realCg.alpha = 1f;
        realCg.blocksRaycasts = true;
        realCg.interactable = true;

        Destroy(fake.gameObject);
    }

    Sprite GetFrontSprite(Card card)
    {
        Image img = card.GetComponent<Image>();
        if (img != null && img.sprite != null)
            return img.sprite;

        Image childImg = card.GetComponentInChildren<Image>();
        if (childImg != null)
            return childImg.sprite;

        return null;
    }

    Sprite GetRandomBackSprite()
    {
        if (cardBackSprites == null || cardBackSprites.Length == 0)
            return null;

        return cardBackSprites[Random.Range(0, cardBackSprites.Length)];
    }

    IEnumerator FlipFakeCard(RectTransform rt, Image img, Sprite revealSprite, float duration)
    {
        if (rt == null || img == null)
            yield break;

        Vector3 baseScale = rt.localScale;
        float half = duration * 0.5f;
        float t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / half);
            float eased = EaseInOutCubic(p);

            float x = Mathf.Lerp(baseScale.x, 0.04f, eased);
            rt.localScale = new Vector3(x, baseScale.y, baseScale.z);

            yield return null;
        }

        if (revealSprite != null)
            img.sprite = revealSprite;

        t = 0f;

        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / half);
            float eased = EaseInOutCubic(p);

            float x = Mathf.Lerp(0.04f, baseScale.x, eased);
            rt.localScale = new Vector3(x, baseScale.y, baseScale.z);

            yield return null;
        }

        rt.localScale = baseScale;
    }

    public IEnumerator AnimateSend(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            yield break;

        foreach (Card card in cards)
        {
            if (card == null) continue;

            RectTransform rt = card.GetComponent<RectTransform>();
            if (rt == null) continue;

            StartCoroutine(MoveAndFade(rt, rt.position, rt.position + new Vector3(0f, 220f, 0f), 0.28f));
        }

        yield return new WaitForSeconds(0.30f);

        if (enemyShakeTarget != null)
            yield return StartCoroutine(Shake(enemyShakeTarget, 0.22f, 18f));
    }

    public IEnumerator AnimateDiscard(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            yield break;

        foreach (Card card in cards)
        {
            if (card == null) continue;

            RectTransform rt = card.GetComponent<RectTransform>();
            if (rt == null) continue;

            Vector3 start = rt.position;
            Vector3 end = discardTarget != null ? discardTarget.position : start + new Vector3(0f, -220f, 0f);

            StartCoroutine(MoveAndFade(rt, start, end, 0.25f));
        }

        yield return new WaitForSeconds(0.28f);
    }

    IEnumerator MoveAndFade(RectTransform rt, Vector3 start, Vector3 end, float duration)
    {
        if (rt == null) yield break;

        CanvasGroup cg = rt.GetComponent<CanvasGroup>();
        if (cg == null) cg = rt.gameObject.AddComponent<CanvasGroup>();

        cg.blocksRaycasts = false;
        cg.interactable = false;

        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = EaseOutCubic(p);

            rt.position = Vector3.Lerp(start, end, eased);
            cg.alpha = Mathf.Lerp(1f, 0f, eased);

            yield return null;
        }

        rt.position = end;
        cg.alpha = 0f;
    }

    IEnumerator Shake(RectTransform target, float duration, float strength)
    {
        if (target == null) yield break;

        Vector2 original = target.anchoredPosition;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float damper = 1f - Mathf.Clamp01(t / duration);

            float x = Random.Range(-strength, strength) * damper;
            float y = Random.Range(-strength * 0.4f, strength * 0.4f) * damper;

            target.anchoredPosition = original + new Vector2(x, y);
            yield return null;
        }

        target.anchoredPosition = original;
    }

    float EaseOutCubic(float x)
    {
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    float EaseInOutCubic(float x)
    {
        return x < 0.5f
            ? 4f * x * x * x
            : 1f - Mathf.Pow(-2f * x + 2f, 3f) / 2f;
    }
}