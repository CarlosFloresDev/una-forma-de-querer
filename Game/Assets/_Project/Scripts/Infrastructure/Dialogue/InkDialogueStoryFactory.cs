using Ink.Runtime;
using UFQ.UseCases.Dialogue;
using UFQ.UseCases.Narrative;

namespace UFQ.Infrastructure.Dialogue
{
    /// <summary>
    /// Implementa el puerto IDialogueStoryFactory con Ink: cada historia nueva queda
    /// conectada al dominio (funciones EXTERNAL y variables sincronizadas).
    /// </summary>
    public sealed class InkDialogueStoryFactory : IDialogueStoryFactory
    {
        private readonly InkStoryLibrary _library;
        private readonly NarrativeService _narrative;

        public InkDialogueStoryFactory(InkStoryLibrary library, NarrativeService narrative)
        {
            _library = library;
            _narrative = narrative;
        }

        public bool Exists(string storyId) => _library != null && _library.TryGetJson(storyId, out _);

        public IDialogueStory Create(string storyId)
        {
            if (!_library.TryGetJson(storyId, out var json))
            {
                throw new System.ArgumentException($"No hay historia «{storyId}» en la biblioteca de Ink.");
            }

            var bridge = new InkNarrativeBridge(new Story(json), _narrative);
            return InkDialogueStory.Connected(bridge);
        }
    }
}
