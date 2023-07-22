
using Games.Model.HandTypes;
using Games.Tools;
using static Games.Tools.Definitions;

namespace Games.Model;

public class Hand
{
    readonly List<Card> _cards = new();
    readonly List<Card> _bestCards = new();
    readonly List<Card> _myCards = new();
    readonly HandType _handType;

    public List<Card> BestCards { get => _bestCards; }
    public List<Card> MyCards { get => _myCards; }
    public List<Card> Cards { get => _cards; }
    public HandType HandType { get => _handType; }

    public Hand(List<Card> cards)
    {
        _cards.AddRange(cards);
        _myCards.Add(new Card(cards[0]));
        _myCards.Add(new Card(cards[1]));
        _handType = GetHandType(cards);
        _bestCards = GetBestCards(_cards, _handType);
    }

    public Hand(List<Card> cards, HandType handType)
    {
        _cards.AddRange(cards);
        _myCards.Add(new Card(cards[0]));
        _myCards.Add(new Card(cards[1]));
        _handType = handType;
        _bestCards = GetBestCards(_cards, _handType);
    }

    public static List<CardId> GetIds(List<Card> cards)
    {
        List<CardId> ids = new();
        cards.ForEach(c => ids.Add(c.Id));
        return ids;
    }

    public static List<Suit> GetSuits(List<Card> cards)
    {
        List<Suit> suits = new();
        cards.ForEach(c => suits.Add(c.Suit));
        return suits;
    }

    public static string GetIds(string handString) => handString.Substring(0, HandCount);
    public static string GetSuits(string handString) => handString.Substring(HandCount, HandCount);

    public static string ToString(List<Card> cards)
    {
        string strIds = CommonTools.ToString(GetIds(cards));
        string strSuits = CommonTools.ToString(GetSuits(cards));
        while(strIds.Length < HandCount)
        {
            strIds += "0";
            strSuits += "0";
        }
        return strIds + strSuits + "0";
    }

    public static string ToDisplayString(Hand hand)
    {
        string str = string.Empty;
        str += $"[{ToDisplayString(hand.Cards)}] ";
        str += $"[{ToDisplayString(hand.BestCards)}] ";
        str += $"[{CommonTools.GetEnumDescription(hand.HandType)}]";
        return str;
    }

    public static string ToDisplayString(List<Card> cards, bool format = true)
    {
        if (!format) return ToString(cards, true);
        return $"[{ ToString(cards, true)}] ";
    }

    public HandType GetHandType() => _handType;

    public static HandType GetHandType(List<Card> handCards)
    {
        List<Card> cards;
        cards = RoyalFlush.GetBestCards(handCards);
        if (cards.Count >= 5) return HandType.RoyalFlush;
        cards = StraightFlush.GetBestCards(handCards);
        if (cards.Count >= 5) return HandType.StraightFlush;
        cards = FourOfKind.GetBestCards(handCards);
        if (cards.Count == 4) return HandType.FourOfKind;
        cards = FullHouse.GetBestCards(handCards);
        if (cards.Count == 5) return HandType.FullHouse;
        cards = Flush.GetBestCards(handCards);
        if (cards.Count >= 5) return HandType.Flush;
        cards = Straight.GetBestCards(handCards);
        if (cards.Count >= 5) return HandType.Straight;
        cards = ThreeOfKind.GetBestCards(handCards);
        if (cards.Count == 3) return HandType.ThreeOfKind;
        cards = TwoPairs.GetBestCards(handCards);
        if (cards.Count == 4) return HandType.TwoPairs;
        cards = Pair.GetBestCards(handCards);
        if (cards.Count == 2) return HandType.Pair;
        return HandType.HighCard;
    }

    public static List<Card> GetBestCards(List<Card> cards, HandType handType)
    {
        return handType switch
        {
            HandType.RoyalFlush => RoyalFlush.GetBestCards(cards),
            HandType.StraightFlush => StraightFlush.GetBestCards(cards),
            HandType.FourOfKind => FourOfKind.GetBestCards(cards),
            HandType.FullHouse => FullHouse.GetBestCards(cards),
            HandType.Flush => Flush.GetBestCards(cards),
            HandType.Straight => Straight.GetBestCards(cards),
            HandType.ThreeOfKind => ThreeOfKind.GetBestCards(cards),
            HandType.TwoPairs => TwoPairs.GetBestCards(cards),
            HandType.Pair => Pair.GetBestCards(cards),
            _ => HighCard.GetBestCards(cards)
        };
    }

