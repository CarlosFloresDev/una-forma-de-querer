using UFQ.Bootstrap;
using UFQ.Editor.Setup;
using UFQ.Presentation.Dialogue;
using UFQ.Presentation.Locations;
using UFQ.Presentation.Notebook;
using UFQ.Presentation.Player;
using UFQ.Presentation.Scenes;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

namespace UFQ.Editor.Scenes
{
    /// <summary>
    /// Escena persistente «Juego» (índice 0 del build): cámara principal, interfaz,
    /// Mateo, director de capítulos y el ámbito que carga los lugares por encima.
    /// Los lugares (Azotea, Librería…) se cargan de forma aditiva desde Ink (#escena).
    /// </summary>
    internal static class GameSceneBuilder
    {
        private const string MateoModel = "Assets/_Project/Art/Characters/Mateo/CH_Mateo.fbx";

        [MenuItem("Una forma de querer/Escenas/Construir escena Juego (persistente)")]
        internal static void Build()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            ProjectAssetsBuilder.BuildAll();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(ProjectAssetsBuilder.PanelSettingsPath);

            BuildCamera();
            BuildPlayer();

            var dialogue = new GameObject("UI_Dialogo");
            var dialogueDoc = dialogue.AddComponent<UIDocument>();
            dialogueDoc.panelSettings = panel;
            dialogueDoc.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/_Project/UI/Dialogue/Dialogo.uxml");
            dialogueDoc.sortingOrder = 0;
            dialogue.AddComponent<DialogueView>();

            var meter = new GameObject("UI_Cuaderno");
            var meterDoc = meter.AddComponent<UIDocument>();
            meterDoc.panelSettings = panel;
            meterDoc.sortingOrder = -1;
            meter.AddComponent<IllusionMeterView>();

            var fade = new GameObject("UI_Fundido");
            var fadeDoc = fade.AddComponent<UIDocument>();
            fadeDoc.panelSettings = panel;
            fadeDoc.sortingOrder = 10; // por encima de todo
            fade.AddComponent<ScreenFader>();

            new GameObject("Director").AddComponent<ChapterDirector>();

            var scope = new GameObject("GameplayLifetimeScope").AddComponent<GameplayLifetimeScope>();
            var so = new SerializedObject(scope);
            so.FindProperty("locationMap").objectReferenceValue = AssetDatabase.LoadAssetAtPath<LocationMap>(ProjectAssetsBuilder.LocationMapPath);
            so.ApplyModifiedPropertiesWithoutUndo();

            System.IO.Directory.CreateDirectory("Assets/_Project/Scenes/Boot");
            EditorSceneManager.SaveScene(scene, BuildSettingsSync.GameScenePath);
            BuildSettingsSync.EnsureScene(BuildSettingsSync.GameScenePath);
            Debug.Log("[UFQ] Escena Juego construida.");
        }

        private static void BuildCamera()
        {
            var go = new GameObject("Main Camera") { tag = "MainCamera" };
            var camera = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.fieldOfView = 34f;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 400f;
            var data = go.AddComponent<UniversalAdditionalCameraData>();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var brain = go.AddComponent<CinemachineBrain>();
            brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1.2f);
        }

        private static void BuildPlayer()
        {
            var player = new GameObject("Mateo");
            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.72f;
            controller.radius = 0.24f;
            controller.center = new Vector3(0, 0.86f, 0);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(MateoModel);
            if (model != null)
            {
                var visual = (GameObject)PrefabUtility.InstantiatePrefab(model, player.transform);
                visual.transform.localPosition = Vector3.zero;
            }
            else
            {
                // Provisional hasta que exista CH_Mateo.fbx.
                var capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.name = "Provisional";
                Object.DestroyImmediate(capsule.GetComponent<Collider>());
                capsule.transform.SetParent(player.transform, false);
                capsule.transform.localPosition = new Vector3(0, 0.86f, 0);
                capsule.transform.localScale = new Vector3(0.48f, 0.86f, 0.48f);
                capsule.GetComponent<Renderer>().sharedMaterial = SceneBuildKit.Library("M_Tela");
            }

            player.AddComponent<PlayerMover>();
        }
    }
}
