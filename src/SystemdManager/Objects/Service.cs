using SystemdManager.Parser;

namespace SystemdManager.Objects;

public class Service
{

    public string Name { get; set; }
    public string Content { get; set; }
    public string FullName { get; set; }
    public ServiceStatus Status { get; set; }

}
