namespace SystemdManager.UnitParser;

public class SystemdService
{

    public string Name { get; set; }
    public string Raw { get; set; }
    public string FullName { get; set; }
    public List<SystemdSection> Sections { get; set; }

}
