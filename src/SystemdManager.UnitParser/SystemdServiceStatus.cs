namespace SystemdManager.UnitParser;

public class SystemdServiceStatus
{

    public bool IsActive { get; set; }
    public string MemoryUsage { get; set; }

    public SystemdServiceStatus(string commandResult)
    {
        var lines = commandResult.Split("\n");
        var statusLine = lines[2];
        IsActive = statusLine.StartsWith("active");

        var memoryLine = lines[6];
        MemoryUsage = memoryLine.Split(": ")[1];
    }

}
