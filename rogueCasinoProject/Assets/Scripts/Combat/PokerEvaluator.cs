using System.Collections.Generic;
using System.Linq;

public static class PokerEvaluator
{
    public static ComboType DetectBestCombo(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            return ComboType.CARTE_HAUTE;

        if (IsQuinteFlushRoyale(cards)) return ComboType.QUINTE_FLUSH_ROYALE;
        if (IsQuinteFlush(cards)) return ComboType.QUINTE_FLUSH;
        if (IsCarre(cards)) return ComboType.CARRE;
        if (IsFull(cards)) return ComboType.FULL;
        if (IsFlush(cards)) return ComboType.FLUSH;
        if (IsQuinte(cards)) return ComboType.QUINTE;
        if (IsBrelan(cards)) return ComboType.BRELAN;
        if (IsDoublePaire(cards)) return ComboType.DOUBLE_PAIRE;
        if (IsPaire(cards)) return ComboType.PAIRE;

        return ComboType.CARTE_HAUTE;
    }

    public static int EvaluateDamage(List<Card> cards)
    {
        ComboType combo = DetectBestCombo(cards);
        int baseValue = GetBaseValue(combo);
        int highestCard = GetHighestCardValue(cards);

        return baseValue + (highestCard / 2);
    }

    public static bool IsValidSend(List<Card> cards)
    {
        if (cards == null || cards.Count == 0)
            return false;

        ComboType combo = DetectBestCombo(cards);

        if (combo == ComboType.CARTE_HAUTE && cards.Count > 1)
            return false;

        return true;
    }

    static int GetBaseValue(ComboType combo)
    {
        switch (combo)
        {
            case ComboType.PAIRE: return 10;
            case ComboType.DOUBLE_PAIRE: return 20;
            case ComboType.BRELAN: return 35;
            case ComboType.QUINTE: return 40;
            case ComboType.FLUSH: return 45;
            case ComboType.FULL: return 60;
            case ComboType.CARRE: return 100;
            case ComboType.QUINTE_FLUSH: return 150;
            case ComboType.QUINTE_FLUSH_ROYALE: return 200;
            default: return 1000;
        }
    }

    static int GetHighestCardValue(List<Card> cards)
    {
        return cards.Max(c => c.GetValue());
    }

    static List<int> GetSortedValues(List<Card> cards)
    {
        return cards.Select(c => c.GetValue()).OrderBy(v => v).ToList();
    }

    static bool IsPaire(List<Card> cards)
    {
        return cards.Count == 2 &&
               cards[0].GetValue() == cards[1].GetValue();
    }

    static bool IsDoublePaire(List<Card> cards)
    {
        if (cards.Count != 4)
            return false;

        List<int> groups = cards
            .GroupBy(c => c.GetValue())
            .Select(g => g.Count())
            .OrderByDescending(x => x)
            .ToList();

        return groups.Count == 2 && groups[0] == 2 && groups[1] == 2;
    }

    static bool IsBrelan(List<Card> cards)
    {
        return cards.Count == 3 &&
               cards.All(c => c.GetValue() == cards[0].GetValue());
    }

    static bool IsCarre(List<Card> cards)
    {
        return cards.Count == 4 &&
               cards.All(c => c.GetValue() == cards[0].GetValue());
    }

    static bool IsFull(List<Card> cards)
    {
        if (cards.Count != 5)
            return false;

        List<int> groups = cards
            .GroupBy(c => c.GetValue())
            .Select(g => g.Count())
            .OrderByDescending(x => x)
            .ToList();

        return groups.Count == 2 && groups[0] == 3 && groups[1] == 2;
    }

    static bool IsFlush(List<Card> cards)
    {
        if (cards.Count != 5)
            return false;

        string firstSuit = cards[0].GetSuit();
        return cards.All(c => c.GetSuit() == firstSuit);
    }

    static bool IsQuinte(List<Card> cards)
    {
        if (cards.Count != 5)
            return false;

        List<int> values = GetSortedValues(cards);

        if (values.Distinct().Count() != 5)
            return false;

        // Quinte normale (ex: 5-6-7-8-9 ou 10-V-D-R-A)
        if (values[4] - values[0] == 4)
            return true;

        // Quinte basse avec As (A-2-3-4-5 → stocké comme 2,3,4,5,14)
        if (values.SequenceEqual(new List<int> { 2, 3, 4, 5, 14 }))
            return true;

        return false;
    }

    static bool IsQuinteFlush(List<Card> cards)
    {
        return IsQuinte(cards) && IsFlush(cards);
    }

    static bool IsQuinteFlushRoyale(List<Card> cards)
    {
        if (!IsQuinteFlush(cards))
            return false;

        List<int> values = GetSortedValues(cards);
        return values.SequenceEqual(new List<int> { 10, 11, 12, 13, 14 });
    }
}
