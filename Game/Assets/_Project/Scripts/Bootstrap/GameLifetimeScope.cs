using UFQ.Infrastructure.Dialogue;
using UFQ.Infrastructure.Persistence;
using UFQ.Presentation.Input;
using UFQ.UseCases.Dialogue;
using UFQ.UseCases.Events;
using UFQ.UseCases.Narrative;
using UFQ.UseCases.Persistence;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UFQ.Bootstrap
{
    /// <summary>
    /// Ámbito RAÍZ del juego. Registra los servicios que viven toda la partida.
    /// Es el «Root Lifetime Scope» de VContainerSettings (precargado), así cada
    /// SceneLifetimeScope de capítulo lo toma como padre automáticamente.
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private string saveFileName = "partida.json";
        [SerializeField] private InkStoryLibrary storyLibrary;

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);
            builder.RegisterInstance<INarrativeRepository>(JsonNarrativeRepository.InPersistentData(saveFileName));
            builder.Register<NarrativeService>(Lifetime.Singleton);
            builder.Register<GameInput>(Lifetime.Singleton);

            if (storyLibrary != null)
            {
                builder.RegisterInstance(storyLibrary);
                builder.Register<IDialogueStoryFactory, InkDialogueStoryFactory>(Lifetime.Singleton);
            }
            else
            {
                Debug.LogError("[UFQ] GameLifetimeScope sin InkStoryLibrary: no habrá diálogos.", this);
            }
        }
    }
}
