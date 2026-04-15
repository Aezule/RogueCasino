using UnityEngine;

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
        Debug.Log($"=== Tour Joueur — {ActionsLeft} actions ===");
    }

    public void PlayerUsedAction()
    {
        if (State != CombatState.PLAYER_TURN) return;

        ActionsLeft--;
        Debug.Log($"Action utilisée. Reste : {ActionsLeft}");

        if (ActionsLeft <= 0)
            StartCoroutine(EnemyTurnRoutine());
    }

    System.Collections.IEnumerator EnemyTurnRoutine()
    {
        State = CombatState.ENEMY_TURN;
        Debug.Log("=== Tour Ennemi ===");

        yield return new WaitForSeconds(1f);

        if (Enemy.Instance != null)
            Enemy.Instance.TakeTurn();

        yield return new WaitForSeconds(1f);

        StartPlayerTurn();
    }

    public void OnEnemyDied()
{
    Debug.Log("=== Ennemi vaincu ===");
    // TODO : transition vers casino
}

}
