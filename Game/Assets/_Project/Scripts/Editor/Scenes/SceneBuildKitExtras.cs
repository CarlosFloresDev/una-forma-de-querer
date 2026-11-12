using System.Collections.Generic;
using System.IO;
using UFQ.Presentation.Ambience;
using UnityEditor;
using UnityEngine;

namespace UFQ.Editor.Scenes
{
    /// <summary>
    /// Más piezas del kit: arcos de sillar (la seña de Arequipa), figuras provisionales
    /// de personajes, fuegos artificiales y bancas. Todo sale del shader UFQ/Pintado.
    /// </summary>
    internal static class SceneBuildKitExtras
    {
        // ------------------------------------------------------------------ arcos
        /// <summary>
        /// Arco de medio punto extruido: dos pilares y un medio anillo. Ancho total
        /// <paramref name="span"/> (luz + pilares), alto hasta la clave, grosor = profundidad.
        /// Pivote en la base, centrado; se abre a lo largo del eje Z local.
        /// </summary>
        internal static Mesh ArchMesh(float span, float height, float thickness, float depth, int segments = 16)
        {
            var name = $"Arco_{span:0.0}_{height:0.0}_{thickness:0.00}_{depth:0.00}";
            var path = $"Assets/_Project/Art/Environments/Arequipa/{name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing != null) return existing;

            var radiusOuter = span * 0.5f;
            var radiusInner = radiusOuter - thickness;
            var springLine = height - radiusOuter; // altura donde empieza la curva
            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
            {
                var i = vertices.Count;
                vertices.AddRange(new[] { a, b, c, d });
                triangles.AddRange(new[] { i, i + 1, i + 2, i, i + 2, i + 3 });
            }

            void Box(Vector3 min, Vector3 max)
            {
                var p = new[]
                {
                    new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z), new Vector3(min.x, max.y, min.z),
                    new Vector3(min.x, min.y, max.z), new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z),
                };
                Quad(p[0], p[3], p[2], p[1]); Quad(p[4], p[5], p[6], p[7]);
                Quad(p[0], p[1], p[5], p[4]); Quad(p[3], p[7], p[6], p[2]);
                Quad(p[0], p[4], p[7], p[3]); Quad(p[1], p[2], p[6], p[5]);
            }

            var half = depth * 0.5f;
            // Pilares
            Box(new Vector3(-radiusOuter, 0, -half), new Vector3(-radiusInner, springLine, half));
            Box(new Vector3(radiusInner, 0, -half), new Vector3(radiusOuter, springLine, half));

