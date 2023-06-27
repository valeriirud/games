
namespace Games.Model;

public class PlayerObject
{
    public event EventHandler Changed = delegate { };
    public int Id { get; private set; }
    public string Name { get; private set; }
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

    public enum Action
    {
        SetName = 1,
        SetCards = 2,
        SetDealer = 3,
        SetStack = 4,
        SetBet = 5
    }

    public PlayerObject(int id, string name, int stack = 0)
    {
        Id = id;
        Name = name;
        _message = Name;
        _cards = string.Empty;
        _isDealer = false;
        _stack = stack;
        _bet = 0;
    }

    public void Clear()
    {
        _message = Name;
        _cards = string.Empty;
        _isDealer = false;
        _stack = 0;
        _bet = 0;
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
            default: break;
        }
    }

    void SetName(object value) => Name = value as string ?? string.Empty;
    void SetCards(object value) => Cards = value as string ?? string.Empty;
    void SetDealer(object value) => IsDealer = Convert.ToBoolean(value);
    void SetStack(object value) => Stack = SetValue(Stack, value);
    void SetBet(object value) => Bet = SetValue(Bet, value);

    static int SetValue(int value, object ob)
    {
        int num = Convert.ToInt32(ob);
        if (num == 0) return 0;
        return value + num;
    }
}
