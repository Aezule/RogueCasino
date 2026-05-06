using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnDisplay : MonoBehaviour
{
    public static TurnDisplay Instance;

    public TextMeshProUGUI turnText;  //text de l'ui

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpdateTurnDisplay(int actionLeft, int totalActions)
    {
        turnText.text = $"C'est votre tour : {totalActions - actionLeft + 1} / {totalActions}"; // Affiche -> "C'est votre tour : X / 3"
    }

    public void ShowEnemyTurn()
    {
        turnText.text = "C'est le tour de l'enemi"; 
    }

    public void HideTurnDisplay()
    {
        turnText.text = "";
    }
}