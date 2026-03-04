using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    public static Score Instance;
    
    [Header("Config")]
    public int currentScore = 0;
    public int bestScore = 0;
    public Text scoreUI;     // "Score: 1250" (drag)
    public Text bestUI;      // "Best: 5000" (drag)
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadScores();
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
    
    // AJOUTER score
    public void AddScore(int points)
    {
        currentScore += points;
        if (currentScore > bestScore) bestScore = currentScore;
        
        UpdateUI();
        SaveScores();
        Debug.Log($" +{points} pts. Score: {currentScore} | Best: {bestScore}");
    }
    
    // RESET score (ex: nouvelle partie)
    public void ResetScore()
    {
        currentScore = 0;
        UpdateUI();
    }
    
    // UPDATE UI
    void UpdateUI()
    {
        if (scoreUI) scoreUI.text = $"Score: {currentScore}";
        if (bestUI) bestUI.text = $"Best: {bestScore}";
    }
    
    // SAUVEGARDE
    void SaveScores()
    {
        PlayerPrefs.SetInt("BestScore", bestScore);
        PlayerPrefs.SetInt("CurrentScore", currentScore);
    }
    void LoadScores()
    {
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        currentScore = PlayerPrefs.GetInt("CurrentScore", 0);
    }
}
