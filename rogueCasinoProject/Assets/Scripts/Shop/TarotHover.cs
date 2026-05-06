using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TarotHoverVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.2f;
    public float rotateAmount = 5f;

    private RectTransform rect;
    private Image cardImage;
    private Shadow shadow;
    private Vector3 originalScale;
    private Vector3 originalRotation;
    private bool disabledHover = false;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        cardImage = GetComponent<Image>();
        shadow = GetComponent<Shadow>();

        originalScale = rect.localScale;
        originalRotation = rect.localEulerAngles;

        if (cardImage != null)
            cardImage.color = Color.white;

        if (shadow == null)
        {
            shadow = gameObject.AddComponent<Shadow>();
            shadow.effectDistance = new Vector2(8, -8);
            shadow.effectColor = new Color(0f, 0f, 0f, 0f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (disabledHover) return;

        DOTween.Kill(transform);
        DOTween.Kill(shadow);

        rect.DOScale(hoverScale, 0.2f);
        rect.DORotate(originalRotation + Vector3.forward * rotateAmount, 0.5f).SetLoops(-1, LoopType.Yoyo);
        shadow.effectColor = new Color(0f, 1f, 1f, 0.8f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (disabledHover) return;

        DOTween.Kill(transform);
        DOTween.Kill(shadow);

        rect.DOScale(originalScale, 0.2f);
        rect.DORotate(originalRotation, 0.3f);
        shadow.effectColor = new Color(0f, 0f, 0f, 0f);
    }

    public void DisableHover()
    {
        disabledHover = true;

        DOTween.Kill(transform);
        DOTween.Kill(shadow);

        rect.localScale = originalScale;
        rect.localEulerAngles = originalRotation;
        shadow.effectColor = new Color(0f, 0f, 0f, 0f);
    }
}