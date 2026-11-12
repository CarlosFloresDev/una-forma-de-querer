using System.IO;
using UnityEditor;
using UnityEngine;

namespace UFQ.Editor.Setup
{
    /// <summary>Herramientas de desarrollo (no van en el juego final).</summary>
    internal static class DevTools
    {
        [MenuItem("Una forma de querer/Desarrollo/Borrar partida guardada")]
        private static void DeleteSave()
        {
            var folder = Application.persistentDataPath;
            var deleted = 0;
            foreach (var file in Directory.GetFiles(folder, "partida.json*"))
            {
                File.Delete(file);
                deleted++;
            }

            Debug.Log($"[UFQ] Partida borrada ({deleted} archivo(s)) en {folder}");
        }

        [MenuItem("Una forma de querer/Desarrollo/Abrir carpeta de partidas")]
        private static void OpenSaveFolder() => EditorUtility.RevealInFinder(Application.persistentDataPath);
    }
}
