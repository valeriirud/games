using Microsoft.AspNetCore.Components;

namespace Games.Pages;

public class PokerBase : ComponentBase
{
    public const int PLAYERS_COUNT = 9;
    const int MAX_BANKROLL = 100;
    const int MIN_BET = 10;
    public string Title { get; set; } = "Texas hold'em";
    public string[,] Images { get; }  = new string[PLAYERS_COUNT, 5];
    public string[] Board { get; } = new string[5];
    public string CroupierImage { get; set; } = string.Empty;
    public string ImageDialer { get; } = @"images\dialer.svg";
    public int DialerId { get; set; } = -1;
    public int Bank { get; set; } = 0;

    public string[] Actions { get; } = new string[PLAYERS_COUNT];
    

    public int[] Bankroll { get; } = new int[PLAYERS_COUNT];

    readonly List<Tuple<int,int>> _indexes = new();
    readonly List<Tuple<int, int>>[] _hands = new List<Tuple<int, int>>[PLAYERS_COUNT];

    readonly string CardBack = @"images\red.svg";
    readonly string[,] _images = 
        {
            {
                @"images\clubs_2.svg",
                @"images\clubs_3.svg",
                @"images\clubs_4.svg",
                @"images\clubs_5.svg",
                @"images\clubs_6.svg",
                @"images\clubs_7.svg",
                @"images\clubs_8.svg",
                @"images\clubs_9.svg",
                @"images\clubs_10.svg",
                @"images\clubs_jack.svg",
                @"images\clubs_queen.svg",
                @"images\clubs_king.svg",
                @"images\clubs_ace.svg" 
            },
            {
                @"images\diamonds_2.svg",
                @"images\diamonds_3.svg",
                @"images\diamonds_4.svg",
                @"images\diamonds_5.svg",
                @"images\diamonds_6.svg",
                @"images\diamonds_7.svg",
                @"images\diamonds_8.svg",
                @"images\diamonds_9.svg",
                @"images\diamonds_10.svg",
                @"images\diamonds_jack.svg",
                @"images\diamonds_queen.svg",
                @"images\diamonds_king.svg",
                @"images\diamonds_ace.svg" 
            },
            {
                @"images\hearts_2.svg",
                @"images\hearts_3.svg",
                @"images\hearts_4.svg",
                @"images\hearts_5.svg",
                @"images\hearts_6.svg",
                @"images\hearts_7.svg",
                @"images\hearts_8.svg",
                @"images\hearts_9.svg",
                @"images\hearts_10.svg",
                @"images\hearts_jack.svg",
                @"images\hearts_queen.svg",
                @"images\hearts_king.svg",
                @"images\hearts_ace.svg"
            },
            {
                @"images\spades_2.svg",
                @"images\spades_3.svg",
                @"images\spades_4.svg",
                @"images\spades_5.svg",
                @"images\spades_6.svg",
                @"images\spades_7.svg",
                @"images\spades_8.svg",
                @"images\spades_9.svg",
                @"images\spades_10.svg",
                @"images\spades_jack.svg",
                @"images\spades_queen.svg",
                @"images\spades_king.svg",
                @"images\spades_ace.svg"
            }
        };

    void Inint()
    {
        for (int i = 0; i < PLAYERS_COUNT; i++)
        {
            _hands[i] = new ();
            Bankroll[i] = MAX_BANKROLL;
            Bank = 0;
            DialerId = -1;
        }
    }
    public async Task NewGame()
    {
        Inint();
        Clear();
        await DealerSelection();
        await SmallBlind();
        await BigBlind();
        await Task.Delay(3000);
        await PocketDistribution();
    }

    void Clear()
    {
        Title = string.Empty;
        _indexes.Clear();
        CroupierImage = string.Empty;
        for(int i = 0; i < PLAYERS_COUNT; i ++)
        {
            _hands[i].Clear();
            for (int j = 0; j < 5; j++)
            {
                Images[i,j] = string.Empty;
            }
        }                
    }

