using System.Collections.Generic;

public class HandEvaluator
{
    public static int CalculateDamage(List<Card> cards)
    {
        int damage = 0;

        foreach(Card c in cards)
        {
            damage += c.value;
        }

        return damage;
    }
}