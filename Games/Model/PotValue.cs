namespace Games.Model;

public class PotValue
{
    public int Id { get; set; }    
    public int Value { get; set; }

    public PotValue(int id, int value)
    {        
        Id = id;
        Value = value;
    }
}
