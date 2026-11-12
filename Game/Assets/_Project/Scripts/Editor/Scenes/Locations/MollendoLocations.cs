using UnityEditor;
using UnityEngine;
using static UFQ.Editor.Scenes.SceneBuildKit;
using static UFQ.Editor.Scenes.SceneBuildKitExtras;

namespace UFQ.Editor.Scenes.Locations
{
    /// <summary>
    /// Lugares de Mollendo (Capítulo 2, febrero, carnavales). Paleta turquesa y naranja.
    /// El mar queda siempre al oeste (−X) para que todas las escenas sean coherentes.
    /// </summary>
    internal static class MollendoLocations
    {
        private static Material Sea => MaterialAsset("M_Mar", "#3E9AA6", 0.18f, 0.6f, 0.35f);
        private static Material SeaNight => MaterialAsset("M_MarNoche", "#1E3C52", 0.16f, 0.6f, 0.3f);
        private static Material Sand => MaterialAsset("M_Arena", "#E6D3AE", 0.16f, 1.8f, 0.05f);
        private static Material Rock => MaterialAsset("M_Roca", "#6E6A66", 0.2f, 1.2f, 0.1f);

        // ------------------------------------------------------------------ 2.1 / 2.9
        [MenuItem("Una forma de querer/Lugares/Mollendo/Bus (ida y vuelta)")]
        internal static void Bus()
        {
            var scene = NewLocationScene();
            var cabin = new GameObject("Cabina").transform;
            var seat = MaterialAsset("M_Asiento", "#6F4A5E", 0.06f, 4f, 0.1f);
            Block(cabin, "Piso", new Vector3(0, -0.1f, 0), new Vector3(2.6f, 0.2f, 12f), Library("M_Metal"));
            Block(cabin, "Techo", new Vector3(0, 2.3f, 0), new Vector3(2.6f, 0.1f, 12f), MaterialAsset("M_TechoBus", "#D8D2C6", 0.04f), collide: false);
            Block(cabin, "Pared_Este", new Vector3(1.3f, 1.1f, 0), new Vector3(0.1f, 2.2f, 12f), MaterialAsset("M_TechoBus", "#D8D2C6", 0.04f));
            // Pared oeste: ventanas grandes hacia el desierto y el mar
            Block(cabin, "Pared_Oeste_Baja", new Vector3(-1.3f, 0.45f, 0), new Vector3(0.1f, 0.9f, 12f), MaterialAsset("M_TechoBus", "#D8D2C6", 0.04f));
            for (var r = 0; r < 6; r++)
            {
                var z = -4.5f + r * 1.8f;
                Block(cabin, $"Asiento_O_{r}", new Vector3(-0.75f, 0.55f, z), new Vector3(0.95f, 1.1f, 0.6f), seat);
                Block(cabin, $"Asiento_E_{r}", new Vector3(0.75f, 0.55f, z), new Vector3(0.95f, 1.1f, 0.6f), seat);
            }

            var people = new GameObject("Personajes").transform;
            Figure(people, "Abril", new Vector3(-1.0f, 0.2f, 0.3f), 90, "#E3B53F", "#5A3A26", 1.3f);   // sentada, junto a la ventana
            Figure(people, "Sofi", new Vector3(0.95f, 0.2f, 2.1f), 270, "#C77D8E", "#D9B46A", 1.3f);
            Figure(people, "Nico", new Vector3(0.55f, 0.2f, 2.1f), 270, "#4E6B5A", "#2A221E", 1.4f);
            Figure(people, "DonAurelio", new Vector3(0.1f, 0f, 4.2f), 180, "#2C3550", "#BDB8B0", 1.7f);

            // Paisaje: desierto que baja al mar
            var land = new GameObject("Paisaje").transform;
            Block(land, "Desierto", new Vector3(-60f, -2f, 0), new Vector3(110f, 2f, 300f), MaterialAsset("M_Desierto", "#D9B98A", 0.18f, 0.8f, 0.05f), collide: false);
            Block(land, "Mar_Lejano", new Vector3(-150f, -1.5f, 0), new Vector3(80f, 1f, 300f), Sea, collide: false);
            for (var i = 0; i < 8; i++) Block(land, $"Cerro_{i}", new Vector3(-30f - i * 9f, 2f + i % 3, -60f + i * 18f), new Vector3(14f, 8f + i % 4 * 3f, 18f), MaterialAsset("M_Cerro", "#C4A27A", 0.2f, 0.5f, 0.05f), collide: false);

            var lights = new GameObject("Luces").transform;
            var sun = NewLight(lights, "Sol", LightType.Directional, new Color(1f, 0.9f, 0.72f), 1.3f);
            sun.transform.rotation = Quaternion.Euler(35, 100, 0);
            sun.shadows = LightShadows.Soft;
            Ambient(new Color(0.62f, 0.74f, 0.9f), new Color(0.78f, 0.68f, 0.56f), new Color(0.45f, 0.38f, 0.3f), new Color(0.82f, 0.86f, 0.92f), 60f, 400f);

            var rig = CameraRig("bus_ventana").transform;
            Shot(rig, "bus_ventana", new Vector3(0.6f, 1.45f, -1.6f), new Vector3(-0.8f, 1.0f, 0.4f), 48f, followPlayer: false);
            Shot(rig, "bus_vuelta", new Vector3(0.7f, 1.6f, -2.8f), new Vector3(-0.6f, 1.0f, 0.6f), 44f, followPlayer: false);

            Volume("VP_Bus_Verano", DayLook(6.5f));
            Anchor(new Color(0.62f, 0.78f, 0.94f), new Vector3(-0.35f, 0.25f, 0.3f), 90f, null, showPlayer: true);
            SaveLocation(scene, "BusMollendo");
        }

