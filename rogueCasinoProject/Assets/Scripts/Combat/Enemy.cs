using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public static Enemy Instance;

    [Header("Data")]
    public List<EnemyData> possibleEnemies;
    [SerializeField] private EnemyData bossEnemyData;
    public EnemyData data { get; private set; }

    [Header("UI")]
    public Image visualImage;
    public Image healthBarFill;
    public RectTransform visualRect;

    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }

    bool isDead = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (MapState.GetPendingEncounter() == MapState.EncounterType.Boss)
        {
            data = ResolveBossEnemyData();
            if (data == null)
            {
                Debug.LogError("Combat boss demandé mais aucun EnemyData_Boss trouvé. Fallback sur un ennemi aléatoire.");
                data = possibleEnemies[Random.Range(0, possibleEnemies.Count)];
            }
        }
        else
        {
            data = possibleEnemies[Random.Range(0, possibleEnemies.Count)];
        }

        MaxHP = data.GetMaxHP();
        CurrentHP = MaxHP;

        if (visualImage != null && data.sprite != null)
            visualImage.sprite = data.sprite;

        Debug.Log($"[Combat] Ennemi : {data.enemyName} — PV : {MaxHP} | Dégâts : {data.GetDamageMin()}-{data.GetDamageMax()} | Crit : {data.GetCritChance() * 100}%");

        UpdateBar();
    }

    private EnemyData ResolveBossEnemyData()
    {
        if (bossEnemyData != null) return bossEnemyData;

        foreach (EnemyData enemy in possibleEnemies)
        {
            if (enemy != null && enemy.name == "EnemyData_Boss")
                return enemy;
        }

        return null;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        CurrentHP = Mathf.Max(CurrentHP - damage, 0);
        UpdateBar();
        Debug.Log($"[{data.enemyName}] -{damage} PV — {CurrentHP}/{MaxHP}");

        if (CurrentHP <= 0)
            StartCoroutine(DieRoutine());
    }

    public void TakeTurn()
    {
        if (isDead) return;

        int a = Random.Range(data.GetDamageMin(), data.GetDamageMax() + 1);
        int b = Random.Range(data.GetDamageMin(), data.GetDamageMax() + 1);
        int damage = Mathf.Min(a, b);

        bool isCrit = Random.value < data.GetCritChance();
        if (isCrit)
        {
            damage = Mathf.RoundToInt(damage * data.critMultiplier);
            Debug.Log($"[{data.enemyName}] CRITIQUE ! Dégâts : {damage}");
        }
        else
        {
            Debug.Log($"[{data.enemyName}] Dégâts : {damage}");
        }

        Health.Instance.TakeDamage(damage);
    }

    IEnumerator DieRoutine()
    {
        isDead = true;
        Debug.Log($"[{data.enemyName}] est mort.");

        float duration = 0.25f;
        float elapsed = 0f;
        Vector2 startPos = visualRect.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0, -800f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            visualRect.anchoredPosition = Vector2.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        CombatManager.Instance.OnEnemyDied();
    }

    void UpdateBar()
    {
        if (healthBarFill != null)
            healthBarFill.fillAmount = (float)CurrentHP / MaxHP;
    }
}
