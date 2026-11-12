using UFQ.UseCases.Events;
using UFQ.UseCases.Persistence;
using UFQ.Core.Narrative;

namespace UFQ.UseCases.Narrative
{
    /// <summary>
    /// Casos de uso narrativos. Es el único punto por donde escenas, Ink y UI
    /// modifican el estado: aplica la regla, guarda y publica el evento.
    /// </summary>
    public sealed class NarrativeService
    {
        private readonly IEventBus _bus;
        private readonly INarrativeRepository _repository;

        public NarrativeService(IEventBus bus, INarrativeRepository repository)
        {
            _bus = bus;
            _repository = repository;
            State = repository.TryLoad(out var snapshot) ? snapshot.ToState() : new NarrativeState();
        }

        public NarrativeState State { get; private set; }

        public void NewGame()
        {
            _repository.Delete();
            State = new NarrativeState();
            _bus.Publish(new ChapterStarted(State.CurrentChapter));
        }

        public bool SaveSignal(string signalId)
        {
            if (!State.SaveSignal(signalId)) return false;
            Persist();
            _bus.Publish(new SignalSaved(signalId, State.Signals));
            return true;
        }

        public bool NoticeGesture(string gestureId)
        {
            if (!State.NoticeGesture(gestureId)) return false;
            Persist();
            _bus.Publish(new GestureNoticed(gestureId, State.Vale));
            return true;
        }

        public bool ApplyLens(string momentId, LensReading reading)
        {
            var counted = State.ApplyLens(momentId, reading);
            if (counted) Persist();
            _bus.Publish(new LensApplied(momentId, reading, State.Lens));
            return counted;
        }

        public bool FindDadMemory(string memoryId)
        {
            if (!State.FindDadMemory(memoryId)) return false;
            Persist();
            _bus.Publish(new DadMemoryFound(memoryId, State.DadMemoriesFound));
            return true;
        }

        public void StartChapter(Chapter chapter)
        {
            State.AdvanceTo(chapter);
            Persist();
            _bus.Publish(new ChapterStarted(chapter));
        }

        public void DecideC3(DecisionC3 decision) =>
            Decide(() => State.DecideC3(decision), "DECISION_C3", decision.ToString());

        public void SetNicoResponse(NicoResponse response) =>
            Decide(() => State.SetNicoResponse(response), "NICO_RESP", response.ToString());

        public void SetNewYearWish(NewYearWish wish) =>
            Decide(() => State.SetNewYearWish(wish), "DESEO", wish.ToString());

        public void DecideHonestyWithValeria(bool honest) =>
            Decide(() => State.DecideHonestyWithValeria(honest), "HONESTO_VALE", honest.ToString());

        public void DecideFinalMessage(FinalMessage message) =>
            Decide(() => State.DecideFinalMessage(message), "MENSAJE", message.ToString());

        public bool SaveInChapter5(string gestureId)
        {
            if (!State.SaveInChapter5(gestureId)) return false;
            Persist();
            _bus.Publish(new DecisionTaken("GUARDAR_C5", State.SavesInChapter5.ToString()));
            return true;
        }

        /// <summary>El tiempo de espera se acumula a menudo; se persiste en el siguiente guardado.</summary>
        public void AddWaitTime(double seconds) => State.AddWaitTime(seconds);

        public EndingResult ResolveEnding() => EndingResolver.Resolve(State);

        private void Decide(System.Action apply, string variable, string value)
        {
            apply();
            Persist();
            _bus.Publish(new DecisionTaken(variable, value));
        }

        private void Persist() => _repository.Save(NarrativeSnapshot.From(State));
    }
}
