using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;

    [Header("UI")]
    public GameObject cardUiPrefab;
    public TextMeshProUGUI deckCountText;
    public Sprite backSprite;

    [Header("Sprites")]
    public Sprite[] clubs;
    public Sprite[] diamonds;
    public Sprite[] hearts;
    public Sprite[] spades;

    private List<CardData> deck = new List<CardData>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GenerateDeck();
        ShuffleDeck();
        LogDeckInConsole();
        UpdateDeckCountUI();

        StartCoroutine(DrawInitialHandCoroutine());
    }

    void GenerateDeck()
    {
        deck.Clear();

        int[] values = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };

        for (int i = 0; i < 13; i++)
        {
            deck.Add(new CardData(values[i], "Trèfle", clubs[i]));
            deck.Add(new CardData(values[i], "Carreau", diamonds[i]));
            deck.Add(new CardData(values[i], "Coeur", hearts[i]));
            deck.Add(new CardData(values[i], "Pique", spades[i]));
        }
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
    }

    void LogDeckInConsole()
    {
        Debug.Log("Deck mélangé :");

        for (int i = 0; i < deck.Count; i++)
        {
            Debug.Log($"{i} -> {deck[i]}");
        }
    }

    void UpdateDeckCountUI()
    {
        if (deckCountText != null)
            deckCountText.text = deck.Count.ToString();
    }

    CardData DrawTopCard()
    {
        if (deck.Count == 0)
            return null;

        CardData drawnCard = deck[0];
        deck.RemoveAt(0);
        UpdateDeckCountUI();
        return drawnCard;
    }

    IEnumerator DrawInitialHandCoroutine()
    {
        for (int i = 0; i < 7; i++)
        {
            yield return StartCoroutine(DrawOneIntoFirstEmptySlotRoutine());
            yield return new WaitForSeconds(0.08f);
        }
    }

    public void DrawOneIntoFirstEmptySlot()
    {
        StartCoroutine(DrawOneIntoFirstEmptySlotRoutine());
    }

    IEnumerator DrawOneIntoFirstEmptySlotRoutine()
    {
        if (HandManager.Instance == null || !HandManager.Instance.HasEmptySlot())
            yield break;

        CardData drawnCard = DrawTopCard();
        if (drawnCard == null)
            yield break;

        yield return StartCoroutine(
            HandManager.Instance.SpawnCardInFirstEmptySlotRoutine(cardUiPrefab, drawnCard, backSprite)
        );

        Debug.Log("Carte piochée : " + drawnCard);
    }

    public IEnumerator RefillEmptySlotsCoroutine()
    {
        if (HandManager.Instance == null)
            yield break;

        List<int> emptySlots = HandManager.Instance.GetEmptySlotIndices();

        foreach (int slotIndex in emptySlots)
        {
            CardData drawnCard = DrawTopCard();
            if (drawnCard == null)
                yield break;

            yield return StartCoroutine(
                HandManager.Instance.SpawnCardInSlotRoutine(cardUiPrefab, drawnCard, backSprite, slotIndex)
            );

            Debug.Log("Carte repiochée : " + drawnCard);

            yield return new WaitForSeconds(0.08f);
        }
    }
}