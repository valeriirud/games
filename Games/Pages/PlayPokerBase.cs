
using Microsoft.AspNetCore.Components;
using Games.Model;
using Games.Tools;
using static Games.Tools.Definitions;
using System.Reflection.Metadata;

namespace Games.Pages;

public class PlayPokerBase : ComponentBase
{
    List<Card> CardDeck = new();
    List<int>? RandomList;
    public List<string> Hands { get; } = new(new string[MaxNumberOfPlayers]);

    public bool IsGameRunning { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        CardDeck.Clear();
        CardDeck.AddRange(Hand.GetCardDeck());
        await Task.Delay(Definitions.Timeout);
        //await new Task(
        //    new Action(
        //        delegate () {
        //            CardDeck.Clear();
        //            CardDeck.AddRange(Hand.GetCardDeck());
        //        }
        //    )
        //);
    }

    public async Task NewGame()
    {
        await StartNewGame();
    }

    async Task StartNewGame()
    {
        RandomList = CommonTools.GetRandomList();
        await StartGame();

    }

    async Task StartGame()
    {
        await DealerSelection();
        
    }

    async Task DealerSelection()
    {
        List<int> randomList = CommonTools.GetRandomList();
        int n = 0;
        for (int i = 0; i < Hands.Count; i++)
        {
            Card card = CardDeck[randomList[n]];
            Hands[i] = card.ToDisplayString();
            n++;
        }
        await Task.Delay(Definitions.Timeout);
    }
}