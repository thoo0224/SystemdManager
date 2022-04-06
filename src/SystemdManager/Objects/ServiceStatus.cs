using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SystemdManager.Objects;

public class ServiceStatus
{

    private static readonly Regex DatetimeRegex 
        = new(@"(\d{4})-(\d{2})-(\d{2}) (\d{2}):(\d{2}):(\d{2})", RegexOptions.Compiled);

    public bool IsActive { get; set; }
    public string MemoryUsage { get; set; }
    public int MainPid { get; set; }
    public DateTime RunningSince { get; set; }

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
                var key = x[0].ToLower().TrimStart(' ');

                return new KeyValuePair<string, string>(key, x[1]);
            }).ToList();

        var activeEntry = GetEntry(entries, "active");
        IsActive = activeEntry.StartsWith("active");
        if (!IsActive)
            return;

        RunningSince = GetRunningSince(activeEntry);
        MemoryUsage = GetEntry(entries, "memory"); // TODO: Parsing this to ByteSize
        MainPid = int.Parse(GetEntry(entries, "main pid").Split(" ")[0]);
    }

    private DateTime GetRunningSince(string entry)
    {
        var match = DatetimeRegex.Match(entry);
        if (match.Groups.Count != 7)
            throw new Exception("Failed to get datetime.");

        var groups = match.Groups;
        var year = int.Parse(groups[1].Value);
        var month = int.Parse(groups[2].Value);
        var day = int.Parse(groups[3].Value);

        var hour = int.Parse(groups[4].Value);
        var minute = int.Parse(groups[5].Value);
        var second = int.Parse(groups[6].Value);

        return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
    }

    private string GetEntry(IEnumerable<KeyValuePair<string, string>> source, string key)
        => source.FirstOrDefault(x => x.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase)).Value;

}
