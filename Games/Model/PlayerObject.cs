﻿
using Games.Tools;
using Microsoft.VisualBasic;
using System;
using static Games.Tools.Definitions;

namespace Games.Model;

public class PlayerObject
{
    public event EventHandler Changed = delegate { };
    public int Id { get; private set; }
    readonly string _name;
    public string Name { get; private set; }

    bool _showData;
    public bool ShowData 
    { 
        get => _showData;
        set 
        {
            _showData = value;
            Changed?.Invoke(this, EventArgs.Empty);
        } 
    }

    PlayerAction _action;
    public PlayerAction Action 
    { 
        get => _action;
        set 
        { 
            _action = value;
            Changed.Invoke(this, new EventArgs());
        } 
    }

    bool _isWinner;
    public bool IsWinner
    {
        get => _isWinner;
        set
        {
            _isWinner = value;
            Changed.Invoke(this, new EventArgs());
        }
    }

    bool _isThinks;
    public bool IsThinks
    {
        get => _isThinks;
        set
        {
            _isThinks = value;
            Changed.Invoke(this, new EventArgs());
        }
    }

    string _message;
    public string Message 
    {
        get => _message;
        set
        {
            _message = value;
            Changed.Invoke(this, new EventArgs());
        }
    }
    string _cards;
    public string Cards
    {
        get => _cards; 
        set
        {
            _cards = value;
            Changed.Invoke(this, new EventArgs());
        }
    }
    bool _isDealer;
    public bool IsDealer
    {
        get => _isDealer;
        set
        {
            _isDealer = value;
            Changed.Invoke(this, new EventArgs());
        }
    }

    // true - place bet, false - fold, null - do not place bet 
    bool? _state;
    public bool? State 
    {
        get => _state;
        set
        {
            _state = value;
            Changed.Invoke(this, new EventArgs());
        }
    }
    int _stack;
    public int Stack
    {
        get => _stack;
        set
        {
            _stack = value;
            Changed.Invoke(this, new EventArgs());
        }
    }
    int _bet;
    public int Bet
    {
        get => _bet;
        set
        {
            _bet = value;
            Changed.Invoke(this, new EventArgs());
        }
    }

    int _odds;
    public int Odds
    {
        get => _odds;
        private set
        {
            _odds = value;            
            Changed.Invoke(this, new EventArgs());
        }
    }

    int _changeBet;

    public bool IsActive => State ?? true;
    public bool IsAllIn => State ?? true && Bet > 0 && Stack == 0;

    public enum Operation
    {
        SetName = 1,
        SetCards = 2,
        SetDealer = 3,
        SetStack = 4,
        SetBet = 5,
        ResetState = 6,
        SetOdds = 7,
        SetMessage = 8,
        SetThinks = 9,
        SetWinner = 10,
        SetState = 11,
        SetShowData = 12
    }

    public PlayerObject(int id, string name, int stack = 0)
    {
        Id = id;
        _name = name;
        Name = _name;
        _message = Name;
        _cards = string.Empty;
        _isDealer = false;
        _state = null;
        _stack = stack;
        _bet = 0;
        _isThinks = false;
        _showData = true;
        Action = PlayerAction.None;
    }

    public void Clear(bool stack = false)
    {
        Message = Name;
        Cards = string.Empty;
        IsDealer = false;
        State = null;
        Bet = 0;
        Odds = 0;
        IsThinks = false;
        ShowData = true;
        Action = PlayerAction.None;
        if (!stack) return;
        Stack = 0;        
    }

    public void Update(Operation action, object value)
    {
        switch(action)
        {
            case Operation.SetName:
                SetName(value);
                break;
            case Operation.SetCards:
                SetCards(value);
                break;
            case Operation.SetDealer:
                SetDealer(value);
                break;
            case Operation.SetStack:
                SetStack(value);
                break;
            case Operation.SetBet:
                SetBet(value);
                break;
            case Operation.ResetState:
                ResetState(value);
                break;
            case Operation.SetOdds:
                SetOdds(value);
                break;
            case Operation.SetMessage:
                SetMessage(value);
                break;
            case Operation.SetThinks:
                SetThinks(value);
                break;
            case Operation.SetWinner:
                SetWinner(value);
                break;
            case Operation.SetState:
                SetState(value);
                break;
            case Operation.SetShowData:
                SetShowData(value);
                break;
            default: break;
        }
    }

    void SetMessage(object value)
    {
        Message = value as string ?? string.Empty;
        if(string.IsNullOrEmpty(_message))
        {
            Message = (State ?? true) ? Name : PlayerAction.Fold.Description();
        }
    }
    void SetName(object value) 
    {
        string name = value as string ?? string.Empty;
        Name = string.IsNullOrEmpty(name) ? _name : name;
        SetMessage(Name);
    }
    void SetCards(object value) => Cards = value as string ?? string.Empty;
    void SetDealer(object value) => IsDealer = Convert.ToBoolean(value);
    void SetStack(object value, bool update = false) => Stack = SetValue(Stack, value, update);
    void SetBet(object value, bool update = false) => Bet = SetValue(Bet, value, update);    

