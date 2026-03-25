using UnityEngine;

public class CardData
{
    public int value;
    public string suit;
    public Sprite sprite;

    public CardData(int value, string suit, Sprite sprite)
    {
        this.value = value;
        this.suit = suit;
        this.sprite = sprite;
    }

    public override string ToString()
    {
        return $"{GetValueName()} de {suit}";
    }

    public string GetValueName()
    {
        switch (value)
        {
            case 14: return "As";
            case 11: return "Valet";
            case 12: return "Dame";
            case 13: return "Roi";
            default: return value.ToString();
        }
    }
}