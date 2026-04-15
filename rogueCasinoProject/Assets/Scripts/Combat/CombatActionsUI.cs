using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatActionsUI : MonoBehaviour
{
    public Button discardButton;
    public Button sendButton;

    void Update()
    {
        bool canAct = CombatManager.Instance != null
                   && CombatManager.Instance.State == CombatState.PLAYER_TURN
                   && CombatManager.Instance.ActionsLeft > 0;

        bool hasSelection = !HandManager.Instance.IsSelectionEmpty();

        if (discardButton != null)
            discardButton.interactable = canAct && hasSelection;

        if (sendButton != null)
            sendButton.interactable = canAct && hasSelection;
    }

    public void OnClickDiscard()
    {
        if (!CanAct()) return;
        StartCoroutine(DiscardRoutine());
    }

    public void OnClickSend()
    {
        if (!CanAct()) return;
        StartCoroutine(SendRoutine());
    }

    bool CanAct()
    {
        return CombatManager.Instance != null
            && CombatManager.Instance.State == CombatState.PLAYER_TURN
            && CombatManager.Instance.ActionsLeft > 0;
    }

    IEnumerator DiscardRoutine()
    {
        HandManager.Instance.RemoveSelectedCards();
        yield return StartCoroutine(DeckManager.Instance.RefillEmptySlotsCoroutine());
        CombatManager.Instance.PlayerUsedAction();
    }

    IEnumerator SendRoutine()
{
    List<Card> selected = HandManager.Instance.GetSelectedCards();

    if (selected.Count == 0)
        yield break;

    if (!PokerEvaluator.IsValidSend(selected))
    {
        yield return StartCoroutine(FlashButtonRed(sendButton));
        yield break;
    }

    CombatConfig config = CombatManager.Instance.config;
    ComboType combo = PokerEvaluator.DetectBestCombo(selected);
    int damage = PokerEvaluator.EvaluateDamage(selected);

    float comboMultiplier = config.GetComboMultiplier(combo);
    damage = Mathf.RoundToInt(damage * comboMultiplier * config.globalDamageMultiplier);

    bool isCrit = Random.value < config.playerCritChance;
    if (isCrit)
    {
        damage = Mathf.RoundToInt(damage * config.playerCritMultiplier);
        Debug.Log($"[Joueur] CRITIQUE ! Combo : {combo} — Dégâts : {damage}");
    }
    else
    {
        Debug.Log($"[Joueur] Combo : {combo} — Dégâts : {damage}");
    }

    if (Enemy.Instance != null)
        Enemy.Instance.TakeDamage(damage);

    HandManager.Instance.RemoveSelectedCards();
    yield return StartCoroutine(DeckManager.Instance.RefillEmptySlotsCoroutine());

    CombatManager.Instance.PlayerUsedAction();
}


    IEnumerator FlashButtonRed(Button button)
    {
        if (button == null) yield break;

        Color originalColor = button.image.color;
        button.image.color = Color.red;
        yield return new WaitForSeconds(1f);
        button.image.color = originalColor;
    }
}