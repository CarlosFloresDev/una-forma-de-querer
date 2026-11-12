using System.Collections.Generic;
using System.IO;
using Ink.UnityIntegration;
using UFQ.Bootstrap;
using UFQ.Infrastructure.Dialogue;
using UFQ.Presentation.Locations;
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
        internal const string LocationMapPath = "Assets/_Project/Settings/LocationMap.asset";
        private const string ThemePath = "Assets/_Project/UI/Theme/TemaJuego.tss";
        private const string InkFolder = "Assets/_Project/Narrative/Ink";
        private const string RootScopePrefab = "Assets/_Project/Prefabs/Core/GameLifetimeScope.prefab";

        [MenuItem("Una forma de querer/Configuración/Actualizar assets del proyecto")]
        internal static void BuildAll()
        {
            var library = BuildStoryLibrary();
            BuildPanelSettings();
            BuildLocationMap();
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

        /// <summary>
        /// Escena del guion → lugar de Unity. Fuente: la columna «Escenas» de la tabla de
        /// lugares del guion (sección 8). Las sub-escenas (1.6b) heredan de su escena (1.6).
        /// </summary>
        internal static LocationMap BuildLocationMap()
        {
            var map = AssetDatabase.LoadAssetAtPath<LocationMap>(LocationMapPath);
            if (map == null)
            {
                map = ScriptableObject.CreateInstance<LocationMap>();
                AssetDatabase.CreateAsset(map, LocationMapPath);
            }

            (string, string)[] rows =
            {
                ("P.1", "Azotea"),
                ("1.1", "Terminal"), ("1.2", "Libreria"), ("1.3", "Azotea"), ("1.4", "Aula"),
                ("1.5", "Tintero"), ("1.6", "Mercado"), ("1.6m", "Mercado"), ("1.6p", "Picanteria"),
                ("1.6b", "Biblioteca"), ("1.7", "Azotea"), ("1.8", "Aula"), ("1.9", "Libreria"),
                ("1.10", "Tintero"), ("1.11", "Yanahuara"), ("1.12", "Azotea"),
                ("2.1", "BusMollendo"), ("2.2", "CasaBenavides"), ("2.3", "Playa"), ("2.4", "Playa"),
                ("2.5", "Feria"), ("2.6", "CastilloForga"), ("2.7", "Fogata"), ("2.8", "Muelle"), ("2.9", "BusMollendo"),
                ("3.1", "Azotea"), ("3.2", "BusLima"), ("3.3", "DepartamentoCarmen"), ("3.4", "EstudioCobre"),
                ("3.5", "Barranco"), ("3.6", "CuartoLima"), ("3.7", "Barranco"), ("3.8", "DepartamentoCarmen"),
                ("3.9", "DepartamentoCarmen"), ("3.10", "CuartoLima"), ("3.11", "BusLima"),
            };

            var entries = new List<LocationMap.Entry>();
            foreach (var (script, unity) in rows) entries.Add(new LocationMap.Entry { scriptScene = script, unityScene = unity });
            map.SetEntries(entries);
            EditorUtility.SetDirty(map);
            return map;
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
