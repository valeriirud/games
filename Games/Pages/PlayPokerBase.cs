
using Microsoft.AspNetCore.Components;
using Games.Model;
using Games.Tools;
using static Games.Tools.Definitions;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Web;

namespace Games.Pages;

public class PlayPokerBase : ComponentBase
{
    int _dealerId;
    List<Card> _cardDeck = new();
    List<int>? _randomList;
    public List<PlayerObject> PlayerObjects { get; } = new(new PlayerObject[MaxNumberOfPlayers]);

    public bool IsGameRunning { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        _cardDeck.Clear();
        _cardDeck.AddRange(Hand.GetCardDeck());
        for(int i = 0; i < PlayerObjects.Count; i ++)
        {
            PlayerObjects[i] = new ();
        }
        await Task.Delay(Definitions.Timeout);
    }

    public async Task NewGame()
    {
        await StartNewGame();
    }

    async Task StartNewGame()
    {
        _randomList = CommonTools.GetRandomList();
        await StartGame();

    }

    async Task StartGame()
    {
        await DealerSelection();
        
    }

    void ClearPlayerObjects()
    {
        for (int i = 0; i < PlayerObjects.Count; i++)
        {
            PlayerObjects[i].Cards = string.Empty;
        }
    }

    async Task DealerSelection()
    {
        _dealerId = -1;
        while (true)
        {
            ClearPlayerObjects();
            StateHasChanged();
            await Task.Delay(Definitions.Timeout);
            List<int> randomList = CommonTools.GetRandomList();
            int n = 0;
            for (int i = 0; i < PlayerObjects.Count; i++)
            {
                Card card = _cardDeck[randomList[n]];
                PlayerObjects[i].Update(PlayerObject.Action.SetCards, card.ToDisplayString());
                StateHasChanged();
                if (card.Id == CardId.Ace)
                {
                    _dealerId = i;
                    break;
                }
                await Task.Delay(Definitions.Timeout);
                n++;
            }
            if (_dealerId != -1) break;
            StateHasChanged();
        }
    }
}