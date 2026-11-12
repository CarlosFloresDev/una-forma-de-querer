using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UFQ.Editor.Scenes
{
    /// <summary>
    /// Construye la escena del cuarto de la azotea (Prólogo P.1, escenas 1.3, 1.7, 4.3, 5.3)
    /// a partir del kit de Art/Props/Azotea. Es reproducible: si cambian los modelos o la
    /// distribución, se ajusta aquí y se vuelve a ejecutar el menú.
    ///
    /// Grilla: módulos de 2 m. Cuarto de 4 × 4 m (x: -2..2, z: -2..2), piso en y = 0.
    /// Corte de diorama: falta la pared sur (z = -2), por donde mira la cámara.
    /// </summary>
    internal static class AzoteaSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Chapters/Azotea.unity";
        private const string PropsPath = "Assets/_Project/Art/Props/Azotea/";
        private const string VolumePath = "Assets/_Project/Settings/Volumes/VP_Azotea_Noche.asset";
        private const string MaterialsPath = "Assets/_Project/Art/Materials/";

        [MenuItem("Una forma de querer/Escenas/Construir cuarto de la azotea")]
        private static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var room = new GameObject("Cuarto").transform;
            BuildShell(room);
            BuildFurniture(new GameObject("Muebles").transform);
            BuildExterior(new GameObject("Exterior").transform);
            BuildLighting(new GameObject("Luces").transform);
            BuildCamera();
            BuildVolume();

            var scope = new GameObject("SceneLifetimeScope");
            var scopeType = System.Type.GetType("UFQ.Bootstrap.SceneLifetimeScope, UFQ.Bootstrap");
            if (scopeType != null) scope.AddComponent(scopeType);

            Directory.CreateDirectory(Path.GetDirectoryName(ScenePath) ?? ".");
            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[UFQ] Escena construida: {ScenePath}");
        }

        // ------------------------------------------------------------------ estructura
        private static void BuildShell(Transform parent)
        {
            // Piso 2×2 módulos; el FBX mide 12 cm de alto con pivote en la base.
            for (var x = -1; x <= 1; x += 2)
            for (var z = -1; z <= 1; z += 2)
            {
                var floor = Place("SM_Piso", parent, new Vector3(x, -0.12f, z), 0);
                SetMaterial(floor, "M_Cemento");
            }

            // Norte: ventana (vista al Misti) + liso.
            Place("SM_Muro_Ventana", parent, new Vector3(-1, 0, 2), 0);
            Place("SM_Muro_Liso", parent, new Vector3(1, 0, 2), 0);
            Place("SM_Ventana", parent, new Vector3(-1, 1.0f, 2), 0);

            // Oeste: liso ×2 (la cama va contra esta pared).
            Place("SM_Muro_Liso", parent, new Vector3(-2, 0, 1), 90);
            Place("SM_Muro_Liso", parent, new Vector3(-2, 0, -1), 90);

            // Este: puerta (por donde baja Mateo a la librería) + liso.
            Place("SM_Muro_Liso", parent, new Vector3(2, 0, 1), 90);
            Place("SM_Muro_Puerta", parent, new Vector3(2, 0, -1), 90);
            Place("SM_Puerta", parent, new Vector3(2, 0, -1), 90);
        }

        private static void BuildFurniture(Transform parent)
        {
            Place("SM_Cama", parent, new Vector3(-1.45f, 0, 0.55f), 0);
            Place("SM_Ballena", parent, new Vector3(-1.6f, 0.62f, 1.25f), 200);
            Place("SM_Celular", parent, new Vector3(-1.25f, 0.62f, 0.1f), 25);

            Place("SM_Escritorio", parent, new Vector3(-0.1f, 0, 1.6f), 0);
            Place("SM_Silla", parent, new Vector3(-0.1f, 0, 0.95f), 180);
            Place("SM_Cuaderno", parent, new Vector3(-0.05f, 0.78f, 1.5f), 172);
            Place("SM_Lampara", parent, new Vector3(-0.55f, 0.78f, 1.7f), 150);
            Place("SM_MarcoFoto", parent, new Vector3(0.35f, 0.78f, 1.72f), 190);

            Place("SM_Estufa", parent, new Vector3(0.8f, 0, 1.7f), 180);
            Place("SM_Maleta", parent, new Vector3(1.5f, 0, 0.35f), 80);
        }

        // ------------------------------------------------------------------ exterior
        private static void BuildExterior(Transform parent)
        {
            // El Misti: cono con gorro de nieve, lejos tras la ventana norte.
            var misti = new GameObject("Misti").transform;
            misti.SetParent(parent, false);
            // Alineado con la línea cámara → ventana: la cumbre nevada queda enmarcada en el vidrio.
            misti.localPosition = new Vector3(-17, -30, 85);
            AddMesh(misti, "Cuerpo", ConeMesh(52f, 46f, 0f, 1f, 48), Material("M_Volcan", "#7D7F99", brush: 0.15f));
            AddMesh(misti, "Nieve", ConeMesh(52f, 46f, 0.8f, 1f, 48, 0.08f), Material("M_Nieve", "#F4F1EC", brush: 0.05f));

            // Chachani, más bajo y a la izquierda, para dar profundidad.
            var chachani = new GameObject("Chachani").transform;
            chachani.SetParent(parent, false);
            chachani.localPosition = new Vector3(-75, -30, 115);
            AddMesh(chachani, "Cuerpo", ConeMesh(60f, 38f, 0f, 1f, 36), Material("M_VolcanLejano", "#8E8FA8", brush: 0.12f));

            // Techos de sillar de San Lázaro: por DEBAJO del cuarto (estamos en una azotea).
            var city = new GameObject("Techos").transform;
            city.SetParent(parent, false);
            var sillar = AssetDatabase.LoadAssetAtPath<Material>(MaterialsPath + "M_Sillar.mat");
            var random = new System.Random(14); // semilla fija: la ciudad sale igual siempre
            for (var i = 0; i < 70; i++)
            {
                var house = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(house.GetComponent<Collider>());
                house.name = $"Casa_{i:00}";
                house.transform.SetParent(city, false);
                var w = 3f + (float)random.NextDouble() * 5f;
                var h = 3f + (float)random.NextDouble() * 4f;
                house.transform.localScale = new Vector3(w, h, 3f + (float)random.NextDouble() * 5f);
                house.transform.localPosition = new Vector3(-50 + (float)random.NextDouble() * 100f, -12f + h * 0.5f, 8 + (float)random.NextDouble() * 55f);
                house.GetComponent<Renderer>().sharedMaterial = sillar;
                house.isStatic = true;
            }
        }

        // ------------------------------------------------------------------ luz
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

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.20f, 0.22f, 0.36f);
            RenderSettings.ambientEquatorColor = new Color(0.22f, 0.18f, 0.26f);
            RenderSettings.ambientGroundColor = new Color(0.09f, 0.08f, 0.11f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.16f, 0.17f, 0.30f);
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 320f;
        }

        private static Light NewLight(Transform parent, string name, LightType type, Color color, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var light = go.AddComponent<Light>();
            light.type = type;
            light.color = color;
            light.intensity = intensity;
            return light;
        }

        // ------------------------------------------------------------------ cámara y volumen
        private static void BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.07f, 0.08f, 0.17f);
            camera.fieldOfView = 34f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 300f;
            go.transform.position = new Vector3(0.6f, 1.75f, -5.2f);
            go.transform.LookAt(new Vector3(-0.45f, 0.85f, 0.7f));

            var data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }

        private static void BuildVolume()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(VolumePath) ?? ".");
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, VolumePath);
            }

            Override<Tonemapping>(profile).mode.Override(TonemappingMode.Neutral);

            var bloom = Override<Bloom>(profile);
            bloom.threshold.Override(0.85f);
            bloom.intensity.Override(0.6f);
            bloom.scatter.Override(0.78f);
            bloom.tint.Override(new Color(1f, 0.86f, 0.7f));

            // Profundidad de campo: lo que define el «diorama». Foco en el escritorio.
            var dof = Override<DepthOfField>(profile);
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.focusDistance.Override(5.4f);
            dof.focalLength.Override(70f);
            dof.aperture.Override(3.2f);

            var color = Override<ColorAdjustments>(profile);
            color.postExposure.Override(-0.15f);
            color.contrast.Override(8f);
            color.saturation.Override(6f);
            color.colorFilter.Override(new Color(1f, 0.95f, 0.98f));

            var vignette = Override<Vignette>(profile);
            vignette.intensity.Override(0.32f);
            vignette.smoothness.Override(0.55f);
            vignette.color.Override(new Color(0.09f, 0.05f, 0.14f));

            var grain = Override<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0.18f);

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            var go = new GameObject("Volumen_Pictorico");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = profile;
        }

        private static T Override<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet(out T component)) component = profile.Add<T>(true);
            component.active = true;
            return component;
        }

        // ------------------------------------------------------------------ utilidades
        private static GameObject Place(string model, Transform parent, Vector3 position, float yaw)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PropsPath + model + ".fbx");
            if (prefab == null)
            {
                Debug.LogError($"[UFQ] Falta el modelo {model}");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = position;
            // El FBX trae la conversión de ejes en la raíz: se respeta y se gira sobre Y.
            instance.transform.rotation = Quaternion.Euler(0, yaw, 0) * prefab.transform.rotation;
            instance.isStatic = !model.Contains("Celular") && !model.Contains("Cuaderno") && !model.Contains("Ballena");
            return instance;
        }

        private static void SetMaterial(GameObject instance, string materialName)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialsPath + materialName + ".mat");
            if (instance == null || material == null) return;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                var shared = renderer.sharedMaterials;
                for (var i = 0; i < shared.Length; i++) shared[i] = material;
                renderer.sharedMaterials = shared;
            }
        }

        private static Material Material(string name, string hex, float brush)
        {
            var path = MaterialsPath + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("UFQ/Pintado"));
                AssetDatabase.CreateAsset(material, path);
            }

            ColorUtility.TryParseHtmlString(hex, out var color);
            material.SetColor("_BaseColor", color);
            material.SetFloat("_BrushStrength", brush);
            material.SetFloat("_BrushScale", 0.25f);
            material.SetFloat("_RimStrength", 0.05f);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void AddMesh(Transform parent, string name, Mesh mesh, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            go.isStatic = true;
        }

        /// <summary>
        /// Cono truncado entre las alturas relativas <paramref name="from"/>..<paramref name="to"/>
        /// (0 = base, 1 = cima). <paramref name="lift"/> separa la nieve de la ladera.
        /// La malla se guarda como asset para que la escena no dependa de este código.
        /// </summary>
        private static Mesh ConeMesh(float radius, float height, float from, float to, int segments, float lift = 0f)
        {
            var name = $"Cono_{radius:0}_{height:0}_{from:0.00}_{to:0.00}";
            var path = $"Assets/_Project/Art/Environments/Arequipa/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;

            // Cima achatada como el Misti (cráter): el radio no llega a 0.
            float RadiusAt(float t) => Mathf.Lerp(radius, radius * 0.08f, t);
            var vertices = new System.Collections.Generic.List<Vector3>();
            var triangles = new System.Collections.Generic.List<int>();
            for (var i = 0; i <= segments; i++)
            {
                var a = i / (float)segments * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
                // Borde de la nieve irregular para que no sea un corte perfecto.
                var jag = from > 0 ? Mathf.Sin(a * 7f) * 0.03f + Mathf.Sin(a * 13f) * 0.02f : 0f;
                var t0 = Mathf.Clamp01(from + jag);
                vertices.Add(dir * (RadiusAt(t0) + lift) + Vector3.up * (height * t0));
                vertices.Add(dir * (RadiusAt(to) + lift) + Vector3.up * (height * to));
            }

            for (var i = 0; i < segments; i++)
            {
                var b = i * 2;
                triangles.AddRange(new[] { b, b + 1, b + 2, b + 2, b + 1, b + 3 });
            }

            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ".");
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }
}
