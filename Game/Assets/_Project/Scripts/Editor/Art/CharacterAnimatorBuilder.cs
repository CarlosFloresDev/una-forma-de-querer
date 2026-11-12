using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace UFQ.Editor.Art
{
    /// <summary>
    /// Crea el Animator Controller de un personaje a partir de los clips de su FBX:
    /// un árbol de mezcla 1D por «Speed» (0 = Idle, 1 = Walk), el parámetro que
    /// alimenta PlayerMover. Los clips extra (LookPhone…) quedan como estados aparte.
    /// </summary>
    internal static class CharacterAnimatorBuilder
    {
        internal const string MateoFbx = "Assets/_Project/Art/Characters/Mateo/CH_Mateo.fbx";
        internal const string MateoController = "Assets/_Project/Animation/AC_Mateo.controller";

        [MenuItem("Una forma de querer/Arte/Crear Animator de Mateo")]
        internal static RuntimeAnimatorController BuildMateo() => Build(MateoFbx, MateoController);

        internal static RuntimeAnimatorController Build(string fbxPath, string controllerPath)
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(fbxPath).OfType<AnimationClip>()
                .Where(c => !c.name.StartsWith("__preview__")).ToArray();
            var idle = clips.FirstOrDefault(c => c.name.EndsWith("Idle"));
            var walk = clips.FirstOrDefault(c => c.name.EndsWith("Walk"));
            if (idle == null || walk == null)
            {
                Debug.LogError($"[UFQ] {fbxPath} no tiene clips Idle y Walk (encontrados: {string.Join(", ", clips.Select(c => c.name))}).");
                return null;
            }

            AssetDatabase.DeleteAsset(controllerPath); // se regenera entero: es un asset derivado
            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);

            var machine = controller.layers[0].stateMachine;
            var locomotion = controller.CreateBlendTreeInController("Locomocion", out var tree);
            tree.blendType = BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            tree.AddChild(idle, 0f);
            tree.AddChild(walk, 1f);
            machine.defaultState = locomotion;

            foreach (var extra in clips.Where(c => c != idle && c != walk))
            {
                var state = machine.AddState(extra.name.Split('|').Last());
                state.motion = extra;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"[UFQ] Animator creado: {controllerPath} (Idle/Walk por Speed + {clips.Length - 2} estado(s) extra)");
            return controller;
        }
    }
}
