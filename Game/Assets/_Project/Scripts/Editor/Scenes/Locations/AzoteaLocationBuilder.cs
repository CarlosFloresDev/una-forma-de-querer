using UFQ.Presentation.Ambience;
using UnityEditor;
using UnityEngine;
using static UFQ.Editor.Scenes.SceneBuildKit;

namespace UFQ.Editor.Scenes.Locations
{
    /// <summary>
    /// Lugar: cuarto de la azotea de La Pluma Quieta (P.1, 1.3, 1.7, 1.12, 4.3, 5.3).
    /// Grilla de 2 m; cuarto de 4 × 4 m (x: -2..2, z: -2..2); corte de diorama al sur.
    /// </summary>
    internal static class AzoteaLocationBuilder
    {
        internal const string SceneName = "Azotea";
        private const string Props = "Assets/_Project/Art/Props/Azotea/";

        private static readonly Vector3 CameraPosition = new Vector3(0.6f, 1.75f, -5.2f);
        private static readonly Vector3 CameraLookAt = new Vector3(-0.45f, 0.85f, 0.7f);

        [MenuItem("Una forma de querer/Lugares/Azotea (cuarto de Mateo)")]
        internal static void Build()
        {
            var scene = NewLocationScene();

            BuildShell(new GameObject("Cuarto").transform);
            BuildFurniture(new GameObject("Muebles").transform);
            BuildExterior(new GameObject("Exterior").transform);
            BuildLighting(new GameObject("Luces").transform);
            BuildFireworks();
            BuildCameras();

            Volume("VP_Azotea_Noche", new Look
            {
                Exposure = -0.15f, Contrast = 8f, Saturation = 6f, Filter = new Color(1f, 0.95f, 0.98f),
                FocusDistance = 5.4f, Aperture = 3.2f, Bloom = 0.6f,
                Vignette = 0.32f, VignetteColor = new Color(0.09f, 0.05f, 0.14f),
            });

            Anchor(new Color(0.07f, 0.08f, 0.17f), new Vector3(0.45f, 0f, 0.1f), -20f, new[]
            {
                ("P.1", new Vector3(-0.85f, 0f, 0.1f), -100f),   // junto a la cama, con el celular
                ("1.3", new Vector3(0.2f, 0f, 0.6f), -10f),      // recién llegado, desempacando
                ("1.12", new Vector3(-0.1f, 0f, 0.55f), 0f),     // frente al escritorio y el cuaderno
            });

            SaveLocation(scene, SceneName);
        }

        private static void BuildShell(Transform parent)
        {
            for (var x = -1; x <= 1; x += 2)
            for (var z = -1; z <= 1; z += 2)
                SetMaterial(Place(Props, "SM_Piso", parent, new Vector3(x, -0.12f, z), 0), "M_Cemento");

            Place(Props, "SM_Muro_Ventana", parent, new Vector3(-1, 0, 2), 0);
            Place(Props, "SM_Muro_Liso", parent, new Vector3(1, 0, 2), 0);
            Place(Props, "SM_Ventana", parent, new Vector3(-1, 1.0f, 2), 0, collide: false);

            Place(Props, "SM_Muro_Liso", parent, new Vector3(-2, 0, 1), 90);
            Place(Props, "SM_Muro_Liso", parent, new Vector3(-2, 0, -1), 90);

            Place(Props, "SM_Muro_Liso", parent, new Vector3(2, 0, 1), 90);
            Place(Props, "SM_Muro_Puerta", parent, new Vector3(2, 0, -1), 90);
            Place(Props, "SM_Puerta", parent, new Vector3(2, 0, -1), 90);

            // Lado abierto del diorama (sur): límite invisible.
            var edge = new GameObject("Limite_Sur");
            edge.transform.SetParent(parent, false);
            edge.transform.position = new Vector3(0, 1.3f, -2.05f);
            edge.AddComponent<BoxCollider>().size = new Vector3(4.2f, 2.6f, 0.1f);
        }

        private static void BuildFurniture(Transform parent)
        {
            Place(Props, "SM_Cama", parent, new Vector3(-1.45f, 0, 0.55f), 0);
            Place(Props, "SM_Ballena", parent, new Vector3(-1.6f, 0.62f, 1.25f), 200, collide: false);
            Place(Props, "SM_Celular", parent, new Vector3(-1.25f, 0.62f, 0.1f), 25, collide: false);

            Place(Props, "SM_Escritorio", parent, new Vector3(-0.1f, 0, 1.6f), 0);
            Place(Props, "SM_Silla", parent, new Vector3(-0.1f, 0, 0.95f), 180);
            Place(Props, "SM_Cuaderno", parent, new Vector3(-0.05f, 0.78f, 1.5f), 172, collide: false);
            Place(Props, "SM_Lampara", parent, new Vector3(-0.55f, 0.78f, 1.7f), 150, collide: false);
            Place(Props, "SM_MarcoFoto", parent, new Vector3(0.35f, 0.78f, 1.72f), 190, collide: false);

            Place(Props, "SM_Estufa", parent, new Vector3(0.8f, 0, 1.7f), 180);
            Place(Props, "SM_Maleta", parent, new Vector3(1.5f, 0, 0.35f), 80);
        }