        // ------------------------------------------------------------------ 2.2
        [MenuItem("Una forma de querer/Lugares/Mollendo/Casa de los Benavides")]
        internal static void Casa()
        {
            var scene = NewLocationScene();
            var street = new GameObject("Calle").transform;
            Block(street, "Vereda", new Vector3(0, -0.15f, -2f), new Vector3(30f, 0.3f, 10f), MaterialAsset("M_Vereda", "#C9C1B5", 0.12f, 2.5f, 0.05f));
            var white = MaterialAsset("M_CasaBlanca", "#F2EEE6", 0.1f, 2.5f, 0.1f);
            var blue = MaterialAsset("M_PersianaAzul", "#3F6E9C", 0.06f, 3f, 0.15f);
            Block(street, "Casa", new Vector3(0, 2.6f, 4f), new Vector3(12f, 5.2f, 6f), white);
            for (var i = 0; i < 4; i++) Block(street, $"Ventana_{i}", new Vector3(-4.2f + i * 2.8f, 2.4f, 0.98f), new Vector3(1.1f, 1.5f, 0.1f), blue, collide: false);
            Block(street, "Puerta", new Vector3(0.2f, 1.1f, 0.98f), new Vector3(1.2f, 2.2f, 0.1f), Library("M_Madera"), collide: false);
            // Buganvillas fucsia sobre el muro
            var bugan = MaterialAsset("M_Buganvilla", "#C2337E", 0.25f, 4f, 0.1f);
            for (var i = 0; i < 6; i++) Block(street, $"Buganvilla_{i}", new Vector3(-5.5f + i * 0.7f, 4.6f - (i % 2) * 0.4f, 1.0f), new Vector3(0.9f, 0.7f, 0.5f), bugan, collide: false);
            // Casas vecinas de colores
            string[] colors = { "#E6C35A", "#7BB3B8", "#E59A6B", "#B9D58A" };
            for (var i = 0; i < 4; i++) Block(street, $"Vecina_{i}", new Vector3(i < 2 ? -10f - i * 6f : 10f + (i - 2) * 6f, 2.4f, 4f), new Vector3(5.6f, 4.8f, 6f), MaterialAsset("M_CasaColor_" + i, colors[i], 0.1f, 2.5f, 0.1f));

            var people = new GameObject("Personajes").transform;
            Figure(people, "Marta", new Vector3(0.8f, 0, -0.4f), 180, "#5D7FA6", "#4A3428", 1.62f);
            Figure(people, "Abril", new Vector3(-0.6f, 0, -1.6f), 20, "#E3B53F", "#5A3A26", 1.64f);
            Figure(people, "Sofi", new Vector3(1.6f, 0, -1.8f), 340, "#C77D8E", "#D9B46A", 1.70f);
            Figure(people, "Nico", new Vector3(2.8f, 0, -2.4f), 330, "#4E6B5A", "#2A221E", 1.83f);

            DayLight(new Vector3(50, 120, 0));
            var rig = CameraRig("casa_benavides").transform;
            Shot(rig, "casa_benavides", new Vector3(-3.5f, 1.8f, -8.5f), new Vector3(0.5f, 1.6f, 0.5f), 40f, followPlayer: true);
            Volume("VP_Mollendo_Dia", DayLook(8.5f));
            Anchor(new Color(0.55f, 0.75f, 0.95f), new Vector3(-1.6f, 0f, -2.6f), 20f);
            SaveLocation(scene, "CasaBenavides");
        }

