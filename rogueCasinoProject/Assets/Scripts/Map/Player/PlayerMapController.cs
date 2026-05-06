using UnityEngine;

public class PlayerMapController : MonoBehaviour
{
    public MapManager mapManager;
    public HexCase currentCase;

    public void InitAtStart(HexCase start)
    {
        currentCase = start;
        currentCase.isVisited = true;
        Debug.Log("Player start on: " + currentCase.type);
    }

    public void TryMoveTo(HexCase target)
    {
        if (currentCase == null)
        {
            Debug.LogWarning("currentCase is null — assign Player in MapManager Inspector !");
            return;
        }

        // Mouvement forward-only : le voisin doit être devant (depth supérieur)
        if (currentCase.neighbors.Contains(target) && target.depth > currentCase.depth)
        {
            currentCase.isVisited = true;
            currentCase = target;
            currentCase.isVisited = true;
            Debug.Log($"→ {target.type} (depth {target.depth})");
            TriggerCaseEvent(target);
        }
        else
        {
            Debug.Log("Mouvement invalide");
        }
    }

    void TriggerCaseEvent(HexCase hex)
    {
        float hp  = mapManager.GetScaledHP(hex);
        float dmg = mapManager.GetScaledDamage(hex);

        switch (hex.type)
        {
            case CaseType.Combat:
                Debug.Log($"Combat ! Ennemi HP={hp:F0} DMG={dmg:F0}");
                break;
            case CaseType.Casino:
                Debug.Log("Shop !");
                break;
            case CaseType.Shop:
                Debug.Log("Shop !");
                break;
            case CaseType.Event:
                Debug.Log($"Combat ! Ennemi HP={hp:F0} DMG={dmg:F0}");
                break;
            case CaseType.Boss:
                Debug.Log($"Boss fight ! HP={hp:F0} DMG={dmg:F0}");
                // TODO (Léo) : lancer le combat de boss
                // Après victoire, appeler : mapManager.OnBossDefeated();
                // Décommenter pour tester :
                // mapManager.OnBossDefeated();
                break;
        }
    }
}
