
using System.ComponentModel;

namespace Games.Tools;

public static class CommonTools
{
    public static string ToString<T>(List<T> list) where T : notnull
    {
        string str = string.Empty;
        foreach (var item in list)
        {
            string s = Convert.ToInt32(item).ToString("X");
            str += s;
        }
        return str.ToLower();
    }

    public static string GetEnumDescription<T>(T value)
    {
        var enumType = typeof(T);
        var memberData = enumType.GetMember(value?.ToString() ?? string.Empty);
        string description = (memberData[0].GetCustomAttributes(typeof(DescriptionAttribute), false)?
            .FirstOrDefault() as DescriptionAttribute)?.Description ?? string.Empty;
        return description;
    }

    public static T GetEnumFromDescription<T>(string value)
    {
        if (Enum.IsDefined(typeof(T), value))
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }
        else
        {
            string[] enumNames = Enum.GetNames(typeof(T));
            foreach (string enumName in enumNames)
            {
                object e = Enum.Parse(typeof(T), enumName);
                if (value == GetEnumDescription((T)e))
                {
                    return (T)e;
                }
            }
        }
        throw new ArgumentException("The value '" + value
            + "' does not match a valid enum name or description.");
    }

    public static T GetEnumFromInt<T>(int id) => (T)Enum.ToObject(typeof(T), id);

    public static int GetRandomInt(int min, int max)
    {
        Random random = new ();
        return random.Next(min, max);
    }

    public static void GenerateRandomList<T>(List<T> list) where T : notnull  => Shuffle(list);

    public static void Shuffle<T>(IList<T> list)
    {
        Random rand = new();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rand.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}




