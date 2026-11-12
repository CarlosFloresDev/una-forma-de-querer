using System;
using UFQ.Presentation.Input;
using UFQ.UseCases.Events;
using UnityEngine;
using VContainer;

namespace UFQ.Presentation.Player
{
    /// <summary>
    /// Movimiento de Mateo: paso tranquilo relativo a la cámara (juego contemplativo,
    /// sin correr ni saltar). Se detiene mientras hay un diálogo en pantalla.
    /// Si hay un Animator con el parámetro «Speed», lo alimenta (0 = quieto, 1 = caminando).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMover : MonoBehaviour, ISceneInjectable
    {
        private static readonly int SpeedParam = Animator.StringToHash("Speed");

        [SerializeField] private float walkSpeed = 1.5f;
        [SerializeField] private float turnSpeed = 540f;
        [SerializeField] private float acceleration = 6f;
        [SerializeField] private Animator animator;

        private CharacterController _controller;
        private GameInput _input;
        private IDisposable _onStart;
        private IDisposable _onEnd;
        private int _blockers;
        private float _currentSpeed;
        private float _verticalVelocity;
        private bool _hasSpeedParam;

        public bool CanMove => _blockers == 0;

        [Inject]
        public void Construct(GameInput input, IEventBus bus)
        {
            _input = input;
            _onStart = bus.Subscribe<DialogueStarted>(_ => _blockers++);
            _onEnd = bus.Subscribe<DialogueEnded>(_ => _blockers = Mathf.Max(0, _blockers - 1));
        }

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator != null)
            {
                foreach (var p in animator.parameters)
                {
                    if (p.nameHash == SpeedParam) _hasSpeedParam = true;
                }
            }
        }

        private void Update()
        {
            if (_input == null) return;

            var desired = CanMove ? CameraRelative(_input.MoveValue) : Vector3.zero;
            var targetSpeed = Mathf.Clamp01(desired.magnitude) * walkSpeed;
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, targetSpeed, acceleration * Time.deltaTime);

            if (desired.sqrMagnitude > 0.0001f)
            {
                var look = Quaternion.LookRotation(desired.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
            }

            _verticalVelocity = _controller.isGrounded ? -1f : _verticalVelocity + Physics.gravity.y * Time.deltaTime;
            var motion = transform.forward * _currentSpeed + Vector3.up * _verticalVelocity;
            _controller.Move(motion * Time.deltaTime);

            if (_hasSpeedParam) animator.SetFloat(SpeedParam, _currentSpeed / Mathf.Max(0.01f, walkSpeed), 0.1f, Time.deltaTime);
        }

        private static Vector3 CameraRelative(Vector2 input)
        {
            var camera = Camera.main;
            if (camera == null) return new Vector3(input.x, 0, input.y);

            var forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;
            return forward * input.y + right * input.x;
        }

        private void OnDestroy()
        {
            _onStart?.Dispose();
            _onEnd?.Dispose();
        }
    }
}
