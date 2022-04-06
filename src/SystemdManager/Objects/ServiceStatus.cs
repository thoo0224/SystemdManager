
using System;
using System.Collections.Generic;
using System.Linq;

namespace SystemdManager.Objects;

public class ServiceStatus
{

    public bool IsActive { get; set; }
    public string MemoryUsage { get; set; }

    public ServiceStatus(string commandResult)
    {
        if (string.IsNullOrEmpty(commandResult))
        {
            return;
        }

        var entries = commandResult
            .Split("\n")
            .Select(x => x.Split(": "))
            .Where(x => x.Length >= 2)
            .Select(x =>
            {
                var key = x[0].ToLower();
                var keyLastIndex = key.LastIndexOf(' ') + 1;

                return new KeyValuePair<string, string>(key[keyLastIndex..], x[1]);
            }).ToList();

        var activeEntry = GetEntry(entries, "active");
        IsActive = activeEntry.StartsWith("active");
        if (!IsActive)
        {
            return;
        }

        MemoryUsage = GetEntry(entries, "memory"); // TODO: Parsing this to ByteSize
    }

    private string GetEntry(IEnumerable<KeyValuePair<string, string>> source, string key)
        => source.FirstOrDefault(x => x.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase)).Value;

    private string FindSectionValue(IEnumerable<string> lines, string section)
    {
        var line = lines.FirstOrDefault(x => x.Split($"{section}: ").Length >= 2);
        return line?.Split(": ")[1];
    }

}
