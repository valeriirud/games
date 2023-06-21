using System.Collections.Generic;
using System.Linq;

namespace Games.Model.HandTypes;

public class Pair
{
    public static List<Card> GetBestCards(List<Card> cards)
    {
        List<List<Card>> pairs = Hand.GetCountOfKind(cards, 2);
        if (pairs.Count == 0) return new();
        int index = 0;
        for (int i = 1; i < pairs.Count; i++)
        {
            int cmp = Card.Compare(pairs[index][0], pairs[i][0]);
            if (cmp > 0) continue;
            if (cmp < 0)
            {
                index = i;
            }
        }
        return new(pairs[index]);
    }

    public static int Compare(Hand hand1, Hand hand2)
    {
        List<Card> pair1 = GetBestCards(hand1.Cards);
        List<Card> pair2 = GetBestCards(hand2.Cards);
        if(! pair1.Any()) return -1;
        if (!pair2.Any()) return 1;
        return Card.Compare(pair1[0], pair2[0]);
    }

    public static List<List<Card>> GetPairs(List<Card> cards) => Hand.GetCountOfKind(cards, 2);

}