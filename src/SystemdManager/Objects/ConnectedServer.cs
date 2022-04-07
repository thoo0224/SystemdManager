using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Renci.SshNet;
using Renci.SshNet.Sftp;

using Serilog;

using SystemdManager.Ext;
using SystemdManager.Framework;

namespace SystemdManager.Objects;

public class ConnectedServer : ViewModel
{

    public Server Server { get; set; }

    private Service _selectedService;
    public Service SelectedService
    {
        get => _selectedService;
        set => SetProperty(ref _selectedService, value);
    }

    private List<Service> _services = new();
    public List<Service> Services
    {
        get => _services;
        set => SetProperty(ref _services, value);
    }

    private readonly SshClient _sshClient;
    private readonly SftpClient _sftpClient;

    // TODO: Separate SSH and SFTP configuration
    public ConnectedServer(Server server)
    {
        Server = server;

        _sshClient = new SshClient(server.Host, server.Port, server.User, server.Password);
        _sftpClient = new SftpClient(_sshClient.ConnectionInfo);
    }

    public async Task LoadServicesAsync()
    {
        Log.Information("Loading services...");
        var sw = new Stopwatch();
        sw.Start();

        var serviceFiles = _sftpClient.ListDirectory("/etc/systemd/system").ToList();
        var command = BuildCatCommand(serviceFiles.Where(x => x.Name.EndsWith(".service")).ToList());
        var commandResult = _sshClient.RunCommand(command);
        var output = commandResult.Result;

        Services = new List<Service>();
        LoadServices(output);

        await Services.ForEachAsync(20, service =>
        {
            service.LoadStatus(_sshClient);
        });

        sw.Stop();

        Log.Information("Successfully loaded {ServiceCount} in {Elapsed}",
            _services.Count, sw.Elapsed);
    }

    public void SaveService(Service service, string content)
    {
        Log.Information("Saving service: {ServiceName}",
            service.FullName);
        _sftpClient.WriteAllText(service.FullName, content);
    }

    public string LoadServiceJournal(Service service)
    {
        var commandResult = _sshClient.RunCommand($"journalctl -u {service.Name} -b --no-pager -n 100000");
        return commandResult.Result;
    }

    public void ConnectAsync()
    {
        Log.Information("Connecting with SSH & SFTP");
        var sw = new Stopwatch();
        sw.Start();

        _sshClient.Connect();
        _sftpClient.Connect();

        sw.Stop();
        if (!_sshClient.IsConnected || !_sftpClient.IsConnected)
        {
            throw new Exception("Failed to connect to sftp or ssh.");
        }

        Log.Information("Successfully connected in {Elapsed}", sw.Elapsed);
    }

    public void RefreshService(ref Service service)
    {
        service = LoadService(service.FullName);
    }

    private void LoadServices(string output)
    {
        var span = output.AsSpan();
        while (true)
        {
            if (span.IsEmpty)
                break;

            var off = SftpFileStartHeaderSeparator.Length;
            var startHeaderIdx = span.IndexOf(SftpFileStartHeaderSeparator, StringComparison.Ordinal) + off;
            var endHeaderIdx = span.IndexOf(SftpFileEndHeaderSeparator, StringComparison.Ordinal) - off;
            var header = span.Slice(startHeaderIdx, endHeaderIdx);

            var fullNameIdx = header.IndexOf(":");
            var nameIdx = header.IndexOf(":") + 1;
            var fullName = header[..fullNameIdx].TrimStart();
            var name = header[nameIdx..];

            var fullHeaderEnd = endHeaderIdx + off + SftpFileEndHeaderSeparator.Length + 1;
            var endFileIdx = span.IndexOf(SftpFileEndSeparator, StringComparison.Ordinal);
            var service = new Service(name.ToString(), fullName.ToString(), span[fullHeaderEnd..endFileIdx].ToString());
            Services.Add(service);

            var actualEnd = endFileIdx + SftpFileEndSeparator.Length + 1;
            span = span[actualEnd..];
        }
    }

    private Service LoadService(string path)
    {
        var commandResult = _sshClient.RunCommand($"cat {path}");
        var content = commandResult.Result;

        var service = new Service(path[path.LastIndexOf('/')..], path, content);
        service.LoadStatus(_sshClient);

        return service;
    }

    private const string SftpFileEndSeparator = "SYSTEMD_MANAGER__END_OF_FILE__SYSTEMD_MANAGER";
    private const string SftpFileStartHeaderSeparator = "SYSTEMD_MANAGER__START__HEADER";
    private const string SftpFileEndHeaderSeparator = "SYSTEMD_MANAGER__END__HEADER";

    private static string BuildCatCommand(IReadOnlyList<SftpFile> files)
    {
        var commandBuilder = new StringBuilder();
        var numFiles = files.Count;
        for (var i = 0; i < numFiles; i++)
        {
            var file = files[i];
            commandBuilder.Append($"echo {SftpFileStartHeaderSeparator} {file.FullName}:{file.Name} {SftpFileEndHeaderSeparator} && cat {file.FullName} && echo {SftpFileEndSeparator}");

            if (i < numFiles-1)
            {
                commandBuilder.Append(" && ");
            }
        }

        return commandBuilder.ToString();
    }

}
