
using Microsoft.AspNetCore.Components;
using Games.Model;
using Games.Tools;
using static Games.Tools.Definitions;

namespace Games.Pages;

public class PlayPokerBase : ComponentBase
{
    int _pos;
    int _dealerId;
    readonly List<Card> _cardDeck = new();
    List<int> _randomList = new();
    public PlayerObject[] PlayerObjects { get; } = new PlayerObject[MaxNumberOfPlayers];
    public Card?[] BoardCards { get; } = new Card[NumberOfCommunityCards];

    public bool IsGameRunning { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        _cardDeck.Clear();
        _cardDeck.AddRange(Hand.GetCardDeck());
        for (int i = 0; i < PlayerObjects.Length; i++)
        {
            PlayerObjects[i] = new(i, $"Player{i + 1}");
            PlayerObjects[i].Changed += PlayerObject_Changed;
        }
        await Task.Delay(Definitions.Timeout);
    }

    public async Task NewGame()
    {
        
        await StartGame();
    }

    async Task StartGame()
    {
        ClearBoardCards();
        InitPlayers();
        await DealerSelection();
        _randomList = CommonTools.GetRandomList();
        _pos = 0;
        await Preflop();
    }

    void ClearPlayerObjects() => PlayerObjects.ToList().ForEach(p => p.Clear());

    async Task DealerSelection()
    {
        _dealerId = -1;
        PlayerObjects.ToList().ForEach(p => p.Update(PlayerObject.Action.SetDealer, false));
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

    void InitPlayers() => 
        PlayerObjects.ToList().ForEach(p => p.Update(PlayerObject.Action.SetStack, MaxStack));

    async Task Preflop()
    {        
        PlayerObjects.ToList()
            .ForEach(p => p.Update(PlayerObject.Action.SetCards, GetCards()));
        await Task.Delay(Definitions.Timeout);

        string GetCards()
        {
            Card card1 = _cardDeck[_randomList[_pos]];
            Card card2 = _cardDeck[_randomList[_pos+1]];
            _pos += 2;
            return $"{card1.ToDisplayString()}{card2.ToDisplayString()}";
        }
    }

    void SetBoardCard(int index, Card card)
    {
        BoardCards[index] = card;
        BoardCards_Changed();
    }

    void ClearBoardCards()
    {
        for (int i = 0; i < BoardCards.Length; i++)
        {
            BoardCards[i] = null;            
        }
        BoardCards_Changed();
    }

    void PlayerObject_Changed(object? sender, EventArgs e) => StateHasChanged();
    void BoardCards_Changed() => StateHasChanged();
}