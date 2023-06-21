
using System.Collections.Generic;
using System.Linq;
using Games.Model;
using Games.Tools;
using static Games.Tools.Definitions;

namespace Games.Model.HandTypes;

public class Flush
{
    public static List<Card> GetBestCards(List<Card> handCards)
    {
        List<Card> bestCards = new();
        List<Card> flush = GetFlush(handCards);
        if (flush.Count < FlushCount) return bestCards;
        flush.Sort(Card.CompareDesc);
        for (int i = 0; i < FlushCount; i++)
        {
            bestCards.Add(flush[i]);
        }
        return bestCards;
    }
    public static List<Card> GetFlush(List<Card> handCards)
    {
        List<Card> flush = new();
        List<Suit> suits = Hand.GetSuits(handCards).Distinct().ToList();
        foreach (Suit suit in suits)
        {
            List<Card> cards = Hand.GetCardsBySuit(handCards, suit);
            if (cards.Count < FlushCount) continue;
            flush.AddRange(cards);
            break;
        }
        return flush;
    }

    public static int Compare(Hand hand1, Hand hand2)
    {
        List<Card> cards1 = GetBestCards(hand1.Cards);
        List<Card> cards2 = GetBestCards(hand2.Cards);
        if (cards1.Count < FlushCount) return -1;
        if (cards2.Count < FlushCount) return 1;
        int cmp = 0;
        for (int i = 0; i < FlushCount; i ++)
        {
            cmp = Card.Compare(cards1[i], cards2[i]);
            if (cmp != 0) break;
        }
        return cmp;
    }


}
