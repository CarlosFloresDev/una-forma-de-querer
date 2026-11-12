using System.Collections.Generic;
using UnityEditor;

namespace UFQ.Editor.Scenes
{
    /// <summary>
    /// Mantiene Build Settings al día: la escena persistente «Juego» siempre en el
    /// índice 0 y cada lugar construido añadido una sola vez.
    /// </summary>
    internal static class BuildSettingsSync
    {
        internal const string GameScenePath = "Assets/_Project/Scenes/Boot/Juego.unity";

        internal static void EnsureScene(string path)
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == path || string.IsNullOrEmpty(s.path) || !System.IO.File.Exists(s.path) && s.path != path);
            scenes.Add(new EditorBuildSettingsScene(path, true));

            // «Juego» primero; el resto en orden alfabético para que el archivo sea estable.
            scenes.Sort((a, b) =>
            {
                if (a.path == GameScenePath) return -1;
                if (b.path == GameScenePath) return 1;
                return string.CompareOrdinal(a.path, b.path);
            });

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
