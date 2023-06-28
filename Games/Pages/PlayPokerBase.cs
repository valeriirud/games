﻿
using Microsoft.AspNetCore.Components;
using Games.Model;
using Games.Tools;
using static Games.Tools.Definitions;
using System.Linq;

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

    public int NumberOfActivePlayers => PlayerObjects.Where(p => ! p.IsFold).Count();

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
        await Task.Delay(Definitions.Timeout * 2);
        _randomList = CommonTools.GetRandomList();
        _pos = 0;
        await Preflop();
    }

    void ClearPlayerObjects(bool stack = false) => PlayerObjects.ToList().ForEach(p => p.Clear(stack));

    async Task DealerSelection()
    {
        _dealerId = -1;
        PlayerObjects.ToList().ForEach(p => p.Update(PlayerObject.Action.SetDealer, false));
        while (true)
        {
            ClearPlayerObjects();
            await Task.Delay(Definitions.Timeout / 2);
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
                await Task.Delay(Definitions.Timeout / 2);
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
        int smallBlindId = GetNextPlayerId(_dealerId);
        int bigBlindId = GetNextPlayerId(smallBlindId);

        PlayerObjects[smallBlindId].Update(PlayerObject.Action.SetBet, _smallBlind);
        PlayerObjects[smallBlindId].PlaceBet(
            Hand.ToDisplayString(GetListOfBoardCards(), false), NumberOfActivePlayers);
        await Task.Delay(Definitions.Timeout);

        PlayerObjects[bigBlindId].Update(PlayerObject.Action.SetBet, _bigBlind);
        PlayerObjects[bigBlindId].PlaceBet(
            Hand.ToDisplayString(GetListOfBoardCards(), false), NumberOfActivePlayers);
        await Task.Delay(Definitions.Timeout);
#if false
        int id = bigBlindId;    
        while (!BetsAreSame())
        {
            int lastBet = PlayerObjects[id].Bet;
            id = GetNextPlayerId(id);
            PlayerObjects[id].PlaceBet(
                Hand.ToDisplayString(GetListOfBoardCards(), false), NumberOfActivePlayers);
            await Task.Delay(Definitions.Timeout);
        }
#endif
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

    List<Card> GetListOfBoardCards()
    {
        List<Card> list = new(); 
        foreach(Card? card in BoardCards)
        {
            if (card == null) continue;
            list.Add(card);
        }
        return list;
    }

    int GetNextPlayerId(int id)
    {
        List<int> ids = GetIdsOfActivePlayers();
        if (id == ids[^1]) return 0;
        int index = ids.IndexOf(id);
        return ids[index + 1];
    }

    List<int> GetIdsOfActivePlayers() => 
        PlayerObjects.Where(p => ! p.IsFold).Select(p => p.Id).OrderBy(i => i).ToList();   

    bool BetsAreSame() 
    {
        List<PlayerObject> players = PlayerObjects.Where(o => !o.IsFold).ToList();
        if (players.Count < 2) return true;
        return players.All(p => p.Bet == players[0].Bet);
    }

    void PlayerObject_Changed(object? sender, EventArgs e)
    {
        StateHasChanged();
    }
    
    void BoardCards_Changed() => StateHasChanged();
}