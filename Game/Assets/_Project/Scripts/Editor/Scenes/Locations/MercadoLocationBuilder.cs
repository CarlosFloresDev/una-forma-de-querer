using UnityEditor;
using UnityEngine;
using static UFQ.Editor.Scenes.SceneBuildKit;
using static UFQ.Editor.Scenes.SceneBuildKitExtras;

namespace UFQ.Editor.Scenes.Locations
{
    /// <summary>
    /// Lugar: Mercado San Camilo (1.6 montaje, 5.2). Nave techada con arcos de sillar,
    /// pasillos de puestos de fruta, el relojero y la señora del queso helado.
    /// </summary>
    internal static class MercadoLocationBuilder
    {
        internal const string SceneName = "Mercado";

        private static readonly string[] FruitColors = { "#E3A23A", "#C8463C", "#7DA246", "#E8C84A", "#B7457A", "#E07A32" };

        [MenuItem("Una forma de querer/Lugares/Mercado San Camilo")]
        internal static void Build()
        {
            var scene = NewLocationScene();
            var sillar = Library("M_Sillar");
            var wood = Library("M_Madera");

            var hall = new GameObject("Nave").transform;
            Block(hall, "Piso", new Vector3(0, -0.15f, 0), new Vector3(22f, 0.3f, 16f), MaterialAsset("M_Losetas", "#B9AE9F", 0.16f, 2.8f, 0.05f));
            for (var i = 0; i < 5; i++)
            {
                Arch(hall, $"Arco_Norte_{i}", new Vector3(-8.8f + i * 4.4f, 0, 7.8f), 0, 4.2f, 6.2f, 0.7f, sillar);
                Arch(hall, $"Arco_Oeste_{i}", new Vector3(-10.8f, 0, -6f + i * 3.2f), 90, 3.0f, 6.2f, 0.7f, sillar);
            }

            Block(hall, "Muro_Este", new Vector3(10.9f, 3.1f, 0), new Vector3(0.6f, 6.2f, 16f), sillar);
            // Techo de calamina sobre vigas (deja ver el cielo por los lados: diorama)
            for (var i = 0; i < 6; i++) Block(hall, $"Viga_{i}", new Vector3(-9f + i * 3.6f, 6.4f, 0.5f), new Vector3(0.25f, 0.3f, 15f), wood, collide: false);

            // Puestos en tres pasillos
            var stalls = new GameObject("Puestos").transform;
            var random = new System.Random(6);
            for (var row = 0; row < 3; row++)
            for (var col = 0; col < 4; col++)
            {
                var basePos = new Vector3(-6.5f + col * 4.2f, 0, -4.5f + row * 4.2f);
                Block(stalls, $"Mesa_{row}_{col}", basePos + new Vector3(0, 0.45f, 0), new Vector3(2.6f, 0.9f, 1.1f), wood);
                Block(stalls, $"Toldo_{row}_{col}", basePos + new Vector3(0, 2.3f, 0), new Vector3(2.8f, 0.08f, 1.5f),
                    MaterialAsset($"M_Toldo_{(row + col) % 3}", (row + col) % 3 == 0 ? "#B5523B" : (row + col) % 3 == 1 ? "#3E5A6E" : "#C2A15A", 0.05f), collide: false);
                for (var k = 0; k < 6; k++)
                {
                    var color = FruitColors[random.Next(FruitColors.Length)];
                    Block(stalls, $"Fruta_{row}_{col}_{k}", basePos + new Vector3(-1f + k * 0.4f, 1.0f, 0f), new Vector3(0.36f, 0.22f, 0.7f),
                        MaterialAsset("M_Fruta_" + color.Substring(1), color, 0.12f, 6f), collide: false);
                }
            }

            var people = new GameObject("Personajes").transform;
            Figure(people, "Abril", new Vector3(-0.6f, 0, -2.2f), 30, "#E3B53F", "#5A3A26", 1.64f);
            Figure(people, "Relojero", new Vector3(6.2f, 0, 4.9f), 200, "#6E5A48", "#CFC8BE", 1.62f);
            Figure(people, "SeñoraQuesoHelado", new Vector3(-6.4f, 0, 5.2f), 170, "#E8E1D2", "#2E2420", 1.55f);
            Figure(people, "Casera", new Vector3(2f, 0, 0.4f), 180, "#7A3B3B", "#2E2420", 1.56f);

            var lights = new GameObject("Luces").transform;
            var sun = NewLight(lights, "Sol", LightType.Directional, new Color(1f, 0.92f, 0.78f), 1.2f);
            sun.transform.rotation = Quaternion.Euler(55, 120, 0);
            sun.shadows = LightShadows.Soft;
            Ambient(new Color(0.6f, 0.68f, 0.85f), new Color(0.7f, 0.62f, 0.55f), new Color(0.4f, 0.36f, 0.32f),
                new Color(0.75f, 0.8f, 0.9f), 60f, 300f);

            var exterior = new GameObject("Exterior").transform;
            Volcanoes(exterior, new Vector3(25, -10, 170), new Vector3(-70, -10, 185));

            var rig = CameraRig("mercado").transform;
            Shot(rig, "mercado", new Vector3(4.5f, 2.6f, -9.5f), new Vector3(-0.5f, 1.2f, 1f), 40f, followPlayer: true);

            Volume("VP_Mercado_Dia", new Look
            {
                Exposure = 0.05f, Contrast = 7f, Saturation = 12f, Filter = new Color(1f, 0.96f, 0.9f),
                FocusDistance = 9.5f, Aperture = 4.2f, Bloom = 0.35f,
                Vignette = 0.25f, VignetteColor = new Color(0.25f, 0.16f, 0.1f),
            });

            Anchor(new Color(0.55f, 0.72f, 0.92f), new Vector3(0.6f, 0f, -3.4f), 10f);
            SaveLocation(scene, SceneName);
        }
    }
}
