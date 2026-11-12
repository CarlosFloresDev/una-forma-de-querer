using UnityEditor;
using UnityEngine;
using static UFQ.Editor.Scenes.SceneBuildKit;
using static UFQ.Editor.Scenes.SceneBuildKitExtras;

namespace UFQ.Editor.Scenes.Locations
{
    /// <summary>
    /// Lugar: picantería de Sachaca (1.6 montaje). Patio de sillar a cielo abierto con
    /// mesas largas, la cocina a leña de Doña Lucha y una enramada. Nico toca la
    /// guitarra y Sofi graba; Abril le regala a Doña Lucha un dibujo de su picantería.
    /// </summary>
    internal static class PicanteriaLocationBuilder
    {
        internal const string SceneName = "Picanteria";

        [MenuItem("Una forma de querer/Lugares/Picantería de Sachaca")]
        internal static void Build()
        {
            var scene = NewLocationScene();
            var sillar = Library("M_Sillar");
            var wood = Library("M_Madera");

            var patio = new GameObject("Patio").transform;
            Block(patio, "Piso", new Vector3(0, -0.15f, 0), new Vector3(18f, 0.3f, 14f), MaterialAsset("M_TierraApisonada", "#B79C7E", 0.2f, 2f, 0.04f));
            Block(patio, "Muro_Norte", new Vector3(0, 1.6f, 7f), new Vector3(18f, 3.2f, 0.6f), sillar);
            Block(patio, "Muro_Oeste", new Vector3(-9f, 1.6f, 0), new Vector3(0.6f, 3.2f, 14f), sillar);
            Arch(patio, "Arco_Entrada", new Vector3(9f, 0, 2f), 90, 3.2f, 3.6f, 0.7f, sillar);
            Block(patio, "Muro_Este_A", new Vector3(9f, 1.6f, -3.2f), new Vector3(0.6f, 3.2f, 7.6f), sillar);
            Block(patio, "Muro_Este_B", new Vector3(9f, 1.6f, 5.6f), new Vector3(0.6f, 3.2f, 2.8f), sillar);

            // Enramada de madera y caña sobre las mesas
            for (var i = 0; i < 4; i++) Block(patio, $"Poste_Enramada_{i}", new Vector3(-6f + i * 4f, 1.4f, 3.6f), new Vector3(0.18f, 2.8f, 0.18f), wood);
            Block(patio, "Enramada", new Vector3(0, 2.85f, 2.2f), new Vector3(13f, 0.12f, 3.6f), MaterialAsset("M_Cana", "#C9B07A", 0.2f, 6f, 0.05f), collide: false);

            // Mesas largas con bancas y platos
            var plate = MaterialAsset("M_Ceramica", "#E9E2D6", 0.04f, 3f, 0.1f);
            var rocoto = MaterialAsset("M_Rocoto", "#C8352E", 0.06f, 6f, 0.2f);
            for (var t = 0; t < 2; t++)
            {
                var z = 0.8f + t * 2.6f;
                Block(patio, $"Mesa_{t}", new Vector3(-1f, 0.75f, z), new Vector3(7f, 0.1f, 1.0f), wood);
                Block(patio, $"Patas_{t}", new Vector3(-1f, 0.37f, z), new Vector3(6.6f, 0.74f, 0.7f), wood);
                Block(patio, $"Banca_A_{t}", new Vector3(-1f, 0.42f, z - 0.75f), new Vector3(7f, 0.08f, 0.35f), wood);
                Block(patio, $"Banca_B_{t}", new Vector3(-1f, 0.42f, z + 0.75f), new Vector3(7f, 0.08f, 0.35f), wood);
                for (var p = 0; p < 5; p++)
                {
                    Block(patio, $"Plato_{t}_{p}", new Vector3(-3.8f + p * 1.4f, 0.82f, z), new Vector3(0.32f, 0.03f, 0.32f), plate, collide: false);
                    Block(patio, $"Rocoto_{t}_{p}", new Vector3(-3.8f + p * 1.4f, 0.88f, z), new Vector3(0.12f, 0.1f, 0.12f), rocoto, collide: false);
                }
            }

            // Cocina a leña de Doña Lucha (fogón de adobe con humo de luz)
            Block(patio, "Fogon", new Vector3(-6.8f, 0.55f, 5.6f), new Vector3(2.4f, 1.1f, 1.2f), MaterialAsset("M_Adobe", "#9C6B4C", 0.18f, 3f, 0.05f));
            Block(patio, "Chimenea", new Vector3(-7.6f, 2.0f, 6.2f), new Vector3(0.6f, 2.8f, 0.6f), MaterialAsset("M_Adobe", "#9C6B4C", 0.18f, 3f, 0.05f));

            var people = new GameObject("Personajes").transform;
            Figure(people, "DonaLucha", new Vector3(-6.2f, 0, 4.4f), 160, "#7A3B3B", "#2E2420", 1.56f);
            Figure(people, "Abril", new Vector3(-4.6f, 0, 3.6f), 250, "#E3B53F", "#5A3A26", 1.64f);
            Figure(people, "Nico", new Vector3(1.6f, 0, -0.6f), 200, "#4E6B5A", "#2A221E", 1.83f);
            Figure(people, "Sofi", new Vector3(3.1f, 0, -0.9f), 220, "#C77D8E", "#D9B46A", 1.70f);

            var lights = new GameObject("Luces").transform;
            var sun = NewLight(lights, "Sol", LightType.Directional, new Color(1f, 0.9f, 0.74f), 1.25f);
            sun.transform.rotation = Quaternion.Euler(42, 210, 0);
            sun.shadows = LightShadows.Soft;
            var fire = NewLight(lights, "Fuego_Fogon", LightType.Point, new Color(1f, 0.55f, 0.25f), 1.4f);
            fire.transform.position = new Vector3(-6.8f, 1.3f, 5.0f);
            fire.range = 4f;
            Ambient(new Color(0.62f, 0.7f, 0.88f), new Color(0.74f, 0.64f, 0.54f), new Color(0.42f, 0.36f, 0.3f),
                new Color(0.78f, 0.82f, 0.9f), 50f, 260f);

            var exterior = new GameObject("Exterior").transform;
            Volcanoes(exterior, new Vector3(30, -10, 160), new Vector3(-60, -10, 175));
            City(exterior, new Vector3(0, 0, 60), new Vector2(140, 70), -1f, 50, 21, minDistance: 25f);

            var rig = CameraRig("picanteria").transform;
            Shot(rig, "picanteria", new Vector3(5.5f, 2.4f, -6.5f), new Vector3(-2f, 1f, 2.4f), 40f, followPlayer: true);

            Volume("VP_Picanteria_Tarde", new Look
            {
                Exposure = 0.1f, Contrast = 7f, Saturation = 12f, Filter = new Color(1f, 0.94f, 0.86f),
                FocusDistance = 8f, Aperture = 4f, Bloom = 0.4f,
                Vignette = 0.26f, VignetteColor = new Color(0.28f, 0.16f, 0.1f),
            });

            Anchor(new Color(0.6f, 0.74f, 0.92f), new Vector3(2.2f, 0f, -2.6f), -40f);
            SaveLocation(scene, SceneName);
        }
    }
}