        // ------------------------------------------------------------------ 2.3 / 2.4
        [MenuItem("Una forma de querer/Lugares/Mollendo/Primera Playa")]
        internal static void Playa()
        {
            var scene = NewLocationScene();
            var beach = new GameObject("Playa").transform;
            Block(beach, "Arena", new Vector3(10f, -0.3f, 0), new Vector3(40f, 0.6f, 60f), Sand);
            Block(beach, "Mar", new Vector3(-40f, -0.45f, 0), new Vector3(60f, 0.3f, 120f), Sea, collide: false);
            Block(beach, "Orilla", new Vector3(-10.5f, -0.05f, 0), new Vector3(1.2f, 0.05f, 60f), MaterialAsset("M_Espuma", "#F4F1EA", 0.1f, 2f, 0.1f), collide: false);
            // Malla de vóley y sombrillas
            Block(beach, "Poste_Voley_A", new Vector3(4f, 1.2f, -4f), new Vector3(0.1f, 2.4f, 0.1f), Library("M_Madera"));
            Block(beach, "Poste_Voley_B", new Vector3(4f, 1.2f, 4f), new Vector3(0.1f, 2.4f, 0.1f), Library("M_Madera"));
            Block(beach, "Malla", new Vector3(4f, 2.0f, 0), new Vector3(0.03f, 0.8f, 8f), MaterialAsset("M_Malla", "#F2EEE6", 0.02f), collide: false);
            Rocks(beach, new Vector3(-6f, 0, 18f), 9, 3);
            Rocks(beach, new Vector3(-7f, 0, -20f), 7, 5);

            var people = new GameObject("Personajes").transform;
            Figure(people, "Abril", new Vector3(-7.5f, 0, 1.5f), 270, "#E3B53F", "#5A3A26", 1.64f);
            Figure(people, "Sofi", new Vector3(-8.2f, 0, 3.2f), 260, "#C77D8E", "#D9B46A", 1.70f);
            Figure(people, "Nico", new Vector3(1.2f, 0, -1.2f), 250, "#4E6B5A", "#2A221E", 1.83f);

            DayLight(new Vector3(30, 80, 0), warm: true);
            var rig = CameraRig("primera_playa").transform;
            Shot(rig, "primera_playa", new Vector3(6.5f, 2.2f, -7.5f), new Vector3(-4f, 0.8f, 1.5f), 42f, followPlayer: true);
            Shot(rig, "playa_atardecer", new Vector3(4.5f, 1.4f, -3.2f), new Vector3(-12f, 1.2f, 1f), 40f, followPlayer: false);
            Shot(rig, "nico_solo", new Vector3(3.6f, 1.2f, -3.6f), new Vector3(1.2f, 0.8f, -1.2f), 30f, followPlayer: false);
            Volume("VP_Playa_Atardecer", SunsetLook());
            Anchor(new Color(0.96f, 0.72f, 0.5f), new Vector3(1.8f, 0f, -0.8f), 250f);
            SaveLocation(scene, "Playa");
        }

