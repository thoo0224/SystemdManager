using Serilog;

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

        var lines = commandResult.Split("\n");
        var line = FindSectionValue(lines, "Active");

        IsActive = line.StartsWith("active (running)");
        if (!IsActive)
        {
            return;
        }

        MemoryUsage = FindSectionValue(lines, "Memory");
    }

    private string FindSectionValue(IEnumerable<string> lines, string section)
    {
        var line = lines.FirstOrDefault(x => x.Split($"{section}: ").Length >= 2);
        return line?.Split(": ")[1];
    }

}
