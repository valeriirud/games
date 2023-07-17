
using Microsoft.AspNetCore.Components;
using Games.Model;
using Games.Tools;
using static Games.Tools.Definitions;

namespace Games.Pages;

public class PlayPokerBase : ComponentBase
{
    int _pos;
    int _dealerId;
    readonly static int _myId = 4;
    readonly static int _bigBlind = 10;
    readonly static int _smallBlind = _bigBlind / 2;
    readonly List<Card> _cardDeck = new();
    List<int> _randomList = new();
    public PlayerObject[] PlayerObjects { get; } = new PlayerObject[MaxNumberOfPlayers];
    public Card?[] BoardCards { get; } = new Card[NumberOfCommunityCards];
    public bool IsGameRunning { get; set; } = false;
    public bool AutoPlay { get; set; } = true;

    bool _isMyAction;
    public bool IsMyAction
    {
        get => _isMyAction;
        set
        {
            _isMyAction = value;
            IsMyAction_Changed();
        }
    }

    int _currentTimeout = Definitions.Timeout;
    public int CurrentTimeout
    {
        get => _currentTimeout;
        set
        {            
            _currentTimeout = value;
            if (_currentTimeout < 200 || _currentTimeout > 5000)
            {
                _currentTimeout = Definitions.Timeout;
            }
        }
    }

    int _currentId;
    public int CurrentId
    {
        get => _currentId;
        set
        {
            _currentId = value;
            IsMyAction = _currentId == _myId;
        }
    }

    int _myBet;
    public int MyBet
    {
        get => _myBet;
        set { _myBet = value; }            
    }

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

    public bool IsCheckAvailable { get; set; } = false;
    public int NumberOfActivePlayers => PlayerObjects.Where(p => p.State ?? true).Count();        

    protected override async Task OnInitializedAsync()
    {
        _cardDeck.Clear();
        _cardDeck.AddRange(Hand.GetCardDeck());
        for (int i = 0; i < PlayerObjects.Length; i++)
        {
            PlayerObjects[i] = new(i, $"Player{i + 1}");
            PlayerObjects[i].Changed += PlayerObject_Changed;
        }
        await Task.Delay(CurrentTimeout);
    }

    async Task<int> MyAction()
    {
        PlayerObjects[_myId].Update(PlayerObject.Operation.SetThinks, true);
        await Task.Delay(CurrentTimeout / 4);
        int maxBet = GetMaxBet();
        string cards = Hand.ToDisplayString(GetListOfBoardCards(), false);
        int odds = PlayerObjects[_myId].GetOdds(cards, NumberOfActivePlayers);
        PlayerObjects[_myId].Update(PlayerObject.Operation.SetOdds, odds);
        while (PlayerObjects[_myId].IsThinks)
        {
            await Task.Delay(CurrentTimeout);
        }
        return PlayerObjects[_myId].Bet > maxBet ? PlayerObjects[_myId].Bet - maxBet : 0;
    }

    public async Task Fold()
    {
        PlayerObjects[_myId].Fold();
        PlayerObjects[_myId].Update(PlayerObject.Operation.SetThinks, false);        
        await Task.Delay(CurrentTimeout);
    }

    public async Task Check()
    {
        PlayerObjects[_myId].Update(PlayerObject.Operation.SetState, true);
        await PlayerObjects[_myId].ChangeBet(0, PlayerAction.Check, true);
        PlayerObjects[_myId].Update(PlayerObject.Operation.SetThinks, false);        
        await Task.Delay(CurrentTimeout);
    }

    public async Task Bet()
    {
        PlayerObjects[_myId].Update(PlayerObject.Operation.SetState, true);
        _ = await PlayerObjects[_myId].ChangeBet(MyBet, PlayerAction.Call, true);
        PlayerObjects[_myId].Update(PlayerObject.Operation.SetThinks, false);
        await Task.Delay(CurrentTimeout);
    }

    public async Task NewGame()
    {
        //List<string> cards = new List<string>() { "5♠A♦A♣Q♦6♦Q♥A♥", "K♣A♠A♣Q♦6♦Q♥A♥" };
        //List<int> ids = await TestWinners(cards);

        if (IsGameRunning) return;
        await StartGame();
    }

    async Task StartGame()
    {
        IsGameRunning = true;
        Pot = 0;
        ClearBoardCards();
        InitPlayers();
        ClearWinners();
        await DealerSelection();
        await Task.Delay(CurrentTimeout * 2);
        _randomList = CommonTools.GetRandomList();
        _pos = 0;
        ShowData(AutoPlay);
        await Task.Delay(CurrentTimeout);
        await Preflop();
        await Task.Delay(CurrentTimeout);
        List<int> activeIds = GetIdsOfActivePlayers();
        if(activeIds.Count == 1)
        {
            Finish();
            return;
        }
        await Flop();
        await Task.Delay(CurrentTimeout);
        activeIds = GetIdsOfActivePlayers();
        if (activeIds.Count == 1)
        {
            Finish();
            return;
        }
        await Turn();
        await Task.Delay(CurrentTimeout);
        activeIds = GetIdsOfActivePlayers();
        if (activeIds.Count == 1)
        {
            Finish();
            return;
        }
        await River();
        await Task.Delay(CurrentTimeout);
        activeIds = GetIdsOfActivePlayers();
        if (activeIds.Count == 1)
        {
            Finish();
            return;
        }
        activeIds = await Shutdown();
        Finish();

        void Finish()
        {
            ShowData(true);
            activeIds.ForEach(id => 
            PlayerObjects[id].Update(PlayerObject.Operation.SetStack, Pot / activeIds.Count));
            activeIds.ForEach(id => PlayerObjects[id].Update(PlayerObject.Operation.SetWinner, true));
            Pot = 0;
            IsGameRunning = false;
        }
    }

