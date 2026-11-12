using System.IO;
using UnityEditor;
using UnityEngine;

namespace UFQ.Editor
{
    /// <summary>
    /// Ajustes de ESTA máquina (C: casi lleno): manda la caché de iluminación global a D:
    /// y usa Rider como editor de código. Solo actúa si las rutas existen, así que en otra
    /// PC no hace nada. Se puede volver a aplicar desde el menú.
    /// </summary>
    [InitializeOnLoad]
    internal static class LocalEnvironmentSetup
    {
        private const string GiCacheFolder = @"D:\DevTools\Unity\Cache\GiCache";
        private const string RiderPath = @"D:\jetbrains\JetBrains Rider 2026.2.1\bin\rider64.exe";
        private const string AppliedKey = "UFQ.LocalEnvironmentSetup.v1";

        static LocalEnvironmentSetup()
        {
            if (!EditorPrefs.GetBool(AppliedKey, false)) Apply();
        }

        [MenuItem("Una forma de querer/Configurar entorno local (caché en D:, Rider)")]
        private static void Apply()
        {
            if (Directory.Exists(@"D:\DevTools\Unity"))
            {
                Directory.CreateDirectory(GiCacheFolder);
                EditorPrefs.SetBool("GICacheEnableCustomPath", true);
                EditorPrefs.SetString("GICacheFolder", GiCacheFolder);
                Debug.Log($"[UFQ] Caché GI en {GiCacheFolder}. Compruébalo en Preferences > GI Cache.");
            }

            if (File.Exists(RiderPath))
            {
                EditorPrefs.SetString("kScriptsDefaultApp", RiderPath);
                Debug.Log("[UFQ] Rider configurado como editor de scripts.");
            }

            EditorPrefs.SetBool(AppliedKey, true);
        }
    }
}
