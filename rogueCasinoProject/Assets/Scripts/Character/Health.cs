using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public static Health Instance;

    [Header("Config")]
    public CombatConfig config;

    [Header("UI")]
    public Image fillImage;

    public int MaxHP { get; private set; }
    public int CurrentHP { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        MaxHP = config.playerBaseHP;
        CurrentHP = MaxHP;
        UpdateBar();
    }

    public void TakeDamage(int damage)
    {
        CurrentHP = Mathf.Max(CurrentHP - damage, 0);
        UpdateBar();
        Debug.Log($"[Joueur] -{damage} PV — {CurrentHP}/{MaxHP}");

        if (CurrentHP <= 0)
        {
            Debug.Log("[Joueur] MORT");
            StartCoroutine(GameOverRoutine());
        }
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(0.8f);
        FadeScreen.Instance.FadeToBlack();
    }


    public void Heal(int amount)
    {
        CurrentHP = Mathf.Min(CurrentHP + amount, MaxHP);
        UpdateBar();
        Debug.Log($"[Joueur] +{amount} PV — {CurrentHP}/{MaxHP}");
    }

    void UpdateBar()
    {
        if (fillImage != null)
            fillImage.fillAmount = (float)CurrentHP / MaxHP;
    }
}
