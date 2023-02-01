using Microsoft.AspNetCore.Components;

namespace Games.Pages;

public class PokerBase : ComponentBase
{
    public const int MY_ID = 4;
    public const int HANDS_COUNT = 2;
    public const int COMMON_COUNT = 5;
    public const int PLAYERS_COUNT = 9;
    public const int MAX_BANKROLL = 1000;
    public const int MIN_BET = 2;
    const int D_BB = 2;
    const int D_SB = 1;
    const int TIMEOUT = 1000;
    const int SHORT_TIMEOUT = 500;
    const int TINY_TIMEOUT = 100;

    GameState GameState { get; set; }   
    public string Title { get; set; } = "TEXAS HOLD'EM";
    public string[,] Images { get; }  = new string[PLAYERS_COUNT, HANDS_COUNT];
    public string[] Board { get; } = new string[COMMON_COUNT];
    public string CroupierImage { get; set; } = string.Empty;
    public string ImageDialer { get; } = @"images\dialer.svg";
    public int DealerId { get; set; } = -1;
    public int WinnerId { get; set; } = -1;    
    public int MyBet { get; set; } = 0;
    int _betId = -1;
    public bool Started { get; set; } = false;
    public bool ContinueGame { get; set; } = true;
    public bool WaitYou { get; set; } = false;
    public int Bank { get; set; } = 0;
    public string[] Actions { get; } = new string[PLAYERS_COUNT];
    public int[] Bankroll { get; } = new int[PLAYERS_COUNT];
    public int[] Bet { get; } = new int[PLAYERS_COUNT];
    public List<string> WinnerCards { get; } = new();   
    public List<Tuple<int, int>>[] Bets { get; } = new List<Tuple<int, int>>[PLAYERS_COUNT];
    readonly List<Tuple<int,int>> _indexes = new();
    readonly List<Tuple<int, int>>[] _hands = new List<Tuple<int, int>>[PLAYERS_COUNT];
    readonly List<Tuple<int, int>> _board = new();
    readonly List<int> _fold = new();
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
        Started = true;
        StateHasChanged();
        await Task.Delay(TIMEOUT);
        await StartNewGame();

