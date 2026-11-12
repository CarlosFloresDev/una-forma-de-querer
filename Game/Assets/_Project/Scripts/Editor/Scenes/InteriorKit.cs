using UnityEngine;

namespace UFQ.Editor.Scenes
{
    /// <summary>
    /// Habitaciones de casona de sillar armadas con los módulos de Art/Props/Arquitectura
    /// (muros de 2 m × 3.2 m). Las habitaciones son «dioramas»: el muro sur queda abierto
    /// porque por ahí mira la cámara, y una pared invisible evita que Mateo salga.
    /// Convención de los módulos: su −Z local mira hacia afuera de la habitación.
    /// </summary>
    internal static class InteriorKit
    {
        internal const string Arquitectura = "Assets/_Project/Art/Props/Arquitectura/";
        internal const string Libreria = "Assets/_Project/Art/Props/Libreria/";
        internal const string Tintero = "Assets/_Project/Art/Props/Tintero/";
        internal const string Aula = "Assets/_Project/Art/Props/Aula/";
        internal const float WallHeight = 3.2f;
        internal const float WallThickness = 0.3f;
        internal const float WindowSill = 0.9f;

        internal enum Side { North, East, West }

        /// <summary>
        /// Habitación de <paramref name="width"/> (X) × <paramref name="depth"/> (Z), centrada en el
        /// origen, ambas medidas múltiplos de 2. Cada muro es una cadena de módulos:
        /// «A» muro alto, «V» ventana con reja, «R» arco de paso, «-» hueco.
        /// </summary>
        internal static Transform Room(string name, float width, float depth, string north, string east, string west, Material floor)
        {
            var room = new GameObject(name).transform;
            SceneBuildKit.Block(room, "Piso", new Vector3(0, -0.1f, 0), new Vector3(width + 0.6f, 0.2f, depth + 0.6f), floor);
            Wall(room, Side.North, width, depth, north);
            Wall(room, Side.East, width, depth, east);
            Wall(room, Side.West, width, depth, west);

            var sillar = SceneBuildKit.Library("M_Sillar");
            foreach (var x in new[] { -width / 2f, width / 2f })
                SceneBuildKit.Block(room, "Esquina", new Vector3(x, WallHeight / 2f, depth / 2f), new Vector3(0.42f, WallHeight, 0.42f), sillar);

            InvisibleWall(room, "Limite_Sur", new Vector3(0, 1f, -depth / 2f - 0.1f), new Vector3(width, 2f, 0.2f));
            return room;
        }

        internal static void Wall(Transform parent, Side side, float width, float depth, string modules)
        {
            float yaw;
            Vector3 start;
            switch (side)
            {
                case Side.North: yaw = 180f; start = new Vector3(width / 2f, 0, depth / 2f); break;
                case Side.West: yaw = 90f; start = new Vector3(-width / 2f, 0, depth / 2f); break;
                default: yaw = -90f; start = new Vector3(width / 2f, 0, -depth / 2f); break;
            }

            var direction = Quaternion.Euler(0, yaw, 0) * Vector3.right;
            for (var i = 0; i < modules.Length; i++)
            {
                var center = start + direction * (1f + 2f * i);
                switch (modules[i])
                {
                    case 'A': SceneBuildKit.Place(Arquitectura, "SM_Muro_Sillar_Alto", parent, center, yaw); break;
                    case 'R': SceneBuildKit.Place(Arquitectura, "SM_Muro_Sillar_Arco", parent, center, yaw); break;
                    case 'V':
                        SceneBuildKit.Place(Arquitectura, "SM_Muro_Sillar_Ventana", parent, center, yaw);
                        SceneBuildKit.Place(Arquitectura, "SM_Ventana_Reja", parent, center + Vector3.up * WindowSill, yaw, collide: false);
                        break;
                }
            }
        }

        /// <summary>Punto pegado a la cara interior de un muro, a la altura indicada.</summary>
        internal static Vector3 OnWall(Side side, float width, float depth, float along, float height, float inset = 0f)
        {
            var face = WallThickness / 2f + inset;
            switch (side)
            {
                case Side.North: return new Vector3(along, height, depth / 2f - face);
                case Side.West: return new Vector3(-width / 2f + face, height, along);
                default: return new Vector3(width / 2f - face, height, along);
            }
        }

        /// <summary>Giro para que el frente (−Z local) de un mueble mire hacia el centro desde ese muro.</summary>
        internal static float FacingRoom(Side side)
        {
            switch (side)
            {
                case Side.North: return 0f;
                case Side.West: return -90f;
                default: return 90f;
            }
        }

        internal static void InvisibleWall(Transform parent, string name, Vector3 center, Vector3 size)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            go.AddComponent<BoxCollider>().size = size;
            go.isStatic = true;
        }

        /// <summary>Lámpara colgante con su luz cálida justo bajo la pantalla.</summary>
        internal static void HangingLamp(Transform parent, Vector3 shadePosition, float intensity = 1.2f, float range = 4.5f)
        {
            SceneBuildKit.Place(Tintero, "SM_Lampara_Colgante", parent, shadePosition, 0, collide: false);
            var light = SceneBuildKit.NewLight(parent, "Luz_Lampara", LightType.Point, new Color(1f, 0.78f, 0.52f), intensity);
            light.transform.position = shadePosition + Vector3.down * 0.05f;
            light.range = range;
        }

        /// <summary>Luz de ventana: un foco suave que entra en diagonal desde afuera.</summary>
        internal static void WindowLight(Transform parent, Vector3 windowCenter, Vector3 inward, Color color, float intensity)
        {
            var light = SceneBuildKit.NewLight(parent, "Luz_Ventana", LightType.Spot, color, intensity);
            light.transform.position = windowCenter - inward * 0.6f + Vector3.up * 0.8f;
            light.transform.rotation = Quaternion.LookRotation(inward + Vector3.down * 0.55f);
            light.spotAngle = 70f;
            light.innerSpotAngle = 35f;
            light.range = 7f;
            light.shadows = LightShadows.Soft;
        }
    }
}