    static List<CardId> ListOfIds => new()
        {
            CardId.Two,
            CardId.Three,
            CardId.Four,
            CardId.Five,
            CardId.Six,
            CardId.Seven,
            CardId.Eight,
            CardId.Nine,
            CardId.Ten,
            CardId.Jack,
            CardId.Queen,
            CardId.King,
            CardId.Ace
        };

    static List<Suit> ListOfSuits => new()
        {
            Suit.Club,
            Suit.Diamond,
            Suit.Heart,
            Suit.Spade
        };

    public static List<List<Card>> GetCountOfKind(List<Card> cards, int count)
    {
        List<CardId> ids = ListOfIds;
        List<List<Card>> found = new();
        foreach (CardId id in ids)
        {
            List<Card> cardsById = GetCardsById(cards, id);
            if (cardsById.Count < count) continue;
            found.Add(cardsById);
        }
        return found;
    }

    public static List<Card> GetCardsById(List<Card> cards, CardId cardId) =>
        cards.Where(c => c.Id == cardId).ToList();

    public static List<Card> GetCardsBySuit(List<Card> cards, Suit suit) =>
        cards.Where(c => c.Suit == suit).ToList();

    public static string DisplayStringToString(string displayString)
    { 
        string str = displayString;
        foreach(CardId cardId in ListOfIds)
        {
            string displayName = cardId.DisplayName();
            if (string.IsNullOrEmpty(displayName)) continue;
            string description = cardId.Description();
            str = str.Replace(displayName, description);
        }
        foreach (Suit suit in ListOfSuits)
        {
            string displayName = suit.DisplayName();
            if (string.IsNullOrEmpty(displayName)) continue;
            string description = suit.Description();
            str = str.Replace(displayName, description);
        }
        return str;
    }

    public static string AlternateStringToString(string alternateString)
    {
        string str = alternateString;
        foreach (CardId cardId in ListOfIds)
        {
            string description = cardId.Description();
            string alternate = cardId.AlternateValue();
            string value = cardId.ToInt().ToString("X");
            if (!string.IsNullOrEmpty(description))
            {
                str = str.Replace(description, value);
            }
            if (!string.IsNullOrEmpty(alternate))
            {
                str = str.Replace(alternate, value);
            }
        }
        foreach (Suit suit in ListOfSuits)
        {
            string alternate = suit.AlternateValue();            
            string value = suit.ToInt().ToString();
            if (!string.IsNullOrEmpty(alternate))
            {
                str = str.Replace(alternate, value);
            }
        }
        string ret = string.Empty;
        for(int i = 0; i < str.Length - 1; i += 2)
        {
            ret += str[i];
        }
        for (int i = 1; i < str.Length; i += 2)
        {
            ret += str[i];
        }

        //Console.WriteLine($"[AlternateStringToString]({alternateString})({str})({ret})");

        return ret;
    }

    public static Hand FromDisplayString(string str) => FromString(DisplayStringToString(str));
    public static Hand FromAlternateString(string str) => FromString(AlternateStringToString(str));

    public static Hand FromString(string str)
    {
        List<char> chars = new(str);
        int handSize = str.Length / 2;
        List<Card> cards = new();
        for (int i = 0; i < handSize; i++)
        {
            if (chars[i].ToString() == "0") break;
            CardId cardId = CommonTools.GetEnumFromInt<CardId>(Convert.ToInt32(chars[i].ToString(), 16));
            Suit suit = CommonTools.GetEnumFromInt<Suit>(Convert.ToInt32(chars[i + handSize].ToString()));
            cards.Add(new(cardId, suit));
        }
        if(str.Length <= HandCount*2) return new Hand(cards);
        HandType handType = CommonTools
            .GetEnumFromInt<HandType>(Convert.ToInt32(chars[HandCount * 2]
            .ToString(), 16));
        return new Hand(cards, handType);
    }

