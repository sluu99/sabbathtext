using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Extensions for IEnumerable types
/// </summary>
public static class IEnumerableExtension
{
    /// <summary>
    /// Gets a random element of the enumerable
    /// </summary>
    /// <typeparam name="T">The element type</typeparam>
    /// <param name="enumerable">The enumerable</param>
    /// <returns>A random element</returns>
    public static T RandomElement<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.ElementAt(StaticRand.Next(enumerable.Count()));
    }
}
