using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform hand;

    public void DrawCard()
    {
        GameObject card = Instantiate(cardPrefab);
        card.transform.SetParent(hand);
    }
}