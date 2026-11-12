using UFQ.Presentation.Dialogue;
using UFQ.Presentation.Locations;
using UFQ.Presentation.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UFQ.Bootstrap
{
    /// <summary>
    /// Ámbito de la escena persistente «Juego» (UI, cámara, Mateo, director).
    /// Además de lo de cualquier escena, registra el cargador de lugares, que el
    /// DialogueRunner espera cuando Ink pide «#escena:…».
    ///
    /// Nota: VContainerSettings tiene DisableScriptModifier = true; si no, VContainer
    /// reescribe los scripts nuevos que heredan de LifetimeScope con una plantilla vacía.
    /// </summary>
    public sealed class GameplayLifetimeScope : SceneLifetimeScope
    {
        [SerializeField] private LocationMap locationMap;

        protected override void ConfigureScene(IContainerBuilder builder)
        {
            var fader = FindInSceneOrNull<ScreenFader>();
            var player = FindInSceneOrNull<PlayerMover>();
            if (locationMap == null || fader == null || player == null)
            {
                Debug.LogError("[UFQ] GameplayLifetimeScope necesita LocationMap, ScreenFader y PlayerMover en la escena.", this);
                return;
            }

            builder.RegisterInstance(locationMap);
            builder.RegisterComponent(fader);
            builder.RegisterComponent(player);
            builder.Register<LocationLoader>(Lifetime.Scoped).As<IDialogueCueHandler>().AsSelf();
        }
    }
}