        // ------------------------------------------------------------------ 2.5
        [MenuItem("Una forma de querer/Lugares/Mollendo/Feria del carnaval")]
        internal static void Feria()
        {
            var scene = NewLocationScene();
            var fair = new GameObject("Feria").transform;
            Block(fair, "Piso", new Vector3(0, -0.15f, 0), new Vector3(36f, 0.3f, 26f), MaterialAsset("M_TierraApisonada", "#B79C7E", 0.2f, 2f, 0.04f));

            // Rueda de Chicago: aro de cabinas alrededor de un eje
            var wheel = new GameObject("Rueda_Chicago").transform;
            wheel.SetParent(fair, false);
            wheel.position = new Vector3(-8f, 9f, 8f);
            var frame = MaterialAsset("M_RuedaMetal", "#E6E0D2", 0.03f, 3f, 0.3f);
            Block(fair, "Soporte_A", new Vector3(-8f, 4.5f, 7.2f), new Vector3(0.4f, 9f, 0.4f), frame);
            Block(fair, "Soporte_B", new Vector3(-8f, 4.5f, 8.8f), new Vector3(0.4f, 9f, 0.4f), frame);
            string[] cabinColors = { "#C8463C", "#3E9AA6", "#E6C35A", "#B7457A", "#7DA246", "#E07A32" };
            for (var i = 0; i < 12; i++)
            {
                var a = i / 12f * Mathf.PI * 2f;
                var offset = new Vector3(Mathf.Cos(a) * 7f, Mathf.Sin(a) * 7f, 0);
                Block(wheel, $"Rayo_{i}", wheel.position + offset * 0.5f, new Vector3(0.12f, 7f, 0.12f), frame, collide: false)
                    .transform.rotation = Quaternion.Euler(0, 0, Mathf.Rad2Deg * a - 90f);
                Block(wheel, $"Cabina_{i}", wheel.position + offset + Vector3.down * 0.6f, new Vector3(1.2f, 1.1f, 1.4f),
                    MaterialAsset("M_Cabina_" + (i % 6), cabinColors[i % 6], 0.05f, 3f, 0.2f), collide: false);
            }

            // Puestos con focos
            var bulbs = MaterialAsset("M_Foco", "#FFD38A", 0f, 1f, 0f);
            bulbs.SetFloat("_GlowStrength", 1f);
            bulbs.SetColor("_GlowColor", new Color(2.4f, 1.6f, 0.7f));
            for (var i = 0; i < 5; i++)
            {
                var x = -2f + i * 4.2f;
                Block(fair, $"Puesto_{i}", new Vector3(x, 1.1f, -6f), new Vector3(3.4f, 2.2f, 2f), MaterialAsset("M_Toldo_" + (i % 3), i % 3 == 0 ? "#B5523B" : i % 3 == 1 ? "#3E5A6E" : "#C2A15A", 0.05f));
                for (var b = 0; b < 6; b++) Block(fair, $"Foco_{i}_{b}", new Vector3(x - 1.4f + b * 0.56f, 2.35f, -5f), Vector3.one * 0.14f, bulbs, collide: false);
            }

            var people = new GameObject("Personajes").transform;
            Figure(people, "Abril", new Vector3(1.6f, 0, -3.6f), 180, "#E3B53F", "#5A3A26", 1.64f);
            Figure(people, "Sofi", new Vector3(3.0f, 0, -3.2f), 200, "#C77D8E", "#D9B46A", 1.70f);
            Figure(people, "Nico", new Vector3(4.4f, 0, -2.6f), 210, "#4E6B5A", "#2A221E", 1.83f);
            Figure(people, "Lalo", new Vector3(6.5f, 0, -4.2f), 160, "#3F6E9C", "#2A221E", 1.15f);

            NightLight(new Color(0.18f, 0.16f, 0.32f));
            for (var i = 0; i < 4; i++)
            {
                var lamp = NewLight(new GameObject("Luces_Feria").transform, $"Luz_Feria_{i}", LightType.Point, i % 2 == 0 ? new Color(1f, 0.6f, 0.4f) : new Color(0.6f, 0.75f, 1f), 1.6f);
                lamp.transform.position = new Vector3(-2f + i * 5f, 3f, -4.5f);
                lamp.range = 8f;
            }

            var rig = CameraRig("feria").transform;
            Shot(rig, "feria", new Vector3(8.5f, 2.2f, -11f), new Vector3(0f, 2.2f, 0f), 44f, followPlayer: true);
            Shot(rig, "rueda_chicago", new Vector3(-9.6f, 15.6f, 4.2f), new Vector3(-30f, 2f, 10f), 46f, followPlayer: false);
            Volume("VP_Feria_Noche", NightLook(0.95f));
            Anchor(new Color(0.08f, 0.07f, 0.18f), new Vector3(2.4f, 0f, -2.2f), 190f, new[] { ("2.5b", new Vector3(-8f, 2.4f, 1.2f), 0f) });
            SaveLocation(scene, "Feria");
        }

