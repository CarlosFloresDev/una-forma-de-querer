using UnityEditor;
using UnityEngine;
using static UFQ.Editor.Scenes.SceneBuildKit;
using static UFQ.Editor.Scenes.SceneBuildKitExtras;

namespace UFQ.Editor.Scenes.Locations
{
    /// <summary>
    /// Lugar: Mirador de Yanahuara, noche del 31 de diciembre (1.11).
    /// Arcos de sillar al borde norte, la ciudad encendida abajo, el Misti al fondo y
    /// fuegos artificiales de Año Nuevo. Plaza de 24 × 12 m centrada en el origen.
    /// </summary>
    internal static class YanahuaraLocationBuilder
    {
        internal const string SceneName = "Yanahuara";

        [MenuItem("Una forma de querer/Lugares/Mirador de Yanahuara (Año Nuevo)")]
        internal static void Build()
        {
            var scene = NewLocationScene();
            var sillar = Library("M_Sillar");
            var paving = MaterialAsset("M_Empedrado", "#A79E92", 0.18f, 2.2f, 0.05f);

            // --- plaza y arcos
            var plaza = new GameObject("Mirador").transform;
            Block(plaza, "Plaza", new Vector3(0, -0.15f, 0), new Vector3(24f, 0.3f, 12f), paving);
            for (var i = 0; i < 7; i++)
            {
                var x = -10.8f + i * 3.6f;
                Arch(plaza, $"Arco_{i}", new Vector3(x, 0, 5.6f), 0, 3.4f, 4.6f, 0.8f, sillar);
            }

            Block(plaza, "Cornisa", new Vector3(0, 4.85f, 5.6f), new Vector3(25.6f, 0.5f, 1.0f), sillar);
            Block(plaza, "Antepecho", new Vector3(0, 0.5f, 5.6f), new Vector3(25.2f, 1.0f, 0.5f), sillar);
            Block(plaza, "Muro_Oeste", new Vector3(-12.3f, 1.4f, 0), new Vector3(0.6f, 2.8f, 12f), sillar);
            Block(plaza, "Muro_Este", new Vector3(12.3f, 1.4f, 0), new Vector3(0.6f, 2.8f, 12f), sillar);
            Block(plaza, "Iglesia_Fondo", new Vector3(0, 4f, -9f), new Vector3(14f, 8f, 4f), sillar);

            // Bancas de piedra
            for (var i = 0; i < 4; i++) Block(plaza, $"Banca_{i}", new Vector3(-7.5f + i * 5f, 0.25f, -3.5f), new Vector3(2.2f, 0.5f, 0.6f), sillar);

            // --- exterior: la ciudad encendida abajo y los volcanes
            var exterior = new GameObject("Exterior").transform;
            City(exterior, new Vector3(0, 0, 70), new Vector2(160, 110), -22f, 160, 31);
            CityLights(exterior);
            Volcanoes(exterior, new Vector3(-10, -40, 170), new Vector3(-95, -40, 190));

            // --- personajes del grupo (provisionales) frente a los arcos
            var people = new GameObject("Personajes").transform;
            Figure(people, "Abril", new Vector3(-0.9f, 0, 3.2f), 200, "#E3B53F", "#5A3A26", 1.64f);
            Figure(people, "Sofi", new Vector3(0.9f, 0, 3.4f), 160, "#C77D8E", "#D9B46A", 1.70f);
            Figure(people, "Nico", new Vector3(2.4f, 0, 2.6f), 220, "#4E6B5A", "#2A221E", 1.83f);
            // Familias con ropa amarilla (costumbre de Año Nuevo)
            Figure(people, "Vecina", new Vector3(-6.5f, 0, 2.8f), 180, "#E8C25A", "#2E2420", 1.58f);
            Figure(people, "Vecino", new Vector3(-5.6f, 0, 3.1f), 190, "#5D6A8A", "#2E2420", 1.74f);
            Figure(people, "Nina", new Vector3(-6.0f, 0, 2.2f), 170, "#F1D46B", "#3B2A22", 1.15f);

            // --- luz de medianoche
            var lights = new GameObject("Luces").transform;
            var moon = NewLight(lights, "Luna", LightType.Directional, new Color(0.55f, 0.62f, 0.9f), 0.35f);
            moon.transform.rotation = Quaternion.Euler(30, 160, 0);
            moon.shadows = LightShadows.Soft;
            for (var i = 0; i < 4; i++)
            {
                var x = -9f + i * 6f;
                Block(lights, $"Poste_{i}", new Vector3(x, 1.6f, -6.4f), new Vector3(0.12f, 3.2f, 0.12f), Library("M_Metal"), collide: false);
                var lamp = NewLight(lights, $"Farol_{i}", LightType.Point, new Color(1f, 0.74f, 0.45f), 1.4f);
                lamp.transform.position = new Vector3(x, 3.3f, -6.4f);
                lamp.range = 10f;
                lamp.shadows = i == 1 ? LightShadows.Soft : LightShadows.None;
            }

            Ambient(new Color(0.22f, 0.22f, 0.38f), new Color(0.28f, 0.2f, 0.26f), new Color(0.1f, 0.08f, 0.12f),
                new Color(0.14f, 0.13f, 0.26f), 60f, 260f);

            // Fuegos de medianoche: muchos y seguidos sobre la ciudad.
            Fireworks(new Vector3(0, 22f, 75f), new Vector3(90f, 24f, 40f), new Vector3(0, 6f, 8f), 22f, new Vector2(0.6f, 1.8f));

            // --- cámaras
            var rig = CameraRig("mirador_yanahuara").transform;
            Shot(rig, "mirador_yanahuara", new Vector3(1.2f, 1.9f, -4.5f), new Vector3(-0.2f, 2.4f, 8f), 42f, followPlayer: true, damping: 2f);
            Shot(rig, "fuegos_artificiales", new Vector3(0f, 1.5f, 1.2f), new Vector3(0f, 14f, 60f), 50f, followPlayer: false);

            Volume("VP_Yanahuara_AnoNuevo", new Look
            {
                Exposure = 0.05f, Contrast = 10f, Saturation = 10f, Filter = new Color(1f, 0.95f, 0.92f),
                FocusDistance = 5.2f, Aperture = 3.5f, Bloom = 0.9f,
                Vignette = 0.3f, VignetteColor = new Color(0.08f, 0.04f, 0.12f),
            });

            Anchor(new Color(0.05f, 0.06f, 0.15f), new Vector3(0f, 0f, 1.6f), 0f);
            SaveLocation(scene, SceneName);
        }

        /// <summary>Ventanas encendidas de la ciudad: puntos cálidos con brillo (bloom).</summary>
        private static void CityLights(Transform parent)
        {
            var glow = MaterialAsset("M_LuzCiudad", "#FFC27A", 0f, 1f, 0f);
            glow.SetFloat("_GlowStrength", 1f);
            glow.SetColor("_GlowColor", new Color(2.2f, 1.4f, 0.6f));
            var holder = new GameObject("Luces_Ciudad").transform;
            holder.SetParent(parent, false);
            var random = new System.Random(77);
            for (var i = 0; i < 220; i++)
            {
                var position = new Vector3(-80 + (float)random.NextDouble() * 160f, -20f + (float)random.NextDouble() * 6f, 20 + (float)random.NextDouble() * 110f);
                Block(holder, $"Luz_{i:000}", position, Vector3.one * (0.4f + (float)random.NextDouble() * 0.5f), glow, collide: false);
            }
        }
    }
}