    void ShowData(bool show) => PlayerObjects.Where(p => p.Id != _myId).ToList()
            .ForEach(p => p.Update(PlayerObject.Operation.SetShowData, show));    

    void ClearPlayerObjects(bool stack = false) => PlayerObjects.ToList().ForEach(p => p.Clear(stack));

    async Task DealerSelection()
    {
        _dealerId = -1;
        PlayerObjects.ToList().ForEach(p => p.Update(PlayerObject.Operation.SetDealer, false));
        while (true)
        {
            ClearPlayerObjects();
            await Task.Delay(CurrentTimeout / 2);
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
                await Task.Delay(CurrentTimeout / 2);
                n++;
            }
            if (_dealerId != -1) break;
        }
        PlayerObjects[_dealerId].Update(PlayerObject.Operation.SetDealer, true);
    }

    void InitPlayers()
    {
        PlayerObjects.ToList().ForEach(p => p.Update(PlayerObject.Operation.SetStack, 0));
        PlayerObjects.ToList().ForEach(p => p.Update(PlayerObject.Operation.SetStack, MaxStack));
    }

    void ClearWinners() => 
        PlayerObjects.ToList().ForEach(p => p.Update(PlayerObject.Operation.SetWinner, false));    

    async Task Preflop()
    {
        int smallBlindId = GetNextPlayerId(_dealerId);
        if (smallBlindId < 0) return;
        PlayerObjects[smallBlindId].Update(PlayerObject.Operation.SetBet, _smallBlind);
        PlayerObjects[smallBlindId].Update(PlayerObject.Operation.SetStack, -1 * _smallBlind);
        Pot += _smallBlind;           
        await Task.Delay(CurrentTimeout);

        int bigBlindId = GetNextPlayerId(smallBlindId);
        if (bigBlindId < 0) return;
        PlayerObjects[bigBlindId].Update(PlayerObject.Operation.SetBet, _bigBlind);
        PlayerObjects[bigBlindId].Update(PlayerObject.Operation.SetStack, -1*_bigBlind);
        Pot += _bigBlind;
        await Task.Delay(CurrentTimeout);

        PlayerObjects.ToList()
            .ForEach(p => p.Update(PlayerObject.Operation.SetCards, GetCards()));
        await Task.Delay(CurrentTimeout);

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
            await Task.Delay(CurrentTimeout);
        }

        CurrentId = startId;
//        int bet;
        while (true)
        {
            int nextId = GetNextPlayerId(CurrentId);
            if (nextId < 0) break;
            CurrentId = nextId;
#if false
            if (CurrentId != _myId)
            {
                string cards = Hand.ToDisplayString(GetListOfBoardCards(), false);
                bet = await PlayerObjects[CurrentId]
                    .PlaceBet(cards, GetMaxBet(), _bigBlind, Pot, NumberOfActivePlayers);
            }
            else
            {
                bet = await MyAction();
            }
            Pot += bet;   
#endif
            Pot += await GetBet(CurrentId, AutoPlay);
            await Task.Delay(CurrentTimeout);
            PlayerObjects[CurrentId].Update(PlayerObject.Operation.SetMessage, string.Empty);
            if (BetsAreSame()) break;
        }
        PlayerObjects.ToList().ForEach(p => Clear(p));

        async Task<int> GetBet(int id, bool autoPlay)
        {
            if (id == _myId && ! autoPlay) return await MyAction();
            string cards = Hand.ToDisplayString(GetListOfBoardCards(), false);
            return await PlayerObjects[CurrentId]
                .PlaceBet(cards, GetMaxBet(), _bigBlind, Pot, NumberOfActivePlayers);
        }
    }

    async Task Flop()
    {
        await GameStage(3, _dealerId);
        await Task.Delay(CurrentTimeout);
    }

    async Task Turn()
    {
        await GameStage(1, _dealerId);
        await Task.Delay(CurrentTimeout);
    }

    async Task River()
    {
        await GameStage(1, _dealerId);
        await Task.Delay(CurrentTimeout);
    }

    async Task<List<int>> Shutdown()
    {
        List<int> activeIds = GetIdsOfActivePlayers();
        string boardCards = Hand.ToDisplayString(GetListOfBoardCards(), false);
        Dictionary<string, int> cards = new();
        activeIds.ForEach(id => cards[$"{PlayerObjects[id].Cards}{boardCards}"] = id);
        List<string> keys = cards.Keys.ToList();
        List<int> ids = Hand.GetWinnersIds(keys);
        List<int> winners = new();
        ids.ForEach(id => winners.Add(cards[keys[id]]));
        await Task.Delay(CurrentTimeout);
        return winners;
    }

    async Task<List<int>> TestWinners(List<string> cards)
    {        
        List<int> ids = Hand.GetWinnersIds(cards);
        await Task.Delay(CurrentTimeout);
        return ids;
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
    void IsMyAction_Changed() => StateHasChanged();

    public void Checkbox_Changed(ChangeEventArgs e) => AutoPlay = Convert.ToBoolean(e.Value);        
}