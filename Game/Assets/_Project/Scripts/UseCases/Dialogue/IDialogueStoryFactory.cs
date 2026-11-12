namespace UFQ.UseCases.Dialogue
{
    /// <summary>
    /// Puerto: crea una historia lista para recorrer a partir de su id
    /// («Prologo», «Capitulo1»…). La infraestructura decide de dónde sale (Ink).
    /// </summary>
    public interface IDialogueStoryFactory
    {
        bool Exists(string storyId);
        IDialogueStory Create(string storyId);
    }
}
