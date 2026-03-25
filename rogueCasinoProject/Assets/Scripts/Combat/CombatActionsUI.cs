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
        if (discardButton != null)
            discardButton.interactable = !HandManager.Instance.IsSelectionEmpty();

        if (sendButton != null)
            sendButton.interactable = !HandManager.Instance.IsSelectionEmpty();
    }

    public void OnClickDiscard()
    {
        StartCoroutine(DiscardRoutine());
    }

    public void OnClickSend()
    {
        StartCoroutine(SendRoutine());
    }

    IEnumerator DiscardRoutine()
    {
        HandManager.Instance.RemoveSelectedCards();
        yield return StartCoroutine(DeckManager.Instance.RefillEmptySlotsCoroutine());
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

        ComboType combo = PokerEvaluator.DetectBestCombo(selected);
        int damage = PokerEvaluator.EvaluateDamage(selected);

        Debug.Log("Combo envoyé : " + combo);
        Debug.Log("Dégâts : " + damage);

        HandManager.Instance.RemoveSelectedCards();
        yield return StartCoroutine(DeckManager.Instance.RefillEmptySlotsCoroutine());
    }

    IEnumerator FlashButtonRed(Button button)
    {
        if (button == null)
            yield break;

        Color originalColor = button.image.color;
        button.image.color = Color.red;

        yield return new WaitForSeconds(1f);

        button.image.color = originalColor;
    }
}