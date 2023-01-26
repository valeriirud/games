using Microsoft.AspNetCore.Components;

namespace Games.Pages;

public class PokerBase : ComponentBase
{
    public const int MY_ID = 4;
    public const int HANDS_COUNT = 2;
    public const int COMMON_COUNT = 5;
    public const int PLAYERS_COUNT = 9;
    public const int MAX_BANKROLL = 1000;
    public const int MIN_BET = 10;
    const int TIMEOUT = 1000;
    const int SHORT_TIMEOUT = 500;
    const int TINY_TIMEOUT = 100;
    public string Title { get; set; } = "TEXAS HOLD'EM";
    public string[,] Images { get; }  = new string[PLAYERS_COUNT, HANDS_COUNT];
    public string[] Board { get; } = new string[COMMON_COUNT];
    public string CroupierImage { get; set; } = string.Empty;
    public string ImageDialer { get; } = @"images\dialer.svg";
    public int DealerId { get; set; } = -1;
    public int MyBet { get; set; } = 0;
    int _betId = -1;
    public bool Started { get; set; } = false;
    public bool WaitYou { get; set; } = false;
    public int Bank { get; set; } = 0;
    public string[] Actions { get; } = new string[PLAYERS_COUNT];
    public int[] Bankroll { get; } = new int[PLAYERS_COUNT];
    public int[] Bet { get; } = new int[PLAYERS_COUNT];
    public List<int>[] Bets { get; } = new List<int>[PLAYERS_COUNT];
    readonly List<Tuple<int,int>> _indexes = new();
    readonly List<Tuple<int, int>>[] _hands = new List<Tuple<int, int>>[PLAYERS_COUNT];
    readonly List<Tuple<int, int>> _board = new();
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

    public async Task NewGame()
    {
        if (Started) return;
        Inint();
        Clear();
        Started = true;
        StateHasChanged();
        await DealerSelection();        
        for (int i = 0; i < 2; i++)
        {
            await SmallBlind();
            await BigBlind();
            await Task.Delay(TIMEOUT);
            await PreFlop(0);
            await Task.Delay(TIMEOUT);
            await Flop();
            await Task.Delay(TIMEOUT);
            await Turn();
            await Task.Delay(TIMEOUT);
            await River();
            await Task.Delay(TIMEOUT);
            await Shutdown();
            await Task.Delay(TIMEOUT);
            DealerId = GetNextId(1);
            StateHasChanged();
        }
        Started = false;
        StateHasChanged();
    }

    void Inint()
    {
        for (int i = 0; i < PLAYERS_COUNT; i++)
        {
            _hands[i] = new ();
            Bankroll[i] = MAX_BANKROLL;
            Bet[i] = 0;
            Bets[i] = new();
        }
        DealerId = -1;        
    }

    void Clear()
    {
        Title = string.Empty;
        _indexes.Clear();
        CroupierImage = string.Empty;
        ClearBoard();
    }

    void ClearBoard()
    {
        for (int i = 0; i < PLAYERS_COUNT; i++)
        {
            _hands[i].Clear();
            for (int j = 0; j < HANDS_COUNT; j++)
            {
                Images[i, j] = string.Empty;
            }
            Bet[i] = 0;
        }
        _board.Clear();
        for (int j = 0; j < COMMON_COUNT; j++)
        {
            Board[j] = string.Empty;
        }
        Bank = 0;
        _betId = -1;
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
        await Task.Delay(TIMEOUT*2);
        StateHasChanged();
        Clear();
        DealerId = id;
        await Task.Delay(TIMEOUT);
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
        await ShowAction(id, "SB");
    }

    async Task BigBlind()
    {
        int id = Blind(2, MIN_BET);
        await ShowAction(id, "BB");
    }

    async Task PocketDistribution()
    {
        int id = GetNextId(1);
        for (int j = 0; j < HANDS_COUNT; j++)
        {
            int nextId = id;
            for (int i = 0; i < PLAYERS_COUNT; i++)
            {
                Tuple<int, int> index = GetIndex();
                string image = CardBack;
                CroupierImage = image;
                StateHasChanged();
                await Task.Delay(200);
                CroupierImage = string.Empty;
                StateHasChanged();
                Images[nextId, j] = image = nextId != PLAYERS_COUNT / 2 ? CardBack : _images[index.Item1, index.Item2];
                _hands[nextId].Add(index);
                StateHasChanged();
                nextId++;
                if(nextId == PLAYERS_COUNT)
                {
                    nextId = 0;
                }
            }
        }
        
        await Task.Delay(TIMEOUT);
        StateHasChanged();
    }

    async Task CommonDistribution(int count)
    {
        for(int i = 0; i < count; i++)
        {
            Tuple<int, int> index = GetIndex();
            _board.Add(index);
            for(int j = COMMON_COUNT - 1; j >= 0; j --)
            {
                if (string.IsNullOrEmpty(Board[j]))
                {
                    Board[j] = _images[index.Item1, index.Item2];
                    await Task.Delay(SHORT_TIMEOUT);
                    StateHasChanged();
                    break;
                }
            }            
        }
        await Task.Delay(TIMEOUT);
        StateHasChanged();
    }

