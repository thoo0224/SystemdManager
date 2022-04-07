using Renci.SshNet;

using SystemdManager.Parser;

namespace SystemdManager.Objects;

public class Service
{

    public string Name { get; set; }
    public string Content { get; set; }
    public string FullName { get; set; }
    public ServiceStatus Status { get; set; }

    public Service(string name, string fullName, string content)
    {
        Name = name;
        FullName = fullName;
        Content = content;
    }

    public void LoadStatus(SshClient sshClient)
    {
        var statusCommandResult = sshClient.RunCommand($"systemctl -l -n 0 status {Name}");
        var status = new ServiceStatus(statusCommandResult.Result);

        Status = status;
    }

}
