using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class Health : MonoBehaviour
{
    public static Health Instance;

    [Header("Config")]
    public CombatConfig config;

    [Header("UI")]
    public Image fillImage;

    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }

    private static bool initialized;
    private static int savedCurrentHP;
    private static int savedMaxHP;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (!initialized)
        {
            MaxHP = config.playerBaseHP;
            CurrentHP = MaxHP;

            savedMaxHP = MaxHP;
            savedCurrentHP = CurrentHP;
            initialized = true;
        }
        else
        {
            MaxHP = savedMaxHP;
            CurrentHP = savedCurrentHP;
        }
    }

    void Start()
    {
        UpdateBar();
    }

    public void SetUI(Image newFillImage)
    {
        fillImage = newFillImage;
        UpdateBar();
    }

    public void TakeDamage(int damage)
    {
        CurrentHP = Mathf.Max(CurrentHP - damage, 0);
        savedCurrentHP = CurrentHP;
        UpdateBar();
        Debug.Log($"HP joueur : {CurrentHP}/{MaxHP}");

        if (CurrentHP <= 0)
            StartCoroutine(GameOverRoutine());
    }

    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        savedCurrentHP = CurrentHP;
        UpdateBar();
    }

    public void ResetHealthToFull()
    {
        CurrentHP = MaxHP;
        savedCurrentHP = CurrentHP;
        UpdateBar();
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(0.8f);

        initialized = false;
        savedCurrentHP = 0;
        savedMaxHP = 0;

        SceneManager.LoadScene("Menu");
    }

    void UpdateBar()
    {
        if (fillImage != null && MaxHP > 0)
            fillImage.fillAmount = (float)CurrentHP / MaxHP;
    }
}