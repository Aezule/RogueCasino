using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class ShowTarot : MonoBehaviour
{
    [SerializeField] private Image[] cardSlots;
    [SerializeField] private Sprite emptySprite;

    void Start()
    {
        RefreshTarotDisplay();
    }

    public void RefreshTarotDisplay()
    {
        if (Inventory.Instance == null) return;

        for (int i = 0; i < cardSlots.Length; i++)
        {
            if (i >= Inventory.Instance.tarotCards.Length)
                break;

            string cardName = Inventory.Instance.tarotCards[i];

            if (string.IsNullOrEmpty(cardName))
            {
                cardSlots[i].sprite = emptySprite;
                cardSlots[i].color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                Sprite cardSprite = Resources.Load<Sprite>("Images/Tarots/" + cardName);

                if (cardSprite != null)
                {
                    cardSlots[i].sprite = cardSprite;
                    cardSlots[i].color = Color.white;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Sprite introuvable : " + cardName);
                    cardSlots[i].sprite = emptySprite;
                    cardSlots[i].color = Color.white;
                }
            }
        }
    }
}