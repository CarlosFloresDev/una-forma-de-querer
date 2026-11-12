using System;
using System.Collections.Generic;

namespace UFQ.Core.Narrative
{
    /// <summary>
    /// Estado narrativo completo de una partida (tabla de variables, sección 4.6 del guion).
    /// Es la entidad del dominio: aplica las reglas (ids válidos, idempotencia, topes)
    /// y no sabe nada de Unity, Ink ni de cómo se guarda.
    /// </summary>
    public sealed class NarrativeState : IReadOnlyNarrativeState
    {
        private readonly HashSet<string> _savedSignals = new HashSet<string>();
        private readonly HashSet<string> _notedGestures = new HashSet<string>();
        private readonly HashSet<string> _dadMemories = new HashSet<string>();
        private readonly HashSet<string> _lensMoments = new HashSet<string>();
        private readonly HashSet<string> _chapter5Saves = new HashSet<string>();
        private readonly HashSet<string> _storyMarks = new HashSet<string>();

        public Chapter CurrentChapter { get; private set; } = Chapter.Prologue;
        public DecisionC3 DecisionC3 { get; private set; }
        public NicoResponse NicoResponse { get; private set; }
        public FinalMessage FinalMessage { get; private set; }
        public NewYearWish NewYearWish { get; private set; }
        public bool HonestWithValeria { get; private set; }
        public bool HonestWithValeriaDecided { get; private set; }
        public double WaitSecondsTotal { get; private set; }

        /// <summary>SEÑALES (0–17).</summary>
        public int Signals => _savedSignals.Count;

        /// <summary>LENTE (0–4): veces que se eligió «algo más».</summary>
        public int Lens => _lensMoments.Count;

        /// <summary>VALE (0–7).</summary>
        public int Vale => _notedGestures.Count;

        /// <summary>GUARDAR_C5 (0–3).</summary>
        public int SavesInChapter5 => _chapter5Saves.Count;

        /// <summary>RECUERDOS_PAPA (0–6).</summary>
        public int DadMemoriesFound => _dadMemories.Count;

        public IReadOnlyCollection<string> SavedSignalIds => _savedSignals;
        public IReadOnlyCollection<string> NotedGestureIds => _notedGestures;
        public IReadOnlyCollection<string> DadMemoryIds => _dadMemories;
        public IReadOnlyCollection<string> LensMomentIds => _lensMoments;
        public IReadOnlyCollection<string> Chapter5SaveIds => _chapter5Saves;
        public IReadOnlyCollection<string> StoryMarkIds => _storyMarks;

        public bool HasSavedSignal(string id) => _savedSignals.Contains(id);
        public bool HasNotedGesture(string id) => _notedGestures.Contains(id);
        public bool HasStoryMark(string id) => _storyMarks.Contains(id);

        /// <summary>Deja una marca de historia para escenas posteriores. false si ya estaba.</summary>
        public bool Mark(string markId)
        {
            Require(NarrativeCatalog.IsStoryMark(markId), $"Marca de historia desconocida: {markId}");
            return _storyMarks.Add(markId);
        }

        /// <summary>Guarda una señal dorada. Devuelve false si ya estaba guardada.</summary>
        public bool SaveSignal(string signalId)
        {
            Require(NarrativeCatalog.IsSignal(signalId), $"Señal desconocida: {signalId}");
            return _savedSignals.Add(signalId);
        }

        /// <summary>Registra un gesto de Valeria notado. Devuelve false si ya contaba.</summary>
        public bool NoticeGesture(string gestureId)
        {
            Require(NarrativeCatalog.IsValeriaGesture(gestureId), $"Gesto desconocido: {gestureId}");
            return _notedGestures.Add(gestureId);
        }

        public bool FindDadMemory(string memoryId)
        {
            Require(NarrativeCatalog.IsDadMemory(memoryId), $"Recuerdo desconocido: {memoryId}");
            return _dadMemories.Add(memoryId);
        }

        /// <summary>
        /// Lente: cada momento (p. ej. "2.5b") cuenta una sola vez y solo si se lee como «algo más».
        /// En 3.10 la Lente ya no funciona: el llamador no debe enviarlo como momento válido.
        /// </summary>
        public bool ApplyLens(string momentId, LensReading reading)
        {
            Require(!string.IsNullOrWhiteSpace(momentId), "Momento de Lente vacío");
            if (reading != LensReading.SomethingMore || _lensMoments.Count >= NarrativeCatalog.MaxLens)
            {
                return false;
            }

            return _lensMoments.Add(momentId);
        }

        public void SetNicoResponse(NicoResponse response)
        {
            RequireDefined(response, NicoResponse.None, "Respuesta de Nico inválida");
            NicoResponse = response;
        }

        public void SetNewYearWish(NewYearWish wish)
        {
            RequireDefined(wish, NewYearWish.None, "Deseo inválido");
            NewYearWish = wish;
        }

        /// <summary>Decisión principal del Capítulo 3. Solo se toma una vez.</summary>
        public void DecideC3(DecisionC3 decision)
        {
            RequireDefined(decision, DecisionC3.None, "Decisión C3 inválida");
            Require(DecisionC3 == DecisionC3.None, "La decisión del Capítulo 3 ya se tomó");
            DecisionC3 = decision;
        }

        public void DecideHonestyWithValeria(bool honest)
        {
            Require(!HonestWithValeriaDecided, "La conversación con Valeria (5.1) ya ocurrió");
            HonestWithValeria = honest;
            HonestWithValeriaDecided = true;
        }

        /// <summary>Volver a guardar un gesto de Abril en 5.2 (prueba silenciosa del final C).</summary>
        public bool SaveInChapter5(string gestureId)
        {
            Require(NarrativeCatalog.IsChapter5Gesture(gestureId), $"Gesto del Cap. 5 desconocido: {gestureId}");
            return _chapter5Saves.Add(gestureId);
        }

        public void DecideFinalMessage(FinalMessage message)
        {
            RequireDefined(message, FinalMessage.None, "Mensaje final inválido");
            FinalMessage = message;
        }

        public void AddWaitTime(double seconds)
        {
            Require(seconds >= 0 && !double.IsNaN(seconds) && !double.IsInfinity(seconds), "Tiempo de espera inválido");
            WaitSecondsTotal += seconds;
        }

        /// <summary>Los capítulos solo avanzan; no se puede retroceder sin reiniciar.</summary>
        public void AdvanceTo(Chapter chapter)
        {
            Require(Enum.IsDefined(typeof(Chapter), chapter), $"Capítulo inexistente: {(int)chapter}");
            Require(chapter >= CurrentChapter, $"No se puede volver de {CurrentChapter} a {chapter}");
            CurrentChapter = chapter;
        }

        /// <summary>Reinicio desde el Capítulo 4 («¿Volver a mirar?», final C).</summary>
        public void RewindToChapter4()
        {
            CurrentChapter = Chapter.Chapter4OtraMirada;
            _chapter5Saves.Clear();
            FinalMessage = FinalMessage.None;
            HonestWithValeria = false;
            HonestWithValeriaDecided = false;
        }

        /// <summary>El valor debe ser un miembro real del enum (no un número suelto) y distinto de None.</summary>
        private static void RequireDefined<T>(T value, T none, string message) where T : struct, Enum
        {
            Require(Enum.IsDefined(typeof(T), value) && !value.Equals(none), $"{message}: {value}");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
            {
                throw new NarrativeRuleException(message);
            }
        }
    }

    public sealed class NarrativeRuleException : InvalidOperationException
    {
        public NarrativeRuleException(string message) : base(message)
        {
        }
    }
}
