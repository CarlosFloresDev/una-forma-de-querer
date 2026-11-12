using UFQ.Infrastructure.Persistence;
using UFQ.Presentation.Interaction;
using UFQ.UseCases.Events;
using UFQ.UseCases.Narrative;
using UFQ.UseCases.Persistence;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UFQ.Bootstrap
{
    /// <summary>
    /// Raíz de composición: el ÚNICO lugar que conoce todas las implementaciones.
    /// Vive en la escena Boot y persiste entre escenas; cada capítulo puede tener
    /// un LifetimeScope hijo con sus propios servicios.
    /// </summary>
    public sealed class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private string saveFileName = "partida.json";

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IEventBus, EventBus>(Lifetime.Singleton);
            builder.RegisterInstance<INarrativeRepository>(JsonNarrativeRepository.InPersistentData(saveFileName));
            builder.Register<NarrativeService>(Lifetime.Singleton);

            // Los objetos interactivos de la escena reciben sus dependencias al arrancar.
            builder.RegisterBuildCallback(container =>
            {
                foreach (var s in FindObjectsByType<SignalInteractable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    container.Inject(s);
                foreach (var g in FindObjectsByType<ValeriaGestureObservable>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                    container.Inject(g);
            });
        }
    }
}
