
using SystemdManager.Framework;

namespace SystemdManager.ViewModels.Commands;

public class SaveVisualizedServiceCommand : ViewModelCommand<ServerViewModel>
{

    public SaveVisualizedServiceCommand(ServerViewModel contextViewModel)
        : base(contextViewModel) { }

    public override void Execute(ServerViewModel contextViewModel, object parameter)
    {
        // TODO
        //var window = (ServerWindow) parameter;
        //var avalonEdit = window.RawTextEditor;
        //var server = contextViewModel.ConnectedServer;
        //var service = server.SelectedService;
        //var newContent = service.Serialize();

        //server.SaveService(service, newContent);
        //server.RefreshService(ref service);

        //var oldCaretPosition = avalonEdit.TextArea.Caret.Position;
        //avalonEdit.Text = service.Content;
        //avalonEdit.TextArea.Caret.Position = oldCaretPosition;

        //contextViewModel.EditorTabItemHeader = "Editor";
    }

}