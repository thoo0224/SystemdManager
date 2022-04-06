using ICSharpCode.AvalonEdit;

using SystemdManager.Framework;

namespace SystemdManager.ViewModels.Commands;

public class SaveRawServiceCommand : ViewModelCommand<ServerViewModel>
{

    public SaveRawServiceCommand(ServerViewModel contextViewModel)
        : base(contextViewModel) { }

    public override void Execute(ServerViewModel contextViewModel, object parameter)
    {
        var avalonEdit = (TextEditor) parameter;
        var server = contextViewModel.ConnectedServer;
        var newContent = avalonEdit.Text;
        var service = server.SelectedService;

        server.SaveService(service, newContent);
        server.RefreshService(ref service);

        var oldCaretPosition = avalonEdit.TextArea.Caret.Position;
        avalonEdit.Text = service.Content;
        avalonEdit.TextArea.Caret.Position = oldCaretPosition;

        contextViewModel.EditorTabItemHeader = "Editor";
    }

}