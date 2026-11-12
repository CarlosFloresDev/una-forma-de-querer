using UFQ.Infrastructure.Persistence;
using UFQ.UseCases.Events;
using UFQ.UseCases.Narrative;
using UFQ.UseCases.Persistence;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UFQ.Bootstrap
{
    /// <summary>
    /// Ámbito RAÍZ del juego (escena Boot). Registra los servicios que viven toda la
    /// partida. Configúralo como «Root Lifetime Scope» en el asset VContainerSettings
    /// para que cada SceneLifetimeScope de capítulo lo tome como padre automáticamente.
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private string saveFileName = "partida.json";

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
        }
    }
}
