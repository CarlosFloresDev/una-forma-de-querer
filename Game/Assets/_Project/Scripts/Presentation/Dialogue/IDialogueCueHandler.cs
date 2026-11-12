using System.Threading;
using Cysharp.Threading.Tasks;
using UFQ.UseCases.Dialogue;

namespace UFQ.Presentation.Dialogue
{
    /// <summary>
    /// Algo que reacciona a una etiqueta de Ink y que el diálogo debe ESPERAR antes de
    /// mostrar la línea (cambiar de lugar, un fundido, un minijuego). Las reacciones que
    /// no hay que esperar (música, sonidos) escuchan DialogueCue en el bus de eventos.
    /// </summary>
    public interface IDialogueCueHandler
    {
        bool CanHandle(DialogueTag tag);
        UniTask HandleAsync(DialogueTag tag, CancellationToken token);
    }
}
