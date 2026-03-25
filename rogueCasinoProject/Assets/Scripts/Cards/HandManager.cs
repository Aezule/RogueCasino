using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance;

    [Header("Slots fixes")]
    public RectTransform[] slots = new RectTransform[7];

    private Card[] cardsInSlots = new Card[7];
    private List<Card> selectedCards = new List<Card>();

    void Awake()
    {
        Instance = this;
    }

    public bool HasEmptySlot()
    {
        for (int i = 0; i < cardsInSlots.Length; i++)
        {
            if (cardsInSlots[i] == null)
                return true;
        }

        return false;
    }

    public int GetFirstEmptySlotIndex()
    {
        for (int i = 0; i < cardsInSlots.Length; i++)
        {
            if (cardsInSlots[i] == null)
                return i;
        }

        return -1;
    }

    public void PutCardInSlot(Card card, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            return;

        if (cardsInSlots[slotIndex] != null)
            return;

        RectTransform cardRect = card.GetComponent<RectTransform>();
        cardRect.SetParent(slots[slotIndex], false);

        cardRect.anchorMin = Vector2.zero;
        cardRect.anchorMax = Vector2.one;
        cardRect.offsetMin = Vector2.zero;
        cardRect.offsetMax = Vector2.zero;
        cardRect.anchoredPosition = Vector2.zero;
        cardRect.localScale = Vector3.one;
        cardRect.localRotation = Quaternion.identity;

        card.slotIndex = slotIndex;
        cardsInSlots[slotIndex] = card;
    }

    public void SpawnCardInFirstEmptySlot(GameObject cardPrefab, CardData data, Sprite backSprite)
    {
        int slotIndex = GetFirstEmptySlotIndex();
        if (slotIndex == -1)
            return;

        GameObject cardObject = Instantiate(cardPrefab);
        Card card = cardObject.GetComponent<Card>();
        card.Setup(data, backSprite, slotIndex, true);

        PutCardInSlot(card, slotIndex);
    }

    public void ToggleCardSelection(Card card)
    {
        if (card == null)
            return;

        if (card.IsSelected())
        {
            card.SetSelected(false);
            selectedCards.Remove(card);
            return;
        }

        if (selectedCards.Count >= 5)
            return;

        card.SetSelected(true);
        selectedCards.Add(card);
    }

    public List<Card> GetSelectedCards()
    {
        return new List<Card>(selectedCards);
    }

    public void RemoveSelectedCards()
    {
        List<Card> toRemove = new List<Card>(selectedCards);

        foreach (Card card in toRemove)
        {
            if (card == null)
                continue;

            int slotIndex = card.slotIndex;

            if (slotIndex >= 0 && slotIndex < cardsInSlots.Length)
                cardsInSlots[slotIndex] = null;

            Destroy(card.gameObject);
        }

        selectedCards.Clear();
    }

    public List<int> GetEmptySlotIndices()
    {
        List<int> result = new List<int>();

        for (int i = 0; i < cardsInSlots.Length; i++)
        {
            if (cardsInSlots[i] == null)
                result.Add(i);
        }

        return result;
    }

    public bool IsSelectionEmpty()
    {
        return selectedCards.Count == 0;
    }
}