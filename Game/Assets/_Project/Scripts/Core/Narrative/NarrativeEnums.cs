namespace UFQ.Core.Narrative
{
    /// <summary>Capítulos del guion (sección 9, cronología).</summary>
    public enum Chapter
    {
        Prologue = 0,
        Chapter1Arequipa = 1,
        Chapter2Mollendo = 2,
        Chapter3Lima = 3,
        Chapter4OtraMirada = 4,
        Chapter5AprenderAMirar = 5,
        EpilogueColca = 6,
    }

    /// <summary>DECISION_C3: escena 3.10.</summary>
    public enum DecisionC3
    {
        None = 0,
        Confess = 1,
        StaySilent = 2,
        HangUp = 3,
    }

    /// <summary>NICO_RESP: escena 2.4.</summary>
    public enum NicoResponse
    {
        None = 0,
        Anger = 1,
        Denial = 2,
        Accept = 3,
    }

    /// <summary>MENSAJE: escena 5.3.</summary>
    public enum FinalMessage
    {
        None = 0,
        Sent = 1,
        Deleted = 2,
    }

    /// <summary>Deseo de Año Nuevo (1.11), recordado en E.4.</summary>
    public enum NewYearWish
    {
        None = 0,
        ThatThisLasts = 1,
        DrawWithoutFear = 2,
        ThatSheUnfinished = 3,
    }

    /// <summary>Lectura elegida en la Lente.</summary>
    public enum LensReading
    {
        Friendship = 0,
        SomethingMore = 1,
    }

    /// <summary>Finales del Anexo A.</summary>
    public enum Ending
    {
        A_UnaFormaDeQuerer = 0,
        B_Distancia = 1,
        C_CuadernoInfinito = 2,
    }
}
