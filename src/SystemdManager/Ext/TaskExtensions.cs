using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SystemdManager.Ext;

public static class TaskExtensions
{

    public static Task ForEachAsync<T>(this IEnumerable<T> source, int processors, Action<T> body)
    {
        var partition = Partitioner.Create(source).GetPartitions(processors);
        var tasks = partition.Select(x => Task.Run(() =>
        {
            while (x.MoveNext())
            {
                body(x.Current);
            }
        }));

        return Task.WhenAll(tasks);
    }

}