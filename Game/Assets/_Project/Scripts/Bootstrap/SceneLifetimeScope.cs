using System.Collections.Generic;
using UFQ.Presentation;
using UFQ.Presentation.Dialogue;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace UFQ.Bootstrap
{
    /// <summary>
    /// Ámbito de UNA escena (un capítulo o un lugar). Hereda los servicios del
    /// GameLifetimeScope raíz e inyecta solo los objetos de SU escena, así funciona
    /// igual con escenas cargadas después o de forma aditiva.
    ///
    /// Si la escena tiene una DialogueView, registra también el DialogueRunner.
    /// Para objetos creados en tiempo de juego: <c>Container.Instantiate(prefab)</c>.
    /// </summary>
    public class SceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            var view = FindInSceneOrNull<DialogueView>();
            if (view != null)
            {
                builder.RegisterComponent(view).As<IDialogueView>();
                builder.Register<DialogueRunner>(Lifetime.Scoped);
            }

            builder.RegisterBuildCallback(InjectSceneObjects);
            ConfigureScene(builder);
        }

        /// <summary>Punto de extensión para servicios propios de un capítulo.</summary>
        protected virtual void ConfigureScene(IContainerBuilder builder)
        {
        }

        /// <summary>Primer componente T dentro de ESTA escena (no busca en otras).</summary>
        protected T FindInSceneOrNull<T>() where T : Component
        {
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                var found = root.GetComponentInChildren<T>(true);
                if (found != null) return found;
            }

            return null;
        }

        private void InjectSceneObjects(IObjectResolver container)
        {
            var buffer = new List<MonoBehaviour>();
            foreach (var root in gameObject.scene.GetRootGameObjects())
            {
                buffer.Clear();
                root.GetComponentsInChildren(true, buffer);
                foreach (var behaviour in buffer)
                {
                    if (behaviour is ISceneInjectable)
                    {
                        container.Inject(behaviour);
                    }
                }
            }
        }
    }
}
