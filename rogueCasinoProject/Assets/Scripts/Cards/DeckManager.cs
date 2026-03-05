using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform playerHand;
    public GameObject cardUiPrefab;
    public TextMeshProUGUI deckCountText;

    [Header("Card Sprites")]
    public Sprite backSprite;
    public Sprite[] clubs;
    public Sprite[] diamonds;
    public Sprite[] hearts;
    public Sprite[] spades;

    [Header("Hand Settings")]
    public int startingHandSize = 7;
    public float cardSpacing = 90f;

    private List<CardData> deck = new List<CardData>();

    void Start()
    {
        GenerateDeck();
        ShuffleDeck();
        LogDeckInConsole();
        UpdateDeckCountUI();
        DrawStartingHand();
    }

    void GenerateDeck()
    {
        deck.Clear();

        for (int i = 0; i < 13; i++)
        {
            deck.Add(new CardData(i + 1, "Trèfle", clubs[i]));
            deck.Add(new CardData(i + 1, "Carreau", diamonds[i]));
            deck.Add(new CardData(i + 1, "Coeur", hearts[i]));
            deck.Add(new CardData(i + 1, "Pique", spades[i]));
        }

        Debug.Log("Deck généré : " + deck.Count + " cartes.");
    }

    void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            int randomIndex = Random.Range(i, deck.Count);
            CardData temp = deck[i];
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }

        Debug.Log("Deck mélangé.");
    }

    void LogDeckInConsole()
    {
        Debug.Log("Contenu du deck après mélange :");

        for (int i = 0; i < deck.Count; i++)
        {
            Debug.Log((i + 1) + " - " + deck[i].ToString());
        }
    }

    void UpdateDeckCountUI()
    {
        if (deckCountText != null)
            deckCountText.text = deck.Count.ToString();
    }

    void DrawStartingHand()
    {
        for (int i = 0; i < startingHandSize; i++)
        {
            DrawOneCard();
        }
    }

    public void DrawOneCard()
    {
        if (deck.Count == 0)
        {
            Debug.Log("Le deck est vide.");
            return;
        }

        CardData drawnCard = deck[0];
        deck.RemoveAt(0);

        GameObject newCardObject = Instantiate(cardUiPrefab, playerHand);
        Card newCard = newCardObject.GetComponent<Card>();

        newCard.Setup(drawnCard, backSprite, true);

        ArrangeHand();
        UpdateDeckCountUI();

        Debug.Log("Carte piochée : " + drawnCard.ToString());
    }

    void ArrangeHand()
    {
        int cardCount = playerHand.childCount;
        if (cardCount == 0) return;

        float totalWidth = (cardCount - 1) * cardSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < cardCount; i++)
        {
            RectTransform cardRect = playerHand.GetChild(i).GetComponent<RectTransform>();
            if (cardRect != null)
            {
                cardRect.anchoredPosition = new Vector2(startX + i * cardSpacing, 0f);
                cardRect.localScale = Vector3.one;
                cardRect.localRotation = Quaternion.identity;
            }
        }
    }
}