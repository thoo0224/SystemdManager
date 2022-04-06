using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using SystemdManager.Framework;
using SystemdManager.Objects;
using SystemdManager.Services;
using SystemdManager.ViewModels.Commands;

namespace SystemdManager.ViewModels;

public class ServerViewModel : ViewModel
{

    // TODO: make this better lol, worst way of doing this
    private string _editorTabItemHeader = "Editor";
    public string EditorTabItemHeader
    {
        get => _editorTabItemHeader;
        set => SetProperty(ref _editorTabItemHeader, value);
    }

    private string _visualizedTabItemHeader = "Visualized";
    public string VisualizedTabItemHeader
    {
        get => _visualizedTabItemHeader;
        set => SetProperty(ref _visualizedTabItemHeader, value);
    }

    public SaveVisualizedServiceCommand SaveVisualizedServiceCommand { get; set; }
    public SaveRawServiceCommand SaveRawServiceCommand { get; set; }

    private ConnectedServer _connectedServer;
    public ConnectedServer ConnectedServer
    {
        get => _connectedServer;
        set => SetProperty(ref _connectedServer, value);
    }

    private Server _selectedServer;
    public Server SelectedServer
    {
        get => _selectedServer;
        set => SetProperty(ref _selectedServer, value);
    }

    private ObservableCollection<Server> _servers = new();
    public ObservableCollection<Server> Servers
    {
        get => _servers;
        set => SetProperty(ref _servers, value);
    }

    public ServerViewModel()
    {
        SaveRawServiceCommand = new SaveRawServiceCommand(this);
        SaveVisualizedServiceCommand = new SaveVisualizedServiceCommand(this);
    }

    public async Task<List<Server>> LoadServersAsync()
    {
        var path = ApplicationService.ApplicationView.ApplicationDataPath;
        var file = Path.Combine(path, "servers.json");
        List<Server> servers;
        if (!File.Exists(file))
        {
            servers = new List<Server> { Server.CreateDefault() };
        }
        else
        {
            var content = await File.ReadAllTextAsync(file).ConfigureAwait(false);
            servers = JsonSerializer.Deserialize<List<Server>>(content, App.JsonSerializerOptions);
        }

        var serialized = JsonSerializer.Serialize(servers, App.JsonSerializerOptions);
        await File.WriteAllTextAsync(file, serialized).ConfigureAwait(false);
        return servers;
    }

    public async Task SaveServersAsync()
    {
        var serverView = ApplicationService.ApplicationView.ServerView;
        var servers = serverView.Servers;
        var serialized = JsonSerializer.Serialize(servers, App.JsonSerializerOptions);

        await File.WriteAllTextAsync(ApplicationService.ApplicationView.ServersFile,serialized)
            .ConfigureAwait(false);
    }

}
