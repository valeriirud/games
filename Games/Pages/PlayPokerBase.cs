﻿
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
    public List<Pot> Pots { get; private set; } = new();
    public PlayerObject[] PlayerObjects { get; } = new PlayerObject[MaxNumberOfPlayers];
    public Card?[] BoardCards { get; } = new Card[NumberOfCommunityCards];

    bool _isGameRunning = false;
    public bool IsGameRunning 
    { 
        get => _isGameRunning; 
        set
        {
            _isGameRunning = value;
            IsGameRunning_Changed();
        }
    }
    
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
        set 
        { 
            _myBet = value;
            MyBet_Changed();
        }            
    }

    public string BetTitle => MyBet < PlayerObjects[_myId].Stack ? $"Bet {MyBet}" : "All-In";
    public bool MyBetNotCorrect => MyBet + PlayerObjects[_myId].Bet < MaxBet 
        || MyBet > PlayerObjects[_myId].Stack;
    public bool CheckNotAllowed => ActivePlayers.Any(p => p.Bet > 0);
    List<PlayerObject> ActivePlayers => PlayerObjects.Where(p => p.IsActive == true).ToList();            
    List<int> ActivePlayersIds => ActivePlayers.Select(p => p.Id).OrderBy(i => i).ToList();
    int MaxBet => PlayerObjects.MaxBy(p => p.Bet)?.Bet ?? 0;
    int NumberOfActivePlayers => ActivePlayers.Count;

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
        string cards = Hand.ToDisplayString(GetListOfBoardCards(), false);
        int odds = PlayerObjects[_myId].GetOdds(cards, NumberOfActivePlayers);
        PlayerObjects[_myId].Update(PlayerObject.Operation.SetOdds, odds);
        MyBet = MaxBet - PlayerObjects[_myId].Bet;
        while (PlayerObjects[_myId].IsThinks) 
            await Task.Delay(CurrentTimeout);        
        int bet = PlayerObjects[_myId].Bet - MaxBet;
        return bet >= 0 ? MyBet : 0;
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
        PlayerAction action = PlayerAction.Call;
        if(MyBet > MaxBet)
        {
            action = PlayerAction.Raise;
        }
        if(PlayerObjects[_myId].Stack == 0 && PlayerObjects[_myId].Bet > 0)
        {
            action = PlayerAction.AllIn;
        }
        _ = await PlayerObjects[_myId].ChangeBet(MyBet, action, true);
        PlayerObjects[_myId].Update(PlayerObject.Operation.SetThinks, false);
        await Task.Delay(CurrentTimeout);
    }

    public async Task NewGame()
    {
        //List<string> cards = new List<string>() { "2♣Q♠2♦J♦2♠10♣A♠", "Q♥2♥2♦J♦2♠10♣A♠" };
        //List<int> ids = await TestWinners(cards);

        if (IsGameRunning) return;
        await StartGame();
    }

    async Task StartGame()
    {
        IsGameRunning = true;
        Pots.Clear();
        AddPot();
        
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
        if(NumberOfActivePlayers == 1)
        {
            Finish(ActivePlayersIds);
            return;
        }
        await Flop();
        await Task.Delay(CurrentTimeout);        
        if (NumberOfActivePlayers == 1)
        {
            Finish(ActivePlayersIds);
            return;
        }
        await Turn();
        await Task.Delay(CurrentTimeout);
        if (NumberOfActivePlayers == 1)
        {
            Finish(ActivePlayersIds);
            return;
        }
        await River();
        await Task.Delay(CurrentTimeout);        
        if (NumberOfActivePlayers == 1)
        {
            Finish(ActivePlayersIds);
            return;
        }
        List<int> ids = await Shutdown();
        Finish(ids);

        void Finish(List<int> allIds)
        {
            ShowData(true);
            foreach (Pot pot in Pots)
            {
                List<int> ids = pot.GetIds(allIds);
                ids.ForEach(id => PlayerObjects[id].Update(PlayerObject.Operation.SetStack, pot.Value / ids.Count));
                ids.ForEach(id => PlayerObjects[id].Update(PlayerObject.Operation.SetWinner, true));
                pot.Clear();
            }
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
        Pots[^1].Update(smallBlindId, _smallBlind);           
        await Task.Delay(CurrentTimeout);

        int bigBlindId = GetNextPlayerId(smallBlindId);
        if (bigBlindId < 0) return;
        PlayerObjects[bigBlindId].Update(PlayerObject.Operation.SetBet, _bigBlind);
        PlayerObjects[bigBlindId].Update(PlayerObject.Operation.SetStack, -1*_bigBlind);
        Pots[^1].Update(bigBlindId, _bigBlind);
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
        int bet;
        while (true)
        {
            int nextId = GetNextPlayerId(CurrentId);
            if (nextId < 0) break;
            CurrentId = nextId;
            if (PlayerObjects[CurrentId].Stack == 0) continue;
            bet = await GetBet(CurrentId, AutoPlay);
            Pots[^1].Update(CurrentId, bet);
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
                .PlaceBet(cards, MaxBet, _bigBlind, Pots[^1].Value, NumberOfActivePlayers);
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
        string boardCards = Hand.ToDisplayString(GetListOfBoardCards(), false);
        Dictionary<string, int> cards = new();
        ActivePlayersIds.ForEach(id => cards[$"{PlayerObjects[id].Cards}{boardCards}"] = id);
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

    //List<Card> ListOfBoardCards => BoardCards.Where(c => c != null).Select(c => new Card(c)).ToList();

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
            if (PlayerObjects[id].IsActive) break;
        }
        return id;
    }

    bool BetsAreSame() 
    {
        if(PlayerObjects.Any(o => o.State == null)) return false;
        List<PlayerObject> players = PlayerObjects.Where(o => o.State == true).ToList();
        if (players.Count < 2) return true;
        List<PlayerObject> list = players.Where(p => p.Bet < MaxBet).ToList();
        if (!list.Any()) return true;
        if (list.All(l => l.Stack == 0)) return true;
        return false;
    }    

    void PlayerObject_Changed(object? sender, EventArgs e)
    {
        PlayerObject? playerObject = sender as PlayerObject;
        if (playerObject == null) return;
        StateHasChanged();
    }

    void AddPot()
    {
        Pots.Add(new());
        Pots[^1].Changed += Pot_Changed;
    }

    void BoardCards_Changed() => StateHasChanged();
    public void Pot_Changed(object? sender, EventArgs e) => StateHasChanged();
    void IsMyAction_Changed() => StateHasChanged();
    public void Checkbox_Changed(ChangeEventArgs e) => AutoPlay = Convert.ToBoolean(e.Value);
    public void MyBet_Changed(ChangeEventArgs e) 
    {        
        int number;
        bool success = int.TryParse(e?.Value?.ToString(), out number);
        MyBet = success ? number : 0;
    }
    void MyBet_Changed() => StateHasChanged();

    void IsGameRunning_Changed() => StateHasChanged();

    public void Timeout_Selected(string value) => CurrentTimeout = Convert.ToInt32(value);
    
    public void TableMax_Selected(string value)
    {
        Console.WriteLine(value);
    }

    public async void TestAllIn()
    {
        List<PlayerObject> players = new (ActivePlayers);
        foreach(PlayerObject player in players)
        {
            int bet = await player.AllIn();
        }
    }
}