        //await Test();
        //Started = false;
        //StateHasChanged();
    }

    async Task StartNewGame()
    {
        Started = true;
        Inint();
        Clear();
        await ClearActions();
        StateHasChanged();
        await DealerSelection();        
        while (true)
        {
            ClearHands();
            ClearWinners();
            await ClearActions();
            ClearFold();
            GameState = GameState.PreFlop;
            await SmallBlind();
            await BigBlind();
            await Task.Delay(TIMEOUT);            
            await PreFlop();
            await Task.Delay(TIMEOUT);
            GameState = GameState.PostFlop;
            if (_fold.Count < PLAYERS_COUNT - 1)
            {                
                await Flop();
                await Task.Delay(TIMEOUT);
            }
            if (_fold.Count < PLAYERS_COUNT - 1)
            {
                await Turn();
                await Task.Delay(TIMEOUT);
            }
            if (_fold.Count < PLAYERS_COUNT - 1)
            {
                await River();
                await Task.Delay(TIMEOUT);
            }
            if (_fold.Count < PLAYERS_COUNT - 1)
            {
                await Shutdown();
                await Task.Delay(TIMEOUT);
            }
            await ShowWinners();
            ContinueGame = false;
            Started = false;
            StateHasChanged();
            await Task.Delay(SHORT_TIMEOUT);
            while (true)
            {
                if (ContinueGame || Started) break;
                await Task.Delay(SHORT_TIMEOUT);
            }
            
            ClearBoard();

            if (Started) break;
            Started = true;
            StateHasChanged();
            await Task.Delay(SHORT_TIMEOUT);
            DealerId = GetNextId(1);
        }
        Started = false;
        StateHasChanged();
    }

    public async Task Continue()
    {
        ContinueGame = true;
        await Task.Delay(SHORT_TIMEOUT);
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
        ClearHands();
        CroupierImage = string.Empty;
        ClearBoard();
        ClearFold();
    }

    void ClearWinners()
    {
        WinnerId = -1;
        WinnerCards.Clear();
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
            Bets[i].Clear();
        }
        _board.Clear();
        for (int j = 0; j < COMMON_COUNT; j++)
        {
            Board[j] = string.Empty;
        }
        Bank = 0;
        _betId = -1;
    }

    void ClearBets()
    {
        for (int i = 0; i < PLAYERS_COUNT; i++)
        {
            Bets[i].Clear();
        }
    }

    Tuple<int, int> GetIndex()
    {
        int index1 = -1;
        int index2 = -1;
        while (index1 < 0 || index2 < 0)
        {
            Random r = new();
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
        while (true)
        {
            for (int i = 0; i < PLAYERS_COUNT; i++)
            {
                Tuple<int, int> index = GetIndex();
                CroupierImage = _images[index.Item1, index.Item2];
                StateHasChanged();
                await Task.Delay(TINY_TIMEOUT);
                CroupierImage = string.Empty;
                StateHasChanged();
                Images[i, 0] = _images[index.Item1, index.Item2];
                _hands[i].Add(index);
                StateHasChanged();
            }
            List<int> maxHands = GetMaxHands();
            await Task.Delay(TIMEOUT);
            if (maxHands.Count > 1)
            {
                Clear();
                continue;
            }
            DealerId = maxHands[0];
            break;
        }
        await Task.Delay(TIMEOUT*2);
        StateHasChanged();
        Clear();
        await Task.Delay(TIMEOUT);
        StateHasChanged();
    }

    List<int> GetMaxHands()
    {
        List<int> maxHands = new() { 0 };
        for(int i = 1; i < PLAYERS_COUNT; i ++)
        {
            int maxHand = CompareHands(_hands[maxHands[0]], _hands[i]);
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

    void CompareHands(List<Tuple<int, Hands>> winners)
    {
        if (!winners.Any()) return;
        Hands hand = winners.ElementAt(0).Item2;
        switch(hand)
        {
            case Hands.Pair:
                ComparePair(winners);
                break;
            case Hands.TwoPairs:
                CompareTwoPairs(winners);
                break;
            case Hands.ThreeOfKind:
                CompareThreeOfKind(winners);
                break;
            case Hands.Straight:
                CompareStraight(winners);
                break;
            case Hands.Flush:
                CompareFlush(winners);
                break;
            case Hands.FourOfKind:
                CompareFourOfKind(winners);
                break;
            default:
                break;
        }
    }

    List<Tuple<int, int>> GetIdenticalCardIds(List<Tuple<int, Hands>> winners, int n)
    {
        List<Tuple<int, int>> items = new();
        foreach (Tuple<int, Hands> win in winners)
        {
            List<Tuple<int, int>> hand = GetFullHand(win.Item1);
            List<Tuple<int, int>> cards = GetIdenticalCards(hand, n);
            List<int> cardIds = cards.Select(x => x.Item2).Distinct().ToList();
            cardIds.ForEach(c => items.Add(new Tuple<int, int>(win.Item1, c)));
        }
        return items;
    }

    List<Tuple<int, int>> GetCardStraight(int win, int n)
    {
        List<Tuple<int, int>> cards = new();
        List<Tuple<int, int>> hand = GetFullHand(win).OrderByDescending(h => h.Item2).ToList();
        if (hand.Count < n) return cards;
        for(int i = 0; i < hand.Count - 1; i++)
        {
            if (hand.ElementAt(i).Item2 - hand.ElementAt(i + 1).Item2 == 0) continue;
            if (hand.ElementAt(i).Item2 - hand.ElementAt(i + 1).Item2 > 1)
            {
                cards.Clear();
                continue;
            }                
            cards.Add(hand.ElementAt(i));
            if (cards.Count != n - 1) continue;
            cards.Add(hand.ElementAt(i+1));
            break;
        }
        return cards;
    }

    void CompareFlush(List<Tuple<int, Hands>> winners, Hands hand = Hands.Flush)
    {
        List<Tuple<int, int>> cards = new();
        foreach(Tuple<int, Hands> win in winners)
        {
            List<Tuple<int, int>> fullHand = GetFullHand(win.Item1);
            int suit = GetSuitByCardNumber(fullHand, 5);
            List<int> items = fullHand.Where(h => h.Item1 == suit).Select(h => h.Item2).ToList();
            items.ForEach(i => cards.Add(new Tuple<int, int>(win.Item1, i)));
        }
        cards = cards.OrderByDescending(h => h.Item2).ToList();
        winners.Clear();
        foreach (Tuple<int, int> item in cards)
        {
            if (item.Item2 != cards.ElementAt(0).Item2) break;
            winners.Add(new Tuple<int, Hands>(item.Item1, hand));
        }
        if (!winners.Any()) return;
        FillWinnerCardsBySuit(winners, 5);
    }



    void CompareStraight(List<Tuple<int, Hands>> winners, Hands hand = Hands.Straight)
    {
        int max = 0;
        List<Tuple<int, Hands>> ids = new();
        foreach (Tuple<int, Hands> win in winners)
        {
            List<Tuple<int, int>> cards = GetCardStraight(win.Item1, 5);
            if (!cards.Any()) continue;
            if (cards.ElementAt(0).Item2 < max) continue;
            if (cards.ElementAt(0).Item2 == max)
            {
                ids.Add(Tuple.Create(win.Item1, win.Item2));
                continue;
            }
            max = cards.ElementAt(0).Item2;
            ids.Clear();
            ids.Add(Tuple.Create(win.Item1, win.Item2));
        }
        if (!ids.Any()) return;
        FillWinnerCardsForStraight(ids, 5);
    }

    void CompareTwoPairs(List<Tuple<int, Hands>> winners, Hands hand = Hands.TwoPairs)
    {
        List<Tuple<int, int>> items = GetIdenticalCardIds(winners, 2);
        List<Tuple<int, int>> sorted = items.OrderByDescending(i => i.Item2).ToList();
        if (!sorted.Any()) return;
        winners.Clear();
        int count = sorted.Count;
        int card = sorted.ElementAt(0).Item2;
        foreach (Tuple<int, int> item in sorted)
        {
            if (item.Item2 != sorted.ElementAt(0).Item2) break;
            winners.Add(new Tuple<int, Hands>(item.Item1, hand));
        }
        FillWinnerCardsById(winners, 2);
    }

    void CompareThreeOfKind(List<Tuple<int, Hands>> winners) => ComparePair(winners, 3, Hands.ThreeOfKind);
    void CompareFourOfKind(List<Tuple<int, Hands>> winners) => ComparePair(winners, 4, Hands.FourOfKind);

    void ComparePair(List<Tuple<int, Hands>> winners, int n = 2, Hands hand = Hands.Pair)
    {
        List<Tuple<int, int>> items = GetIdenticalCardIds(winners, n);
        List<Tuple<int, int>> sorted = items.OrderByDescending(i => i.Item2).ToList();
        if (!sorted.Any()) return;
        winners.Clear();
        foreach (Tuple<int, int> item in sorted)
        {
            if (item.Item2 != sorted.ElementAt(0).Item2) break;
            winners.Add(new Tuple<int, Hands>(item.Item1, hand));
        }
        FillWinnerCardsById(winners, n);
    }

    void FillWinnerCardsForStraight(List<Tuple<int, Hands>> winners, int count)
    {
        WinnerCards.Clear();
        foreach (Tuple<int, Hands> win in winners)
        {
            List<Tuple<int, int>> cards = GetCardStraight(win.Item1, count);
            cards.ForEach(c => WinnerCards.Add(_images[c.Item1, c.Item2]));
        }
    }

    void FillWinnerCardsBySuit(List<Tuple<int, Hands>> winners, int count)
    {
        WinnerCards.Clear();
        foreach (Tuple<int, Hands> win in winners)
        {
            List<Tuple<int, int>> hand = GetFullHand(win.Item1);
            int suit = GetSuitByCardNumber(hand, count);
            List<Tuple<int, int>> cards = hand.Where(h => h.Item1 == suit).OrderByDescending(h => h.Item2).
                Take(count).ToList();
            cards.ForEach(c => WinnerCards.Add(_images[c.Item1, c.Item2]));
        }
    }

    void FillWinnerCardsById(List<Tuple<int, Hands>> winners, int n)
    {
        WinnerCards.Clear();
        foreach (Tuple<int, Hands> win in winners)
        {
            List<Tuple<int, int>> hand = GetFullHand(win.Item1);
            List<Tuple<int, int>> cards = GetIdenticalCards(hand, n).OrderByDescending(c => c.Item2).ToList();
            int count = 1;
            foreach(Tuple<int, int> card in cards)
            {
                WinnerCards.Add(_images[card.Item1, card.Item2]);
                if (win.Item2 == Hands.TwoPairs && count == 4) break;
                count++;
            }
        }
    }

    static int CompareHands(List<Tuple<int, int>> hand1, List<Tuple<int, int>> hand2)
    {
        if (hand1.Count == 1) return CompareOne(hand1, hand2);
        return CompareMany(hand1, hand2);
    }

    static int CompareOne(List<Tuple<int, int>> hand1, List<Tuple<int, int>> hand2)
    {
        if (hand1[0].Item2 == hand2[0].Item2) return 0;
        if (hand1[0].Item2 > hand2[0].Item2) return 1;
        return 2;
    }

    static int CompareMany(List<Tuple<int, int>> hand1, List<Tuple<int, int>> hand2)
    {
        return 0;
    }

    async Task SmallBlind()
    {
        int id = Blind(D_SB, MIN_BET / 2, true);
        await ShowAction(id, "SB");
    }

    async Task BigBlind()
    {
        int id = Blind(D_BB, MIN_BET, true);
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
                CroupierImage = CardBack;
                StateHasChanged();
                await Task.Delay(TINY_TIMEOUT);
                CroupierImage = string.Empty;
                StateHasChanged();
                await Task.Delay(TINY_TIMEOUT);
                Images[nextId, j] = nextId != PLAYERS_COUNT / 2 ? CardBack : _images[index.Item1, index.Item2];
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

    async Task PreFlop()
    {
        await PocketDistribution();
        await Bargaining(3);
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
            if (!IsFold(id))
            {
                List<Tuple<int, int>> hand = _hands[id];
                int j = 0;
                foreach (Tuple<int, int> index in hand)
                {
                    Images[id, j] = _images[index.Item1, index.Item2];
                    await Task.Delay(SHORT_TIMEOUT);
                    StateHasChanged();
                    j++;
                }
            }
            id++;
        }
        await Task.Delay(SHORT_TIMEOUT);
        StateHasChanged();
    }

    async Task<List<Tuple<int, Hands>>> ShowWinners()
    {
        List<Tuple<int, Hands>> winners = GetWinners();
        winners.ForEach(w => Bankroll[w.Item1] += Bank / winners.Count);
        List<int> ids = winners.Select(w => w.Item1).ToList();
        await ShowAction(ids, "Winner", TIMEOUT);
        return winners;
    }

    bool IsCircleClosed()
    {
        List<int> foldList = GetFoldList();
        if (foldList.Count == PLAYERS_COUNT - 1) return true;
        List<int> bet = Bet.ToList();
        List<List<Tuple<int, int>>> bets = Bets.ToList();
        foldList.OrderByDescending(v => v).ToList().ForEach(RemoveFold);
        if (bets.Any(b => b.Count == 0)) return false;
        return bet.All(b => b == bet.ElementAt(0));

        void RemoveFold(int id)
        {
            bet.RemoveAt(id);
            bets.RemoveAt(id);
        }
    }

    List<int> GetFoldList() => _fold;

    List<Tuple<int, Hands>> GetWinners()
    {
        List<Tuple<int, Hands>> winners = new();
        List<int> foldList = GetFoldList();
        if (foldList.Count == PLAYERS_COUNT - 1)
        {
            winners.Add( new Tuple<int, Hands>(GetNextActiveId(), Hands.None) );
            return winners;
        }
        winners = GetWinners(CheckRoyalFlush, Hands.RoyalFlush);
        if(winners.Any()) return winners;
        winners = GetWinners(CheckStraightFlush, Hands.StraightFlush);
        if (winners.Any()) return winners;
        winners = GetWinners(CheckFourOfKind, Hands.FourOfKind);
        if (winners.Any()) return winners;
        winners = GetWinners(CheckFullHouse, Hands.FullHouse);
        if (winners.Any()) return winners;
        winners = GetWinners(CheckFlush, Hands.Flush);
        if (winners.Any()) return winners;
        winners = GetWinners(CheckStraight, Hands.Straight);
        if (winners.Any()) return winners;
        winners = GetWinners(CheckThreeOfKind, Hands.ThreeOfKind);
        if (winners.Any()) return winners;
        winners = GetWinners(CheckTwoPair, Hands.TwoPairs);
        if (winners.Any()) return winners;
        winners = GetWinners(CheckPair, Hands.Pair);
        if (winners.Any()) return winners;
        winners = GetWinners(CheckHighCard, Hands.HightCard);
        if (winners.Any()) return winners;    
        return new List<Tuple<int, Hands>>();
    }

    List<Tuple<int, Hands>> GetWinners(Func<int, bool> func, Hands hand)
    {
        List<Tuple<int, Hands>> winners = new();
        for(int i = 0; i < PLAYERS_COUNT; i ++)
        {
            if (!func(i)) continue;
            winners.Add(new Tuple<int, Hands>(i, hand));
        }
        //if(winners.Count > 1)
        {
            CompareHands(winners);
        }
        return winners;
    }

    static int GetSuitByCardNumber(List<Tuple<int, int>> hand, int n)
    {
        List<int> suits = hand.Select(h => h.Item1).OrderBy(Item1 => Item1).ToList();
        int suit = -1;
        foreach (int item in suits.Distinct())
        {
            int count = suits.Count(x => x == item);
            if (count >= n)
            {
                suit = item;
                break;
            }
        }
        return suit;
    }

    static List<Tuple<int, int>> GetIdenticalCards(List<Tuple<int, int>> hand, int n)
    {
        List<int> cards = hand.Select(h => h.Item2).OrderBy(Item2 => Item2).ToList();
        List<Tuple<int, int>> card = new();
        foreach (int item in cards.Distinct())
        {
            if(hand.Count(x => x.Item2 == item) < n) continue;
            card.AddRange(hand.Where(x => x.Item2 == item).ToList());
        }
        return card;
    }

    bool CheckRoyalFlush(int id)
    {
        List<Tuple<int, int>> hand = GetFullHand(id);
        int suit = GetSuitByCardNumber(hand, 5);
        if (suit == -1) return false;
        hand.RemoveAll(h => h.Item1 != suit);
        if (hand.Any(h => h.Item2 == 12) && 
            hand.Any(h => h.Item2 == 11) &&
            hand.Any(h => h.Item2 == 10) &&
            hand.Any(h => h.Item2 == 9) &&
            hand.Any(h => h.Item2 == 8)) return true;
        return false;
    }

    bool CheckStraightFlush(int id)
    {
        List<Tuple<int, int>> hand = GetFullHand(id);
        int suit = GetSuitByCardNumber(hand, 5);
        if (suit == -1) return false;
        hand.RemoveAll(h => h.Item1 != suit);
        int[] cards = hand.Select(h => h.Item2).OrderBy(Item2 => Item2).ToArray();
        for(int i = 0; i < cards.Length - 1; i ++)
        {
            if (cards[i + 1] - cards[i] != 1) return false;
        }
        return true;
    }

    bool CheckFourOfKind(int id)
    {
        List<Tuple<int, int>> hand = GetFullHand(id);
        List<Tuple<int, int>> cards = GetIdenticalCards(hand, 4);
        if (cards.Any()) return true;
        return false;
    }

    bool CheckFullHouse(int id)
    {
        List<Tuple<int, int>> hand = GetFullHand(id);
        List<Tuple<int, int>> cards3 = GetIdenticalCards(hand, 3);
        List<Tuple<int, int>> cards2 = GetIdenticalCards(hand, 2);
        if (!cards3.Any() || !cards2.Any()) return false;
        if(cards3.ElementAt(0).Item2 != cards2.ElementAt(0).Item2) return true;
        return false;
    }

    bool CheckFlush(int id)
    {
        List<Tuple<int, int>> hand = GetFullHand(id);
        int suit = GetSuitByCardNumber(hand, 5);
        if (suit == -1) return false;
        return true;
    }

    bool CheckStraight(int id)
    {
        List<Tuple<int, int>> list = GetCardStraight(id, 5);
        if (list.Count < 5) return false;
        return true;
    }

    bool CheckThreeOfKind(int id) => CheckProc(id, 3, 1);

    bool CheckTwoPair(int id) => CheckProc(id, 2, 2);

    bool CheckPair(int id) => CheckProc(id, 2, 1);

    bool CheckProc(int id, int n, int m)
    {
        List<Tuple<int, int>> hand = GetFullHand(id);
        if(hand.Count != HANDS_COUNT + COMMON_COUNT) return false;
        List<Tuple<int, int>> cards = GetIdenticalCards(hand, n);
        if (cards.Count >= m*n)
        {
            cards.ForEach(c => WinnerCards.Add(_images[c.Item1, c.Item2]));
            return true;
        }
        return false;
    }

    bool CheckHighCard(int id) => true;

    List<Tuple<int, int>> GetFullHand(int id)
    { 
        List<Tuple<int, int>> hand = _hands[id];
        List<Tuple<int, int>> fullHand = new();
        fullHand.AddRange(_board);
        fullHand.AddRange(hand);
        return fullHand;
    }

    async Task Bargaining(int n)
    {
        int id = GetNextId(DealerId, n);
        while (true)
        {
            for (int i = 0; i < PLAYERS_COUNT; i++)
            {
                List<int> foldList = GetFoldList();
                if (foldList.Count == PLAYERS_COUNT - 1) break;
                while (IsFold(id))
                {
                    id = GetNextId(id, 1);
                }
                if (id == MY_ID)
                {
                    await WaitForYou(id);
                }
                else
                {
                    PlayerBet(id);
                }
                string action = GetAction(id);
                await ShowAction(id, action);
                id = GetNextId(id, 1);
                if (IsCircleClosed()) break;
            }
            if (IsCircleClosed())
            {
                ClearBets();
                break;
            }
        }
    }

    void PlayerBet(int id)
    {
        int bet = GetPlayerBet(id);
        switch (bet)
        {
            case 0:
                Check(id);
                break;
            case -1:
                Fold(id);
                break;
            default:
                DoBet(id, bet);
                break;
        }
    }

    int GetPlayerBet(int id)
    {
        Random r = new();
        int n = r.Next(0, 5);
        if ( IsCheckAvailable(id)) return 0;
        if (n == 1) return -1;
        return CheckHand(id);
    }

    int CheckHand(int id)
    {
        int bet = GetMinBet(id);
        int[] otherBets = Bet.Where((_, i) => i != id).ToArray();
        if (otherBets.All(b => b == otherBets[0]))
        {
            bet = otherBets[0] - Bet[id];
        }
        if (bet == 0)
        {
            bet = MIN_BET;
        }
        return bet;
    }

    string GetAction(int id)
    {
        string action = "Call";
        if (Bet.Where((_, i) => i != id).ToArray().All(b => b < Bet[id])) return "Raise";
        if (IsCheck(id)) return "Check";
        if (IsFold(id)) return "Fold";
        return action;
    }

    bool CheckAction(int id, int value)
    {
        if (!Bets[id].Any()) return false;
        if(!Bets[id].Where(b => b.Item2 == _board.Count).ToList().Any()) return false;
        return Bets[id].Where(b => b.Item2 == _board.Count).ToList()[^1].Item1 == value;
    }

    bool IsCheck(int id) => CheckAction(id, 0);
    bool IsFold(int id) => _fold.Contains(id);

    async Task ShowAction(int id, string message, int timeout = TIMEOUT) => 
        await ShowAction(new List<int>() { id }, message, timeout);

    async Task ShowAction(List<int> ids, string message, int timeout = TIMEOUT)
    {
        ids.ForEach(i => Actions[i] = string.Empty);
        StateHasChanged();
        await Task.Delay(timeout);
        ids.ForEach(i => Actions[i] = message);
        StateHasChanged();
        await Task.Delay(timeout);
    }

    async Task ClearAction(int id, int timeout = TIMEOUT)
    {
        Actions[id] = string.Empty;
        StateHasChanged();
        await Task.Delay(timeout);
    }

    async Task ClearActions(int timeout = TIMEOUT)
    {
        for (int i = 0; i < Actions.Length; i ++)
        {
            Actions[i] = string.Empty;
        }
        StateHasChanged();
        await Task.Delay(timeout);
    }

    async Task WaitForYou(int id)
    {
        WaitYou = true;
        while (WaitYou)
        {
            await ShowAction(id, "My Bet");
            StateHasChanged();
            await Task.Delay(SHORT_TIMEOUT);
            await ClearAction(id);
            StateHasChanged();
            await Task.Delay(SHORT_TIMEOUT);
        }
    }

    int Blind(int n, int bet, bool blend = false)
    {
        int id = GetNextId(n);
        DoBet(id, bet, blend);
        return id;
    }

    int GetNextId(int n = 1) => GetNextId(DealerId, n);

    static int GetNextId(int id, int n)
    {
        int nextId = id + n;
        if (nextId >= PLAYERS_COUNT)
        {
            nextId -= PLAYERS_COUNT;
        }
        return nextId;
    }

    int GetNextActiveId()
    {
        int id = DealerId;
        while(IsFold(id))
        {
            id = GetNextId(id);
        }
        return id;
    }

    int GetBigBlendId() => GetNextId(D_BB);
    static int GetPrevId(int myId)
    {
        int id = myId - 1;
        if(id < 0)
        {
            id += PLAYERS_COUNT;
        }
        return id;
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

    public void Check(int id) => ApplyMyBet(0, id);

    public void ApplyMyBet(int bet, int id)
    {
        DoBet(id, bet); 
        if (id == MY_ID)
        {
            WaitYou = false;
        }
    }

    public void Fold(int id)
    {
        Bets[id].Add(new Tuple<int, int>(-1, _board.Count));
        _hands[id].Clear();
        for(int i = 0; i < HANDS_COUNT; i++)
        {
            Images[id, i] = string.Empty;
        }
        if (id == MY_ID)
        {
            WaitYou = false;
        }
        _fold.Add(id);
    }

    void ClearFold() => _fold.Clear();  

    void ClearHands() => _indexes.Clear();

    public bool MyBetOk() => MyBet >= GetMinBet(MY_ID);

    public bool IsCheckAvailable(int id)
    {
        bool ret = false;
        switch(GameState)
        {
            case GameState.PreFlop:
                if( id == GetBigBlendId() && Bet.All(b => b == Bet[id]) )
                {
                    ret = true;
                }
                break;
            case GameState.PostFlop:
                if ( (DealerId == GetPrevId(id) && Bet.All(b => b == Bet[id])) || IsAllCheck()
                    //Bets.Where(b => b.Count > Bets[id].Count).All(l => l[^1] == 0) 
                    )
                {
                    ret = true;
                }
                break;
        }

        return ret;
    }

    bool IsAllCheck()
    {
        foreach(List<Tuple<int, int>> bet in Bets)
        {
            if (!bet.Where(b => b.Item2 == _board.Count).Any()) continue;
            if ( ! (bet.Where(b => b.Item2 == _board.Count).ToList()[^1].Item1 == 0) ) return false;
        }
        return true;
    }

    int GetMinBet(int id)
    {
        int preId = GetPrevId(id);
        while(IsFold(preId))
        {
            preId = GetPrevId(preId);
            if (preId == id) break;
        }
        return Bet[preId] - Bet[id];
    }

    void DoBet(int id, int value, bool blend = false)
    {
        Bet[id] += value;
        Bankroll[id] -= value;
        Bank += value;
        _betId = id;
        if (!blend)
        {
            Bets[id].Add(new Tuple<int, int>(value, _board.Count));
        }
    }

    async Task Test()
    {
        Inint();
        Title = string.Empty;
        MakeTestData();
        Hands hand = Hands.Straight;
        List<Tuple<int, Hands>> winners = new() { 
            Tuple.Create(0, hand), 
            Tuple.Create(1, hand)//, 
            //Tuple.Create(2, hand)//,
            //Tuple.Create(3, hand)
        };

        await ShowWinners();

        //ComparePair(winners);
        //CompareTwoPairs(winners);
        //CompareThreeOfKind(winners);
        //CompareStraight(winners);
        //foreach(Tuple<int, Hands> win in winners)
        //{
        //    if (!CheckFlush(win.Item1)) continue;
        //    int n = win.Item1;
        //}

        await Task.Delay(SHORT_TIMEOUT);
    }

    void MakeTestDataPair1()
    {
        _hands[0].Add(Tuple.Create(1, 0));
        _hands[0].Add(Tuple.Create(2, 0));
        Images[0, 0] = _images[1, 0];
        Images[0, 1] = _images[2, 0];

        _hands[1].Add(Tuple.Create(1, 1));
        _hands[1].Add(Tuple.Create(2, 1));
        Images[1, 0] = _images[1, 1];
        Images[1, 1] = _images[2, 1];

        _hands[2].Add(Tuple.Create(1, 2));
        _hands[2].Add(Tuple.Create(2, 2));
        Images[2, 0] = _images[1, 2];
        Images[2, 1] = _images[2, 2];

        _board.Add(Tuple.Create(0, 3));
        _board.Add(Tuple.Create(1, 5));
        _board.Add(Tuple.Create(2, 7));
        _board.Add(Tuple.Create(3, 9));
        _board.Add(Tuple.Create(0, 11));

        Board[4] = _images[0, 3];
        Board[3] = _images[1, 5];
        Board[2] = _images[2, 7];
        Board[1] = _images[3, 9];
        Board[0] = _images[0, 11];
    }

    void MakeTestDataPair1_1()
    {
        _hands[0].Add(Tuple.Create(1, 0));
        _hands[0].Add(Tuple.Create(2, 4));
        Images[0, 0] = _images[1, 0];
        Images[0, 1] = _images[2, 4];

        _hands[1].Add(Tuple.Create(1, 1));
        _hands[1].Add(Tuple.Create(2, 0));
        Images[1, 0] = _images[1, 1];
        Images[1, 1] = _images[2, 0];

        _board.Add(Tuple.Create(0, 3));
        _board.Add(Tuple.Create(1, 5));
        _board.Add(Tuple.Create(2, 7));
        _board.Add(Tuple.Create(3, 0));
        _board.Add(Tuple.Create(0, 11));

        Board[4] = _images[0, 3];
        Board[3] = _images[1, 5];
        Board[2] = _images[2, 7];
        Board[1] = _images[3, 0];
        Board[0] = _images[0, 11];
    }

    void MakeTestDataPair2()
    {
        _hands[0].Add(Tuple.Create(1, 0));
        _hands[0].Add(Tuple.Create(0, 3));
        Images[0, 0] = _images[1, 0];
        Images[0, 1] = _images[0, 3];

        _hands[1].Add(Tuple.Create(1, 1));
        _hands[1].Add(Tuple.Create(1, 5));
        Images[1, 0] = _images[1, 1];
        Images[1, 1] = _images[1, 5];

        _hands[2].Add(Tuple.Create(1, 2));
        _hands[2].Add(Tuple.Create(2, 7));
        Images[2, 0] = _images[1, 2];
        Images[2, 1] = _images[2, 7];

        _board.Add(Tuple.Create(2, 0));
        _board.Add(Tuple.Create(2, 1));
        _board.Add(Tuple.Create(2, 2));
        _board.Add(Tuple.Create(3, 9));
        _board.Add(Tuple.Create(0, 11));

        Board[4] = _images[2, 0];
        Board[3] = _images[2, 1];
        Board[2] = _images[2, 2];
        Board[1] = _images[3, 9];
        Board[0] = _images[0, 11];
    }

    void MakeTestDataPair3()
    {
        _hands[0].Add(Tuple.Create(1, 0));
        _hands[0].Add(Tuple.Create(0, 3));
        Images[0, 0] = _images[1, 0];
        Images[0, 1] = _images[0, 3];

        _hands[1].Add(Tuple.Create(1, 1));
        _hands[1].Add(Tuple.Create(1, 5));
        Images[1, 0] = _images[1, 1];
        Images[1, 1] = _images[1, 5];

        _hands[2].Add(Tuple.Create(1, 2));
        _hands[2].Add(Tuple.Create(2, 6));
        Images[2, 0] = _images[1, 2];
        Images[2, 1] = _images[2, 6];

        _hands[3].Add(Tuple.Create(1, 7));
        _hands[3].Add(Tuple.Create(2, 9));
        Images[3, 0] = _images[1, 7];
        Images[3, 1] = _images[2, 9];

        _board.Add(Tuple.Create(2, 0));
        _board.Add(Tuple.Create(2, 1));
        _board.Add(Tuple.Create(2, 2));
        _board.Add(Tuple.Create(3, 7));
        _board.Add(Tuple.Create(0, 11));

        Board[4] = _images[2, 0];
        Board[3] = _images[2, 1];
        Board[2] = _images[2, 2];
        Board[1] = _images[3, 7];
        Board[0] = _images[0, 11];
    }

    void MakeTestDataPair4()
    {
        _hands[0].Add(Tuple.Create(1, 7));
        _hands[0].Add(Tuple.Create(0, 3));
        Images[0, 0] = _images[1, 7];
        Images[0, 1] = _images[0, 3];

        _hands[1].Add(Tuple.Create(1, 1));
        _hands[1].Add(Tuple.Create(1, 5));
        Images[1, 0] = _images[1, 1];
        Images[1, 1] = _images[1, 5];

        _hands[2].Add(Tuple.Create(1, 2));
        _hands[2].Add(Tuple.Create(2, 6));
        Images[2, 0] = _images[1, 2];
        Images[2, 1] = _images[2, 6];

        _hands[3].Add(Tuple.Create(1, 7));
        _hands[3].Add(Tuple.Create(2, 9));
        Images[3, 0] = _images[1, 7];
        Images[3, 1] = _images[2, 9];

        _board.Add(Tuple.Create(2, 0));
        _board.Add(Tuple.Create(2, 1));
        _board.Add(Tuple.Create(2, 2));
        _board.Add(Tuple.Create(3, 7));
        _board.Add(Tuple.Create(0, 11));

        Board[4] = _images[2, 0];
        Board[3] = _images[2, 1];
        Board[2] = _images[2, 2];
        Board[1] = _images[3, 7];
        Board[0] = _images[0, 11];
    }

    void MakeTestDataTwoPair1()
    {
        _hands[0].Add(Tuple.Create(1, 0));
        _hands[0].Add(Tuple.Create(2, 1));
        Images[0, 0] = _images[1, 0];
        Images[0, 1] = _images[2, 1];

        _hands[1].Add(Tuple.Create(1, 2));
        _hands[1].Add(Tuple.Create(2, 3));
        Images[1, 0] = _images[1, 2];
        Images[1, 1] = _images[2, 3];

        _board.Add(Tuple.Create(0, 0));
        _board.Add(Tuple.Create(1, 1));
        _board.Add(Tuple.Create(2, 2));
        _board.Add(Tuple.Create(3, 3));
        _board.Add(Tuple.Create(0, 11));

        Board[4] = _images[0, 0];
        Board[3] = _images[1, 1];
        Board[2] = _images[2, 2];
        Board[1] = _images[3, 3];
        Board[0] = _images[0, 11];
    }

    void MakeTestDataSet1()
    {
        _hands[0].Add(Tuple.Create(0, 0));
        _hands[0].Add(Tuple.Create(0, 0));
        Images[0, 0] = _images[0, 0];
        Images[0, 1] = _images[0, 0];

        _hands[1].Add(Tuple.Create(1, 1));
        _hands[1].Add(Tuple.Create(2, 1));
        Images[1, 0] = _images[1, 1];
        Images[1, 1] = _images[2, 1];

        _board.Add(Tuple.Create(0, 0));
        _board.Add(Tuple.Create(0, 1));
        _board.Add(Tuple.Create(2, 7));
        _board.Add(Tuple.Create(3, 9));
        _board.Add(Tuple.Create(0, 11));

        Board[4] = _images[0, 0];
        Board[3] = _images[0, 1];
        Board[2] = _images[2, 7];
        Board[1] = _images[3, 9];
        Board[0] = _images[0, 11];
    }

    void MakeTestDataTwoPair2()
    {
        _hands[0].Add(Tuple.Create(0, 7));
        _hands[0].Add(Tuple.Create(0, 12));
        Images[0, 0] = _images[0, 7];
        Images[0, 1] = _images[0, 12];

        _hands[1].Add(Tuple.Create(0, 10));
        _hands[1].Add(Tuple.Create(2, 12));
        Images[1, 0] = _images[0, 10];
        Images[1, 1] = _images[2, 12];

        _board.Add(Tuple.Create(3, 1));
        _board.Add(Tuple.Create(3, 12));
        _board.Add(Tuple.Create(0, 8));
        _board.Add(Tuple.Create(3, 8));
        _board.Add(Tuple.Create(0, 3));

        Board[4] = _images[3, 1];
        Board[3] = _images[3, 12];
        Board[2] = _images[0, 8];
        Board[1] = _images[3, 8];
        Board[0] = _images[0, 3];
    }

    void MakeTestDataFlush1()
    {
        _hands[0].Add(Tuple.Create(3, 5));
        _hands[0].Add(Tuple.Create(3, 9));
        Images[0, 0] = _images[3, 5];
        Images[0, 1] = _images[3, 9];

        _hands[1].Add(Tuple.Create(0, 7));
        _hands[1].Add(Tuple.Create(0, 12));
        Images[1, 0] = _images[0, 7];
        Images[1, 1] = _images[0, 12];

        _hands[2].Add(Tuple.Create(0, 10));
        _hands[2].Add(Tuple.Create(2, 12));
        Images[2, 0] = _images[0, 10];
        Images[2, 1] = _images[2, 12];

        _board.Add(Tuple.Create(3, 1));
        _board.Add(Tuple.Create(3, 12));
        _board.Add(Tuple.Create(0, 8));
        _board.Add(Tuple.Create(3, 8));
        _board.Add(Tuple.Create(0, 3));

        Board[4] = _images[3, 1];
        Board[3] = _images[3, 12];
        Board[2] = _images[0, 8];
        Board[1] = _images[3, 8];
        Board[0] = _images[0, 3];
    }

    void MakeTestDataTwoPair3()
    {
        _hands[0].Add(Tuple.Create(1, 2));
        _hands[0].Add(Tuple.Create(3, 9));
        Images[0, 0] = _images[1, 2];
        Images[0, 1] = _images[3, 9];

        _hands[1].Add(Tuple.Create(2, 2));
        _hands[1].Add(Tuple.Create(2, 8));
        Images[1, 0] = _images[2, 2];
        Images[1, 1] = _images[2, 8];

        _hands[2].Add(Tuple.Create(0, 6));
        _hands[2].Add(Tuple.Create(0, 8));
        Images[2, 0] = _images[0, 6];
        Images[2, 1] = _images[0, 8];

        _board.Add(Tuple.Create(3, 0));
        _board.Add(Tuple.Create(0, 2));
        _board.Add(Tuple.Create(1, 0));
        _board.Add(Tuple.Create(2, 6));
        _board.Add(Tuple.Create(1, 9));

        Board[4] = _images[3, 0];
        Board[3] = _images[0, 2];
        Board[2] = _images[1, 0];
        Board[1] = _images[2, 6];
        Board[0] = _images[1, 9];
    }

    void MakeTestDataStraight1()
    {
        _hands[0].Add(Tuple.Create(0, 6));
        _hands[0].Add(Tuple.Create(0, 7));
        Images[0, 0] = _images[0, 6];
        Images[0, 1] = _images[0, 7];

        _hands[1].Add(Tuple.Create(1, 11));
        _hands[1].Add(Tuple.Create(2, 7));
        Images[1, 0] = _images[1, 11];
        Images[1, 1] = _images[2, 7];

        _board.Add(Tuple.Create(0, 0));
        _board.Add(Tuple.Create(1, 8));
        _board.Add(Tuple.Create(2, 9));
        _board.Add(Tuple.Create(3, 10));
        _board.Add(Tuple.Create(0, 1));

        Board[4] = _images[0, 0];
        Board[3] = _images[1, 8];
        Board[2] = _images[2, 9];
        Board[1] = _images[3, 10];
        Board[0] = _images[0, 1];
    }

    void MakeTestDataStraight2()
    {
        _hands[0].Add(Tuple.Create(0, 11));
        _hands[0].Add(Tuple.Create(0, 7));
        Images[0, 0] = _images[0, 11];
        Images[0, 1] = _images[0, 7];

        _hands[1].Add(Tuple.Create(1, 11));
        _hands[1].Add(Tuple.Create(2, 7));
        Images[1, 0] = _images[1, 11];
        Images[1, 1] = _images[2, 7];

        _board.Add(Tuple.Create(0, 0));
        _board.Add(Tuple.Create(1, 8));
        _board.Add(Tuple.Create(2, 9));
        _board.Add(Tuple.Create(3, 10));
        _board.Add(Tuple.Create(0, 1));

        Board[4] = _images[0, 0];
        Board[3] = _images[1, 8];
        Board[2] = _images[2, 9];
        Board[1] = _images[3, 10];
        Board[0] = _images[0, 1];
    }

    void MakeTestDataStraightTwoPairs1()
    {
        _hands[0].Add(Tuple.Create(2, 11));
        _hands[0].Add(Tuple.Create(2, 7));
        Images[0, 0] = _images[2, 11];
        Images[0, 1] = _images[2, 7];

        _hands[1].Add(Tuple.Create(0, 5));
        _hands[1].Add(Tuple.Create(3, 8));
        Images[1, 0] = _images[0, 5];
        Images[1, 1] = _images[3, 8];

        _board.Add(Tuple.Create(0, 8));
        _board.Add(Tuple.Create(0, 10));
        _board.Add(Tuple.Create(3, 6));
        _board.Add(Tuple.Create(1, 5));
        _board.Add(Tuple.Create(2, 3));

        Board[4] = _images[0, 8];
        Board[3] = _images[0, 10];
        Board[2] = _images[3, 6];
        Board[1] = _images[1, 5];
        Board[0] = _images[2, 3];
    }

    void MakeTestDataFlush2()
    {
        _hands[0].Add(Tuple.Create(1, 5));
        _hands[0].Add(Tuple.Create(1, 1));
        Images[0, 0] = _images[1, 5];
        Images[0, 1] = _images[1, 1];

        _hands[1].Add(Tuple.Create(0, 5));
        _hands[1].Add(Tuple.Create(2, 12));
        Images[1, 0] = _images[0, 5];
        Images[1, 1] = _images[3, 8];

        _board.Add(Tuple.Create(1, 2));
        _board.Add(Tuple.Create(3, 8));
        _board.Add(Tuple.Create(1, 10));
        _board.Add(Tuple.Create(3, 2));
        _board.Add(Tuple.Create(1, 8));

        Board[4] = _images[1, 2];
        Board[3] = _images[3, 8];
        Board[2] = _images[1, 10];
        Board[1] = _images[3, 2];
        Board[0] = _images[1, 8];
    }

    void MakeTestData()
    {
        //MakeTestDataPair1();
        //MakeTestDataPair2();
        //MakeTestDataPair3();
        //MakeTestDataPair4();
        //MakeTestDataTwoPair1();
        //MakeTestDataPair1_1();
        //MakeTestDataSet1();
        //MakeTestDataTwoPair2();
        //MakeTestDataFlush1();
        //MakeTestDataTwoPair3();
        //MakeTestDataStraight1();
        //MakeTestDataStraight2();
        //MakeTestDataStraightTwoPairs1();
        MakeTestDataFlush2();
    }
}

public enum GameState
{
    PreFlop,
    PostFlop
}

enum Hands
{
    None,
    HightCard,
    Pair,
    TwoPairs,
    ThreeOfKind,
    Straight,
    Flush,
    FullHouse,
    FourOfKind,
    StraightFlush,
    RoyalFlush
}

/*
 
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

cross - clubs


 */