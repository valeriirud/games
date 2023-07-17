
using System.Collections.Generic;
using Games.Model;

namespace Games.Model.HandTypes;

public class ThreeOfKind
{
    public static List<Card> GetBestCards(List<Card> handCards)
    {
        List<List<Card>> hands = Hand.GetCountOfKind(handCards, 3);
        if (hands.Count == 0) return new();
        List<Card> allCards = new();
        hands.ForEach(allCards.AddRange);
        if (allCards.Count < 3) return new();
        allCards.Sort(Card.Compare);
        List<Card> bestCards = new();
        for (int i = 1; i < 4; i++)
        {
            bestCards.Add(allCards[^i]);
        }
        return bestCards;
    }

    public static int Compare(Hand hand1, Hand hand2)
    {
        List<Card> cards1 = GetBestCards(hand1.Cards);
        List<Card> cards2 = GetBestCards(hand2.Cards);
        return Card.Compare(cards1[0], cards2[0]);
    }

}
