using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UFQ.Core.Narrative;
using UFQ.Presentation.Dialogue;
using UFQ.UseCases.Narrative;
using UnityEngine;
using VContainer;

namespace UFQ.Presentation.Scenes
{
    /// <summary>
    /// Dirige la partida: reproduce las historias de cada capítulo en orden
    /// (Prólogo → Capítulo 1 → …) y retoma desde el capítulo guardado.
    /// Cada historia de Ink lleva a Mateo de un lugar a otro con #escena.
    /// </summary>
    public sealed class ChapterDirector : MonoBehaviour, ISceneInjectable
    {
        [Serializable]
        public struct Step
        {
            public Chapter chapter;
            public string storyId;
        }

        [SerializeField] private List<Step> steps = new List<Step>
        {
            new Step { chapter = Chapter.Prologue, storyId = "Prologo" },
            new Step { chapter = Chapter.Chapter1Arequipa, storyId = "Capitulo1" },
            new Step { chapter = Chapter.Chapter2Mollendo, storyId = "Capitulo2" },
            new Step { chapter = Chapter.Chapter3Lima, storyId = "Capitulo3" },
        };

        [SerializeField] private bool playOnStart = true;

        private DialogueRunner _runner;
        private NarrativeService _narrative;

        public event Action<Chapter> StepFinished;

        [Inject]
        public void Construct(DialogueRunner runner, NarrativeService narrative)
        {
            _runner = runner;
            _narrative = narrative;
        }

        private void Start()
        {
            if (playOnStart) PlayFromSave();
        }

        public void PlayFromSave()
        {
            if (_runner == null)
            {
                Debug.LogError("[UFQ] ChapterDirector sin dependencias: ¿hay un SceneLifetimeScope en la escena?", this);
                return;
            }

            RunAsync(destroyCancellationToken).Forget();
        }

        private async UniTaskVoid RunAsync(CancellationToken token)
        {
            foreach (var step in steps)
            {
                // Los capítulos ya superados no se repiten al cargar una partida.
                if (step.chapter < _narrative.State.CurrentChapter) continue;

                if (step.chapter > _narrative.State.CurrentChapter) _narrative.StartChapter(step.chapter);
                await _runner.PlayAsync(step.storyId, token);
                StepFinished?.Invoke(step.chapter);
            }
        }
    }
}
