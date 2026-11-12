using System.Collections.Generic;
using System.IO;
using UFQ.Bootstrap;
using UFQ.Presentation.Cameras;
using UFQ.Presentation.Locations;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace UFQ.Editor.Scenes
{
    /// <summary>
    /// Herramientas compartidas por los constructores de lugares: colocar modelos del
    /// kit, colisiones, luces, planos de cámara, volúmenes de post-proceso y el ancla
    /// del lugar. Cada lugar del guion es un constructor pequeño que usa este kit.
    /// </summary>
    internal static class SceneBuildKit
    {
        internal const string MaterialsPath = "Assets/_Project/Art/Materials/";
        internal const string LocationsFolder = "Assets/_Project/Scenes/Locations/";
        internal const string VolumesFolder = "Assets/_Project/Settings/Volumes/";

        // ------------------------------------------------------------------ escena
        internal static Scene NewLocationScene()
        {
            return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }

        /// <summary>Guarda el lugar con su ámbito de inyección y lo añade a Build Settings.</summary>
        internal static void SaveLocation(Scene scene, string name)
        {
            new GameObject("SceneLifetimeScope").AddComponent<SceneLifetimeScope>();
            Directory.CreateDirectory(LocationsFolder);
            var path = LocationsFolder + name + ".unity";
            EditorSceneManager.SaveScene(scene, path);
            BuildSettingsSync.EnsureScene(path);
            Debug.Log($"[UFQ] Lugar construido: {path}");
        }

        // ------------------------------------------------------------------ modelos
        internal static GameObject Place(string propsFolder, string model, Transform parent, Vector3 position, float yaw, bool collide = true)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(propsFolder + model + ".fbx");
            if (prefab == null)
            {
                Debug.LogWarning($"[UFQ] Falta el modelo {propsFolder}{model}.fbx (se omite).");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            instance.transform.position = position;
            // El FBX trae la conversión de ejes en la raíz: se respeta y se gira sobre Y.
            instance.transform.rotation = Quaternion.Euler(0, yaw, 0) * prefab.transform.rotation;
            instance.isStatic = collide;
            if (collide) AddCollider(instance, model);
            return instance;
        }

        /// <summary>Muros, pisos y puertas: colisión exacta. El resto: caja de sus límites.</summary>
        internal static void AddCollider(GameObject instance, string model)
        {
            if (model.Contains("Muro") || model.Contains("Piso") || model.Contains("Puerta") || model.Contains("Boveda"))
            {
                foreach (var filter in instance.GetComponentsInChildren<MeshFilter>())
                    filter.gameObject.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
                return;
            }

            var bounds = RendererBounds(instance);
            var holder = new GameObject("Colision_" + model);
            holder.transform.SetParent(instance.transform.parent, false);
            holder.transform.position = bounds.center;
            holder.AddComponent<BoxCollider>().size = bounds.size;
            holder.isStatic = true;
        }

        internal static Bounds RendererBounds(GameObject instance)
        {
            var bounds = new Bounds(instance.transform.position, Vector3.zero);
            var first = true;
            foreach (var renderer in instance.GetComponentsInChildren<Renderer>())
            {
                if (first) { bounds = renderer.bounds; first = false; }
                else bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        internal static void SetMaterial(GameObject instance, string materialName)
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

        /// <summary>Caja de color con el shader del juego (bloqueos, edificios lejanos).</summary>
        internal static GameObject Block(Transform parent, string name, Vector3 center, Vector3 size, Material material, bool collide = true)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            if (!collide) Object.DestroyImmediate(block.GetComponent<Collider>());
            block.transform.SetParent(parent, false);
            block.transform.position = center;
            block.transform.localScale = size;
            block.GetComponent<Renderer>().sharedMaterial = material;
            block.isStatic = true;
            return block;
        }

        internal static Material MaterialAsset(string name, string hex, float brush = 0.1f, float brushScale = 3.5f, float rim = 0.15f)
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
            material.SetFloat("_BrushScale", brushScale);
            material.SetFloat("_RimStrength", rim);
            material.enableInstancing = true;
            EditorUtility.SetDirty(material);
            return material;
        }

        internal static Material Library(string name) => AssetDatabase.LoadAssetAtPath<Material>(MaterialsPath + name + ".mat");

        // ------------------------------------------------------------------ luz
        internal static Light NewLight(Transform parent, string name, LightType type, Color color, float intensity)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var light = go.AddComponent<Light>();
            light.type = type;
            light.color = color;
            light.intensity = intensity;
            return light;
        }

        internal static void Ambient(Color sky, Color equator, Color ground, Color fog, float fogStart, float fogEnd)
        {
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = sky;
            RenderSettings.ambientEquatorColor = equator;
            RenderSettings.ambientGroundColor = ground;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fog;
            RenderSettings.fogStartDistance = fogStart;
            RenderSettings.fogEndDistance = fogEnd;
        }

        // ------------------------------------------------------------------ cámaras
        internal static CameraShotDirector CameraRig(string initialShot)
        {
            var rig = new GameObject("Camaras");
            var director = rig.AddComponent<CameraShotDirector>();
            var so = new SerializedObject(director);
            so.FindProperty("initialShot").stringValue = initialShot;
            so.ApplyModifiedPropertiesWithoutUndo();
            return director;
        }

        /// <summary>
        /// Plano de cámara. Si <paramref name="followPlayer"/>, gira con suavidad hacia
        /// Mateo (que vive en la escena persistente: se resuelve en tiempo de juego).
        /// </summary>
        internal static CinemachineCamera Shot(Transform rig, string name, Vector3 position, Vector3 lookAt, float fov, bool followPlayer, float damping = 1.6f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(rig, false);
            go.transform.position = position;
            go.transform.LookAt(lookAt);
            var vcam = go.AddComponent<CinemachineCamera>();
            var lens = vcam.Lens;
            lens.FieldOfView = fov;
            lens.NearClipPlane = 0.05f;
            lens.FarClipPlane = 400f;
            vcam.Lens = lens;
            if (followPlayer)
            {
                var composer = go.AddComponent<CinemachineRotationComposer>();
                composer.Damping = new Vector2(damping, damping);
                composer.TargetOffset = new Vector3(0, 1.1f, 0);
                go.AddComponent<FollowPlayerTarget>();
            }

            return vcam;
        }

        // ------------------------------------------------------------------ post-proceso
        internal struct Look
        {
            public float Exposure;
            public float Contrast;
            public float Saturation;
            public Color Filter;
            public float FocusDistance;
            public float Aperture;
            public float Bloom;
            public float Vignette;
            public Color VignetteColor;
        }

        internal static void Volume(string profileName, Look look)
        {
            Directory.CreateDirectory(VolumesFolder);
            var path = VolumesFolder + profileName + ".asset";
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, path);
            }

            Override<Tonemapping>(profile).mode.Override(TonemappingMode.Neutral);
            var bloom = Override<Bloom>(profile);
            bloom.threshold.Override(0.85f);
            bloom.intensity.Override(look.Bloom);
            bloom.scatter.Override(0.78f);
            var dof = Override<DepthOfField>(profile);
            dof.mode.Override(DepthOfFieldMode.Bokeh);
            dof.focusDistance.Override(look.FocusDistance);
            dof.focalLength.Override(70f);
            dof.aperture.Override(look.Aperture);
            var color = Override<ColorAdjustments>(profile);
            color.postExposure.Override(look.Exposure);
            color.contrast.Override(look.Contrast);
            color.saturation.Override(look.Saturation);
            color.colorFilter.Override(look.Filter);
            var vignette = Override<Vignette>(profile);
            vignette.intensity.Override(look.Vignette);
            vignette.smoothness.Override(0.55f);
            vignette.color.Override(look.VignetteColor);
            var grain = Override<FilmGrain>(profile);
            grain.type.Override(FilmGrainLookup.Thin1);
            grain.intensity.Override(0.16f);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            var go = new GameObject("Volumen_" + profileName);
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

        // ------------------------------------------------------------------ ancla
        internal static void Anchor(Color sky, Vector3 spawn, float spawnYaw, IEnumerable<(string scene, Vector3 position, float yaw)> extra = null, bool showPlayer = true)
        {
            var go = new GameObject("Lugar");
            var anchor = go.AddComponent<LocationAnchor>();
            var fallback = SpawnPoint(go.transform, "Aparicion", spawn, spawnYaw);
            var list = new List<LocationAnchor.Spawn>();
            if (extra != null)
            {
                foreach (var (scene, position, yaw) in extra)
                    list.Add(new LocationAnchor.Spawn { scriptScene = scene, point = SpawnPoint(go.transform, "Aparicion_" + scene, position, yaw) });
            }

            anchor.Configure(fallback, sky, list, showPlayer);
        }

        private static Transform SpawnPoint(Transform parent, string name, Vector3 position, float yaw)
        {
            var point = new GameObject(name).transform;
            point.SetParent(parent, false);
            point.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            return point;
        }

        // ------------------------------------------------------------------ mallas
        /// <summary>Cono truncado (volcanes). Se guarda como asset para no depender de este código.</summary>
        internal static Mesh ConeMesh(float radius, float height, float from, float to, int segments, float lift = 0f)
        {
            var name = $"Cono_{radius:0}_{height:0}_{from:0.00}_{to:0.00}";
            var path = $"Assets/_Project/Art/Environments/Arequipa/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;

            float RadiusAt(float t) => Mathf.Lerp(radius, radius * 0.08f, t);
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            for (var i = 0; i <= segments; i++)
            {
                var a = i / (float)segments * Mathf.PI * 2f;
                var dir = new Vector3(Mathf.Cos(a), 0, Mathf.Sin(a));
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

        internal static void AddMesh(Transform parent, string name, Mesh mesh, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            go.isStatic = true;
        }

        /// <summary>El Misti (con nieve) y el Chachani: presentes en todo Arequipa.</summary>
        internal static void Volcanoes(Transform parent, Vector3 mistiPosition, Vector3 chachaniPosition)
        {
            var misti = new GameObject("Misti").transform;
            misti.SetParent(parent, false);
            misti.localPosition = mistiPosition;
            AddMesh(misti, "Cuerpo", ConeMesh(52f, 46f, 0f, 1f, 48), MaterialAsset("M_Volcan", "#7D7F99", 0.15f, 0.25f, 0.05f));
            AddMesh(misti, "Nieve", ConeMesh(52f, 46f, 0.8f, 1f, 48, 0.08f), MaterialAsset("M_Nieve", "#F4F1EC", 0.05f, 0.25f, 0.05f));

            var chachani = new GameObject("Chachani").transform;
            chachani.SetParent(parent, false);
            chachani.localPosition = chachaniPosition;
            AddMesh(chachani, "Cuerpo", ConeMesh(60f, 38f, 0f, 1f, 36), MaterialAsset("M_VolcanLejano", "#8E8FA8", 0.12f, 0.25f, 0.05f));
        }
    }
}
