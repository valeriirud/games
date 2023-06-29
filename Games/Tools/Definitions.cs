
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Games.Model;

namespace Games.Tools;

public static class Definitions
{
    public static readonly int NumberOfMyCards = 2;
    public static readonly int NumberOfCommunityCards = 5;
    public static readonly int HandCount = 7;
    public static readonly int FlushCount = 5;
    public static readonly int StraightCount = 5;
    public static readonly int MaxNumberOfPlayers = 9;
    public static readonly int MinNumberOfPlayers = 2;
    public static readonly int TableMax = 6;
    public static readonly int TotalNumberOfCards = 52;
    public static readonly int NumberOfTests = 1000;
    public static readonly int Timeout = 1000;
    public static readonly int MaxStack = 1000;

    public static readonly bool WriteTextLog = false;

    public struct WinInfo
    {
        public int Probability;
        public string HandTypeDescription;
        public List<Card> BestCards;
        public List<Card> AllCards;
        public override string ToString() => 
            $"{Probability}% {HandTypeDescription} {Hand.ToString(BestCards, true)}";
    }

    public enum Suit
    {
        [AlternateValue("\u2663")] // ♣
        [Description("C")]
        Club = 1,
        [AlternateValue("\u2666")] // ♦
        [Description("D")]
        Diamond = 2,
        [AlternateValue("\u2665")] // ♥
        [Description("H")]
        Heart = 3,
        [AlternateValue("\u2660")] // ♠
        [Description("S")]
        Spade = 4
    }

    public enum CardId
    {
        [Description("2")]
        Two = 2,
        [Description("3")]
        Three = 3,
        [Description("4")]
        Four = 4,
        [Description("5")]
        Five = 5,
        [Description("6")]
        Six = 6,
        [Description("7")]
        Seven = 7,
        [Description("8")]
        Eight = 8,
        [Description("9")]
        Nine = 9,
        [AlternateValue("10")]
        [Description("T")]
        Ten = 10,
        [Description("J")]
        Jack = 11,
        [Description("Q")]
        Queen = 12,
        [Description("K")]
        King = 13,
        [Description("A")]
        Ace = 14
    }
    
    public enum HandType
    {
        [Description("HighCard")]
        HighCard = 1,
        [Description("Pair")]
        Pair = 2,
        [Description("TwoPairs")]
        TwoPairs = 3,
        [Description("ThreeOfKind")]
        ThreeOfKind = 4,
        [Description("Straight")]
        Straight = 5,
        [Description("Flush")]
        Flush = 6,
        [Description("FullHouse")]
        FullHouse = 7,
        [Description("FourOfKind")]
        FourOfKind = 8,
        [Description("StraightFlush")]
        StraightFlush = 9,
        [Description("RoyalFlush")]
        RoyalFlush = 10,
        [Description("Undefined")]
        Undefined = 11,
    }
}
