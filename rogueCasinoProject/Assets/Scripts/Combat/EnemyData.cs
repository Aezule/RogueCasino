using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "RogueCasino/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Identité")]
    public string enemyName = "Ennemi";
    public Sprite sprite;

    [Header("Stats de base")]
    public int baseHP = 150;
    public int damageMin = 10;
    public int damageMax = 100;
    public float critChance = 0.1f;
    public float critMultiplier = 2f;

    [Header("Modificateurs")]
    public float hpMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float critChanceMultiplier = 1f;

    public int GetMaxHP() => Mathf.RoundToInt(baseHP * hpMultiplier);
    public int GetDamageMin() => Mathf.RoundToInt(damageMin * damageMultiplier);
    public int GetDamageMax() => Mathf.RoundToInt(damageMax * damageMultiplier);
    public float GetCritChance() => critChance * critChanceMultiplier;
}
