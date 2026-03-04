using UnityEngine;
using UnityEngine.UI;  // Pour Text UI

public class Economy : MonoBehaviour
{
    public static Economy Instance;  // Accès depuis n'importe où
    
    [Header("Config")]
    public int coins = 0;            // Nombre de pièces
    public Text coinsUI;             // Texte UI "Pièces: 100" (drag dans Inspector)
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Persiste entre scènes
            LoadCoins();  // Charge sauvegarde
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        UpdateUI();
    }
    
    // AJOUTER des pièces (ex: ramasser coin)
    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
        SaveCoins();
        Debug.Log($" +{amount} pièces ! Total: {coins}");
    }
    
    // ENLEVER des pièces (ex: achat)
    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            UpdateUI();
            SaveCoins();
            Debug.Log($" -{amount} pièces. Reste: {coins}");
            return true;  // OK
        }
        Debug.Log("Pas assez de pièces !");
        return false;  // Échec
    }
    
    // UPDATE UI
    void UpdateUI()
    {
        if (coinsUI) coinsUI.text = $"Pièces: {coins}";
    }
    
    // SAUVEGARDE
    void SaveCoins() => PlayerPrefs.SetInt("Coins", coins);
    void LoadCoins() => coins = PlayerPrefs.GetInt("Coins", 0);
}