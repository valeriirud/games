
using Games.Tools;
using static Games.Tools.Definitions;

namespace Games.Model;

public class PlayerObject
{
    public event EventHandler Changed = delegate { };
    public int Id { get; private set; }
    readonly string _name;
    public string Name { get; private set; }

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

    public enum Action
    {
        SetName = 1,
        SetCards = 2,
        SetDealer = 3,
        SetStack = 4,
        SetBet = 5,
        ResetState = 6,
        SetOdds = 7,
        SetMessage = 8,
        SetThinks = 9
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
        if (stack)
            Stack = 0;
    }

    public void Update(Action action, object value)
    {
        switch(action)
        {
            case Action.SetName:
                SetName(value);
                break;
            case Action.SetCards:
                SetCards(value);
                break;
            case Action.SetDealer:
                SetDealer(value);
                break;
            case Action.SetStack:
                SetStack(value);
                break;
            case Action.SetBet:
                SetBet(value);
                break;
            case Action.ResetState:
                ResetState(value);
                break;
            case Action.SetOdds:
                SetOdds(value);
                break;
            case Action.SetMessage:
                SetMessage(value);
                break;
            case Action.SetThinks:
                SetThinks(value);
                break;
            default: break;
        }
    }

    void SetMessage(object value)
    {
        _message = value as string ?? string.Empty;
        if(string.IsNullOrEmpty(_message))
        {
            _message = (State ?? true) ? Name : "FOLD";
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

    void ResetState(object value)
    {
        bool state = Convert.ToBoolean(value);
        if (state == true)
        {
            if (State == state)
                State = null;
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
        if( IsThinks )
            Message = "thinks...";
    }

    public int PlaceBet(string commonCards, int maxBet, int bigBlind, int pot, int numberOfPlayers)
    {
        State = true;
        WinInfo winInfo = Hand.GetProbabilityOfWinningByMonteCarlo($"{Cards}{commonCards}", numberOfPlayers);
        Odds = winInfo.Probability;
        int multiplier = Odds / 10;
        int bet = bigBlind * multiplier;

        int stackOdds = Convert.ToInt32(Math.Round(Convert.ToDouble(maxBet) / Convert.ToDouble(Stack), 2) * 100);
        int potOdds = Convert.ToInt32(Math.Round(Convert.ToDouble(maxBet) / Convert.ToDouble(pot), 2) * 100);

        if (stackOdds < 5 && multiplier > 1)
        {
            if (bet < maxBet - Bet)
                bet = maxBet - Bet;
            return ChangeBet(bet, true);
        }
        double factor = Odds > 20 
            ? Math.Round(Convert.ToDouble(Odds*2) / Convert.ToDouble(potOdds)) 
            : 0;
        bet = Convert.ToInt32(factor) * bigBlind;
        if (Bet == bigBlind / 2)
        {
            bet += bigBlind / 2;
        }
        if ((factor == 0 && Bet < maxBet) || bet + Bet < maxBet)
        {
            State = false;
            SetMessage("FOLD");
            return 0;
        }
        return ChangeBet(bet, true);

        int ChangeBet(int changeBet, bool update)
        {
            _changeBet = changeBet;
            SetBet(_changeBet, update);
            SetStack(-1 * _changeBet, update);
            string message = Bet > maxBet 
                ? "RAISE" 
                : Bet > 0 ? "CALL" : "CHECK";
            SetMessage(message);
            return _changeBet;
        }
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
        Console.WriteLine("End =========");
    }

    public void PrintState() => Console.WriteLine($"Id:{Id} State:{State}");
}
