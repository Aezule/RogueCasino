using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public static Health Instance;
    
    [Header("Config")]
    public int maxHealth = 100;
    public int currentHealth = 100;
    public Text healthUI;  // "Vie: 75/100" (drag dans Inspector)
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadHealth();
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
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
    
    // AJOUTER vie (ex: soin)
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateUI();
        SaveHealth();
        Debug.Log($" +{amount} PV. Vie: {currentHealth}/{maxHealth}");
    }
    
    // ENLEVER vie (ex: dégâts)
    public bool TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);  // Pas négatif
        UpdateUI();
        SaveHealth();
        
        if (currentHealth <= 0)
        {
            Debug.Log("t es mort sale merde");
            // GameOver();
            return false;
        }
        Debug.Log($" -{damage} PV. Vie: {currentHealth}/{maxHealth}");
        return true;
    }
    
    // UPDATE UI
    void UpdateUI()
    {
        if (healthUI) healthUI.text = $"Vie: {currentHealth}/{maxHealth}";
    }
    
    // SAUVEGARDE
    void SaveHealth() => PlayerPrefs.SetInt("Health", currentHealth);
    void LoadHealth() => currentHealth = PlayerPrefs.GetInt("Health", maxHealth);
}
