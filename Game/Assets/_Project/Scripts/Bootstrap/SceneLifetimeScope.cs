using System.Collections.Generic;
using UFQ.Presentation;
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
    /// Para objetos que se crean en tiempo de juego, instanciar con
    /// <c>Container.Instantiate(prefab)</c> para que también reciban sus dependencias.
    /// </summary>
    public class SceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterBuildCallback(InjectSceneObjects);
            ConfigureScene(builder);
        }

        /// <summary>Punto de extensión para servicios propios de un capítulo.</summary>
        protected virtual void ConfigureScene(IContainerBuilder builder)
        {
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
