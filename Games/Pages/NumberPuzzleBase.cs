using Microsoft.AspNetCore.Components;

namespace Games.Pages;
public class NumberPuzzleBase : ComponentBase
{
    public int AreaSize { get; set; } = 4;
    string gameMode = string.Empty;

    public string GameMode
    {
        get { return gameMode; }
        set
        {
            gameMode = value;
            ChangeMode();
        }
    }
    public List<GameField>? GameFields { get; set; }
    public string CurrentTime { get; set; } = string.Empty;
    readonly System.Timers.Timer timer = new(1000);
    int currentCount;
    
    protected override void OnInitialized()
    {
        timer.Elapsed += (sender, eventArgs) => OnTimerCallback();
        NewGame();
    }

    public void OnMouseDown(int id)
    {
        GameField? field = GetGameField(id);
        if (field is default(GameField)) return;
        field.State = true;
    }

    public void OnMouseUp(int id)
    {
        GameField? srcField = GetGameField(true);
        GameField? dstField = GetGameField(id);
        if (srcField is not default(GameField) && dstField is not default(GameField))
        {
            srcField.State = false;
            if (ActionAllowed(srcField, dstField))
            {
                dstField.Value = srcField.Value;
                srcField.Value = 0;
            }
        }
        _ = InvokeAsync(StateHasChanged);
    }

    public void OnClick(int id)
    {
        GameField? dstField = GetEmptyGameField();
        GameField? srcField = GetGameField(id);
        if (srcField is not null && dstField is not null)
        {
            srcField.State = false;
            if (ActionAllowed(srcField, dstField))
            {
                dstField.Value = srcField.Value;
                srcField.Value = 0;
            }
        }
        _ = InvokeAsync(StateHasChanged);
    }

    GameField? GetGameField(int id)
    {
        if (GameFields is null) return default;
        return GameFields.FirstOrDefault(gf => gf.Index == id);
    }
    GameField? GetGameField(bool state)
    {
        if (GameFields is null) return default;
        return GameFields.FirstOrDefault(gf => gf.State == state);
    }
    GameField? GetEmptyGameField()
    {
        if (GameFields is null) return default;
        return GameFields.FirstOrDefault(gf => gf.Value == 0);
    }

    static bool ActionAllowed(GameField srcField, GameField dstField)
    {
        if (srcField.Value == 0) return false;
        if (dstField.Value != 0) return false;
        return true;
    }

    public void NewGame()
    {
        timer.Stop();
        currentCount = 0;
        CurrentTime = FormatTime(currentCount);
        Random r = new();
        int[] array = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24 };
        int[] values = array.Take(AreaSize * AreaSize).ToArray();
        values = values.OrderBy(x => r.Next()).ToArray();
        GameFields = new();
        for (int i = 0; i < AreaSize * AreaSize; i++)
        {
            GameFields.Add(new GameField() { Index = i + 1, Value = values[i] });
        }
        timer.Start();
        _ = InvokeAsync(StateHasChanged);
    }

    private void OnTimerCallback()
    {
        _ = InvokeAsync(() =>
        {
            currentCount++;
            CurrentTime = FormatTime(currentCount);
            StateHasChanged();
        });
    }

    void ChangeMode()
    {
        string[] ss = GameMode.Split("x");
        AreaSize = Convert.ToInt32(ss[0]);
        NewGame();
    }

    static string FormatTime(int seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return string.Format("{0:D2}:{1:D2}:{2:D2}",
                time.Hours,
                time.Minutes,
                time.Seconds);
    }

    public void Dispose() => timer.Dispose();

    public class GameField
    {
        public int Index { get; set; }
        public int Value { get; set; }
        public bool State { get; set; } = false;
    }
}
