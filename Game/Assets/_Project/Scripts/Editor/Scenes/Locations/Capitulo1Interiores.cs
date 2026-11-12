using UnityEditor;
using UnityEngine;
using static UFQ.Editor.Scenes.InteriorKit;
using static UFQ.Editor.Scenes.SceneBuildKit;
using static UFQ.Editor.Scenes.SceneBuildKitExtras;

namespace UFQ.Editor.Scenes.Locations
{
    /// <summary>
    /// Interiores del Capítulo 1 (Arequipa): la librería de Doña Inés, el café El Tintero,
    /// el aula 3B y la Biblioteca Regional. Todos son casonas de sillar armadas con los
    /// módulos de Art/Props/Arquitectura y amuebladas con los kits del capítulo.
    /// </summary>
    internal static class Capitulo1Interiores
    {
        // Colores provisionales de personajes que aún no tienen modelo.
        private const string ValeriaBody = "#3E4A6B", ValeriaHair = "#1E1714";
        private const string AbrilBody = "#E3B53F", AbrilHair = "#5A3A26";
        private const string NicoBody = "#4E6B5A", NicoHair = "#2A221E";
        private const string SofiBody = "#C77D8E", SofiHair = "#D9B46A";

        [MenuItem("Una forma de querer/Lugares/Arequipa/Todos los interiores")]
        internal static void BuildAll()
        {
            Libreria();
            Tintero();
            Aula();
            Biblioteca();
        }

