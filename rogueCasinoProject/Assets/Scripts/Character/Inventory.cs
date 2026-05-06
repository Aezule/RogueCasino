using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    public int maxTarotCard = 3;
    public string[] tarotCards;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            tarotCards = new string[maxTarotCard];
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool AddCard(string cardName)
    {
        if (string.IsNullOrEmpty(cardName))
            return false;

        for (int i = 0; i < tarotCards.Length; i++)
        {
            if (string.IsNullOrEmpty(tarotCards[i]))
            {
                tarotCards[i] = cardName;
                Debug.Log(cardName + " ajouté à l'inventaire.");
                PrintInventory();
                return true;
            }
        }

        Debug.Log("Inventaire plein.");
        return false;
    }

    public void PrintInventory()
    {
        string result = "Inventaire : ";

        for (int i = 0; i < tarotCards.Length; i++)
        {
            result += "[" + i + "] " + (string.IsNullOrEmpty(tarotCards[i]) ? "vide" : tarotCards[i]) + " ";
        }

        Debug.Log(result);
    }
}