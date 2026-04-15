using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatConfig", menuName = "RogueCasino/CombatConfig")]
public class CombatConfig : ScriptableObject
{
    [Header("Joueur")]
    public int playerBaseHP = 200;
    public int actionsPerTurn = 3;
    public float playerCritChance = 0.1f;
    public float playerCritMultiplier = 2f;

    [Header("Multiplicateurs de combo")]
    public float globalDamageMultiplier = 1f;
    public List<ComboMultiplier> comboMultipliers = new List<ComboMultiplier>();

    public float GetComboMultiplier(ComboType combo)
    {
        foreach (var entry in comboMultipliers)
            if (entry.combo == combo)
                return entry.multiplier;
        return 1f;
    }
}

[System.Serializable]
public class ComboMultiplier
{
    public ComboType combo;
    public float multiplier = 1f;
}
