namespace Games.Model;

public class Pot
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Value { get => _potValues.Sum(v => v.Value); }

    public event EventHandler? Changed;

    readonly Dictionary<int, int> _potValues = new ();

    public Dictionary<int, int> PotValues { get => _potValues; }

    public void Update(int id, int value) 
    {
        if(! _potValues.ContainsKey(id))
            _potValues[id] = 0;
        _potValues[id] += value;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Update(Dictionary<int, int> dict)
    {
        foreach(KeyValuePair<int, int> kvp in dict)
        {
            Update(kvp.Key, kvp.Value);
        }
    }

    public void Clear() => _potValues.Clear();

    public List<int> GetIds(List<int> ids) => _potValues.Keys
        .Where(v => ids.Any(i => i == v)).Select(v => v).ToList();
    

}