        // ------------------------------------------------------------------ 1.2 / 1.9
        [MenuItem("Una forma de querer/Lugares/Arequipa/Librería de Doña Inés")]
        internal static void Libreria()
        {
            const float w = 10f, d = 8f;
            var scene = NewLocationScene();
            var room = Room("Libreria", w, d, north: "AAVAA", east: "VAAA", west: "AVAR", floor: Library("M_Madera"));
            Sidewalk(room, w, d);

            var shop = new GameObject("Libreria_Muebles").transform;
            // Estantes hasta el techo: muro norte y muro oeste
            foreach (var x in new[] { 2f, -2f, -4f })
                Place(InteriorKit.Libreria, "SM_Estante_Libros", shop, OnWall(Side.North, w, d, x, 0, 0.21f), FacingRoom(Side.North));
            Place(InteriorKit.Libreria, "SM_Estante_Libros", shop, OnWall(Side.West, w, d, 3f, 0, 0.21f), FacingRoom(Side.West));
            Place(InteriorKit.Libreria, "SM_Estante_Libros_Bajo", shop, OnWall(Side.West, w, d, -1f, 0, 0.2f), FacingRoom(Side.West));

            // Mostrador de Inés con la canasta de separadores (semilla A)
            Place(InteriorKit.Libreria, "SM_Mostrador", shop, new Vector3(1.6f, 0, 0.6f), 0);
            Place(InteriorKit.Libreria, "SM_Canasta_Separadores", shop, new Vector3(0.95f, 1.0f, 0.55f), 20, collide: false);

            // Tinta duerme sobre una pila de diccionarios
            Place(InteriorKit.Libreria, "SM_Pila_Libros", shop, new Vector3(-1.4f, 0, 1.6f), 15);
            Place(InteriorKit.Libreria, "SM_Pila_Libros", shop, new Vector3(-1.4f, 0.25f, 1.6f), -10, collide: false);
            Place(InteriorKit.Libreria, "SM_Gato_Tinta", shop, new Vector3(-1.4f, 0.5f, 1.6f), 140, collide: false);
            Place(InteriorKit.Libreria, "SM_Pila_Libros", shop, new Vector3(-3.9f, 0, -2.2f), 70);
            Place(InteriorKit.Libreria, "SM_Sillon", shop, new Vector3(-3.7f, 0, 0.9f), -70);

            // Escalera a la cocina (donde está el calendario de cumpleaños, semilla B)
            Place(InteriorKit.Libreria, "SM_Escalera_Madera", shop, new Vector3(4.3f, 0, 0.9f), 0);
            Place(InteriorKit.Libreria, "SM_Calendario_Pared", shop, OnWall(Side.North, w, d, 4.1f, 1.9f, 0.025f), FacingRoom(Side.North), collide: false);

            var people = new GameObject("Personajes").transform;
            Figure(people, "DonaInes", new Vector3(1.6f, 0, 1.35f), 180, "#8A6E8F", "#C9C3BC", 1.58f);
            Figure(people, "Abril", new Vector3(3.3f, 0, -1.4f), 240, AbrilBody, AbrilHair, 1.64f);

            var lights = new GameObject("Luces").transform;
            var sun = NewLight(lights, "Sol", LightType.Directional, new Color(1f, 0.88f, 0.7f), 0.55f);
            sun.transform.rotation = Quaternion.Euler(38, 130, 0);
            sun.shadows = LightShadows.Soft;
            WindowLight(lights, OnWall(Side.North, w, d, 0, 1.6f), Vector3.back, new Color(1f, 0.86f, 0.64f), 2.4f);
            WindowLight(lights, OnWall(Side.West, w, d, 1f, 1.6f), Vector3.right, new Color(1f, 0.86f, 0.64f), 2.0f);
            HangingLamp(lights, new Vector3(1.6f, 2.3f, 0.6f));
            HangingLamp(lights, new Vector3(-2f, 2.3f, 1.4f), 1.0f);
            Ambient(new Color(0.55f, 0.5f, 0.48f), new Color(0.5f, 0.42f, 0.34f), new Color(0.3f, 0.24f, 0.2f), new Color(0.2f, 0.16f, 0.13f), 18f, 60f);

            var rig = CameraRig("libreria_interior").transform;
            Shot(rig, "libreria_interior", new Vector3(1.5f, 3.4f, -7.5f), new Vector3(0, 1f, 1f), 42f, followPlayer: true);
            Shot(rig, "libreria_noche", new Vector3(-1.2f, 1.9f, -3.6f), new Vector3(1.4f, 1.1f, 0.6f), 38f, followPlayer: true);
            Shot(rig, "libreria_manana", new Vector3(3.4f, 2.0f, -2.6f), new Vector3(1.2f, 1.1f, 1.2f), 40f, followPlayer: true);
            Shot(rig, "libreria_vereda", new Vector3(-4.5f, 1.6f, -8.8f), new Vector3(0.5f, 1.1f, -5.6f), 44f, followPlayer: true);

            Volume("VP_Libreria", new Look
            {
                Exposure = 0.15f, Contrast = 8f, Saturation = 6f, Filter = new Color(1f, 0.93f, 0.84f),
                FocusDistance = 6f, Aperture = 3.2f, Bloom = 0.45f,
                Vignette = 0.3f, VignetteColor = new Color(0.22f, 0.13f, 0.08f),
            });

            Anchor(new Color(0.18f, 0.14f, 0.12f), new Vector3(-3.6f, 0, -3f), 90f, new[]
            {
                ("1.2", new Vector3(-3.6f, 0, -3f), 90f),
                ("1.9", new Vector3(0.6f, 0, -1.2f), 40f),
                ("4.2", new Vector3(-3.6f, 0, -3f), 90f),
                ("4.6b", new Vector3(0.6f, 0, -0.4f), 20f),
                ("4.6c", new Vector3(-1.5f, 0, -5.6f), 90f),
            });
            SaveLocation(scene, "Libreria");
        }

