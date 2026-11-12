namespace UFQ.Core.Narrative
{
    public readonly struct EndingResult
    {
        public EndingResult(Ending ending, bool includesSecretCodaD)
        {
            Ending = ending;
            IncludesSecretCodaD = includesSecretCodaD;
        }

        public Ending Ending { get; }

        /// <summary>Coda D «Cosas que sí dije», que se añade tras la coda A.</summary>
        public bool IncludesSecretCodaD { get; }

        public override string ToString() => IncludesSecretCodaD ? $"{Ending}+D" : Ending.ToString();
    }

    /// <summary>Árbol de decisión del Anexo A, evaluado en orden.</summary>
    public static class EndingResolver
    {
        public static EndingResult Resolve(NarrativeState state)
        {
            if (state.SavesInChapter5 >= NarrativeCatalog.InfiniteNotebookThreshold)
            {
                return new EndingResult(Ending.C_CuadernoInfinito, false);
            }

            if (state.FinalMessage == FinalMessage.Deleted)
            {
                return new EndingResult(Ending.B_Distancia, false);
            }

            if (state.FinalMessage != FinalMessage.Sent)
            {
                throw new NarrativeRuleException("No se puede resolver el final sin la decisión del mensaje (5.3)");
            }

            var codaD = state.HonestWithValeria && state.Vale >= NarrativeCatalog.SecretEndingValeThreshold;
            return new EndingResult(Ending.A_UnaFormaDeQuerer, codaD);
        }

        /// <summary>El Epílogo se corta en E.1 si el jugador sigue guardando (final C).</summary>
        public static bool EpilogueIsCutShort(NarrativeState state) =>
            state.SavesInChapter5 >= NarrativeCatalog.InfiniteNotebookThreshold;

        /// <summary>La última página del cuaderno de Andrés (E.3) se ve nítida con los 6 recuerdos.</summary>
        public static bool FathersLastPageUnlocked(NarrativeState state) =>
            state.DadMemoriesFound == NarrativeCatalog.DadMemories.Count;
    }
}
