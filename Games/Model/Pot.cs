namespace Games.Model;

public class Pot
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Value { get => _potValues.Sum(v => v.Value); }

    public event EventHandler? Changed;

    List<PotValue> _potValues = new ();

    public void Update(int id, int value) 
    {
        _potValues.Add(new(id, value));
        Changed?.Invoke(this, EventArgs.Empty);
    }
    
    public void Clear() => _potValues.Clear();

    public List<int> GetIds(List<int> ids) => _potValues
        .Where(v => ids.Any(i => i == v.Id)).Select(v => v.Id).ToList();
    

}
