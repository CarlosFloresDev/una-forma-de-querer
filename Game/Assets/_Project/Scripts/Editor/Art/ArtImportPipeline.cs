using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UFQ.Editor.Art
{
    /// <summary>
    /// Pipeline de arte: los modelos de Blender traen materiales con nombre (M_Sillar,
    /// M_Madera…). Al importarlos, cada uno se sustituye por el material del proyecto
    /// con el mismo nombre (shader UFQ/Pintado). Un modelo nuevo queda con el estilo
    /// del juego sin pasos manuales; si falta un material, se avisa en consola.
    /// </summary>
    internal sealed class ArtImportPipeline : AssetPostprocessor
    {
        internal const string ArtRoot = "Assets/_Project/Art/";
        internal const string MaterialsFolder = "Assets/_Project/Art/Materials";

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ArtRoot)) return;

            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.bakeAxisConversion = true;       // +Y arriba sin rotaciones raras en el objeto raíz
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.importAnimation = assetPath.Contains("/Characters/");
            importer.meshCompression = ModelImporterMeshCompression.Low;
            importer.isReadable = false;
            importer.importNormals = ModelImporterNormals.Import;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;

            var library = LoadLibrary();
            foreach (var pair in library)
            {
                importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), pair.Key), pair.Value);
            }
        }

        private void OnPostprocessModel(GameObject root)
        {
            if (!assetPath.StartsWith(ArtRoot)) return;

            var library = LoadLibrary();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null) continue;
                    var name = material.name;
                    if (name.StartsWith("M_") && !library.ContainsKey(name))
                    {
                        Debug.LogWarning($"[UFQ Arte] {assetPath}: no existe el material «{name}» en {MaterialsFolder}. " +
                                         "Créalo (menú Una forma de querer > Arte) y reimporta.");
                    }
                }
            }
        }

        internal static Dictionary<string, Material> LoadLibrary()
        {
            var library = new Dictionary<string, Material>();
            if (!AssetDatabase.IsValidFolder(MaterialsFolder)) return library;

            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { MaterialsFolder }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (material != null) library[material.name] = material;
            }

            return library;
        }
    }
}
