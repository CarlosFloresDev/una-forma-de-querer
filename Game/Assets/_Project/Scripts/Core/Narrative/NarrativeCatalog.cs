using System.Collections.Generic;

namespace UFQ.Core.Narrative
{
    /// <summary>
    /// Identificadores canónicos del guion (Anexos C y D). Son la única fuente de verdad
    /// para escenas, datos de Ink y pruebas: si cambia el guion, cambia aquí.
    /// </summary>
    public static class NarrativeCatalog
    {
        public const int MaxLens = 4;
        public const int SecretEndingValeThreshold = 5;
        public const int InfiniteNotebookThreshold = 3;

        /// <summary>17 señales de Abril (S01–S17), en orden de aparición.</summary>
        public static readonly IReadOnlyList<string> Signals = new[]
        {
            "S01", "S02", "S03", "S04", "S05", "S06", "S07", "S08", "S09",
            "S10", "S11", "S12", "S13", "S14", "S15", "S16", "S17",
        };

        /// <summary>7 gestos de Valeria (V1–V7). No brillan; suman VALE al notarlos.</summary>
        public static readonly IReadOnlyList<string> ValeriaGestures = new[]
        {
            "V1", "V2", "V3", "V4", "V5", "V6", "V7",
        };

        /// <summary>6 recuerdos de papá (coleccionables).</summary>
        public static readonly IReadOnlyList<string> DadMemories = new[]
        {
            "M1_Foto", "M2_CastilloTallado", "M3_CartelTaller",
            "M4_BoletoChivay", "M5_RetratoRosa", "M6_Silla",
        };

        /// <summary>Gestos de Abril en 5.2 que se pueden volver a «guardar» (GUARDAR_C5).</summary>
        public static readonly IReadOnlyList<string> Chapter5Gestures = new[]
        {
            "C5_Chalina", "C5_QuesoHelado", "C5_TeVoyAExtranar",
        };

        private static readonly HashSet<string> SignalSet = new HashSet<string>(Signals);
        private static readonly HashSet<string> GestureSet = new HashSet<string>(ValeriaGestures);
        private static readonly HashSet<string> MemorySet = new HashSet<string>(DadMemories);
        private static readonly HashSet<string> C5Set = new HashSet<string>(Chapter5Gestures);

        public static bool IsSignal(string id) => id != null && SignalSet.Contains(id);
        public static bool IsValeriaGesture(string id) => id != null && GestureSet.Contains(id);
        public static bool IsDadMemory(string id) => id != null && MemorySet.Contains(id);
        public static bool IsChapter5Gesture(string id) => id != null && C5Set.Contains(id);
    }
}