    Tuple<int, int> GetIndex()
    {
        int index1 = -1;
        int index2 = -1;
        while (index1 < 0 || index2 < 0)
        {
            Random r = new Random();
            if (index1 < 0)
            {
                index1 = r.Next(0, _images.GetUpperBound(0) + 1);
            }
            if (index2 < 0)
            {
                index2 = r.Next(0, _images.GetUpperBound(1) + 1);
            }
            if (_indexes.Any(i => i.Item1 == index1 && i.Item2 == index2))
            {
                index1 = -1;
                index2 = -1;
            }
            else
            {
                _indexes.Add(new(index1, index2));
            }
        }
        return new (index1, index2);
    }

    async Task DealerSelection()
    {
        int id = 0;
        while (true)
        {
            for (int i = 0; i < PLAYERS_COUNT; i++)
            {
                Tuple<int, int> index = GetIndex();
                CroupierImage = _images[index.Item1, index.Item2];
                StateHasChanged();
                await Task.Delay(200);
                CroupierImage = string.Empty;
                StateHasChanged();
                Images[i, 0] = _images[index.Item1, index.Item2];
                _hands[i].Add(index);
                StateHasChanged();
            }
            List<int> maxHands = GetMaxHands();
            await Task.Delay(2000);
            if (maxHands.Count > 1)
            {
                Clear();
                continue;
            }
            id = maxHands[0];
            break;
        }
        await Task.Delay(5000);
        StateHasChanged();
        Clear();
        DialerId = id;
        await Task.Delay(3000);
        StateHasChanged();
    }

    List<int> GetMaxHands()
    {
        int maxHand = -1;
        List<int> maxHands = new() { 0 };
        for(int i = 1; i < PLAYERS_COUNT; i ++)
        {
            maxHand = CompareHands(_hands[maxHands[0]], _hands[i]);
            if(maxHand == 0)
            {
                maxHands.Add(i);
            }
            else if(maxHand == 2)
            {
                maxHands.Clear();
                maxHands.Add(i);
            }
        }

        return maxHands;
    }

    int CompareHands(List<Tuple<int, int>> hand1, List<Tuple<int, int>> hand2)
    {
        if (hand1.Count == 1) return CompareOne(hand1, hand2);
        return CompareMany(hand1, hand2);
    }

    int CompareOne(List<Tuple<int, int>> hand1, List<Tuple<int, int>> hand2)
    {
        if (hand1[0].Item2 == hand2[0].Item2) return 0;
        if (hand1[0].Item2 > hand2[0].Item2) return 1;
        return 2;
    }

    int CompareMany(List<Tuple<int, int>> hand1, List<Tuple<int, int>> hand2)
    {
        return 0;
    }

    async Task SmallBlind()
    {
        int id = Blind(1, MIN_BET / 2);
        Actions[id] = "Small Blind";
        await Task.Delay(3000);
        StateHasChanged();
        Actions[id] = string.Empty;
        await Task.Delay(3000);
        StateHasChanged();
    }

    async Task BigBlind()
    {
        int id = Blind(2, MIN_BET);
        Actions[id] = "Big Blind";
        await Task.Delay(3000);
        StateHasChanged();
        Actions[id] = string.Empty;
        await Task.Delay(3000);
        StateHasChanged();
    }

    int Blind(int n, int bet)
    {
        int id = GetNextId(n);
        Bankroll[id] -= bet;
        Bank += bet;
        return id;
    }

    int GetNextId(int n)
    {
        int id = DialerId + n;
        if (id >= PLAYERS_COUNT)
        {
            id = id - PLAYERS_COUNT;
        }
        return id;
    }

    async Task PocketDistribution()
    {
        int id = GetNextId(1);
        for (int j = 0; j < 2; j++)
        {
            int nextId = id;
            for (int i = 0; i < PLAYERS_COUNT; i++)
            {
                Tuple<int, int> index = GetIndex();
                string image = nextId != PLAYERS_COUNT / 2 ? CardBack : _images[index.Item1, index.Item2];
                CroupierImage = image;
                StateHasChanged();
                await Task.Delay(200);
                CroupierImage = string.Empty;
                StateHasChanged();
                Images[nextId, j] = image;
                _hands[nextId].Add(index);
                StateHasChanged();
                nextId++;
                if(nextId == PLAYERS_COUNT)
                {
                    nextId = 0;
                }
            }
        }
        
        await Task.Delay(3000);
        StateHasChanged();
    }

    async Task CommonDistribution()
    {

        await Task.Delay(3000);
        StateHasChanged();
    }
}
