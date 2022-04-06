using Renci.SshNet;
using Renci.SshNet.Sftp;

using Serilog;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SystemdManager.Ext;
using SystemdManager.Framework;
using SystemdManager.Parser;

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

    private ObservableCollection<Service> _services = new();
    public ObservableCollection<Service> Services
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

    // TODO: Add custom systemd directory(s) in the settings
    // TODO: Add file watchers (if possible, if not, periodically check the file content)
    public async Task LoadServices()
    {
        Log.Information("Loading services...");
        var sw = new Stopwatch();
        sw.Start();
 
        // todo: maybe to do this with ssh? & multiple paths
        var serviceFiles = _sftpClient.ListDirectory("/etc/systemd/system").ToList();
        var processors = Environment.ProcessorCount;
        var services = new List<Service>();

        await serviceFiles.ForEachAsync(processors, file =>
        {
            var service = LoadService(file);
            if (service == null)
            {
                return;
            }

            services.Add(service);
        });

        Services = new ObservableCollection<Service>(services);
        sw.Stop();

        Log.Information("Successfully loaded {ServiceCount} in {Elapsed}", 
            _services.Count, sw.Elapsed);
    }

    public void SaveService(Service service, string content)
    {
        Log.Information("Saving service: {ServiceName}",
            service.FullName);
        _sftpClient.DeleteFile(service.FullName);
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
        service = LoadService(service.Name, service.FullName);
    }

    private Service LoadService(SftpFile file)
        => LoadService(file.Name, file.FullName);

    private Service LoadService(string name, string fullName)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        if (!fullName.EndsWith(".service"))
        {
            return null;
        }

        var command = _sshClient.RunCommand($"cat {fullName}");
        var content = command.Result;

        var service = new Service
        {
            FullName = fullName,
            Name = name,
            Content = content
        };

        var statusCommandResult = _sshClient.RunCommand($"systemctl -l -n 0 status {service.Name}");
        var status = new ServiceStatus(statusCommandResult.Result);
        service.Status = status;

        stopwatch.Stop();
        Log.Information("{Filename} took {time}ms to load!", name, stopwatch.ElapsedMilliseconds);

        return service;
    }

}
