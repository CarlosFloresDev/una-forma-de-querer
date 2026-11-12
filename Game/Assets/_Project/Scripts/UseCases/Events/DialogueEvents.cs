using UFQ.UseCases.Dialogue;

namespace UFQ.UseCases.Events
{
    /// <summary>Empieza un diálogo: el jugador deja de moverse, la UI se muestra.</summary>
    public readonly struct DialogueStarted
    {
        public DialogueStarted(string storyId) { StoryId = storyId; }
        public string StoryId { get; }
    }

    /// <summary>Termina un diálogo: el jugador recupera el control.</summary>
    public readonly struct DialogueEnded
    {
        public DialogueEnded(string storyId) { StoryId = storyId; }
        public string StoryId { get; }
    }

    /// <summary>
    /// Una línea trae indicaciones de dirección (#camara, #musica, #sfx, #juego…).
    /// Cámara, audio y minijuegos las escuchan sin que el diálogo los conozca.
    /// </summary>
    public readonly struct DialogueCue
    {
        public DialogueCue(DialogueTag tag) { Tag = tag; }
        public DialogueTag Tag { get; }
    }
}
