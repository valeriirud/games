using System.ComponentModel;

namespace Games.Tools;

public static class Extensions
{
    public static int EnumToInt<T>(T value) => Convert.ToInt32(value);
    public static int ToInt<T>(this T value) => EnumToInt(value);
    public static string GetEnumDescription<T>(T value)
    {
        var enumType = typeof(T);
        var memberData = enumType.GetMember(value?.ToString() ?? string.Empty);
        return (memberData[0].GetCustomAttributes(typeof(DescriptionAttribute), false)?
            .FirstOrDefault() as DescriptionAttribute)?.Description ?? string.Empty;        
    }
    public static string Description<T>(this T value) => GetEnumDescription(value);

    public static string GetEnumDisplayName<T>(T value)
    {        
        var enumType = typeof(T);
        var memberData = enumType.GetMember(value?.ToString() ?? string.Empty);
        return (memberData[0]
            .GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.DisplayAttribute), false)?
            .FirstOrDefault() as System.ComponentModel.DataAnnotations.DisplayAttribute)?.Name ?? string.Empty;  
    }
    public static string DisplayName<T>(this T value) => GetEnumDisplayName(value);

    public static T GetEnumFromInt<T>(int value) => (T)Enum.ToObject(typeof(T), value);
}
