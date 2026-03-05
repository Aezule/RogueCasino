using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public CardData data;

    public Image cardFaceImage;
    public Image cardBackImage;
    public GameObject selectionBorder;

    private bool selected = false;
    private bool faceUp = true;

    public void Setup(CardData cardData, Sprite backSprite, bool showFace = true)
    {
        data = cardData;
        faceUp = showFace;

        if (cardFaceImage != null)
            cardFaceImage.sprite = cardData.sprite;

        if (cardBackImage != null)
            cardBackImage.sprite = backSprite;

        UpdateVisualState();
    }

    public void SetFaceUp(bool showFace)
    {
        faceUp = showFace;
        UpdateVisualState();
    }

    public void ToggleSelection()
    {
        selected = !selected;
        UpdateVisualState();
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