            // Medio anillo
            for (var s = 0; s < segments; s++)
            {
                var a0 = Mathf.PI * s / segments;
                var a1 = Mathf.PI * (s + 1) / segments;
                Vector3 P(float a, float r, float z) => new Vector3(Mathf.Cos(a) * r, springLine + Mathf.Sin(a) * r, z);
                // caras frontal y trasera
                Quad(P(a0, radiusInner, -half), P(a0, radiusOuter, -half), P(a1, radiusOuter, -half), P(a1, radiusInner, -half));
                Quad(P(a0, radiusInner, half), P(a1, radiusInner, half), P(a1, radiusOuter, half), P(a0, radiusOuter, half));
                // intradós y trasdós
                Quad(P(a0, radiusInner, -half), P(a1, radiusInner, -half), P(a1, radiusInner, half), P(a0, radiusInner, half));
                Quad(P(a0, radiusOuter, -half), P(a0, radiusOuter, half), P(a1, radiusOuter, half), P(a1, radiusOuter, -half));
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

        internal static GameObject Arch(Transform parent, string name, Vector3 position, float yaw, float span, float height, float depth, Material material)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));
            var mesh = ArchMesh(span, height, 0.55f, depth);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
            go.AddComponent<MeshCollider>().sharedMesh = mesh;
            go.isStatic = true;
            return go;
        }

        // ------------------------------------------------------------------ personajes provisionales
        /// <summary>
        /// Figura provisional de un personaje (cuerpo + cabeza, sin rostro, como el estilo
        /// final). Se reemplaza cuando exista su modelo en Art/Characters.
        /// </summary>
        internal static GameObject Figure(Transform parent, string name, Vector3 position, float yaw, string bodyHex, string hairHex, float height = 1.68f)
        {
            var root = new GameObject("Personaje_" + name);
            root.transform.SetParent(parent, false);
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0, yaw, 0));

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Cuerpo";
            Object.DestroyImmediate(body.GetComponent<Collider>());
            body.transform.SetParent(root.transform, false);
            body.transform.localPosition = new Vector3(0, height * 0.4f, 0);
            body.transform.localScale = new Vector3(0.42f, height * 0.4f, 0.32f);
            body.GetComponent<Renderer>().sharedMaterial = SceneBuildKit.MaterialAsset("M_Fig_" + name, bodyHex, 0.06f);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Cabeza";
            Object.DestroyImmediate(head.GetComponent<Collider>());
            head.transform.SetParent(root.transform, false);
            head.transform.localPosition = new Vector3(0, height * 0.88f, 0);
            head.transform.localScale = Vector3.one * 0.24f;
            head.GetComponent<Renderer>().sharedMaterial = SceneBuildKit.Library("M_Piel") ?? SceneBuildKit.MaterialAsset("M_Piel", "#C99A7A", 0.04f);

            var hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hair.name = "Pelo";
            Object.DestroyImmediate(hair.GetComponent<Collider>());
            hair.transform.SetParent(root.transform, false);
            hair.transform.localPosition = new Vector3(0, height * 0.91f, -0.02f);
            hair.transform.localScale = new Vector3(0.26f, 0.2f, 0.27f);
            hair.GetComponent<Renderer>().sharedMaterial = SceneBuildKit.MaterialAsset("M_Pelo_" + name, hairHex, 0.05f);

            var collider = root.AddComponent<CapsuleCollider>();
            collider.center = new Vector3(0, height * 0.5f, 0);
            collider.height = height;
            collider.radius = 0.22f;
            return root;
        }

        // ------------------------------------------------------------------ fuegos artificiales
        internal static void Fireworks(Vector3 areaCenter, Vector3 areaSize, Vector3 tintPosition, float tintRange, Vector2 interval)
        {
            var go = new GameObject("Fuegos_Artificiales");
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
            main.maxParticles = 4000;

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

            var path = SceneBuildKit.MaterialsPath + "M_Chispa.mat";
            var spark = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (spark == null)
            {
                spark = new Material(Shader.Find("UFQ/Chispa"));
                AssetDatabase.CreateAsset(spark, path);
            }

            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = spark;

            var tint = SceneBuildKit.NewLight(go.transform, "Tinte_Fuegos", LightType.Point, Color.white, 0f);
            tint.transform.position = tintPosition;
            tint.range = tintRange;

            var ambience = go.AddComponent<FireworksAmbience>();
            var so = new SerializedObject(ambience);
            so.FindProperty("burst").objectReferenceValue = particles;
            so.FindProperty("roomTint").objectReferenceValue = tint;
            so.FindProperty("areaCenter").vector3Value = areaCenter;
            so.FindProperty("areaSize").vector3Value = areaSize;
            so.FindProperty("interval").vector2Value = interval;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Ciudad de techos de sillar alrededor/debajo de un punto (semilla fija).</summary>
        internal static void City(Transform parent, Vector3 center, Vector2 extent, float baseY, int count, int seed, float minDistance = 10f)
        {
            var sillar = SceneBuildKit.Library("M_Sillar");
            var random = new System.Random(seed);
            var placed = 0;
            for (var i = 0; placed < count && i < count * 4; i++)
            {
                var x = center.x + ((float)random.NextDouble() - 0.5f) * extent.x;
                var z = center.z + ((float)random.NextDouble() - 0.5f) * extent.y;
                if (new Vector2(x - center.x, z - center.z).magnitude < minDistance) continue;
                var w = 3f + (float)random.NextDouble() * 5f;
                var h = 3f + (float)random.NextDouble() * 5f;
                var d = 3f + (float)random.NextDouble() * 5f;
                SceneBuildKit.Block(parent, $"Casa_{placed:000}", new Vector3(x, baseY + h * 0.5f, z), new Vector3(w, h, d), sillar, collide: false);
                placed++;
            }
        }
    }
}
