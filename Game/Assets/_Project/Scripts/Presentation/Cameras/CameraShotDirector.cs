using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UFQ.UseCases.Events;
using UnityEngine;
using VContainer;

namespace UFQ.Presentation.Cameras
{
    /// <summary>
    /// Planos de cámara dirigidos desde Ink: «#camara:azotea_plano_general» activa la
    /// CinemachineCamera hija con ese nombre (Cinemachine hace la mezcla). Así el guion
    /// decide el encuadre sin que el código conozca cada escena.
    /// </summary>
    public sealed class CameraShotDirector : MonoBehaviour, ISceneInjectable
    {
        [Tooltip("Plano inicial (vacío = el primero que encuentre).")]
        [SerializeField] private string initialShot;

        private readonly Dictionary<string, CinemachineCamera> _shots = new Dictionary<string, CinemachineCamera>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _warned = new HashSet<string>();
        private IDisposable _subscription;

        public string CurrentShot { get; private set; }

        [Inject]
        public void Construct(IEventBus bus)
        {
            _subscription = bus.Subscribe<DialogueCue>(OnCue);
        }

        private void Awake()
        {
            foreach (var shot in GetComponentsInChildren<CinemachineCamera>(true)) _shots[shot.name] = shot;
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(initialShot)) Cut(initialShot);
        }

        public bool Cut(string shotName)
        {
            if (!_shots.TryGetValue(shotName, out var shot))
            {
                if (_warned.Add(shotName)) Debug.LogWarning($"[UFQ] No hay plano «{shotName}» en {name}. Planos: {string.Join(", ", _shots.Keys)}", this);
                return false;
            }

            shot.Prioritize();
            CurrentShot = shotName;
            return true;
        }

        private void OnCue(DialogueCue cue)
        {
            if (cue.Tag.Key == "camara") Cut(cue.Tag.Value);
        }

        private void OnDestroy() => _subscription?.Dispose();
    }
}
