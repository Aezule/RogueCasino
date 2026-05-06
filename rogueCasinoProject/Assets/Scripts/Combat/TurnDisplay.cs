using UnityEngine;
using UnityEngine.UI;

public class TurnDisplay : MonoBehaviour
{
    public static TurnDisplay Instance;

    public Text turnText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateTurnDisplay(int actionLeft, int totalActions)
    {
        turnText.text = $" {totalActions - actionLeft + 1} "; // Affiche -> "C'est votre tour : X / 3"
    }

    public void ShowEnemyTurn()
    {
        turnText.text = "1";
    }

    public void HideTurnDisplay()
    {
        turnText.text = "";
    }
}