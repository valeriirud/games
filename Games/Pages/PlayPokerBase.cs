
using Microsoft.AspNetCore.Components;
using Games.Model;
using Games.Tools;
using static Games.Tools.Definitions;

namespace Games.Pages;

public class PlayPokerBase : ComponentBase
{
    int _pos;
    int _dealerId;
    readonly static int _bigBlind = 10;
    readonly static int _smallBlind = _bigBlind / 2;
    readonly List<Card> _cardDeck = new();
    List<int> _randomList = new();
    public PlayerObject[] PlayerObjects { get; } = new PlayerObject[MaxNumberOfPlayers];
    public Card?[] BoardCards { get; } = new Card[NumberOfCommunityCards];
    public bool IsGameRunning { get; set; } = false;
    public int NumberOfActivePlayers => PlayerObjects.Where(p => p.State ?? true).Count();
    int _pot;
    public int Pot
    {
        get => _pot;
        set
        {
            _pot = value;
            Pot_Changed();
        }
    }

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
        if (IsGameRunning) return;
        await StartGame();
    }

    async Task StartGame()
    {
        IsGameRunning = true;
        Pot = 0;
        ClearBoardCards();
        InitPlayers();
        await DealerSelection();
        await Task.Delay(Definitions.Timeout * 2);
        _randomList = CommonTools.GetRandomList();
        _pos = 0;        
        await Preflop();
        await Task.Delay(Definitions.Timeout);
        List<int> activeIds = GetIdsOfActivePlayers();
        if(activeIds.Count == 1)
        {
            Finish();
            return;
        }
        await Flop();
        await Task.Delay(Definitions.Timeout);
        activeIds = GetIdsOfActivePlayers();
        if (activeIds.Count == 1)
        {
            Finish();
            return;
        }
        await Turn();
        await Task.Delay(Definitions.Timeout);
        activeIds = GetIdsOfActivePlayers();
        if (activeIds.Count == 1)
        {
            Finish();
            return;
        }
        await River();
        await Task.Delay(Definitions.Timeout);
        activeIds = GetIdsOfActivePlayers();
        if (activeIds.Count == 1)
        {
            Finish();
            return;
        }
        await Shutdown();
        Finish();

        void Finish()
        {
            activeIds.ForEach(id => 
            PlayerObjects[id].Update(PlayerObject.Operation.SetStack, Pot / activeIds.Count));        
            Pot = 0;
            IsGameRunning = false;
        }
    }

    void ClearPlayerObjects(bool stack = false) => PlayerObjects.ToList().ForEach(p => p.Clear(stack));

    async Task DealerSelection()
    {
        _dealerId = -1;
        PlayerObjects.ToList().ForEach(p => p.Update(PlayerObject.Operation.SetDealer, false));
        while (true)
        {
            ClearPlayerObjects();
            await Task.Delay(Definitions.Timeout / 2);
            List<int> randomList = CommonTools.GetRandomList();
            int n = 0;
            foreach (PlayerObject player in PlayerObjects)
            {
                Card card = _cardDeck[randomList[n]];
                player.Update(PlayerObject.Operation.SetCards, card.ToDisplayString());
                if (card.Id == CardId.Ace)
                {
                    _dealerId = player.Id;
                    break;
                }
                await Task.Delay(Definitions.Timeout / 2);
                n++;
            }
            if (_dealerId != -1) break;
        }
        PlayerObjects[_dealerId].Update(PlayerObject.Operation.SetDealer, true);
    }

    void InitPlayers() => 
        PlayerObjects.ToList().ForEach(p => p.Update(PlayerObject.Operation.SetStack, MaxStack));
    
    async Task Preflop()
    {
        int smallBlindId = GetNextPlayerId(_dealerId);
        if (smallBlindId < 0) return;
        PlayerObjects[smallBlindId].Update(PlayerObject.Operation.SetBet, _smallBlind);
        PlayerObjects[smallBlindId].Update(PlayerObject.Operation.SetStack, -1 * _smallBlind);
        Pot += _smallBlind;           
        await Task.Delay(Definitions.Timeout);

        int bigBlindId = GetNextPlayerId(smallBlindId);
        if (bigBlindId < 0) return;
        PlayerObjects[bigBlindId].Update(PlayerObject.Operation.SetBet, _bigBlind);
        PlayerObjects[bigBlindId].Update(PlayerObject.Operation.SetStack, -1*_bigBlind);
        Pot += _bigBlind;
        await Task.Delay(Definitions.Timeout);

        PlayerObjects.ToList()
            .ForEach(p => p.Update(PlayerObject.Operation.SetCards, GetCards()));
        await Task.Delay(Definitions.Timeout);

        await GameStage(0, bigBlindId);

        string GetCards()
        {
            Card card1 = _cardDeck[_randomList[_pos]];
            Card card2 = _cardDeck[_randomList[_pos + 1]];
            _pos += 2;
            return $"{card1.ToDisplayString()}{card2.ToDisplayString()}";
        }
    }

    static void Clear(PlayerObject player)
    {
        player.Update(PlayerObject.Operation.SetBet, 0);
        player.Update(PlayerObject.Operation.ResetState, true);
        player.Update(PlayerObject.Operation.SetOdds, 0);
    }

    async Task GameStage(int n, int startId)
    {
        for (int i = 0; i < n; i++)
        {
            SetBoardCard(GetCard());
            await Task.Delay(Definitions.Timeout);
        }

        int id = startId;

        while (true)
        {
            int nextId = GetNextPlayerId(id);
            if (nextId < 0) break;
            id = nextId;
            string cards = Hand.ToDisplayString(GetListOfBoardCards(), false);
            int bet = await PlayerObjects[id].PlaceBet(cards, GetMaxBet(), _bigBlind, Pot, NumberOfActivePlayers);
            Pot += bet;     
            PlayerObjects[id].Update(PlayerObject.Operation.SetMessage, string.Empty);
            if (BetsAreSame()) break;
        }
        PlayerObjects.ToList().ForEach(p => Clear(p));
    }

    async Task Flop()
    {
        await GameStage(3, _dealerId);
        await Task.Delay(Definitions.Timeout);
    }

    async Task Turn()
    {
        await GameStage(1, _dealerId);
        await Task.Delay(Definitions.Timeout);
    }

    async Task River()
    {
        await GameStage(1, _dealerId);
        await Task.Delay(Definitions.Timeout);
    }

    async Task Shutdown()
    {
        await Task.Delay(Definitions.Timeout);
    }

    Card GetCard()
    {
        Card card = _cardDeck[_randomList[_pos]];        
        _pos ++;
        return card;
    }
    void SetBoardCard(Card card, int index = -1)
    {
        if(index >= 0)
            BoardCards[index] = card;
        else
        {
            for(int i = 0; i < BoardCards.Length; i ++)
            {
                if (BoardCards[i] != null) continue;
                BoardCards[i] = card;
                break;
            }
        }
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

    List<Card> GetListOfBoardCards()
    {
        List<Card> list = new();
        foreach (Card? card in BoardCards)
        {
            if (card == null) continue;
            list.Add(card);
        }
        return list;
    }

    int GetNextPlayerId(int prevId)
    {
        int id = prevId;
        for (int i = 0; i < PlayerObjects.Length; i++)
        {
            id++;
            if(id > PlayerObjects[^1].Id)
            {
                id = 0;
            }
            if (PlayerObjects[id].State ?? true) break;
        }
        return id;
    }

    List<int> GetIdsOfActivePlayers() => 
        PlayerObjects.Where(p => p.State ?? true).Select(p => p.Id).OrderBy(i => i).ToList();

    bool BetsAreSame() 
    {
        if(PlayerObjects.Any(o => o.State == null)) return false;
        List<PlayerObject> players = PlayerObjects.Where(o => o.State == true).ToList();
        if (players.Count < 2) return true;
        return players.All(p => p.Bet == players[0].Bet);
    }

    int GetMaxBet() => PlayerObjects.MaxBy(p => p.Bet)?.Bet ?? 0;

    void PlayerObject_Changed(object? sender, EventArgs e)
    {
        PlayerObject? playerObject = sender as PlayerObject;
        if (playerObject == null) return;
        StateHasChanged();
    }
    void BoardCards_Changed() => StateHasChanged();
    void Pot_Changed() => StateHasChanged();
}