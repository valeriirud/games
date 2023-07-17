using System.Collections.Generic;
using System.Linq;

namespace Games.Model.HandTypes;

public class FullHouse
{
    public static List<Card> GetBestCards(List<Card> handCards)
    {
        List<Card> bestCards = ThreeOfKind.GetBestCards(handCards);
        if (bestCards.Count < 3) return new();
        List<List<Card>> pairs = Pair.GetPairs(handCards);
        List<Card> allPairs = new();
        pairs.ForEach(allPairs.AddRange);
        if (allPairs.Count < 2) return new();
        allPairs.Sort(Card.Compare);
        for (int i = 1; i < allPairs.Count + 1; i++)
        {
            if (allPairs[allPairs.Count - i].Compare(bestCards[0]) == 0) continue;
            bestCards.Add(allPairs[allPairs.Count - i]);
            bestCards.Add(allPairs[allPairs.Count - i - 1]);
            break;
        }
        return bestCards;
    }
    public static int Compare(Hand hand1, Hand hand2)
    {
        List<Card> cards1 = GetBestCards(hand1.Cards);
        List<Card> cards2 = GetBestCards(hand2.Cards);
        List<Card> threeOfKind1 = ThreeOfKind.GetBestCards(cards1);
        List<Card> threeOfKind2 = ThreeOfKind.GetBestCards(cards2);
        int cmp = Card.Compare(threeOfKind1[0], threeOfKind2[0]);
        if(cmp != 0) return cmp;
        List<List<Card>> pairs1 = Pair.GetPairs(cards1);
        pairs1.RemoveAll(p => p[0].Id == threeOfKind1[0].Id);
        List<List<Card>> pairs2 = Pair.GetPairs(cards2);
        pairs2.RemoveAll(p => p[0].Id == threeOfKind2[0].Id);
        List<Card> pair1 = Pair.GetTopPair(pairs1);
        List<Card> pair2 = Pair.GetTopPair(pairs2);
#if false
        List<Card> pair1 = new(pairs1[0]);        
        foreach (List<Card> pair in pairs1.Skip(1))
        {
            cmp = Card.Compare(pair1[0], pair[0]);
            if (cmp > 0) continue;
            pair1 = new(pair);
        }
        List<Card> pair2 = new(pairs2[0]);
        foreach (List<Card> pair in pairs2.Skip(1))
        {
            cmp = Card.Compare(pair2[0], pair[0]);
            if (cmp > 0) continue;
            pair2 = new(pair);
        }
#endif
        return Card.Compare(pair1[0], pair2[0]);
    }
}
