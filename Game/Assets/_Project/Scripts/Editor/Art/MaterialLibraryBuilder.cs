using System.IO;
using UnityEditor;
using UnityEngine;

namespace UFQ.Editor.Art
{
    /// <summary>
    /// Crea o actualiza la biblioteca de materiales del juego (shader UFQ/Pintado).
    /// La paleta es la de la dirección de arte (Docs y Art/Blender/README.md): si cambia
    /// un color, se cambia aquí y se vuelve a ejecutar el menú.
    /// </summary>
    internal static class MaterialLibraryBuilder
    {
        private const string ShaderName = "UFQ/Pintado";

        private struct Entry
        {
            public string Name;
            public string Hex;
            public float Brush;      // variación de pincel
            public float Rim;        // fuerza del borde
            public float Glow;       // brillo dorado

            public Entry(string name, string hex, float brush = 0.08f, float rim = 0.2f, float glow = 0f)
            {
                Name = name;
                Hex = hex;
                Brush = brush;
                Rim = rim;
                Glow = glow;
            }
        }

        private static readonly Entry[] Palette =
        {
            new Entry("M_Sillar", "#EDE6DA", brush: 0.12f, rim: 0.1f),
            new Entry("M_Madera", "#8A5A3C", brush: 0.10f),
            new Entry("M_Tela", "#6F7C8C", brush: 0.06f),
            new Entry("M_Lana", "#B5523B", brush: 0.10f),
            new Entry("M_Metal", "#3B3A3F", brush: 0.03f, rim: 0.35f),
            new Entry("M_Papel", "#F3EAD3", brush: 0.04f),
            new Entry("M_Dorado", "#D9A441", brush: 0.02f, rim: 0.4f),
            new Entry("M_Vidrio", "#9FB7BF", brush: 0.0f, rim: 0.5f),
            new Entry("M_Cemento", "#B7AEA4", brush: 0.14f, rim: 0.05f),
            // Capítulo 1: librería, Tintero, aula (Art/Props/Libreria, Tintero, Aula)
            new Entry("M_LibroA", "#7A3B3B", brush: 0.08f),
            new Entry("M_LibroB", "#3E5A6E", brush: 0.08f),
            new Entry("M_LibroC", "#C2A15A", brush: 0.08f),
            new Entry("M_Ceramica", "#E9E2D6", brush: 0.04f, rim: 0.1f),
            new Entry("M_Pizarra", "#2F3B35", brush: 0.06f, rim: 0.1f),
            new Entry("M_Planta", "#5E7A4A", brush: 0.12f),
            new Entry("M_GatoNegro", "#1E1C22", brush: 0.05f, rim: 0.45f),
            new Entry("M_Corcho", "#B5865A", brush: 0.2f),
            new Entry("M_Mimbre", "#C8A46B", brush: 0.16f),
            // Personajes: Mateo (Art/Characters/Mateo)
            new Entry("M_Casaca", "#6B6B3E", brush: 0.08f, rim: 0.3f),
            new Entry("M_Chalina", "#8E8C8A", brush: 0.1f, rim: 0.3f),
            new Entry("M_Piel", "#C99A7A", brush: 0.03f, rim: 0.35f),
            new Entry("M_Pelo", "#3A2A22", brush: 0.06f, rim: 0.3f),
            new Entry("M_Pantalon", "#2F3340", brush: 0.06f, rim: 0.25f),
            new Entry("M_Zapatillas", "#E3DED4", brush: 0.05f, rim: 0.2f),
            new Entry("M_Morral", "#A08B6A", brush: 0.1f, rim: 0.25f),
        };

        [MenuItem("Una forma de querer/Arte/Crear o actualizar materiales")]
        internal static void BuildAndReimport()
        {
            Build();
            AssetDatabase.ImportAsset(ArtImportPipeline.ArtRoot + "Props", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(ArtImportPipeline.ArtRoot + "Characters", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
            Debug.Log("[UFQ Arte] Materiales actualizados y modelos reimportados.");
        }

        internal static void Build()
        {
            var shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                Debug.LogError($"[UFQ Arte] No se encontró el shader {ShaderName}.");
                return;
            }

            Directory.CreateDirectory(ArtImportPipeline.MaterialsFolder);
            foreach (var entry in Palette)
            {
                var path = $"{ArtImportPipeline.MaterialsFolder}/{entry.Name}.mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(shader) { name = entry.Name };
                    AssetDatabase.CreateAsset(material, path);
                }

                // El vidrio es la única excepción: transparente, con su propio shader.
                var isGlass = entry.Name == "M_Vidrio";
                material.shader = isGlass ? Shader.Find("UFQ/Vidrio") : shader;
                ColorUtility.TryParseHtmlString(entry.Hex, out var color);
                if (isGlass) color.a = 0.12f;
                material.SetColor("_BaseColor", color);
                material.SetFloat("_BrushStrength", entry.Brush);
                material.SetFloat("_RimStrength", entry.Rim);
                material.SetFloat("_GlowStrength", entry.Glow);
                material.enableInstancing = true;
                EditorUtility.SetDirty(material);
            }

            AssetDatabase.SaveAssets();
        }
    }
}
