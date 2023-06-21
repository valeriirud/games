
namespace Games.Model;

public class CardStateEventArgs : EventArgs
{
    int _id { get; set; }
    int _suit { get; set; }
    bool _state { get; set; }

    public CardStateEventArgs(int id, int suit, bool state)
    {
        _id = id;
        _suit = suit;   
        _state = state;
    }

    public int Id { get { return _id; } set { _id = value; } }
    public int Suit { get { return _suit; } set { _suit = value; } }
    public bool State { get { return _state; } set { _state = value; } }

}
