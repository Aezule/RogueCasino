using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CombatActionsUI : MonoBehaviour
{
    public Button discardButton;
    public Button sendButton;

    void Update()
    {
        bool canAct = CombatManager.Instance != null
                   && CombatManager.Instance.State == CombatState.PLAYER_TURN
                   && CombatManager.Instance.ActionsLeft > 0;

        bool hasSelection = HandManager.Instance != null
                         && !HandManager.Instance.IsSelectionEmpty();

        if (discardButton != null)
            discardButton.interactable = canAct && hasSelection;

        if (sendButton != null)
            sendButton.interactable = canAct && hasSelection;
    }

    public void OnClickDiscard()
    {
        UnityEngine.Debug.Log("CLICK DISCARD");

        if (!CanAct()) return;
        if (HandManager.Instance == null) return;

        StartCoroutine(DiscardRoutine());
    }

    public void OnClickSend()
    {
        UnityEngine.Debug.Log("CLICK SEND");

        if (!CanAct()) return;
        if (HandManager.Instance == null) return;

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
        List<Card> selected = HandManager.Instance.GetSelectedCards();

        if (selected == null || selected.Count == 0)
            yield break;

        if (UICombatAnimations.Instance != null)
            yield return StartCoroutine(UICombatAnimations.Instance.AnimateDiscard(selected));

        HandManager.Instance.RemoveSelectedCards();

        if (DeckManager.Instance != null)
            yield return StartCoroutine(DeckManager.Instance.RefillEmptySlotsCoroutine());

        if (CombatManager.Instance != null)
            CombatManager.Instance.PlayerUsedAction();
    }

    IEnumerator SendRoutine()
    {
        List<Card> selected = HandManager.Instance.GetSelectedCards();

        if (selected == null || selected.Count == 0)
            yield break;

        if (!PokerEvaluator.IsValidSend(selected))
        {
            yield return StartCoroutine(FlashButtonRed(sendButton));
            yield break;
        }

        if (CombatManager.Instance == null || CombatManager.Instance.config == null)
            yield break;

        CombatConfig config = CombatManager.Instance.config;
        ComboType combo = PokerEvaluator.DetectBestCombo(selected);
        int damage = PokerEvaluator.EvaluateDamage(selected);

        float comboMultiplier = config.GetComboMultiplier(combo);
        damage = Mathf.RoundToInt(damage * comboMultiplier * config.globalDamageMultiplier);

        bool isCrit = Random.value < config.playerCritChance;
        if (isCrit)
            damage = Mathf.RoundToInt(damage * config.playerCritMultiplier);

        if (UICombatAnimations.Instance != null)
            yield return StartCoroutine(UICombatAnimations.Instance.AnimateSend(selected));

        if (Enemy.Instance != null)
            Enemy.Instance.TakeDamage(damage);

        HandManager.Instance.RemoveSelectedCards();

        if (DeckManager.Instance != null)
            yield return StartCoroutine(DeckManager.Instance.RefillEmptySlotsCoroutine());

        if (CombatManager.Instance != null)
            CombatManager.Instance.PlayerUsedAction();
    }

    IEnumerator FlashButtonRed(Button button)
    {
        if (button == null || button.image == null)
            yield break;

        Color originalColor = button.image.color;
        button.image.color = Color.red;
        yield return new WaitForSeconds(0.25f);
        button.image.color = originalColor;
    }
}