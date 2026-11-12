using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UFQ.Presentation.Input
{
    /// <summary>
    /// Controles del juego, definidos en código para que sean fáciles de revisar y
    /// versionar. Teclado + mando desde el principio.
    ///
    ///  Mover ............ WASD / flechas · stick izquierdo
    ///  Avanzar texto .... Espacio / Enter / clic · A (sur)
    ///  Guardar señal .... E · X (oeste)       ← el botón dorado
    ///  Mirar ............ Q · Y (norte)        ← nunca se anuncia (gestos de Valeria)
    ///  Pausa ............ Esc · Start
    /// </summary>
    public sealed class GameInput : IDisposable
    {
        private readonly InputActionMap _map = new InputActionMap("Juego");

        public GameInput()
        {
            Move = _map.AddAction("Mover", InputActionType.Value, expectedControlLayout: "Vector2");
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
            Move.AddBinding("<Gamepad>/leftStick");

            Advance = _map.AddAction("Avanzar", InputActionType.Button);
            Advance.AddBinding("<Keyboard>/space");
            Advance.AddBinding("<Keyboard>/enter");
            Advance.AddBinding("<Mouse>/leftButton");
            Advance.AddBinding("<Gamepad>/buttonSouth");

            Save = _map.AddAction("Guardar", InputActionType.Button);
            Save.AddBinding("<Keyboard>/e");
            Save.AddBinding("<Gamepad>/buttonWest");

            Look = _map.AddAction("Mirar", InputActionType.Button);
            Look.AddBinding("<Keyboard>/q");
            Look.AddBinding("<Gamepad>/buttonNorth");

            Pause = _map.AddAction("Pausa", InputActionType.Button);
            Pause.AddBinding("<Keyboard>/escape");
            Pause.AddBinding("<Gamepad>/start");

            _map.Enable();
        }

        public InputAction Move { get; }
        public InputAction Advance { get; }
        public InputAction Save { get; }
        public InputAction Look { get; }
        public InputAction Pause { get; }

        public Vector2 MoveValue => Move.ReadValue<Vector2>();

        public void Dispose()
        {
            _map.Disable();
            _map.Dispose();
        }
    }
}