        // ------------------------------------------------------------------ 1.5 / 1.10
        [MenuItem("Una forma de querer/Lugares/Arequipa/Café El Tintero")]
        internal static void Tintero()
        {
            const float w = 10f, d = 8f;
            var scene = NewLocationScene();
            var room = Room("Tintero", w, d, north: "AARAA", east: "VAVA", west: "AVAR",
                floor: MaterialAsset("M_PisoLoseta", "#9C5B44", 0.14f, 4f, 0.05f));
            Sidewalk(room, w, d);

            var cafe = new GameObject("Cafe_Muebles").transform;
            Place(InteriorKit.Tintero, "SM_Barra_Cafe", cafe, new Vector3(1.6f, 0, 2.4f), 0);
            Place(InteriorKit.Libreria, "SM_Estante_Libros_Bajo", cafe, OnWall(Side.North, w, d, 3.6f, 0, 0.2f), FacingRoom(Side.North));
            Place(InteriorKit.Libreria, "SM_Estante_Libros_Bajo", cafe, OnWall(Side.North, w, d, -3.6f, 0, 0.2f), FacingRoom(Side.North));
            // Tablón de cumpleaños (semilla O: «MATEO — 14/07»)
            Place(InteriorKit.Tintero, "SM_Tablon_Corcho", cafe, OnWall(Side.West, w, d, 3f, 1.25f, 0.02f), FacingRoom(Side.West), collide: false);

            var tables = new[] { new Vector3(-2.6f, 0, 0.6f), new Vector3(-2.6f, 0, -2.2f), new Vector3(0.4f, 0, -1.2f), new Vector3(3.2f, 0, -2.4f) };
            for (var i = 0; i < tables.Length; i++)
            {
                var t = tables[i];
                Place(InteriorKit.Tintero, "SM_Mesa_Cafe", cafe, t, i * 23f);
                Place(InteriorKit.Tintero, "SM_Silla_Cafe", cafe, t + new Vector3(0, 0, 0.6f), 0);
                Place(InteriorKit.Tintero, "SM_Silla_Cafe", cafe, t + new Vector3(0, 0, -0.6f), 180);
                Place(InteriorKit.Tintero, "SM_Taza_Cafe", cafe, t + new Vector3(0.08f, 0.74f, 0.05f), i * 47f, collide: false);
                HangingLamp(cafe, t + Vector3.up * 2.2f, 0.9f, 3.5f);
            }

            var people = new GameObject("Personajes").transform;
            Figure(people, "DonFermin", new Vector3(2.8f, 0, 3.15f), 190, "#6B4E3A", "#3A3330", 1.72f);
            Figure(people, "Valeria", new Vector3(0.5f, 0, 3.15f), 170, ValeriaBody, ValeriaHair, 1.62f);

            var lights = new GameObject("Luces").transform;
            var sun = NewLight(lights, "Sol", LightType.Directional, new Color(1f, 0.85f, 0.66f), 0.5f);
            sun.transform.rotation = Quaternion.Euler(30, 250, 0);
            sun.shadows = LightShadows.Soft;
            WindowLight(lights, OnWall(Side.East, w, d, -3f, 1.6f), Vector3.left, new Color(1f, 0.8f, 0.58f), 2.2f);
            WindowLight(lights, OnWall(Side.East, w, d, 1f, 1.6f), Vector3.left, new Color(1f, 0.8f, 0.58f), 2.2f);
            HangingLamp(lights, new Vector3(0.6f, 2.3f, 2.4f), 1.1f);
            HangingLamp(lights, new Vector3(2.6f, 2.3f, 2.4f), 1.1f);
            Ambient(new Color(0.52f, 0.46f, 0.44f), new Color(0.52f, 0.4f, 0.3f), new Color(0.3f, 0.22f, 0.17f), new Color(0.2f, 0.15f, 0.12f), 18f, 60f);

            var rig = CameraRig("tintero_barra").transform;
            Shot(rig, "tintero_barra", new Vector3(-1.5f, 3.0f, -7f), new Vector3(1.5f, 1.1f, 2.2f), 40f, followPlayer: true);
            Shot(rig, "tintero_vereda", new Vector3(-3.5f, 1.7f, -9f), new Vector3(0.5f, 1.2f, 0.5f), 40f, followPlayer: true);
            Shot(rig, "tintero_31", new Vector3(3.9f, 1.8f, -0.6f), new Vector3(0.8f, 1.2f, 2.8f), 36f, followPlayer: true);

            Volume("VP_Tintero", new Look
            {
                Exposure = 0.1f, Contrast = 9f, Saturation = 8f, Filter = new Color(1f, 0.9f, 0.8f),
                FocusDistance = 5.5f, Aperture = 2.8f, Bloom = 0.55f,
                Vignette = 0.32f, VignetteColor = new Color(0.24f, 0.12f, 0.07f),
            });

            Anchor(new Color(0.16f, 0.12f, 0.1f), new Vector3(-3.6f, 0, -3f), 90f, new[]
            {
                ("1.5", new Vector3(-3.6f, 0, -3f), 90f),
                ("1.10", new Vector3(1.6f, 0, 3.25f), 180f),
                ("4.1", new Vector3(-2.5f, 0, -5.8f), 60f),
            });
            SaveLocation(scene, "Tintero");
        }