        // ------------------------------------------------------------------ 2.6
        [MenuItem("Una forma de querer/Lugares/Mollendo/Castillo Forga")]
        internal static void Castillo()
        {
            var scene = NewLocationScene();
            var stone = MaterialAsset("M_CastilloPiedra", "#D9CDB8", 0.16f, 1.4f, 0.1f);
            var castle = new GameObject("Castillo_Forga").transform;
            Rocks(castle, new Vector3(0, -1.2f, 6f), 12, 9, scale: 2.2f);
            Block(castle, "Roca_Base", new Vector3(0, -1f, 2f), new Vector3(16f, 2f, 12f), Rock);
            Block(castle, "Cuerpo", new Vector3(0, 4.5f, 4.5f), new Vector3(11f, 9f, 6f), stone);
            for (var i = 0; i < 2; i++)
            {
                var tower = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                tower.name = $"Torre_{i}";
                tower.transform.SetParent(castle, false);
                tower.transform.position = new Vector3(i == 0 ? -5.8f : 5.8f, 6.5f, 2.5f);
                tower.transform.localScale = new Vector3(3f, 6.5f, 3f);
                tower.GetComponent<Renderer>().sharedMaterial = stone;
                tower.isStatic = true;
                for (var c = 0; c < 8; c++)
                {
                    var a = c / 8f * Mathf.PI * 2f;
                    Block(castle, $"Almena_{i}_{c}", tower.transform.position + new Vector3(Mathf.Cos(a) * 1.4f, 6.8f, Mathf.Sin(a) * 1.4f), new Vector3(0.45f, 0.6f, 0.45f), stone, collide: false);
                }
            }

            // Andamios de restauración (está cerrado, guion 2.6)
            var scaffold = Library("M_Metal");
            for (var i = 0; i < 4; i++) Block(castle, $"Andamio_{i}", new Vector3(-3f + i * 2f, 4.5f, 1.25f), new Vector3(0.08f, 9f, 0.08f), scaffold, collide: false);
            for (var j = 0; j < 4; j++) Block(castle, $"Tablon_{j}", new Vector3(0, 1.8f + j * 2.2f, 1.15f), new Vector3(6.4f, 0.08f, 0.5f), Library("M_Madera"), collide: false);
            // Muro exterior donde se sientan, sobre el mar
            Block(castle, "Muro_Mirador", new Vector3(-2f, 0.45f, -3.6f), new Vector3(7f, 0.9f, 0.7f), stone);
            Block(castle, "Mar", new Vector3(0, -2.2f, -40f), new Vector3(160f, 0.3f, 70f), SeaNight, collide: false);

            var people = new GameObject("Personajes").transform;
            Figure(people, "Abril", new Vector3(-1.4f, 0.9f, -3.6f), 180, "#E3B53F", "#5A3A26", 1.1f);

            NightLight(new Color(0.16f, 0.17f, 0.34f), moonIntensity: 0.6f);
            var rig = CameraRig("castillo_forga").transform;
            Shot(rig, "castillo_forga", new Vector3(7.5f, 3.8f, -15f), new Vector3(-0.5f, 4.2f, 1.5f), 44f, followPlayer: false);
            Shot(rig, "castillo_muro", new Vector3(1.2f, 1.7f, -7.2f), new Vector3(-1.6f, 1.3f, -3.4f), 34f, followPlayer: true);
            Volume("VP_Castillo_Noche", NightLook(0.7f));
            Anchor(new Color(0.06f, 0.08f, 0.18f), new Vector3(-2.8f, 0.9f, -3.6f), 180f);
            SaveLocation(scene, "CastilloForga");
        }

