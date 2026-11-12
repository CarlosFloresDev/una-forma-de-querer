using System;
using System.Collections.Generic;
using UFQ.Core.Narrative;

namespace UFQ.UseCases.Persistence
{
    /// <summary>
    /// DTO plano y serializable del estado narrativo. Los campos públicos permiten usar
    /// JsonUtility de Unity sin que el dominio dependa de él.
    /// </summary>
    [Serializable]
    public sealed class NarrativeSnapshot
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public int chapter;
        public int decisionC3;
        public int nicoResponse;
        public int finalMessage;
        public int newYearWish;
        public bool honestWithValeria;
        public bool honestWithValeriaDecided;
        public double waitSecondsTotal;
        public List<string> savedSignals = new List<string>();
        public List<string> notedGestures = new List<string>();
        public List<string> dadMemories = new List<string>();
        public List<string> lensMoments = new List<string>();
        public List<string> chapter5Saves = new List<string>();

        public static NarrativeSnapshot From(NarrativeState s) => new NarrativeSnapshot
        {
            chapter = (int)s.CurrentChapter,
            decisionC3 = (int)s.DecisionC3,
            nicoResponse = (int)s.NicoResponse,
            finalMessage = (int)s.FinalMessage,
            newYearWish = (int)s.NewYearWish,
            honestWithValeria = s.HonestWithValeria,
            honestWithValeriaDecided = s.HonestWithValeriaDecided,
            waitSecondsTotal = s.WaitSecondsTotal,
            savedSignals = new List<string>(s.SavedSignalIds),
            notedGestures = new List<string>(s.NotedGestureIds),
            dadMemories = new List<string>(s.DadMemoryIds),
            lensMoments = new List<string>(s.LensMomentIds),
            chapter5Saves = new List<string>(s.Chapter5SaveIds),
        };

        /// <summary>Reconstruye el estado pasando por las reglas del dominio (valida ids).</summary>
        public NarrativeState ToState()
        {
            if (version > CurrentVersion)
            {
                throw new NotSupportedException($"Partida guardada con una versión más nueva ({version})");
            }

            var s = new NarrativeState();
            s.AdvanceTo((Chapter)chapter);
            foreach (var id in savedSignals) s.SaveSignal(id);
            foreach (var id in notedGestures) s.NoticeGesture(id);
            foreach (var id in dadMemories) s.FindDadMemory(id);
            foreach (var id in lensMoments) s.ApplyLens(id, LensReading.SomethingMore);
            foreach (var id in chapter5Saves) s.SaveInChapter5(id);
            if (decisionC3 != 0) s.DecideC3((DecisionC3)decisionC3);
            if (nicoResponse != 0) s.SetNicoResponse((NicoResponse)nicoResponse);
            if (finalMessage != 0) s.DecideFinalMessage((FinalMessage)finalMessage);
            if (newYearWish != 0) s.SetNewYearWish((NewYearWish)newYearWish);
            if (honestWithValeriaDecided) s.DecideHonestyWithValeria(honestWithValeria);
            if (waitSecondsTotal > 0) s.AddWaitTime(waitSecondsTotal);
            return s;
        }
    }

    /// <summary>Puerto de persistencia. La infraestructura decide dónde y cómo se guarda.</summary>
    public interface INarrativeRepository
    {
        bool TryLoad(out NarrativeSnapshot snapshot);
        void Save(NarrativeSnapshot snapshot);
        void Delete();
    }
}
