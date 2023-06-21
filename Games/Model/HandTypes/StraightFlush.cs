
using System.Collections.Generic;
using static Games.Tools.Definitions;

namespace Games.Model.HandTypes;

public class StraightFlush
{
    public static List<Card> GetBestCards(List<Card> handCards)
    {
        List<Card> bestCards = new();
        List<Suit> suits = Hand.GetSuits(handCards);
        foreach (Suit suit in suits)
        {
            List<Card> cards = Hand.GetCardsBySuit(handCards, suit);
            if (cards.Count < StraightCount) continue;
            List<Card> straight = Straight.GetBestCards(cards);
            if (straight.Count < StraightCount) continue;
            bestCards.AddRange(straight);
            break;
        }
        return Straight.Sort(bestCards);
    }

    public static int Compare(Hand hand1, Hand hand2)
    {
        int cmp = Straight.Compare(hand1, hand2);
        if(cmp != 0) return cmp;
        return HighCard.Compare(hand1, hand2);
    }
}
