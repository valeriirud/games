
using Microsoft.AspNetCore.Components;
using Games.Model;
using Games.Tools;
using static Games.Tools.Definitions;

namespace Games.Pages;

public class PlayPokerBase : ComponentBase
{
    int _dealerId;
    readonly List<Card> _cardDeck = new();
    //List<int>? _randomList;
    public List<PlayerObject> PlayerObjects { get; } = new(new PlayerObject[MaxNumberOfPlayers]);

    public bool IsGameRunning { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        _cardDeck.Clear();
        _cardDeck.AddRange(Hand.GetCardDeck());
        for (int i = 0; i < PlayerObjects.Count; i++)
        {
            PlayerObjects[i] = new(i, $"Player{i + 1}");
            PlayerObjects[i].Changed += PlayerObject_Changed;
        }
        await Task.Delay(Definitions.Timeout);
    }

    public async Task NewGame()
    {
        await StartNewGame();
    }

    async Task StartNewGame()
    {
        //_randomList = CommonTools.GetRandomList();
        await StartGame();

    }

    async Task StartGame()
    {
        await DealerSelection();
        
    }

    void ClearPlayerObjects() => PlayerObjects.ForEach(p => p.Clear());

    async Task DealerSelection()
    {
        _dealerId = -1;
        PlayerObjects.ForEach(p => p.Update(PlayerObject.Action.SetDealer, false));
        while (true)
        {
            ClearPlayerObjects();
            await Task.Delay(Definitions.Timeout);
            List<int> randomList = CommonTools.GetRandomList();
            int n = 0;
            foreach (PlayerObject player in PlayerObjects)
            {
                Card card = _cardDeck[randomList[n]];
                player.Update(PlayerObject.Action.SetCards, card.ToDisplayString());
                if (card.Id == CardId.Ace)
                {
                    _dealerId = player.Id;
                    break;
                }
                await Task.Delay(Definitions.Timeout);
                n++;
            }
            if (_dealerId != -1) break;
        }
        PlayerObjects[_dealerId].Update(PlayerObject.Action.SetDealer, true);
    }

    void PlayerObject_Changed(object? sender, EventArgs e) => StateHasChanged();
}