        private static void BuildExterior(Transform parent)
        {
            // Alineado con la línea cámara → ventana: la cumbre nevada se enmarca en el vidrio.
            Volcanoes(parent, new Vector3(-17, -30, 85), new Vector3(-75, -30, 115));

            // Techos de San Lázaro por DEBAJO del cuarto (estamos en una azotea).
            var city = new GameObject("Techos").transform;
            city.SetParent(parent, false);
            var sillar = Library("M_Sillar");
            var random = new System.Random(14); // semilla fija: la ciudad sale igual siempre
            for (var i = 0; i < 70; i++)
            {
                var w = 3f + (float)random.NextDouble() * 5f;
                var h = 3f + (float)random.NextDouble() * 4f;
                var d = 3f + (float)random.NextDouble() * 5f;
                var center = new Vector3(-50 + (float)random.NextDouble() * 100f, -12f + h * 0.5f, 8 + (float)random.NextDouble() * 55f);
                Block(city, $"Casa_{i:00}", center, new Vector3(w, h, d), sillar, collide: false);
            }
        }

        private static void BuildLighting(Transform parent)
        {
            var moon = NewLight(parent, "Luna", LightType.Directional, new Color(0.58f, 0.66f, 0.92f), 0.55f);
            moon.transform.rotation = Quaternion.Euler(38, 200, 0);
            moon.shadows = LightShadows.Soft;

            var lamp = NewLight(parent, "Lampara_Escritorio", LightType.Point, new Color(1f, 0.76f, 0.5f), 0.9f);
            lamp.transform.position = new Vector3(-0.45f, 1.18f, 1.55f);
            lamp.range = 3.2f;
            lamp.shadows = LightShadows.Soft;

            var heater = NewLight(parent, "Estufa_Brillo", LightType.Point, new Color(1f, 0.45f, 0.2f), 0.45f);
            heater.transform.position = new Vector3(0.8f, 0.3f, 1.45f);
            heater.range = 1.6f;

            Ambient(new Color(0.20f, 0.22f, 0.36f), new Color(0.22f, 0.18f, 0.26f), new Color(0.09f, 0.08f, 0.11f),
                new Color(0.16f, 0.17f, 0.30f), 70f, 320f);
        }

        private static void BuildCameras()
        {
            var rig = CameraRig("azotea_plano_general").transform;
            Shot(rig, "azotea_plano_general", CameraPosition, CameraLookAt, 34f, followPlayer: true);
            Shot(rig, "azotea_noche", CameraPosition + new Vector3(-0.3f, -0.1f, 0.4f), CameraLookAt, 32f, followPlayer: true);
            Shot(rig, "entrar_en_dibujo", new Vector3(-0.05f, 1.35f, 1.0f), new Vector3(-0.05f, 0.8f, 1.5f), 28f, followPlayer: false);
            Shot(rig, "azotea_cama", new Vector3(-0.2f, 1.25f, -0.9f), new Vector3(-1.4f, 0.7f, 0.4f), 36f, followPlayer: false);
            Shot(rig, "azotea_madrugada", CameraPosition + new Vector3(0.2f, 0.1f, 0.6f), new Vector3(-0.1f, 0.9f, 1.4f), 30f, followPlayer: false);
        }

        private static void BuildFireworks()
        {
            var go = new GameObject("Fuegos_Serenata");
            var particles = go.AddComponent<ParticleSystem>();
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var main = particles.main;
            main.playOnAwake = false;
            main.loop = false;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startLifetime = new ParticleSystem.MinMaxCurve(1.4f, 2.3f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
            main.gravityModifier = 0.35f;
            main.maxParticles = 3000;

            var emission = particles.emission;
            emission.rateOverTime = 0;
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.1f;

            var fade = particles.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;

            var shrink = particles.sizeOverLifetime;
            shrink.enabled = true;
            shrink.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0, 1, 1, 0.2f));

            var path = MaterialsPath + "M_Chispa.mat";
            var spark = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (spark == null)
            {
                spark = new Material(Shader.Find("UFQ/Chispa"));
                AssetDatabase.CreateAsset(spark, path);
            }

            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = spark;

            var tint = NewLight(go.transform, "Tinte_Fuegos", LightType.Point, Color.white, 0f);
            tint.transform.position = new Vector3(-1f, 2.0f, 2.7f);
            tint.range = 7f;

            var ambience = go.AddComponent<FireworksAmbience>();
            var so = new SerializedObject(ambience);
            so.FindProperty("burst").objectReferenceValue = particles;
            so.FindProperty("roomTint").objectReferenceValue = tint;
            so.FindProperty("areaCenter").vector3Value = new Vector3(-14f, 7f, 70f);
            so.FindProperty("areaSize").vector3Value = new Vector3(34f, 8f, 12f);
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