    async Task PreFlop(int i)
    {
        await PocketDistribution();
        await Bargaining(i == 0 ? 3 : 1);
    }

    async Task Flop()
    {
        await CommonDistribution(3);
        await Bargaining(1);
    }

    async Task Turn()
    {
        await CommonDistribution(1);
        await Bargaining(1);
    }

    async Task River()
    {
        await CommonDistribution(1);
        await Bargaining(1);
    }

    async Task Shutdown()
    {
        int id = _betId;
        for (int i = 0; i < PLAYERS_COUNT; i++)
        {
            if (id >= PLAYERS_COUNT)
            {
                id -= PLAYERS_COUNT;
            }            
            List<Tuple<int, int>> hand = _hands[id];
            int j = 0;
            foreach (Tuple<int, int> index in hand)
            {
                Images[id, j] = _images[index.Item1, index.Item2];
                await Task.Delay(500);
                StateHasChanged();
                j++;
            }
            id++;
        }

        List<int> winners = GetWinners();
        foreach(int win in winners)
        {
            Bankroll[win] += Bank / winners.Count();
        }

        await Task.Delay(SHORT_TIMEOUT);
        StateHasChanged();

        ClearBoard();

        await Task.Delay(SHORT_TIMEOUT);
        StateHasChanged();
    }

    bool IsCircleClosed() => Bet.All(b => b == Bet[0]);

    List<int> GetWinners()
    {
        List<int> winners = new();
        for(int i = 0; i < PLAYERS_COUNT; i ++)
        {
            if(CheckRoyalFlash(i))
            {
                winners.Add(i);
            }
        }
        if(winners.Any()) return winners;

        return winners;
    }

    bool CheckRoyalFlash(int id)
    {
        List<Tuple<int, int>> hand = GetFullHand(id);
        return id == 3;
    }

    List<Tuple<int, int>> GetFullHand(int id)
    { 
        List<Tuple<int, int>> hand = _hands[id];
        List<Tuple<int, int>> fullHand = new();
        fullHand.AddRange(_board);
        fullHand.AddRange(hand);
        return fullHand;
    }

    void DoBet(int id, int value)
    {
        Bet[id] += value;
        Bankroll[id] -= value;
        Bank += value;
        _betId = id;
        Bets[id].Add(value);
    }

    async Task Bargaining(int n)
    {
        int id = DealerId + n;
        for (int i = 0; i < PLAYERS_COUNT; i++)
        {
            if (id >= PLAYERS_COUNT)
            {
                id -= PLAYERS_COUNT;
            }

            if (id == MY_ID)
            {
                WaitYou = true;
                while(WaitYou)
                {
                    await Task.Delay(TINY_TIMEOUT);
                    StateHasChanged();                    
                }
            }
            else
            {
                int bet = CheckHand(id, i == 0);
                DoBet(id, bet);                
            }
            string action = "Call";
            //int lastId = _betId - 1;
            //if(lastId < 0)
            //{
            //    lastId = PLAYERS_COUNT - 1; 
            //}
            //if (Bets[id][Bets[id].Count() - 1] > Bets[lastId][Bets[lastId].Count() - 1])
            if( Bet.Where((_, i) => i != id).ToArray().All(b => b < Bet[id]) )
            {
                action = "Raise";
            }

            await ShowAction(id, action);
            if (IsCircleClosed()) break;
            id++;
        }
    }

    async Task ShowAction(int id, string message)
    {
        Actions[id] = message;
        await Task.Delay(TIMEOUT);
        StateHasChanged();
        Actions[id] = string.Empty;
        await Task.Delay(TIMEOUT);
        StateHasChanged();
    }

    int Blind(int n, int bet)
    {
        int id = GetNextId(n);
        DoBet(id, bet);
        return id;
    }

    int GetNextId(int n)
    {
        int id = DealerId + n;
        if (id >= PLAYERS_COUNT)
        {
            id = id - PLAYERS_COUNT;
        }
        return id;
    }

    int CheckHand(int id, bool first = false)
    {
        int bet = MIN_BET;
        if (first) return bet;
        int[] otherBets = Bet.Where((_, i) => i != id).ToArray();
        if(otherBets.All(b => b == otherBets[0]))
        {
            return otherBets[0] - Bet[id];
        }
        return bet;
    }

    public async Task ChangeMyBet()
    {
        int val = MyBet;
        await Task.Delay(TIMEOUT);
        StateHasChanged();
    }

    public void SetMyBet(double d, int id = MY_ID)
    {
        if(d == -1)
        {
            MyBet = Bankroll[id];
            return;
        }
        int bet = Convert.ToInt32(MIN_BET * d);
        if (bet > Bankroll[id]) return;
        MyBet = bet;
    }

    public void ApplyMyBet(int bet, int id)
    {
        DoBet(id, bet); 
        if (id == MY_ID)
        {
            WaitYou = false;
        }
    }
}
