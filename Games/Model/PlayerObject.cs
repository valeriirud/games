
namespace Games.Model;

public class PlayerObject
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Cards { get; set; } = string.Empty;

    public enum Action
    {
        SetCards = 1
    }

    public void Update(Action action, object value)
    {
        switch(action)
        {
            case Action.SetCards:
                SetCards(value);
                break;
            default: break;
        }
    }

    void SetCards(object value) => Cards = value as string ?? string.Empty;

}
