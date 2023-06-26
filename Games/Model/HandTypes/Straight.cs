
using Games.Tools;
using System.Collections.Generic;
using System.Linq;
using static Games.Tools.Definitions;

namespace Games.Model.HandTypes;

public class Straight
{
    public static List<Card> GetBestCards(List<Card> handCards)
    {
        List<Card> straight = GetStraight(handCards);
        if (straight.Count < StraightCount) return new ();
        List<Card> withoutDuplicates = GetStraightWithoutDuplicates(straight);
        straight = GetStraight(withoutDuplicates);
        if (straight.Count < StraightCount) return new ();
        List<Card> sorted = Straight.SortDesc(straight);
        List<Card> bestCards  = sorted.GetRange(0, StraightCount);
        return bestCards;
    }

    public static List<Card> GetStraight(List<Card> handCards)
    {
        List<Card> straight = new();
        List<Card> cards = new();
        cards.AddRange(handCards);
        if (cards.Count < StraightCount) return straight;

        cards.Sort(Card.CompareDesc);
        straight.Add(cards[0]);
        int straightCount = 1;
        for (int i = 1; i < cards.Count; i++)
        {
            int index = straight.Count - 1;
            CardId straightItemId = straight[index].Id;
            CardId cardId = cards[i].Id;
            int d = straight.Count > 0 ? straightItemId - cardId : 1;
            if (d > 1 && straight.Count < StraightCount)
            {
                straight = new List<Card>() { cards[i] };
                straightCount = 1;
                continue;
            }
            if (d <= 1 || straight.Count == 0)
            {
                straight.Add(cards[i]);
                straightCount += d;
            }
        }
        if (straightCount >= StraightCount) return straight;
        return GetStraightWithAce(handCards);
    }

    static List<Card> GetStraightWithAce(List<Card> handCards)
    {
        List<Card> straight = new();
        List<CardId> ids = new() { CardId.Ace, CardId.Two, 
            CardId.Three, CardId.Four, CardId.Five };
        foreach (CardId id in ids)
        {
            List<Card> cards = Hand.GetCardsById(handCards, id);
            if (cards.Count == 0)
            {
                straight.Clear();
                return straight;
            }
            straight.AddRange(cards);
        }
        return straight;
    }

    static List<Card> GetStraightWithoutDuplicates(List<Card> straightCards)
    {
        List<Card> straight = new();
        List<IGrouping<CardId, Card>> cardGroups = straightCards.GroupBy(i => i.Id).ToList();
        cardGroups.ForEach(g => straight.Add(g.ToList().First()));
        return straight;
    }

    public static List<Card> Sort(List<Card> straightCards, bool asc = true)
    {
        if (asc) return SortAsc(straightCards);
        return SortDesc(straightCards);
    }

    static List<Card> SortAsc(List<Card> straightCards)
    {
        List<Card> sorted = new();
        if (straightCards.Count == 0) return sorted;
        sorted.AddRange(straightCards);
        sorted.Sort(Card.Compare);
        if (sorted[0].Id == CardId.Two && sorted[^1].Id == CardId.Ace)
        {
            Card card = sorted[^1];
            sorted.RemoveAt(sorted.Count - 1);
            sorted.Insert(0, card);
        }
        return sorted;
    }

    static List<Card> SortDesc(List<Card> straightCards)
    {
        List<Card> sorted = new();
        if (straightCards.Count == 0) return sorted;
        sorted.AddRange(straightCards);
        sorted.Sort(Card.CompareDesc);
        if (sorted[^1].Id == CardId.Two && sorted[0].Id == CardId.Ace)
        {
            Card card = sorted[0];
            sorted.RemoveAt(0);
            sorted.Add(card);
        }
        return sorted;
    }

    public static int Compare(Hand hand1, Hand hand2)
    {
        List<Card> cards1 = GetBestCards(hand1.Cards);
        List<Card> cards2 = GetBestCards(hand2.Cards);
        if (cards1.Count < StraightCount) return -1;
        if (cards2.Count < StraightCount) return 1;
        List<Card> sorted1 = SortDesc(cards1);
        List<Card> sorted2 = SortDesc(cards2);
        int cmp = 0;
        for (int i = 0; i < StraightCount; i++)
        {
            cmp = Card.Compare(sorted1[i], sorted2[i]);
            if (cmp != 0) break;
        }
        return cmp;
    }
}
