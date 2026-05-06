using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TarotBuyCard : MonoBehaviour, IPointerClickHandler
{
    public float buyScale = 1.35f;
    public float disappearDuration = 0.35f;

    private Image cardImage;
    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Shadow shadow;
    private TarotHoverVisual hoverVisual;
    private bool isBought = false;

    void Start()
    {
        cardImage = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        shadow = GetComponent<Shadow>();
        hoverVisual = GetComponent<TarotHoverVisual>();

        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (shadow == null)
        {
            shadow = gameObject.AddComponent<Shadow>();
            shadow.effectDistance = new Vector2(8, -8);
            shadow.effectColor = new Color(0f, 0f, 0f, 0f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isBought) return;

        if (Inventory.Instance == null)
        {
            Debug.LogWarning("Aucun Inventory dans la scène.");
            return;
        }

        if (cardImage == null || cardImage.sprite == null)
        {
            Debug.LogWarning("Aucun sprite sur cette carte.");
            return;
        }

        string cardName = cardImage.sprite.name;
        bool added = Inventory.Instance.AddCard(cardName);

        if (!added)
            return;

        isBought = true;

        if (hoverVisual != null)
            hoverVisual.DisableHover();

        PlayBuyAnimation(cardName);
    }

    private void PlayBuyAnimation(string cardName)
    {
        DOTween.Kill(transform);
        DOTween.Kill(shadow);

        Sequence seq = DOTween.Sequence();

        seq.Append(rect.DOScale(buyScale, 0.12f));
        seq.Append(rect.DOScale(0f, disappearDuration).SetEase(Ease.InBack));
        seq.Join(canvasGroup.DOFade(0f, disappearDuration));
        seq.Join(rect.DORotate(new Vector3(0f, 0f, 15f), disappearDuration));

        seq.OnStart(() =>
        {
            shadow.effectColor = new Color(1f, 0.84f, 0f, 1f);
            Debug.Log("Acheté : " + cardName);
        });

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }
}