        // ------------------------------------------------------------------ 1.4 / 1.8
        [MenuItem("Una forma de querer/Lugares/Arequipa/Aula 3B")]
        internal static void Aula()
        {
            const float w = 12f, d = 10f;
            var scene = NewLocationScene();
            Room("Aula", w, d, north: "AAAAAA", east: "VVVVV", west: "AARAA", floor: Library("M_Cemento"));

            var classroom = new GameObject("Aula_Muebles").transform;
            Place(InteriorKit.Aula, "SM_Pizarra", classroom, OnWall(Side.North, w, d, -1f, 0.9f, 0.07f), FacingRoom(Side.North), collide: false);

            // Cuatro filas de mesas de dibujo mirando a la pizarra
            var rows = new[] { 2f, 0f, -2f, -4f };
            var columns = new[] { -4.2f, -1.4f, 1.4f, 4.2f };
            foreach (var z in rows)
            foreach (var x in columns)
            {
                if (z > 1f && x > 3f) continue; // rincón de la maqueta
                Place(InteriorKit.Aula, "SM_Mesa_Dibujo", classroom, new Vector3(x, 0, z), 0);
                Place(InteriorKit.Aula, "SM_Taburete", classroom, new Vector3(x, 0, z - 0.65f), 0);
            }

            // Mesa del profesor y la maqueta de «El mapa de la Arequipa que no sale en los mapas»
            var wood = Library("M_Madera");
            Block(classroom, "Escritorio_Profesor", new Vector3(-3.4f, 0.38f, 3.7f), new Vector3(1.6f, 0.76f, 0.7f), wood);
            Block(classroom, "Mesa_Maqueta", new Vector3(4.0f, 0.38f, 3.0f), new Vector3(1.6f, 0.76f, 1.1f), wood);
            Place(InteriorKit.Aula, "SM_Maqueta_Arequipa", classroom, new Vector3(4.0f, 0.76f, 3.0f), 0, collide: false);
            Place(InteriorKit.Libreria, "SM_Pila_Libros", classroom, new Vector3(-3.0f, 0.76f, 3.7f), 30, collide: false);

            var people = new GameObject("Personajes").transform;
            Figure(people, "ProfSalgado", new Vector3(-2.2f, 0, 4.0f), 180, "#5B5F66", "#8C8682", 1.74f);
            Figure(people, "Abril", new Vector3(1.4f, 0, -0.65f), -15, AbrilBody, AbrilHair, 1.3f);
            Figure(people, "Sofi", new Vector3(4.2f, 0, -0.65f), 0, SofiBody, SofiHair, 1.3f);
            Figure(people, "Nico", new Vector3(1.4f, 0, 1.35f), 0, NicoBody, NicoHair, 1.4f);
            Figure(people, "Valeria", new Vector3(-4.2f, 0, -4.65f), 10, ValeriaBody, ValeriaHair, 1.28f);

            var lights = new GameObject("Luces").transform;
            var sun = NewLight(lights, "Sol", LightType.Directional, new Color(1f, 0.93f, 0.8f), 0.9f);
            sun.transform.rotation = Quaternion.Euler(40, 260, 0);
            sun.shadows = LightShadows.Soft;
            for (var i = 0; i < 5; i++)
                WindowLight(lights, OnWall(Side.East, w, d, -4f + i * 2f, 1.6f), Vector3.left, new Color(1f, 0.94f, 0.82f), 1.6f);
            Ambient(new Color(0.66f, 0.68f, 0.72f), new Color(0.6f, 0.56f, 0.5f), new Color(0.36f, 0.33f, 0.3f), new Color(0.6f, 0.6f, 0.62f), 25f, 90f);

            var rig = CameraRig("aula_3b").transform;
            Shot(rig, "aula_3b", new Vector3(-1f, 4.6f, -9f), new Vector3(0, 0.9f, 0.4f), 40f, followPlayer: true);
            Shot(rig, "aula_maqueta", new Vector3(1.6f, 2.1f, 0.9f), new Vector3(4.0f, 0.9f, 3.0f), 34f, followPlayer: false);

            Volume("VP_Aula", new Look
            {
                Exposure = 0.2f, Contrast = 6f, Saturation = 4f, Filter = new Color(0.98f, 0.97f, 0.94f),
                FocusDistance = 8f, Aperture = 4.5f, Bloom = 0.35f,
                Vignette = 0.22f, VignetteColor = new Color(0.16f, 0.14f, 0.12f),
            });

            Anchor(new Color(0.78f, 0.82f, 0.88f), new Vector3(-2.8f, 0, -0.9f), 20f, new[]
            {
                ("1.4", new Vector3(-2.8f, 0, -0.9f), 20f),
                ("1.8", new Vector3(2.6f, 0, 2.2f), 50f),
            });
            SaveLocation(scene, "Aula");
        }

