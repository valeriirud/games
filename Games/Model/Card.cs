﻿
using Games.Tools;
using static Games.Tools.Definitions;
using static Games.Tools.Extensions;

namespace Games.Model;

public class Card
{
    CardId _id;
    Suit _suit;

    public CardId Id { get => _id; }
    public Suit Suit { get => _suit; }

    public Card(CardId id, Suit suit) => Init(id, suit);
    public Card(Card card) => Init(card.Id, card.Suit);
    public Card(int id, int suit) => Init(GetEnumFromInt<CardId>(id), GetEnumFromInt<Suit>(suit));    

    void Init(CardId id, Suit suit)
    {
        _id = id;
        _suit = suit;
    }

    public static int Compare(Card card1, Card card2) => card1.Compare(card2);
    public static int CompareDesc(Card card1, Card card2) => card1.CompareDesc(card2);

    public int Compare(Card card, bool useSuit = false)
    {
        if (card.Id < _id) return 1;
        if (card.Id > _id) return -1;
        if (!useSuit) return 0;
        if (card.Suit < _suit) return 1;
        if (card.Suit > _suit) return -1;
        return 0;
    }

    public int CompareDesc(Card card, bool useSuit = false)
    {
        CardId id = card.Id;
        if (id > _id) return 1;
        if (id < _id) return -1;
        if (!useSuit) return 0;
        Suit suit = card.Suit;
        if (suit > _suit) return 1;
        if (suit < _suit) return -1;
        return 0;
    }

    public new string ToString()
    {
        string strId = Id.Description();
        string strSuit = Suit.Description();
        string str = $"{strId}{strSuit}";
        return str.Replace(" ", string.Empty);
    }

    public string ToDisplayString()
    {
        string strId = Id.DisplayName();
        if (string.IsNullOrEmpty(strId))
            strId = Id.Description();
        string strSuit = Suit.DisplayName();
        if (string.IsNullOrEmpty(strSuit))
            strSuit = Suit.Description();
        string str = $"{strId}{strSuit}";
        return str.Replace(" ", string.Empty);
    }
}
