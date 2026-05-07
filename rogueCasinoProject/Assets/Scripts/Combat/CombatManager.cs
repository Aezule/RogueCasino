using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public enum CombatState { PLAYER_TURN, ENEMY_TURN }

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Config")]
    public CombatConfig config;

    public CombatState State { get; private set; }
    public int ActionsLeft { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartPlayerTurn();
    }

    void StartPlayerTurn()
    {
        State = CombatState.PLAYER_TURN;
        ActionsLeft = config.actionsPerTurn;
        
        // affiche le tour du joueur
        if (TurnDisplay.Instance != null)
            TurnDisplay.Instance.UpdateTurnDisplay(ActionsLeft, config.actionsPerTurn);
        
        Debug.Log($"Tour Joueur — {ActionsLeft} actions ===");
    }

    public void PlayerUsedAction()
    {
        if (State != CombatState.PLAYER_TURN) return;

        ActionsLeft--;
        
        // update l'affichage de l'affichage du tiour du jouer 
        if (TurnDisplay.Instance != null)
            TurnDisplay.Instance.UpdateTurnDisplay(ActionsLeft, config.actionsPerTurn);

        if (ActionsLeft <= 0)
            StartCoroutine(EnemyTurnRoutine());
    }

    IEnumerator EnemyTurnRoutine()
    {
        State = CombatState.ENEMY_TURN;
        
        // ici affiche le tour de l'enemi si le combat est lancé
        if (TurnDisplay.Instance != null)
            TurnDisplay.Instance.ShowEnemyTurn();
        
        Debug.Log("Tour Enemi");

        yield return new WaitForSeconds(1f);

        if (Enemy.Instance != null)
            Enemy.Instance.TakeTurn();

        yield return new WaitForSeconds(1f);

        StartPlayerTurn();
    }

    public void OnEnemyDied()
    {
        if (MapState.GetPendingEncounter() == MapState.EncounterType.Boss)
        {
            Debug.Log("Boss vaincu -> Fade noir puis retour Menu");
            if (FadeScreen.Instance != null)
                FadeScreen.Instance.FadeToBlackAndLoadMenu();
            else
                SceneManager.LoadScene("Menu");
            return;
        }

        Debug.Log("Ennemi vaincu -> Retour à la map");
        MapState.ClearPendingEncounter();
        SceneManager.LoadScene("Map");
    }
}