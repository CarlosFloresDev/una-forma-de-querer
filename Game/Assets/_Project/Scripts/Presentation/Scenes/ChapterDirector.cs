using Cysharp.Threading.Tasks;
using UFQ.Core.Narrative;
using UFQ.Presentation.Dialogue;
using UFQ.UseCases.Narrative;
using UnityEngine;
using VContainer;

namespace UFQ.Presentation.Scenes
{
    /// <summary>
    /// Arranca la historia de una escena (p. ej. el Prólogo en la azotea).
    /// Marca el capítulo en el dominio y entrega el control al DialogueRunner.
    /// </summary>
    public sealed class ChapterDirector : MonoBehaviour, ISceneInjectable
    {
        [SerializeField] private string storyId = "Prologo";
        [SerializeField] private Chapter chapter = Chapter.Prologue;
        [SerializeField] private bool playOnStart = true;

        private DialogueRunner _runner;
        private NarrativeService _narrative;

        [Inject]
        public void Construct(DialogueRunner runner, NarrativeService narrative)
        {
            _runner = runner;
            _narrative = narrative;
        }

        private void Start()
        {
            if (playOnStart) Play();
        }

        public void Play()
        {
            if (_runner == null)
            {
                Debug.LogError("[UFQ] ChapterDirector sin dependencias: ¿hay un SceneLifetimeScope en la escena?", this);
                return;
            }

            if (_narrative.State.CurrentChapter <= chapter) _narrative.StartChapter(chapter);
            _runner.PlayAsync(storyId, destroyCancellationToken).Forget();
        }
    }
}
