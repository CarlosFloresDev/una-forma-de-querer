using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFQ.Presentation.Locations
{
    /// <summary>
    /// Uno por escena de lugar: dónde aparece Mateo (según la escena del guion) y el
    /// color de fondo del cielo para la cámara persistente.
    /// </summary>
    public sealed class LocationAnchor : MonoBehaviour
    {
        [Serializable]
        public struct Spawn
        {
            public string scriptScene;
            public Transform point;
        }

        [SerializeField] private Transform defaultSpawn;
        [SerializeField] private List<Spawn> spawns = new List<Spawn>();
        [SerializeField] private Color skyColor = new Color(0.55f, 0.72f, 0.92f);
        [SerializeField] private bool playerVisible = true;

        public Color SkyColor => skyColor;
        public bool PlayerVisible => playerVisible;

        public Transform SpawnFor(string scriptScene)
        {
            foreach (var spawn in spawns)
            {
                if (spawn.point != null && string.Equals(spawn.scriptScene, scriptScene, StringComparison.OrdinalIgnoreCase))
                    return spawn.point;
            }

            return defaultSpawn != null ? defaultSpawn : transform;
        }

#if UNITY_EDITOR
        public void Configure(Transform fallback, Color sky, IEnumerable<Spawn> extra, bool showPlayer = true)
        {
            defaultSpawn = fallback;
            skyColor = sky;
            spawns = extra != null ? new List<Spawn>(extra) : new List<Spawn>();
            playerVisible = showPlayer;
        }
#endif
    }
}