        // ------------------------------------------------------------------ 2.7
        [MenuItem("Una forma de querer/Lugares/Mollendo/Fogata en la playa")]
        internal static void Fogata()
        {
            var scene = NewLocationScene();
            var beach = new GameObject("Playa").transform;
            Block(beach, "Arena", new Vector3(10f, -0.3f, 0), new Vector3(40f, 0.6f, 60f), Sand);
            Block(beach, "Mar", new Vector3(-40f, -0.45f, 0), new Vector3(60f, 0.3f, 120f), SeaNight, collide: false);
            var embers = MaterialAsset("M_Brasas", "#FF8A3A", 0f, 1f, 0f);
            embers.SetFloat("_GlowStrength", 1f);
            embers.SetColor("_GlowColor", new Color(3f, 1.4f, 0.4f));
            Block(beach, "Fogata", new Vector3(0, 0.15f, 0), new Vector3(0.9f, 0.3f, 0.9f), embers, collide: false);
            for (var i = 0; i < 4; i++)
            {
                var a = i / 4f * Mathf.PI * 2f;
                Block(beach, $"Manta_{i}", new Vector3(Mathf.Cos(a) * 2.2f, 0.03f, Mathf.Sin(a) * 2.2f), new Vector3(1.6f, 0.06f, 1.2f), MaterialAsset("M_Manta_" + i, i % 2 == 0 ? "#B5523B" : "#3E5A6E", 0.1f), collide: false);
            }

            var people = new GameObject("Personajes").transform;
            Figure(people, "Nico", new Vector3(2.0f, 0, 0.6f), 260, "#4E6B5A", "#2A221E", 1.2f);
            Figure(people, "Abril", new Vector3(2.0f, 0, -0.5f), 280, "#E3B53F", "#5A3A26", 1.1f);
            Figure(people, "Sofi", new Vector3(-0.6f, 0, 2.1f), 170, "#C77D8E", "#D9B46A", 1.15f);

            NightLight(new Color(0.12f, 0.12f, 0.26f), moonIntensity: 0.35f);
            var fire = NewLight(new GameObject("Luz_Fogata").transform, "Fuego", LightType.Point, new Color(1f, 0.55f, 0.25f), 2.6f);
            fire.transform.position = new Vector3(0, 0.8f, 0);
            fire.range = 7f;
            fire.shadows = LightShadows.Soft;

            var rig = CameraRig("fogata").transform;
            Shot(rig, "fogata", new Vector3(-3.6f, 1.5f, -3.6f), new Vector3(0.6f, 0.6f, 0.4f), 40f, followPlayer: false);
            Volume("VP_Fogata", NightLook(0.9f));
            Anchor(new Color(0.05f, 0.06f, 0.14f), new Vector3(-1.6f, 0f, -1.4f), 45f);
            SaveLocation(scene, "Fogata");
        }

        // ------------------------------------------------------------------ 2.8
        [MenuItem("Una forma de querer/Lugares/Mollendo/Muelle antiguo")]
        internal static void Muelle()
        {
            var scene = NewLocationScene();
            var pier = new GameObject("Muelle").transform;
            var wood = Library("M_Madera");
            Block(pier, "Tablones", new Vector3(0, 0, 14f), new Vector3(3.2f, 0.25f, 40f), wood);
            for (var i = 0; i < 10; i++)
            {
                Block(pier, $"Pilote_O_{i}", new Vector3(-1.5f, -2f, -4f + i * 4f), new Vector3(0.3f, 4f, 0.3f), wood, collide: false);
                Block(pier, $"Pilote_E_{i}", new Vector3(1.5f, -2f, -4f + i * 4f), new Vector3(0.3f, 4f, 0.3f), wood, collide: false);
            }

            // Rieles oxidados del antiguo puerto
            var rust = MaterialAsset("M_Oxido", "#7A4A34", 0.18f, 3f, 0.2f);
            Block(pier, "Riel_A", new Vector3(-0.5f, 0.15f, 14f), new Vector3(0.08f, 0.06f, 40f), rust, collide: false);
            Block(pier, "Riel_B", new Vector3(0.5f, 0.15f, 14f), new Vector3(0.08f, 0.06f, 40f), rust, collide: false);
            Block(pier, "Baranda_O", new Vector3(-1.55f, 0.6f, 14f), new Vector3(0.08f, 0.9f, 40f), wood);
            Block(pier, "Baranda_E", new Vector3(1.55f, 0.6f, 14f), new Vector3(0.08f, 0.9f, 40f), wood);
            Block(pier, "Mar", new Vector3(0, -1.6f, 10f), new Vector3(160f, 0.3f, 160f), SeaNight, collide: false);
            Block(pier, "Farol_Poste", new Vector3(0.9f, 1.6f, 31f), new Vector3(0.12f, 3.2f, 0.12f), Library("M_Metal"), collide: false);

            var people = new GameObject("Personajes").transform;
            Figure(people, "Daniel", new Vector3(0.3f, 0.12f, 31.5f), 200, "#3A3F55", "#2A221E", 1.2f);   // sentado con la guitarra
            Figure(people, "Abril", new Vector3(-0.4f, 0.12f, 30.4f), 20, "#E3B53F", "#5A3A26", 1.0f);

            NightLight(new Color(0.12f, 0.13f, 0.28f), moonIntensity: 0.45f);
            var lamp = NewLight(new GameObject("Luz_Farol").transform, "Farol", LightType.Point, new Color(1f, 0.78f, 0.5f), 2.2f);
            lamp.transform.position = new Vector3(0.9f, 3.3f, 31f);
            lamp.range = 8f;
            lamp.shadows = LightShadows.Soft;

            var rig = CameraRig("muelle_lejos").transform;
            // Desde 20 m atrás: Mateo se queda lejos, con los dos helados.
            Shot(rig, "muelle_lejos", new Vector3(1.0f, 1.7f, 6.5f), new Vector3(0f, 0.9f, 31f), 26f, followPlayer: false);
            Volume("VP_Muelle_Noche", NightLook(0.8f, focus: 24f));
            Anchor(new Color(0.04f, 0.05f, 0.13f), new Vector3(0f, 0.15f, 10.5f), 0f);
            SaveLocation(scene, "Muelle");
        }

