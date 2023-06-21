
using System.ComponentModel;

namespace Games.Model;

public class CardState : INotifyPropertyChanged
{
    public event EventHandler<CardStateEventArgs> CardStateChanged = delegate { };

#pragma warning disable 67
    public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67

    readonly int _id;
    readonly int _suit;
    bool _isSelected = true;
    public CardState(int id, int suit, bool state = false)
    {
        _id = id;
        _suit = suit;
        _isSelected = state;
    }        
    public int Id => _id;
    public int Suit => _suit;
    public bool IsSelected
    {
        get { return _isSelected; }
        set
        {
            _isSelected = value;
            OnPropertyChanged(_id, _suit, _isSelected);
        }
    }

    protected void OnPropertyChanged(int id, int suit, bool state)
    {
        EventHandler<CardStateEventArgs>? handler = CardStateChanged;
        if (handler == null) return;
        handler(this, new CardStateEventArgs(id, suit, state));
    }
}