    public static List<Card> GetCardsFromDescriptionString(string src)
    {
        if (string.IsNullOrEmpty(src)) return new();
        string str = src.Replace("10", "T");
        List<char> chars = new(str);
        List<Card> cards = new();
        for (int i = 0; i < chars.Count - 1; i += 2)
        {
            CardId cardId = CommonTools.GetEnumFromDescription<CardId>(chars[i].ToString());
            Suit suit = CommonTools.GetEnumFromDescription<Suit>(chars[i + 1].ToString());
            cards.Add(new(cardId, suit));
        }
        return cards;
    }

    public static string ToString(Hand hand)
    {
        string str = ToString(hand.Cards);
        string handType = hand.HandType.ToInt().ToString("X");
        int len = str.Length;
        if(len == HandCount * 2 + 1)
        {
            str = str.Remove(HandCount * 2);
        }
        str += handType;
        return str;
    }

    public static string ToString(List<Card> cards, bool alternative = false)
    {
        string str = string.Empty;
        foreach (Card card in cards)
        {
            CardId cardId = card.Id;
            string s = alternative 
                ? cardId.AlternateValue() 
                : cardId.Description();
            if (string.IsNullOrEmpty(s))
            {
                s = cardId.Description();
            }
            str += s;
            Suit suit = card.Suit;
            str += alternative ? suit.AlternateValue() : suit.Description();
        }
        return str;
    }

    public static string ToString(List<Suit> suits) => CommonTools.ToString<Suit>(suits);

    public static string ToString(List<CardId> ids) => CommonTools.ToString<CardId>(ids);

    public static List<Hand> Distribution(List<Card> handCards, int numberOfPlayers, List<Card> cardDeck)
    {
        List<Card> testedСards = new(handCards);
        List<int> randomList = Enumerable.Range(0, TotalNumberOfCards).ToList();
        CommonTools.GenerateRandomList(randomList);
        int index  = 0;
        while(testedСards.Count < HandCount)
        {
            if (!testedСards.Any(c => c.Compare(cardDeck[randomList[index]], true) == 0))
            {
                testedСards.Add(cardDeck[randomList[index]]);
            }
            index++;
        }
        List<Card> board = testedСards.GetRange(NumberOfMyCards, HandCount - NumberOfMyCards);
        List<Hand> hands = new() { new (testedСards) };
        for(int i = 0; i < numberOfPlayers - 1; i ++)
        {
            List<Card> cards = new();
            for(int j = 0; j < NumberOfMyCards; j ++)
            {
                if (!testedСards.Any(c => c.Compare(cardDeck[randomList[index]], true) == 0))
                {
                    cards.Add(cardDeck[randomList[index]]);
                }
                index++;
            }
            cards.AddRange(board);
            hands.Add(new(cards));
        }

        List<Hand> bestHands = GetBestHands(hands);
        if(bestHands.Count == 1) return bestHands;  
        List<int> bestPositions = GetBestPositions(bestHands);
        List<Hand> winners = new();
        bestPositions.ForEach(p => winners.Add(bestHands[p]));
        return winners; 
    }

    static List<int> GetBestPositions(List<Hand> bestHands)
    {        
        List<int> bestPositions = new() { 0 };
        for (int i = 1; i < bestHands.Count; i++)
        {
            int cmp = Compare(bestHands[bestPositions[0]], bestHands[i]);
            if (cmp > 0) continue;
            if (cmp < 0)
            {
                bestPositions = new() { i };
                continue;
            }
            bestPositions.Add(i);
        }
        return bestPositions;
    }

    public static List<int> GetWinnersIds(List<string> listCards)
    {
        List<Hand> hands = new();
        //listCards.ForEach(i => Console.WriteLine($"Hand:{i}"));
        listCards.ForEach(i => hands.Add(new(GetCardsFromDescriptionString(i))));
        List<Hand> bestHands = GetBestHands(hands);
        //bestHands.ForEach(h => Console.WriteLine($"Best hand:{ToString(h.Cards, true)}"));
        List<int> bestPositions = GetBestPositions(bestHands);
        List<int> positions = new();
        bestPositions.ForEach(p => positions.Add(listCards.IndexOf(ToString(bestHands[p].Cards, true))));
        //.ForEach(p => Console.WriteLine($"Best position:{p}"));        
        return positions;
    }

    public static WinInfo GetProbabilityOfWinningByMonteCarlo(string strCards, int numberOfPlayers)
    {
        List<Card> cards = GetCardsFromDescriptionString(strCards);
        return GetProbabilityOfWinningByMonteCarlo(cards, numberOfPlayers, NumberOfTests);
    }

