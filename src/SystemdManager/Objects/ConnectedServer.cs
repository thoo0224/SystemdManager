using Renci.SshNet;
using Renci.SshNet.Sftp;

using Serilog;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using SystemdManager.Framework;
using SystemdManager.Services;
using SystemdManager.UnitParser;
using SystemdManager.ViewModels;

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

    private readonly ServerViewModel _serverView;
    private readonly SshClient _sshClient;
    private readonly SftpClient _sftpClient;

    public ConnectedServer(Server server)
    {
        Server = server;

        _serverView = ApplicationService.ApplicationView.ServerView;
        _sshClient = new SshClient(server.Host, server.Port, server.User, server.Password);
        _sftpClient = new SftpClient(_sshClient.ConnectionInfo);
    }

    // TODO: Add the systemd directory(s) in the settings
    // TODO: Add file watchers (if possible, if not, periodically check the file content)
    public void LoadServices()
    {
        Log.Information("Loading services...");
        var sw = new Stopwatch();
        sw.Start();

        var serviceFiles = _sftpClient.ListDirectory("/etc/systemd/system").ToList();
        var processors = Environment.ProcessorCount;
        var processorChunks = serviceFiles.Chunk(processors).ToList();
        var threads = new List<Thread>(processors);
        var services = new List<Service>();
        foreach (var chunk in processorChunks)
        {
            var thread = new Thread(() =>
            {
                foreach (var serviceFile in chunk)
                {
                    var service = LoadService(serviceFile);
                    if (service == null)
                    {
                        continue;
                    }

                    services.Add(service);
                }
            });
            thread.Start();
            threads.Add(thread);
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Services = new ObservableCollection<Service>(services);
        sw.Stop();

        Log.Information("Successfully loaded {ServiceCount} in {Elapsed}", 
            _services.Count, sw.Elapsed);
    }

    public void SaveService(Service service, string content)
    {
        _sftpClient.DeleteFile(service.FullName);
        _sftpClient.WriteAllText(service.FullName, content);
    }

    public string LoadServiceJournal(Service service)
    {
        var commandResult = _sshClient.RunCommand($"journalctl -u {service.Name} -b");
        return commandResult.Result;
    }

    public bool Connect(out Exception exception)
    {
        Log.Information("Connecting with SSH & SFTP");
        var sw = new Stopwatch();
        sw.Start();

        try
        {
            _sshClient.Connect();
            _sftpClient.Connect();
            sw.Stop();
            if (!_sshClient.IsConnected)
            {
                exception = null;
                return false;
            }
        }
        catch(Exception e)
        {
            exception = e;
            return false;
        }

        exception = null;
        Log.Information("Successfully connected in {Elapsed}", sw.Elapsed);
        return true;
    }

    public void RefreshService(ref Service service)
    {
        service = LoadService(service.Name, service.FullName);
    }

    private Service LoadService(SftpFile file)
        => LoadService(file.Name, file.FullName);

    private Service LoadService(string name, string fullName)
    {
        if (!fullName.EndsWith(".service"))
        {
            return null;
        }

        var content = _sftpClient.ReadAllText(fullName);
        var lines = content.Split("\n");
        var parser = new UnitFileParser(lines);
        var systemdService = parser.Parse();
        var service = new Service
        {
            FullName = fullName,
            Name = name,
            Raw = content,
            Sections = systemdService.Sections
        };

        var statusCommandResult = _sshClient.RunCommand($"systemctl -l status {service.Name}");
        var status = new ServiceStatus(statusCommandResult.Result);
        service.Status = status;

        return service;
    }

}
