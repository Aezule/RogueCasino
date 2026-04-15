using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public List<Sprite> tarotCards;
    public Image[] shopSlots;

    void Start()
    {
        GenerateShop();
    }

    public void GenerateShop()
    {
        UnityEngine.Debug.Log($"tarotCards count = {tarotCards?.Count}");
        UnityEngine.Debug.Log($"shopSlots length = {shopSlots?.Length}");

        if (tarotCards == null || tarotCards.Count == 0)
        {
            UnityEngine.Debug.LogError("ShopManager : tarotCards est vide ou non assigné.");
            return;
        }

        if (shopSlots == null || shopSlots.Length == 0)
        {
            UnityEngine.Debug.LogError("ShopManager : shopSlots est vide ou non assigné.");
            return;
        }

        List<Sprite> available = new List<Sprite>(tarotCards);

        for (int i = 0; i < shopSlots.Length; i++)
        {
            UnityEngine.Debug.Log($"slot {i} = {shopSlots[i]}");

            if (shopSlots[i] == null)
            {
                UnityEngine.Debug.LogError($"ShopManager : shopSlots[{i}] est NULL");
                continue;
            }

            int index = UnityEngine.Random.Range(0, available.Count);
            shopSlots[i].sprite = available[index];
            available.RemoveAt(index);
        }
    }
}