        // ------------------------------------------------------------------ 1.6b
        private const string SideWalls = "AVAAVAAV";

        [MenuItem("Una forma de querer/Lugares/Arequipa/Biblioteca Regional")]
        internal static void Biblioteca()
        {
            const float w = 8f, d = 16f;
            var scene = NewLocationScene();
            var nave = Room("Biblioteca", w, d, north: "AVVA", east: SideWalls, west: SideWalls,
                floor: MaterialAsset("M_PisoPiedra", "#BDB2A2", 0.12f, 1.6f, 0.05f));

            // Dos bóvedas de cañón (escaladas ×4: 8 m de luz) sobre los muros, y el tímpano norte
            foreach (var z in new[] { -4f, 4f })
            {
                var vault = Place(InteriorKit.Arquitectura, "SM_Boveda_Sillar", nave, new Vector3(0, WallHeight, z), 0);
                if (vault != null) vault.transform.localScale = Vector3.one * 4f;
            }

            var sillar = Library("M_Sillar");
            Block(nave, "Timpano_Norte", new Vector3(0, WallHeight + 2f, d / 2f), new Vector3(w, 4f, WallThickness), sillar, collide: false);

            var books = new GameObject("Biblioteca_Muebles").transform;
            // Estantes en los tramos de muro alto
            // El muro oeste se arma de norte a sur y el este de sur a norte (ver InteriorKit.Wall).
            for (var i = 0; i < SideWalls.Length; i++)
            {
                if (SideWalls[i] != 'A') continue;
                Place(InteriorKit.Libreria, "SM_Estante_Libros", books, OnWall(Side.West, w, d, d / 2f - 1f - 2f * i, 0, 0.21f), FacingRoom(Side.West));
                Place(InteriorKit.Libreria, "SM_Estante_Libros", books, OnWall(Side.East, w, d, -d / 2f + 1f + 2f * i, 0, 0.21f), FacingRoom(Side.East));
            }

            // Dos mesas largas de lectura con sillas
            var wood = Library("M_Madera");
            foreach (var tx in new[] { -1.4f, 1.4f })
            {
                Table(books, "Mesa_Lectura", new Vector3(tx, 0, 1.5f), new Vector3(1.1f, 0.76f, 7f), wood);
                for (var c = 0; c < 5; c++)
                {
                    var cz = -1.3f + c * 1.4f;
                    Place(InteriorKit.Tintero, "SM_Silla_Cafe", books, new Vector3(tx - 0.8f, 0, cz), -90);
                    Place(InteriorKit.Tintero, "SM_Silla_Cafe", books, new Vector3(tx + 0.8f, 0, cz), 90);
                }

                for (var l = 0; l < 3; l++) HangingLamp(books, new Vector3(tx, 2.6f, -0.6f + l * 2.8f), 0.9f, 3.8f);
            }

            // «Te guardé el sitio»: el sitio de Mateo y, al fondo, la chalina y la bolsa de Abril (semilla D)
            Place(InteriorKit.Libreria, "SM_Pila_Libros", books, new Vector3(1.4f, 0.76f, 0.1f), 80, collide: false);
            Block(books, "Chalina_Abril", new Vector3(0.6f, 0.5f, 2.9f), new Vector3(0.42f, 0.06f, 0.36f), Library("M_Chalina") ?? MaterialAsset("M_Chalina", "#E3B53F"), collide: false);
            Block(books, "Bolsa_Abril", new Vector3(0.6f, 0.58f, 4.3f), new Vector3(0.36f, 0.22f, 0.2f), Library("M_Morral") ?? MaterialAsset("M_Morral", "#7A5A3C"), collide: false);

            var people = new GameObject("Personajes").transform;
            Figure(people, "Abril", new Vector3(0.6f, 0, 0.1f), -90, AbrilBody, AbrilHair, 1.3f);
            Figure(people, "Bibliotecaria", new Vector3(-2.6f, 0, 6.6f), 180, "#4F5A4A", "#3B302A", 1.6f);

            var lights = new GameObject("Luces").transform;
            var sun = NewLight(lights, "Sol", LightType.Directional, new Color(1f, 0.9f, 0.74f), 0.6f);
            sun.transform.rotation = Quaternion.Euler(45, 70, 0);
            sun.shadows = LightShadows.Soft;
            for (var i = 0; i < SideWalls.Length; i++)
                if (SideWalls[i] == 'V')
                    WindowLight(lights, OnWall(Side.West, w, d, d / 2f - 1f - 2f * i, 1.6f), Vector3.right, new Color(1f, 0.9f, 0.72f), 1.8f);

            Ambient(new Color(0.5f, 0.48f, 0.46f), new Color(0.46f, 0.4f, 0.34f), new Color(0.28f, 0.24f, 0.2f), new Color(0.22f, 0.19f, 0.16f), 15f, 45f);

            var rig = CameraRig("biblioteca").transform;
            Shot(rig, "biblioteca", new Vector3(-2.4f, 1.8f, -7.4f), new Vector3(0.6f, 1f, 1f), 42f, followPlayer: true);

            Volume("VP_Biblioteca", new Look
            {
                Exposure = 0.2f, Contrast = 7f, Saturation = 2f, Filter = new Color(1f, 0.95f, 0.86f),
                FocusDistance = 7f, Aperture = 3.5f, Bloom = 0.5f,
                Vignette = 0.3f, VignetteColor = new Color(0.2f, 0.14f, 0.1f),
            });

            Anchor(new Color(0.14f, 0.12f, 0.1f), new Vector3(0, 0, -6.4f), 0f);
            SaveLocation(scene, "Biblioteca");
        }
    }
}
