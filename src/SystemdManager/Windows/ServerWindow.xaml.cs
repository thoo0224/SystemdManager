using AdonisUI.Controls;

using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

using SystemdManager.Services;
using SystemdManager.ViewModels;

namespace SystemdManager.Windows;

/// <summary>
/// Interaction logic for ServerWindow.xaml
/// </summary>
public partial class ServerWindow : AdonisWindow
{

    private readonly ServerViewModel _serverView;

    public ServerWindow()
    {
        var applicationView = ApplicationService.ApplicationView;
        _serverView = applicationView.ServerView;
        DataContext = _serverView;

        InitializeComponent();
        LoadAvalonHighlighter();
    }

    private void Window_OnLoaded(object sender, RoutedEventArgs e)
    {
        Title = $"Systemd Manager - {_serverView.ConnectedServer.Server.Name}";
        _serverView.ConnectedServer.LoadServices();
        RawTextEditor.TextArea.TextEntered += RawTextEditor_OnTextEntered;
    }

    private void RawTextEditor_OnTextEntered(object sender, TextCompositionEventArgs e)
    {
        _serverView.EditorTabItemHeader = "Editor*";
    }

    // TODO: Watch Journal CTL
    private void ServiceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var service = _serverView.ConnectedServer.SelectedService;
        if (service == null)
            return;

        RawTextEditor.Text = service.Raw;
        ConsoleTextArea.Text = _serverView.ConnectedServer.LoadServiceJournal(service);
        ConsoleTextArea.ScrollToEnd();
    }

    // TODO: Fix highlighter
    private void LoadAvalonHighlighter()
    {
        using var resource = App.GetAppResourceStream("systemd.xshd");
        using var reader = new XmlTextReader(resource);
        var highlighter = HighlightingLoader.Load(reader, HighlightingManager.Instance);
        RawTextEditor.SyntaxHighlighting = highlighter;
    }

}