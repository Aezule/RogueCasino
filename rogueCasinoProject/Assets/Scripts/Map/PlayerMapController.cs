using UnityEngine;
using UnityEngine.SceneManagement;

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
        switch (hex.type)
        {
            case CaseType.Combat:
                Debug.Log("Combat !");
                // Sauvegarder l'état avant de quitter
                MapState.SaveState(mapManager.map, currentCase);
                MapState.SetPendingEncounter(MapState.EncounterType.Combat);
                SceneManager.LoadScene("Combat");
                break;
            case CaseType.Shop:
                Debug.Log("Shop !");
                // Sauvegarder l'état avant de quitter
                MapState.SaveState(mapManager.map, currentCase);
                SceneManager.LoadScene("Shop");
                break;
            case CaseType.Boss:
                Debug.Log("Boss fight !");
                MapState.SaveState(mapManager.map, currentCase);
                MapState.SetPendingEncounter(MapState.EncounterType.Boss);
                SceneManager.LoadScene("Combat");
                break;
        }
    }
}
