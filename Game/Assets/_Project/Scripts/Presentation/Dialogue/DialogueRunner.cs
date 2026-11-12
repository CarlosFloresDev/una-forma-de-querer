using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UFQ.Presentation.Input;
using UFQ.UseCases.Dialogue;
using UFQ.UseCases.Events;
using UFQ.UseCases.Narrative;

namespace UFQ.Presentation.Dialogue
{
    /// <summary>
    /// Reproduce una historia línea a línea:
    ///  · las líneas con #senal ofrecen «Guardar» (dorado) → NarrativeService.SaveSignal
    ///  · las líneas con #gesto NO ofrecen nada: solo cuentan si el jugador pulsa «Mirar»
    ///    por su cuenta (sección 4.3 del guion)
    ///  · las etiquetas de dirección (#camara, #musica, #sfx, #juego…) se publican como
    ///    DialogueCue para que otros sistemas reaccionen.
    /// </summary>
    public sealed class DialogueRunner
    {
        private readonly IDialogueStoryFactory _factory;
        private readonly IDialogueView _view;
        private readonly GameInput _input;
        private readonly IEventBus _bus;
        private readonly NarrativeService _narrative;
        private readonly IReadOnlyList<IDialogueCueHandler> _handlers;

        public DialogueRunner(IDialogueStoryFactory factory, IDialogueView view, GameInput input, IEventBus bus,
            NarrativeService narrative, IReadOnlyList<IDialogueCueHandler> handlers)
        {
            _handlers = handlers ?? new IDialogueCueHandler[0];
            _factory = factory;
            _view = view;
            _input = input;
            _bus = bus;
            _narrative = narrative;
        }

        public bool IsRunning { get; private set; }

        public async UniTask PlayAsync(string storyId, CancellationToken token)
        {
            if (IsRunning) return;
            IsRunning = true;

            var story = _factory.Create(storyId);
            _bus.Publish(new DialogueStarted(storyId));
            _view.Open();
            try
            {
                while (true)
                {
                    while (story.CanContinue)
                    {
                        var line = story.Continue();
                        await HandleCuesAsync(line, token);
                        PublishCues(line);
                        if (line.Kind == LineKind.Cue) continue;
                        await ShowLineAsync(line, token);
                    }

                    if (story.IsFinished) break;

                    _view.ShowChoices(story.CurrentChoices);
                    var picked = -1;
                    while (picked < 0)
                    {
                        await UniTask.Yield(PlayerLoopTiming.Update, token);
                        picked = _view.TakePickedChoice();
                    }

                    story.Choose(picked);
                }
            }
            finally
            {
                _view.Close();
                IsRunning = false;
                _bus.Publish(new DialogueEnded(storyId));
            }
        }

        private async UniTask ShowLineAsync(DialogueLine line, CancellationToken token)
        {
            var signal = line.SignalId;
            var gesture = line.GestureId;
            var offerSave = signal != null && !_narrative.State.HasSavedSignal(signal);
            _view.ShowLine(line, offerSave);

            // Un frame de margen para que la pulsación que cerró la línea anterior no salte esta.
            await UniTask.Yield(PlayerLoopTiming.Update, token);

            while (true)
            {
                if (offerSave && _input.Save.WasPressedThisFrame())
                {
                    if (_narrative.SaveSignal(signal)) _view.MarkSaved();
                    offerSave = false;
                }

                if (gesture != null && _input.Look.WasPressedThisFrame())
                {
                    _narrative.NoticeGesture(gesture); // sin efecto visible: así lo pide el diseño
                }

                if (_input.Advance.WasPressedThisFrame())
                {
                    if (_view.IsTyping) _view.CompleteTyping();
                    else return;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        /// <summary>Espera a los manejadores (cambio de lugar, fundidos…) antes de mostrar la línea.</summary>
        private async UniTask HandleCuesAsync(DialogueLine line, CancellationToken token)
        {
            foreach (var tag in line.Tags)
            {
                foreach (var handler in _handlers)
                {
                    if (handler.CanHandle(tag)) await handler.HandleAsync(tag, token);
                }
            }
        }

        private void PublishCues(DialogueLine line)
        {
            foreach (var tag in line.Tags)
            {
                switch (tag.Key)
                {
                    // Etiquetas que solo describen la línea: no son indicaciones de dirección.
                    case "accion":
                    case "voz_interior":
                    case "titulo":
                    case "celular":
                    case "senal":
                    case "gesto":
                    case "semilla":
                    case "para":
                        continue;
                    default:
                        _bus.Publish(new DialogueCue(tag));
                        break;
                }
            }
        }
    }
}
