using System.IO;

using SystemdManager.Framework;
using SystemdManager.Services;

namespace SystemdManager.ViewModels;

public class ApplicationViewModel : ViewModel
{

    public ServerViewModel ServerView { get; set; } = new();
    public string ApplicationDataPath { get; set; }

    public string ServersFile => Path.Combine(ApplicationDataPath, "servers.json");

}
