using System.Collections;
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
        if (slotIndex < 0 || slotIndex >= slots.Length || card == null)
            return;

        RectTransform cardRect = card.GetComponent<RectTransform>();
        if (cardRect == null)
            return;

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

    public IEnumerator SpawnCardInFirstEmptySlotRoutine(GameObject cardPrefab, CardData data, Sprite backSprite)
    {
        int slotIndex = GetFirstEmptySlotIndex();
        if (slotIndex == -1)
            yield break;

        yield return StartCoroutine(SpawnCardInSlotRoutine(cardPrefab, data, backSprite, slotIndex));
    }

    public IEnumerator SpawnCardInSlotRoutine(GameObject cardPrefab, CardData data, Sprite backSprite, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length)
            yield break;

        if (cardsInSlots[slotIndex] != null)
            yield break;

        GameObject cardObject = Instantiate(cardPrefab);
        Card card = cardObject.GetComponent<Card>();
        if (card == null)
        {
            Destroy(cardObject);
            yield break;
        }

        card.Setup(data, backSprite, slotIndex, true);
        PutCardInSlot(card, slotIndex);

        CanvasGroup cg = card.GetComponent<CanvasGroup>();
        if (cg == null) cg = card.gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;

        if (UICombatAnimations.Instance != null)
            yield return StartCoroutine(UICombatAnimations.Instance.AnimateDrawReveal(card, slots[slotIndex]));

        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
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