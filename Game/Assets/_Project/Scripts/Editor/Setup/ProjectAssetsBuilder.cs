using System.Collections.Generic;
using System.IO;
using Ink.UnityIntegration;
using UFQ.Bootstrap;
using UFQ.Infrastructure.Dialogue;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace UFQ.Editor.Setup
{
    /// <summary>
    /// Crea o actualiza los assets de configuración compartidos:
    ///  · InkStoryLibrary (todas las historias de Narrative/Ink que son «master»)
    ///  · PanelSettings de UI Toolkit (escala a 1920×1080) con el tema del juego
    ///  · Enlaza la biblioteca en el prefab raíz GameLifetimeScope
    /// Es idempotente: se puede ejecutar las veces que haga falta.
    /// </summary>
    internal static class ProjectAssetsBuilder
    {
        internal const string LibraryPath = "Assets/_Project/Settings/InkStoryLibrary.asset";
        internal const string PanelSettingsPath = "Assets/_Project/UI/PanelSettings_Juego.asset";
        private const string ThemePath = "Assets/_Project/UI/Theme/TemaJuego.tss";
        private const string InkFolder = "Assets/_Project/Narrative/Ink";
        private const string RootScopePrefab = "Assets/_Project/Prefabs/Core/GameLifetimeScope.prefab";

        [MenuItem("Una forma de querer/Configuración/Actualizar assets del proyecto")]
        internal static void BuildAll()
        {
            var library = BuildStoryLibrary();
            BuildPanelSettings();
            LinkLibraryToRootScope(library);
            AssetDatabase.SaveAssets();
            Debug.Log("[UFQ] Assets del proyecto actualizados.");
        }

        internal static InkStoryLibrary BuildStoryLibrary()
        {
            var library = AssetDatabase.LoadAssetAtPath<InkStoryLibrary>(LibraryPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<InkStoryLibrary>();
                AssetDatabase.CreateAsset(library, LibraryPath);
            }

            var entries = new List<InkStoryLibrary.Entry>();
            foreach (var guid in AssetDatabase.FindAssets("", new[] { InkFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".ink")) continue;
                var file = AssetDatabase.LoadAssetAtPath<InkFile>(path);
                if (file == null || !file.isMaster) continue; // los INCLUDE (_variables) no son historias
                entries.Add(new InkStoryLibrary.Entry { id = Path.GetFileNameWithoutExtension(path), file = file });
            }

            library.SetEntries(entries);
            EditorUtility.SetDirty(library);
            return library;
        }

        internal static PanelSettings BuildPanelSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<PanelSettings>();
                AssetDatabase.CreateAsset(settings, PanelSettingsPath);
            }

            settings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            EditorUtility.SetDirty(settings);
            return settings;
        }

        private static void LinkLibraryToRootScope(InkStoryLibrary library)
        {
            var root = PrefabUtility.LoadPrefabContents(RootScopePrefab);
            try
            {
                var scope = root.GetComponent<GameLifetimeScope>();
                var serialized = new SerializedObject(scope);
                serialized.FindProperty("storyLibrary").objectReferenceValue = library;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(root, RootScopePrefab);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }
}
