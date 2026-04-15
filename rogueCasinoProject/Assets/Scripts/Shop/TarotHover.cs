using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Net.Mime;

public class TarotHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverScale = 1.2f;

    private Image cardImage;
    private RectTransform rect;
    private Shadow shadow; // Remplace Outline
    private Vector3 originalScale;
    private Vector3 originalRotation;

    void Start()
    {
        cardImage = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        shadow = GetComponent<Shadow>();

        originalScale = rect.localScale;
        originalRotation = rect.localEulerAngles;

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
        DOTween.Kill(transform);
        DOTween.Kill(shadow);

        rect.DOScale(hoverScale, 0.2f);
        rect.DORotate(originalRotation + Vector3.forward * 5, 0.5f).SetLoops(-1, LoopType.Yoyo);

        // CYAN GLOW #00FFFF
        shadow.effectColor = new Color(0f, 1f, 1f, 0.8f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        DOTween.Kill(transform);
        DOTween.Kill(shadow);

        rect.DOScale(originalScale, 0.2f);
        rect.DORotate(originalRotation, 0.3f);
        shadow.effectColor = new Color(0f, 0f, 0f, 0f);
    }
}