        // ------------------------------------------------------------------ utilidades
        private static void Rocks(Transform parent, Vector3 center, int count, int seed, float scale = 1f)
        {
            var random = new System.Random(seed);
            for (var i = 0; i < count; i++)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"Roca_{seed}_{i}";
                rock.transform.SetParent(parent, false);
                rock.transform.position = center + new Vector3(((float)random.NextDouble() - 0.5f) * 8f * scale, 0, ((float)random.NextDouble() - 0.5f) * 8f * scale);
                rock.transform.localScale = new Vector3(1.5f + (float)random.NextDouble() * 2f, 0.8f + (float)random.NextDouble() * 1.4f, 1.5f + (float)random.NextDouble() * 2f) * scale;
                rock.transform.rotation = Quaternion.Euler(0, (float)random.NextDouble() * 360f, 0);
                rock.GetComponent<Renderer>().sharedMaterial = Rock;
                rock.isStatic = true;
            }
        }

        private static void DayLight(Vector3 sunEuler, bool warm = false)
        {
            var lights = new GameObject("Luces").transform;
            var sun = NewLight(lights, "Sol", LightType.Directional, warm ? new Color(1f, 0.78f, 0.55f) : new Color(1f, 0.93f, 0.8f), warm ? 1.15f : 1.3f);
            sun.transform.rotation = Quaternion.Euler(sunEuler);
            sun.shadows = LightShadows.Soft;
            if (warm)
                Ambient(new Color(0.72f, 0.62f, 0.66f), new Color(0.86f, 0.6f, 0.46f), new Color(0.42f, 0.34f, 0.3f), new Color(0.95f, 0.7f, 0.55f), 70f, 380f);
            else
                Ambient(new Color(0.6f, 0.74f, 0.92f), new Color(0.76f, 0.68f, 0.58f), new Color(0.44f, 0.38f, 0.32f), new Color(0.8f, 0.86f, 0.94f), 70f, 380f);
        }

        private static void NightLight(Color sky, float moonIntensity = 0.4f)
        {
            var lights = new GameObject("Luces").transform;
            var moon = NewLight(lights, "Luna", LightType.Directional, new Color(0.58f, 0.66f, 0.95f), moonIntensity);
            moon.transform.rotation = Quaternion.Euler(32, 250, 0);
            moon.shadows = LightShadows.Soft;
            Ambient(sky, sky * 1.15f, sky * 0.45f, sky * 0.9f, 50f, 260f);
        }

        private static Look DayLook(float focus) => new Look
        {
            Exposure = 0.1f, Contrast = 6f, Saturation = 14f, Filter = new Color(1f, 0.97f, 0.92f),
            FocusDistance = focus, Aperture = 4.2f, Bloom = 0.35f,
            Vignette = 0.22f, VignetteColor = new Color(0.2f, 0.16f, 0.12f),
        };

        private static Look SunsetLook() => new Look
        {
            Exposure = 0.05f, Contrast = 9f, Saturation = 16f, Filter = new Color(1f, 0.88f, 0.8f),
            FocusDistance = 9f, Aperture = 4f, Bloom = 0.6f,
            Vignette = 0.28f, VignetteColor = new Color(0.3f, 0.12f, 0.1f),
        };

        private static Look NightLook(float bloom, float focus = 6f) => new Look
        {
            Exposure = 0f, Contrast = 9f, Saturation = 8f, Filter = new Color(0.96f, 0.94f, 1f),
            FocusDistance = focus, Aperture = 3.4f, Bloom = bloom,
            Vignette = 0.32f, VignetteColor = new Color(0.06f, 0.04f, 0.12f),
        };
    }
}
