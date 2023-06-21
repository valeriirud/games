using System.Collections.Generic;
using System.Linq;

namespace Games.Model.HandTypes;

public class HighCard
{
    public static List<Card> GetBestCards(List<Card> cards)
    {
        if (cards.Count < 2) return new();
        return new() { cards[0], cards[1] };
    }

    public static int Compare(Hand hand1, Hand hand2)
    {
        List<Card> cards1 = hand1.MyCards;
        List<Card> cards2 = hand2.MyCards;
        if (! cards1.Any() || ! cards2.Any()) return 0;
        int cmp = Card.Compare(cards1[0], cards2[0]);
        if (cmp != 0) return cmp;
        if (cards1.Count < 2 || cards2.Count < 2) return 0;
        return Card.Compare(cards1[1], cards2[1]);
    }
}
