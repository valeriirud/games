﻿
using Microsoft.AspNetCore.Components;

using Games.Model;
using Games.Tools;
using static Games.Tools.Definitions;

namespace Games.Pages;

public class OddsCalculatorBase : ComponentBase
{
    public List<string> ListCardId { get; } = new();
    public List<string> ListCardSuit { get; } = new();
    public List<CardState> ListCardState { get; } = new();
    public string TestHand { get; private set; }
    public string WinString { get; private set; }
    public string[] HandCards { get; set; } = new string[7];
    public int NumberOfPlayers { get; set; }

    public OddsCalculatorBase()
    {
        NumberOfPlayers = TableMax;
        TestHand = string.Empty;
        WinString = string.Empty;
        for (Suit s = Suit.Club; s <= Suit.Spade; s++)
        {
            for (CardId c = CardId.Two; c <= CardId.Ace; c++)
            {            
                CardState cardState = new(c.ToInt(), s.ToInt(), true);
                cardState.CardStateChanged += CardState_Changed;
                ListCardState.Add(cardState);
            }
        }
        ListCardId.Add(SvgPath.Two);
        ListCardId.Add(SvgPath.Three);
        ListCardId.Add(SvgPath.Four);
        ListCardId.Add(SvgPath.Five);
        ListCardId.Add(SvgPath.Six);
        ListCardId.Add(SvgPath.Seven);
        ListCardId.Add(SvgPath.Eight);
        ListCardId.Add(SvgPath.Nine);
        ListCardId.Add(SvgPath.Ten);
        ListCardId.Add(SvgPath.Jack);
        ListCardId.Add(SvgPath.Queen);
        ListCardId.Add(SvgPath.King);
        ListCardId.Add(SvgPath.Ace);
        ListCardSuit.Add(SvgPath.Club);
        ListCardSuit.Add(SvgPath.Diamond);
        ListCardSuit.Add(SvgPath.Heart);
        ListCardSuit.Add(SvgPath.Spide);
    }

    void CardState_Changed(object? sender, CardStateEventArgs e)
    {
        Card card = new (e.Id, e.Suit);
        string? cardString = card.ToDisplayString();
        if (string.IsNullOrEmpty(cardString)) return;
        SetHandCard(cardString, e.State);
        TestHand = !e.State
            ? $"{TestHand}{cardString}" 
            : TestHand.Replace(cardString, string.Empty);
        if (TestHand.Length < 4)
        {
            WinString = string.Empty;
            return;
        }
        //WinInfo winInfo = Hand.GetProbabilityOfWinningByMonteCarlo(
        //    Hand.DisplayStringToString(TestHand), NumberOfPlayers);
        //WinString = winInfo.ToString();
        WinString = Hand.GetProbabilityOfWinningByMonteCarlo(
            Hand.DisplayStringToString(TestHand), NumberOfPlayers).ToString();

        void SetHandCard(string card, bool state)
        {
            string handCard = card.Length == 2 
                ? $"{card[1]}\n{card[0]}" : $"{card[2]}\n{card[0]}{card[1]}";
            for (int i = 0; i < HandCards.Length; i++)
            {                
                if (state)
                {
                    if (HandCards[i] != handCard) continue;
                    HandCards[i] = string.Empty;
                }
                else
                {
                    if (!string.IsNullOrEmpty(HandCards[i])) continue;
                    HandCards[i] = handCard;
                }
                break;
            }
        }
    }

    public void ReduceNumberOfPlayers() => ChangeNumberOfPlayers(-1);

    public void IncreaseNumberOfPlayers() => ChangeNumberOfPlayers(1);

    void ChangeNumberOfPlayers(int n)
    {
        int numberOfPlayers = NumberOfPlayers + n;
        if (numberOfPlayers < MinNumberOfPlayers) 
        {
            numberOfPlayers = MinNumberOfPlayers;
        }
        else if (numberOfPlayers > MaxNumberOfPlayers)
        {
            numberOfPlayers = MaxNumberOfPlayers;
        }
        NumberOfPlayers = numberOfPlayers;
        if (TestHand.Length < 4)
        {
            WinString = string.Empty;
            return;
        }
        WinString = Hand.GetProbabilityOfWinningByMonteCarlo(
            Hand.DisplayStringToString(TestHand), NumberOfPlayers).ToString();
    }
    
    public void Reset()
    {
        foreach (CardState cardState in ListCardState)
        {
            if (cardState.IsSelected) continue;
            cardState.CardStateChanged -= CardState_Changed;
            cardState.IsSelected = true;
            cardState.CardStateChanged += CardState_Changed;
        }
        WinString = string.Empty;
        TestHand = string.Empty;
        NumberOfPlayers = TableMax;
        for (int i = 0; i < HandCards.Length; i++)
        {
            HandCards[i] = string.Empty;
        }
    }
}
