using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public CardData data;

    public Image cardFaceImage;
    public Image cardBackImage;
    public GameObject selectionBorder;

    public int slotIndex = -1;

    private bool selected = false;
    private bool faceUp = true;

    public void Setup(CardData cardData, Sprite backSprite, int newSlotIndex, bool showFace = true)
    {
        data = cardData;
        slotIndex = newSlotIndex;
        faceUp = showFace;
        selected = false;

        if (cardFaceImage != null && cardData != null)
            cardFaceImage.sprite = cardData.sprite;

        if (cardBackImage != null)
            cardBackImage.sprite = backSprite;

        UpdateVisualState();
    }

    public void ShowBack(Sprite backSprite = null)
    {
        faceUp = false;

        if (cardBackImage != null && backSprite != null)
            cardBackImage.sprite = backSprite;

        UpdateVisualState();
    }

    public void ShowFront()
    {
        faceUp = true;
        UpdateVisualState();
    }

    public Sprite GetFrontSprite()
    {
        return data != null ? data.sprite : null;
    }

    public void SetSelected(bool value)
    {
        selected = value;
        UpdateVisualState();
    }

    public bool IsSelected()
    {
        return selected;
    }

    public int GetValue()
    {
        return data != null ? data.value : 0;
    }

    public string GetSuit()
    {
        return data != null ? data.suit : "";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (HandManager.Instance != null)
            HandManager.Instance.ToggleCardSelection(this);
    }

    void UpdateVisualState()
    {
        if (cardFaceImage != null)
            cardFaceImage.gameObject.SetActive(faceUp);

        if (cardBackImage != null)
            cardBackImage.gameObject.SetActive(!faceUp);

        if (selectionBorder != null)
            selectionBorder.SetActive(selected);
    }
}