
using System.Collections.Generic;
using System.Linq;
using static Games.Tools.Definitions;

namespace Games.Model.HandTypes;

public class RoyalFlush
{
    public static List<Card> GetBestCards(List<Card> handCards)
    {
        List<Card> straight = Straight.GetBestCards(handCards);
        if (straight.Count < Tools.Definitions.StraightCount) return new();
        List<Suit> suits = Hand.GetSuits(straight).Distinct().ToList();
        if (suits.Count > 1) return new ();
        List<Card> sorted = Straight.Sort(straight, false);
        if(!sorted.Any()) return new ();
        if (sorted[^1].Id != CardId.Ten || sorted[0].Id != CardId.Ace) return new();
        return sorted;
    }

    public static int Compare(Hand hand1, Hand hand2) => HighCard.Compare(hand1, hand2);
}