    static int SetValue(int value, object ob, bool update)
    {
        int num = Convert.ToInt32(ob);
        if (num == 0 && ! update) return 0;
        return value + num;
    }

    void SetOdds(object ob) => Odds = Convert.ToInt32(ob);

    void SetState(object value) => State = Convert.ToBoolean(value);
    void ResetState(object value)
    {
        bool state = Convert.ToBoolean(value);
        if (state == true)
        {
            if (State == state)
            {
                State = null;
            }
        }
        else
        {
            State = null;
        }
    }

    void SetThinks(object value)
    {
        bool state = Convert.ToBoolean(value);
        IsThinks = state;
        if (IsThinks)
        {
            Action = PlayerAction.Thinks;
            Message = Action.Description();
        }
    }

    void SetWinner(object value) => IsWinner = Convert.ToBoolean(value);

    void SetShowData(object value) => ShowData = Convert.ToBoolean(value);

    public int GetOdds(string commonCards, int numberOfPlayers)
    {        
        WinInfo winInfo = Hand.GetProbabilityOfWinningByMonteCarlo($"{Cards}{commonCards}", 
            numberOfPlayers);
        return winInfo.Probability;
    }

    public async Task<int> PlaceBet(string commonCards, int maxBet, int bigBlind, int pot, int numberOfPlayers)
    {
        State = true;
        string handCards = $"{Cards}{commonCards}";
        SetThinks(true);
        await Task.Delay(Definitions.Timeout / 4);
        WinInfo winInfo = Hand.GetProbabilityOfWinningByMonteCarlo($"{handCards}", numberOfPlayers);
        SetThinks(false);
        Odds = winInfo.Probability;
        int stackOdds = GetPercent(maxBet, Stack) + 1;
        int potOdds = GetPercent(maxBet, pot) + 1;

        int multiplier = Odds / 10;
        if(multiplier > 0)
        {
            multiplier -= 1;
        }
        int bet = bigBlind * multiplier;       
        AdjustSmallBlind();
        if (commonCards.Length == 0 && Odds < 20 && Bet < maxBet)
        {
            Hand hand = Hand.FromAlternateString(Cards);
            if (! hand.Cards.Any(c => c.Id > CardId.Jack)
                && ! hand.Cards.All(c => c.Id == hand.Cards[0].Id)) return Fold();
            if (maxBet - Bet > bigBlind * 3) return Fold();
            bet = maxBet - Bet;
        }
        if (Bet + bet < maxBet)
        {
            return Fold();
        }
        if (Bet + bet < maxBet + bigBlind*2 || Bet >= Stack)
        {
            bet = maxBet - Bet;
        }               

        return await ChangeBet(bet, maxBet, true);        

        int GetPercent(int n, int m) =>
            Convert.ToInt32(Math.Round(Convert.ToDouble(n) / Convert.ToDouble(m), 2) * 100);

        void AdjustSmallBlind()
        {
            if (Bet != bigBlind / 2) return;
            bet += bigBlind / 2;            
        }

        async Task<int> ChangeBet(int changeBet, int maxBet, bool update)
        {
            _changeBet = changeBet;
            SetBet(_changeBet, update);
            SetStack(-1 * _changeBet, update);
            Action = Bet > maxBet
                ? PlayerAction.Raise
                : Bet > 0 ? PlayerAction.Call : PlayerAction.Check;
            SetMessage(Action.Description());
            await Task.Delay(Definitions.Timeout);
            return _changeBet;
        }
    }

    public async Task<int> ChangeBet(int changeBet, PlayerAction action, bool update)
    {
        _changeBet = changeBet;
        SetBet(_changeBet, update);
        SetStack(-1 * _changeBet, update);
        SetMessage(action.Description());
        await Task.Delay(Definitions.Timeout);
        return _changeBet;
    }

    public int Fold()
    {
        State = false;
        Action = PlayerAction.Fold;
        SetMessage(Action.Description());
        return 0;
    }

    public async Task<int> AllIn()
    {
        Action = PlayerAction.AllIn;
        int bet = Stack;
        await ChangeBet(bet, PlayerAction.AllIn, true);
        return bet;
    }

    public void PrintInfo()
    {
        Console.WriteLine("Begin ========");
        Console.WriteLine($"Id:{Id}");
        Console.WriteLine($"Bet:{Bet}");
        Console.WriteLine($"ChangeBet:{_changeBet}");
        Console.WriteLine($"Stack:{Stack}");
        Console.WriteLine($"Odds:{Odds}");
        Console.WriteLine($"State:{State}");
        Console.WriteLine($"IsThinks:{IsThinks}");
        Console.WriteLine($"Action:{Action.Description()}");
        Console.WriteLine("End =========");
    }

    public void PrintState() => Console.WriteLine($"Id:{Id} State:{State}");
}