    public static WinInfo GetProbabilityOfWinningByMonteCarlo(List<Card> testedСards, 
        int numberOfPlayers, int numberOfTests)
    {
        int winCount = 0;
        List<Card> cardDeck = GetCardDeck();        
        string testCardIds = ToString(GetIds(testedСards));
        string testCardSuits = ToString(GetSuits(testedСards));
        List<Card> testСards = new(testedСards);
        for (int i = 0; i < numberOfTests; i++)
        {
            List<string> handStrings = new();
            List<Hand> winners = Distribution(testedСards, numberOfPlayers, cardDeck);
            winners.ForEach(w => handStrings.Add(ToString(w)));
            foreach(string handString in handStrings)
            {
                Hand hand = FromString(handString);
                if (! (hand.Cards.Intersect(testСards).Count() == testСards.Count())) continue;                
                winCount++;
            }           
        }
        double n = Convert.ToSingle(numberOfTests);
        double w = Convert.ToSingle(winCount);
        double probability = n > w ? w / n : 1;
        int p = Convert.ToInt32(Math.Round(probability * 100, 2));
        HandType handType = GetHandType(testСards);
        List<Card> bestCards = GetBestCards(testСards, handType);
        WinInfo winInfo = new () { Probability = p, BestCards = bestCards, AllCards = testСards,
            HandTypeDescription = CommonTools.GetEnumDescription(handType) };
        return winInfo;
    }

    public static List<Card> GetCardDeck()
    {
        List<CardId> ids = ListOfIds;
        List<Suit> suits = ListOfSuits;
        List<Card> cards = new();
        ids.ForEach(id => suits.ForEach(suit => cards.Add(new(id, suit))));
        return cards;
    }

    static List<Hand> GetBestHands(List<Hand> hands)
    {
        List<HandType> handTypes = new() { HandType.RoyalFlush, HandType.StraightFlush, 
            HandType.FourOfKind, HandType.FullHouse, HandType.Flush, HandType.Straight, HandType.ThreeOfKind,
        HandType.TwoPairs, HandType.Pair, HandType.HighCard};
        List<Hand> bestHands = new();
        foreach (HandType handType in handTypes) 
        {
            List<Hand> list = GetHandsByHandType(hands, handType);
            if (!list.Any()) continue;
            bestHands.AddRange(list);
            break;
        }
        return bestHands;
    }

    static List<Hand> GetHandsByHandType(List<Hand> hands, HandType handType)
    {
        List<Hand> handsByHandType = new();
        foreach (Hand hand in hands)
        {
            if (hand.GetHandType() != handType) continue;
            handsByHandType.Add(hand);
        }
        return handsByHandType;
    }

    static int Compare(Hand hand1, Hand hand2)
    {
        if (hand1.HandType > hand2.HandType) return 1;
        if (hand1.HandType < hand2.HandType) return -1;
        int cmp = Compare(hand1, hand2, hand1.HandType);
        if (cmp != 0 
            || (hand1.Cards.Count == NumberOfMyCards && hand2.Cards.Count == NumberOfMyCards)) return cmp;
        List<Card> sorted1 = new (hand1.MyCards);
        sorted1.Sort(Card.Compare);
        List<Card> sorted2 = new (hand2.MyCards);
        sorted2.Sort(Card.Compare);
        Hand myHand1 = new(sorted1);
        Hand myHand2 = new(sorted2);
        return Compare(myHand1, myHand2);
    }

    static int Compare(Hand hand1, Hand hand2, HandType handType)
    {
        return handType switch
        {
            HandType.RoyalFlush => RoyalFlush.Compare(hand1, hand2),
            HandType.StraightFlush => StraightFlush.Compare(hand1, hand2),
            HandType.FourOfKind => FourOfKind.Compare(hand1, hand2),
            HandType.FullHouse => FullHouse.Compare(hand1, hand2),
            HandType.Flush => Flush.Compare(hand1, hand2),
            HandType.Straight => Straight.Compare(hand1, hand2),
            HandType.ThreeOfKind => ThreeOfKind.Compare(hand1, hand2),
            HandType.TwoPairs => TwoPairs.Compare(hand1, hand2),
            HandType.Pair => Pair.Compare(hand1, hand2),
            _ => HighCard.Compare(hand1, hand2)
        };
    }
}
