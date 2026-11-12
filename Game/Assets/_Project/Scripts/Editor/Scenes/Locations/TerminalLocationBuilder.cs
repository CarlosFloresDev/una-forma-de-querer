using UnityEditor;
using UnityEngine;
using static UFQ.Editor.Scenes.SceneBuildKit;
using static UFQ.Editor.Scenes.SceneBuildKitExtras;

namespace UFQ.Editor.Scenes.Locations
{
    /// <summary>
    /// Lugar: Terminal Terrestre de Arequipa, mañana de agosto (1.1).
    /// Andén con techo metálico, el bus recién llegado de Lima, Don Aurelio y el
    /// Misti con su gorrito de nieve al fondo: la primera vista de la ciudad.
    /// </summary>
    internal static class TerminalLocationBuilder
    {
        internal const string SceneName = "Terminal";

        [MenuItem("Una forma de querer/Lugares/Terminal Terrestre (llegada)")]
        internal static void Build()
        {
            var scene = NewLocationScene();
            var cement = Library("M_Cemento");
            var metal = Library("M_Metal");
            var sillar = Library("M_Sillar");

            var station = new GameObject("Anden").transform;
            Block(station, "Piso", new Vector3(0, -0.15f, 0), new Vector3(34f, 0.3f, 22f), cement);
            Block(station, "Vereda_Anden", new Vector3(0, 0.1f, -4f), new Vector3(34f, 0.2f, 5f), MaterialAsset("M_Vereda", "#C9C1B5", 0.12f, 2.5f, 0.05f));

            // Techo del andén sobre pilares
            for (var i = 0; i < 6; i++) Block(station, $"Pilar_{i}", new Vector3(-14f + i * 5.6f, 2.6f, -5.8f), new Vector3(0.35f, 5.2f, 0.35f), metal);
            Block(station, "Techo", new Vector3(0, 5.3f, -4.2f), new Vector3(32f, 0.25f, 6.5f), MaterialAsset("M_TechoAnden", "#8C9196", 0.06f, 3f, 0.1f));

            // Edificio del terminal al sur
            Block(station, "Edificio", new Vector3(0, 3.5f, -10f), new Vector3(34f, 7f, 4f), sillar);
            Block(station, "Letrero_Salidas", new Vector3(-6f, 4.4f, -7.9f), new Vector3(5f, 1.2f, 0.15f), MaterialAsset("M_Letrero", "#2F3B35", 0.02f, 3f, 0.1f));

            // Bancas
            for (var i = 0; i < 3; i++) Block(station, $"Banca_{i}", new Vector3(-9f + i * 9f, 0.45f, -6.6f), new Vector3(2.4f, 0.45f, 0.5f), Library("M_Madera"));

            // El bus de Lima
            BuildBus(new GameObject("Bus_Lima").transform, new Vector3(2f, 0, 1.4f));

            // Ambulantes y Don Aurelio (provisionales)
            var people = new GameObject("Personajes").transform;
            Figure(people, "DonAurelio", new Vector3(-2.6f, 0, -1.2f), 150, "#2C3550", "#BDB8B0", 1.7f);
            Figure(people, "Ambulante_QuesoHelado", new Vector3(-9f, 0, -3.2f), 110, "#E8E1D2", "#2E2420", 1.6f);
            Figure(people, "Taxista", new Vector3(11f, 0, -2.6f), 250, "#5E7A4A", "#2A221E", 1.72f);

            // Ciudad y volcanes al norte, a plena luz
            var exterior = new GameObject("Exterior").transform;
            City(exterior, new Vector3(0, 0, 60), new Vector2(180, 90), -1f, 110, 8, minDistance: 22f);
            Volcanoes(exterior, new Vector3(18, -10, 160), new Vector3(-80, -10, 175));

            var lights = new GameObject("Luces").transform;
            var sun = NewLight(lights, "Sol", LightType.Directional, new Color(1f, 0.93f, 0.8f), 1.35f);
            sun.transform.rotation = Quaternion.Euler(48, 140, 0);
            sun.shadows = LightShadows.Soft;
            Ambient(new Color(0.62f, 0.72f, 0.9f), new Color(0.72f, 0.66f, 0.6f), new Color(0.42f, 0.38f, 0.34f),
                new Color(0.72f, 0.80f, 0.92f), 80f, 380f);

            var rig = CameraRig("terminal_llegada").transform;
            Shot(rig, "terminal_llegada", new Vector3(-8.5f, 1.9f, -3.4f), new Vector3(0f, 1.5f, 0.6f), 40f, followPlayer: true);
            Shot(rig, "terminal_misti", new Vector3(-1f, 1.6f, -6f), new Vector3(16f, 12f, 150f), 38f, followPlayer: false);

            Volume("VP_Terminal_Manana", new Look
            {
                Exposure = 0.1f, Contrast = 6f, Saturation = 10f, Filter = new Color(1f, 0.97f, 0.92f),
                FocusDistance = 8f, Aperture = 4.5f, Bloom = 0.35f,
                Vignette = 0.22f, VignetteColor = new Color(0.25f, 0.18f, 0.12f),
            });

            Anchor(new Color(0.55f, 0.72f, 0.92f), new Vector3(-1.6f, 0.2f, -2.4f), 20f);
            SaveLocation(scene, SceneName);
        }

        private static void BuildBus(Transform bus, Vector3 position)
        {
            bus.position = position;
            var body = MaterialAsset("M_BusBlanco", "#E7E1D6", 0.04f, 3f, 0.2f);
            var stripe = MaterialAsset("M_BusFranja", "#B5523B", 0.04f, 3f, 0.2f);
            var glass = MaterialAsset("M_VidrioOscuro", "#2B3440", 0.02f, 3f, 0.4f);
            Block(bus, "Carroceria", position + new Vector3(0, 2.0f, 0), new Vector3(12.5f, 3.3f, 2.6f), body);
            Block(bus, "Franja", position + new Vector3(0, 1.0f, -1.31f), new Vector3(12.5f, 0.4f, 0.02f), stripe, collide: false);
            Block(bus, "Ventanas", position + new Vector3(0.6f, 2.6f, -1.31f), new Vector3(10.4f, 1.1f, 0.02f), glass, collide: false);
            Block(bus, "Ventanas_Altas", position + new Vector3(0.6f, 3.35f, -1.31f), new Vector3(10.4f, 0.5f, 0.02f), glass, collide: false);
            for (var i = 0; i < 2; i++)
            {
                var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                wheel.name = $"Rueda_{i}";
                Object.DestroyImmediate(wheel.GetComponent<Collider>());
                wheel.transform.SetParent(bus, false);
                wheel.transform.position = position + new Vector3(i == 0 ? -4f : 4f, 0.45f, -1.2f);
                wheel.transform.rotation = Quaternion.Euler(90, 0, 0);
                wheel.transform.localScale = new Vector3(0.9f, 0.15f, 0.9f);
                wheel.GetComponent<Renderer>().sharedMaterial = Library("M_Metal");
            }
        }
    }
}
