
namespace Games.Tools;

public class AlternateValueAttribute : Attribute
{
    public string AlternateValue { get; protected set; }

    public AlternateValueAttribute(string value)
    {
        AlternateValue = value;
    }
}
