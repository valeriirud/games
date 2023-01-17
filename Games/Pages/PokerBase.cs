using Microsoft.AspNetCore.Components;

namespace Games.Pages;

public class PokerBase : ComponentBase
{
    public string[,] Images { get; }  = new string[9, 5];
    public string CroupierImage { get; set; } = string.Empty;

    readonly List<int> _indexes = new();
    readonly string[] _images = 
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
            @"images\clubs_ace.svg",
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
            @"images\diamonds_ace.svg",
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
            @"images\hearts_ace.svg",
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
        };
    public async Task NewGame()
    {
        Clear();
        await DealerSelection();
        //int index = GetIndex();
        //CroupierImage = _images[index];
        //for (int i = 0; i < 9; i++)
        //{
        //    for (int j = 0; j < 5; j++)
        //    {
        //        index = GetIndex();
        //        Images[i, j] = _images[index];
        //    }
        //}
    }

    void Clear()
    {
        _indexes.Clear();
        CroupierImage = string.Empty;
        for(int i = 0; i < 9; i ++)
        {
            for (int j = 0; j < 5; j++)
            {
                Images[i,j] = string.Empty;
            }
        }
    }

    int GetIndex()
    {
        int index = -1;
        while (index < 0)
        {
            Random r = new Random();
            index = r.Next(0, _images.Length - 1);
            if(_indexes.Contains(index))
            {
                index = -1;
            }
            else
            {
                _indexes.Add(index);
            }
        }
        return index;
    }

    async Task DealerSelection()
    {
        for (int i = 0; i < 9; i++)
        {
            int index = GetIndex();
            CroupierImage = _images[index];
            StateHasChanged();
            await Task.Delay(200);
            CroupierImage = string.Empty;
            StateHasChanged();
            Images[i, 0] = _images[index];
            StateHasChanged();
        }
    }
}
