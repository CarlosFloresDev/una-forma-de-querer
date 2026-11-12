using System.Threading;
using Cysharp.Threading.Tasks;
using UFQ.Presentation.Dialogue;
using UFQ.Presentation.Player;
using UFQ.UseCases.Dialogue;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UFQ.Presentation.Locations
{
    /// <summary>
    /// Carga los lugares del juego como escenas ADITIVAS sobre la escena persistente
    /// «Juego» (UI, cámara, Mateo). Reacciona a «#escena:1.2» desde Ink:
    /// fundido a negro → descarga el lugar anterior → carga el nuevo → coloca a Mateo
    /// en su punto de aparición → fundido de vuelta. Si el lugar no existe todavía,
    /// avisa y el diálogo sigue (así el juego se puede construir por partes).
    /// </summary>
    public sealed class LocationLoader : IDialogueCueHandler
    {
        private const float FadeSeconds = 0.6f;

        private readonly LocationMap _map;
        private readonly ScreenFader _fader;
        private readonly PlayerMover _player;

        public LocationLoader(LocationMap map, ScreenFader fader, PlayerMover player)
        {
            _map = map;
            _fader = fader;
            _player = player;
        }

        public string CurrentScene { get; private set; }

        public bool CanHandle(DialogueTag tag) => tag.Key == "escena";

        public UniTask HandleAsync(DialogueTag tag, CancellationToken token) => GoToAsync(tag.Value, token);

        /// <summary>Va al lugar de la escena del guion (p. ej. «1.4»).</summary>
        public async UniTask GoToAsync(string scriptScene, CancellationToken token)
        {
            if (!_map.TryGetUnityScene(scriptScene, out var unityScene))
            {
                Debug.LogWarning($"[UFQ] El mapa de lugares no tiene la escena del guion «{scriptScene}».");
                return;
            }

            if (unityScene == CurrentScene)
            {
                PlacePlayer(scriptScene); // mismo lugar, otro momento: solo recolocar
                return;
            }

            if (!Application.CanStreamedLevelBeLoaded(unityScene))
            {
                Debug.LogWarning($"[UFQ] El lugar «{unityScene}» (escena {scriptScene}) aún no existe o no está en Build Settings.");
                return;
            }

            await _fader.FadeOutAsync(FadeSeconds, token);

            if (!string.IsNullOrEmpty(CurrentScene))
            {
                var previous = SceneManager.GetSceneByName(CurrentScene);
                if (previous.isLoaded) await SceneManager.UnloadSceneAsync(previous).ToUniTask(cancellationToken: token);
            }

            await SceneManager.LoadSceneAsync(unityScene, LoadSceneMode.Additive).ToUniTask(cancellationToken: token);
            var scene = SceneManager.GetSceneByName(unityScene);
            SceneManager.SetActiveScene(scene); // la iluminación y la niebla vienen de la escena activa
            CurrentScene = unityScene;

            PlacePlayer(scriptScene);
            await UniTask.Yield(PlayerLoopTiming.Update, token); // un frame para que Cinemachine se asiente
            await _fader.FadeInAsync(FadeSeconds, token);
        }

        private void PlacePlayer(string scriptScene)
        {
            var anchor = FindAnchor();
            if (anchor == null || _player == null) return;

            var spawn = anchor.SpawnFor(scriptScene);
            _player.Teleport(spawn.position, spawn.rotation);
            _player.SetVisible(anchor.PlayerVisible);

            var camera = Camera.main;
            if (camera != null) camera.backgroundColor = anchor.SkyColor;
        }

        private LocationAnchor FindAnchor()
        {
            if (string.IsNullOrEmpty(CurrentScene)) return null;
            var scene = SceneManager.GetSceneByName(CurrentScene);
            if (!scene.isLoaded) return null;
            foreach (var root in scene.GetRootGameObjects())
            {
                var anchor = root.GetComponentInChildren<LocationAnchor>(true);
                if (anchor != null) return anchor;
            }

            return null;
        }
    }
}
