using UFQ.Core.Narrative;

namespace UFQ.UseCases.Events
{
    // Eventos de dominio que la presentación escucha (UI, audio, VFX del cuaderno).

    public readonly struct SignalSaved
    {
        public SignalSaved(string signalId, int total) { SignalId = signalId; Total = total; }
        public string SignalId { get; }
        public int Total { get; }
    }

    public readonly struct GestureNoticed
    {
        public GestureNoticed(string gestureId, int total) { GestureId = gestureId; Total = total; }
        public string GestureId { get; }
        public int Total { get; }
    }

    public readonly struct LensApplied
    {
        public LensApplied(string momentId, LensReading reading, int total) { MomentId = momentId; Reading = reading; Total = total; }
        public string MomentId { get; }
        public LensReading Reading { get; }
        public int Total { get; }
    }

    public readonly struct DadMemoryFound
    {
        public DadMemoryFound(string memoryId, int total) { MemoryId = memoryId; Total = total; }
        public string MemoryId { get; }
        public int Total { get; }
    }

    public readonly struct ChapterStarted
    {
        public ChapterStarted(Chapter chapter) { Chapter = chapter; }
        public Chapter Chapter { get; }
    }

    public readonly struct DecisionTaken
    {
        public DecisionTaken(string variable, string value) { Variable = variable; Value = value; }
        public string Variable { get; }
        public string Value { get; }
    }
}
