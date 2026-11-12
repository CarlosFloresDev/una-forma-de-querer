using System.Collections.Generic;

namespace UFQ.Core.Narrative
{
    /// <summary>
    /// Vista de solo lectura del estado narrativo. Es lo único que reciben la UI, Ink y
    /// los resolvers: para cambiar algo hay que pasar por NarrativeService.
    /// </summary>
    public interface IReadOnlyNarrativeState
    {
        Chapter CurrentChapter { get; }
        DecisionC3 DecisionC3 { get; }
        NicoResponse NicoResponse { get; }
        FinalMessage FinalMessage { get; }
        NewYearWish NewYearWish { get; }
        bool HonestWithValeria { get; }
        bool HonestWithValeriaDecided { get; }
        double WaitSecondsTotal { get; }

        int Signals { get; }
        int Lens { get; }
        int Vale { get; }
        int SavesInChapter5 { get; }
        int DadMemoriesFound { get; }

        IReadOnlyCollection<string> SavedSignalIds { get; }
        IReadOnlyCollection<string> NotedGestureIds { get; }
        IReadOnlyCollection<string> DadMemoryIds { get; }
        IReadOnlyCollection<string> LensMomentIds { get; }
        IReadOnlyCollection<string> Chapter5SaveIds { get; }
        IReadOnlyCollection<string> StoryMarkIds { get; }

        bool HasSavedSignal(string id);
        bool HasNotedGesture(string id);
        bool HasStoryMark(string id);
    }
}
