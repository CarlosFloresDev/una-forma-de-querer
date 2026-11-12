using System;
using System.Collections.Generic;
using Ink.UnityIntegration;
using UnityEngine;

namespace UFQ.Infrastructure.Dialogue
{
    /// <summary>
    /// Asset de datos: enlaza cada id de historia («Prologo», «Capitulo1»…) con su .ink.
    /// Añadir un capítulo = añadir una fila aquí; ningún código cambia.
    /// </summary>
    [CreateAssetMenu(menuName = "Una forma de querer/Biblioteca de historias", fileName = "InkStoryLibrary")]
    public sealed class InkStoryLibrary : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string id;
            public InkFile file;
        }

        [SerializeField] private List<Entry> stories = new List<Entry>();

        public IReadOnlyList<Entry> Stories => stories;

        public bool TryGetJson(string id, out string json)
        {
            foreach (var entry in stories)
            {
                if (entry.id == id && entry.file != null && !string.IsNullOrEmpty(entry.file.storyJson))
                {
                    json = entry.file.storyJson;
                    return true;
                }
            }

            json = null;
            return false;
        }

#if UNITY_EDITOR
        /// <summary>Uso exclusivo del editor (constructores de escena y pruebas).</summary>
        public void SetEntries(IEnumerable<Entry> entries)
        {
            stories = new List<Entry>(entries);
        }
#endif
    }
}
