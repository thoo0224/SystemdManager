namespace SystemdManager.UnitParser;

public class SystemdSection
{

    public static readonly List<string> PropertyNames = new()
    {
        "Description",
        "WorkingDirectory",
        "ExecStartPre",
        "ExecStart"
    };

    public string Name { get; set; }
    public List<SystemdProperty> Properties { get; set; }

}
