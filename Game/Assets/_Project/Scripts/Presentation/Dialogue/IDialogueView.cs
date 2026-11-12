using System.Collections.Generic;
using UFQ.UseCases.Dialogue;

namespace UFQ.Presentation.Dialogue
{
    /// <summary>
    /// Lo que el reproductor de diálogos necesita de una interfaz. Separarlo permite
    /// cambiar la UI (globos en el mundo, celular, libro) sin tocar la lógica.
    /// </summary>
    public interface IDialogueView
    {
        void Open();
        void Close();

        /// <summary>Muestra una línea. <paramref name="offerSave"/> enciende el botón dorado «Guardar».</summary>
        void ShowLine(DialogueLine line, bool offerSave);

        /// <summary>La señal se acaba de guardar: el dorado se dibuja en el cuaderno.</summary>
        void MarkSaved();

        /// <summary>true mientras el texto aún se está escribiendo letra a letra.</summary>
        bool IsTyping { get; }

        void CompleteTyping();

        void ShowChoices(IReadOnlyList<DialogueChoice> choices);

        /// <summary>Índice elegido por el jugador desde que se mostraron las opciones, o -1.</summary>
        int TakePickedChoice();
    }
}
