using System;
using UFQ.Core.Narrative;
using UFQ.UseCases.Events;
using UFQ.UseCases.Persistence;

namespace UFQ.UseCases.Narrative
{
    /// <summary>
    /// Casos de uso narrativos. Es el único punto por donde escenas, Ink y UI
    /// modifican el estado. Cada cambio es transaccional: se aplica la regla, se
    /// guarda y solo entonces se publica el evento; si el guardado falla, el estado
    /// vuelve a como estaba y el jugador puede reintentar.
    /// </summary>
    public sealed class NarrativeService
    {
        private readonly IEventBus _bus;
        private readonly INarrativeRepository _repository;
        private NarrativeState _state;

        public NarrativeService(IEventBus bus, INarrativeRepository repository)
        {
            _bus = bus;
            _repository = repository;
            _state = LoadOrStartFresh(out var issue);
            LoadIssue = issue;
        }

        /// <summary>Vista de solo lectura. Para cambiar el estado usa los métodos del servicio.</summary>
        public IReadOnlyNarrativeState State => _state;

        /// <summary>Si la partida guardada no se pudo usar, explica por qué (null si todo bien).</summary>
        public string LoadIssue { get; }

        public void NewGame()
        {
            _repository.Delete();
            _state = new NarrativeState();
            _bus.Publish(new ChapterStarted(_state.CurrentChapter));
        }

        public bool SaveSignal(string signalId)
        {
            if (!Mutate(s => s.SaveSignal(signalId))) return false;
            _bus.Publish(new SignalSaved(signalId, _state.Signals));
            return true;
        }

        public bool NoticeGesture(string gestureId)
        {
            if (!Mutate(s => s.NoticeGesture(gestureId))) return false;
            _bus.Publish(new GestureNoticed(gestureId, _state.Vale));
            return true;
        }

        public bool ApplyLens(string momentId, LensReading reading)
        {
            var counted = Mutate(s => s.ApplyLens(momentId, reading));
            _bus.Publish(new LensApplied(momentId, reading, _state.Lens));
            return counted;
        }

        public bool FindDadMemory(string memoryId)
        {
            if (!Mutate(s => s.FindDadMemory(memoryId))) return false;
            _bus.Publish(new DadMemoryFound(memoryId, _state.DadMemoriesFound));
            return true;
        }

        /// <summary>Marca de historia (catálogo StoryMarks). Devuelve false si ya existía.</summary>
        public bool Mark(string markId)
        {
            if (!Mutate(s => s.Mark(markId))) return false;
            _bus.Publish(new DecisionTaken("MARCA", markId));
            return true;
        }

        public void StartChapter(Chapter chapter)
        {
            Mutate(s => { s.AdvanceTo(chapter); return true; });
            _bus.Publish(new ChapterStarted(chapter));
        }

        public void DecideC3(DecisionC3 decision) =>
            Decide(s => s.DecideC3(decision), "DECISION_C3", decision.ToString());

        public void SetNicoResponse(NicoResponse response) =>
            Decide(s => s.SetNicoResponse(response), "NICO_RESP", response.ToString());

        public void SetNewYearWish(NewYearWish wish) =>
            Decide(s => s.SetNewYearWish(wish), "DESEO", wish.ToString());

        public void DecideHonestyWithValeria(bool honest) =>
            Decide(s => s.DecideHonestyWithValeria(honest), "HONESTO_VALE", honest.ToString());

        public void DecideFinalMessage(FinalMessage message) =>
            Decide(s => s.DecideFinalMessage(message), "MENSAJE", message.ToString());

        public bool SaveInChapter5(string gestureId)
        {
            if (!Mutate(s => s.SaveInChapter5(gestureId))) return false;
            _bus.Publish(new DecisionTaken("GUARDAR_C5", _state.SavesInChapter5.ToString()));
            return true;
        }

        /// <summary>«¿Volver a mirar?» tras el final C: reinicia desde el Capítulo 4.</summary>
        public void RewindToChapter4()
        {
            Mutate(s => { s.RewindToChapter4(); return true; });
            _bus.Publish(new ChapterStarted(_state.CurrentChapter));
        }

        /// <summary>El tiempo de espera se acumula a menudo; se persiste en el siguiente guardado.</summary>
        public void AddWaitTime(double seconds) => _state.AddWaitTime(seconds);

        public EndingResult ResolveEnding() => EndingResolver.Resolve(_state);

        private void Decide(Action<NarrativeState> apply, string variable, string value)
        {
            Mutate(s => { apply(s); return true; });
            _bus.Publish(new DecisionTaken(variable, value));
        }

        /// <summary>
        /// Aplica el cambio y lo guarda. Si la regla falla, el estado no cambia (el dominio
        /// valida antes de modificar). Si el guardado falla, se restaura la copia previa.
        /// </summary>
        private bool Mutate(Func<NarrativeState, bool> change)
        {
            var before = NarrativeSnapshot.From(_state);
            if (!change(_state)) return false;

            try
            {
                _repository.Save(NarrativeSnapshot.From(_state));
            }
            catch
            {
                _state = before.ToState();
                throw;
            }

            return true;
        }

        private NarrativeState LoadOrStartFresh(out string issue)
        {
            issue = null;
            if (!_repository.TryLoad(out var snapshot)) return new NarrativeState();

            try
            {
                return snapshot.ToState();
            }
            catch (Exception e) when (e is NarrativeRuleException || e is NotSupportedException || e is ArgumentException)
            {
                // Partida con datos imposibles o de una versión más nueva: se aparta
                // (no se borra) y se empieza de cero para no bloquear el juego.
                issue = e.Message;
                _repository.Quarantine(e.Message);
                return new NarrativeState();
            }
        }
    }
}
