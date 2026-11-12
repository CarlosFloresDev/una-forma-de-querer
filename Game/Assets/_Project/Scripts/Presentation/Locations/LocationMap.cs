using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFQ.Presentation.Locations
{
    /// <summary>
    /// Datos: qué escena de Unity corresponde a cada escena del guion
    /// («1.2» → «Libreria»). La etiqueta de Ink #escena:1.2 la usa para cargar el lugar.
    /// </summary>
    [CreateAssetMenu(menuName = "Una forma de querer/Mapa de lugares", fileName = "LocationMap")]
    public sealed class LocationMap : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [Tooltip("Id de escena del guion: P.1, 1.2, 1.6b…")]
            public string scriptScene;

            [Tooltip("Nombre de la escena de Unity del lugar (sin .unity)")]
            public string unityScene;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        /// <summary>
        /// Busca la escena exacta («1.6b»); si no existe, prueba sin las letras finales
        /// («1.6»), para que las sub-escenas hereden el lugar de su escena.
        /// </summary>
        public bool TryGetUnityScene(string scriptScene, out string unityScene)
        {
            if (Find(scriptScene, out unityScene)) return true;
            var trimmed = scriptScene?.TrimEnd('a', 'b', 'c', 'd', 'm', 'p') ?? string.Empty;
            return trimmed != scriptScene && Find(trimmed, out unityScene);
        }

        private bool Find(string id, out string unityScene)
        {
            foreach (var entry in entries)
            {
                if (string.Equals(entry.scriptScene, id, StringComparison.OrdinalIgnoreCase))
                {
                    unityScene = entry.unityScene;
                    return true;
                }
            }

            unityScene = null;
            return false;
        }

#if UNITY_EDITOR
        public void SetEntries(IEnumerable<Entry> newEntries) => entries = new List<Entry>(newEntries);
#endif
    }
}
