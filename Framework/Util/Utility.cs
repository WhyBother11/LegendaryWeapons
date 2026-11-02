namespace LegendaryWeapons.Framework.Util;

public static class Utility
{
    /// <summary>
    /// Prepend mod id and <c>_</c> delimiter to given <paramref name="str"/>.
    /// </summary>
    /// <param name="str">String to prepend id to.</param>
    /// <returns><paramref name="str"/> with prepended mod id.</returns>
    public static string PrependId(this string str) => str.Prepend($"{Id}_");
    
    /// <summary>
    /// Prepend given <paramref name="prefix"/> to given <paramref name="str"/>.
    /// </summary>
    /// <param name="str">String to prepend prefix to.</param>
    /// <param name="prefix">Prefix to prepend.</param>
    /// <returns><paramref name="str"/> with prepended prefix.</returns>
    public static string Prepend(this string str, string prefix) => $"{prefix}{str}";
    
    /// <summary>
    /// Convert given <paramref name="sec"/> seconds to milliseconds.
    /// </summary>
    /// <param name="sec">Seconds amount.</param>
    /// <returns>Milliseconds amount.</returns>
    public static int ToMilliseconds(this float sec)
    {
        return (int) (sec * 1000);
    }
    
    /// <summary>
    /// Convert given <paramref name="sec"/> seconds to milliseconds.
    /// </summary>
    /// <param name="sec">Seconds amount.</param>
    /// <returns>Milliseconds amount.</returns>
    public static int ToMilliseconds(this int sec)
    {
        return sec * 1000;
    }

    public static int ToTicks(this int sec)
    {
        return sec * 60;
    }
    
    /// <summary>
    /// Convert given <paramref name="sec"/> seconds to ticks equivalent.
    /// </summary>
    /// <param name="sec">Seconds amount.</param>
    /// <returns>Ticks amount.</returns>
    public static int ToTicks(this float sec)
    {
        return (int) (sec * 60);
    }
}