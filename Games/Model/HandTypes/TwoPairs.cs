
using System.Collections.Generic;
using static Games.Tools.Definitions;

namespace Games.Model.HandTypes;

public class TwoPairs
{
    public static List<Card> GetBestCards(List<Card> cards)
    {
        List<List<Card>> pairs = Hand.GetCountOfKind(cards, 2);
        if (pairs.Count == 0) return new();
        List<Card> allPairs = new();
        pairs.ForEach(allPairs.AddRange);
        if (allPairs.Count < 4) return new();
        allPairs.Sort(Card.Compare);
        List<Card> bestPairs = new();
        for (int i = 1; i < 5; i++)
        {
            bestPairs.Add(allPairs[allPairs.Count - i]);
        }
        return bestPairs;
    }

    public static int Compare(Hand hand1, Hand hand2)
    {
        List<Card> pairs1 = GetBestCards(hand1.Cards);
        List<Card> pairs2 = GetBestCards(hand2.Cards);
        if (pairs1.Count < 4) return -1;
        if (pairs2.Count < 4) return 1;
        int cmp = 0;
        for(int i = 0; i < 4; i ++)
        {
            cmp = Card.Compare(pairs1[i], pairs2[i]);
            if (cmp != 0) break;
        }
        return cmp;
    }

}
