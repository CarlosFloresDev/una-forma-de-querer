using System;
using System.Collections.Generic;
using Ink.Runtime;
using UFQ.UseCases.Dialogue;

namespace UFQ.Infrastructure.Dialogue
{
    /// <summary>
    /// Adaptador de un Story de Ink al puerto IDialogueStory. Antes de cada línea llama
    /// a <c>beforeContinue</c> (normalmente InkNarrativeBridge.SyncToStory) para que las
    /// condiciones de Ink lean el estado real aunque haya cambiado desde la escena.
    /// </summary>
    public sealed class InkDialogueStory : IDialogueStory
    {
        private readonly Story _story;
        private readonly Action _beforeContinue;
        private readonly List<DialogueChoice> _choices = new List<DialogueChoice>();

        public InkDialogueStory(Story story, Action beforeContinue = null)
        {
            _story = story;
            _beforeContinue = beforeContinue;
        }

        /// <summary>Crea la historia ya conectada al dominio a través del puente.</summary>
        public static InkDialogueStory Connected(InkNarrativeBridge bridge) =>
            new InkDialogueStory(bridge.Story, bridge.SyncToStory);

        public bool CanContinue => _story.canContinue;
        public bool IsFinished => !_story.canContinue && _story.currentChoices.Count == 0;

        public DialogueLine Continue()
        {
            _beforeContinue?.Invoke();
            var text = _story.Continue();
            return DialogueLine.Parse(text, _story.currentTags);
        }

        public IReadOnlyList<DialogueChoice> CurrentChoices
        {
            get
            {
                _choices.Clear();
                foreach (var choice in _story.currentChoices)
                {
                    var tags = new List<DialogueTag>();
                    if (choice.tags != null)
                    {
                        foreach (var raw in choice.tags) tags.Add(DialogueTag.Parse(raw));
                    }

                    _choices.Add(new DialogueChoice(choice.index, choice.text, tags));
                }

                return _choices;
            }
        }

        public void Choose(int index) => _story.ChooseChoiceIndex(index);

        public void JumpTo(string knotPath) => _story.ChoosePathString(knotPath);
    }
}
