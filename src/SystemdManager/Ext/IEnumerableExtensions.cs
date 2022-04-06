using System.Collections.Generic;

namespace SystemdManager.Ext;

public static class IEnumerableExtensions
{
    public static IEnumerable<T> DedupCollection<T>(this IEnumerable<T> source)
    {
        var passedValues = new HashSet<T>();
        foreach (var item in source)
        {
            if (passedValues.Add(item))
                yield return item;
        }
    }


}