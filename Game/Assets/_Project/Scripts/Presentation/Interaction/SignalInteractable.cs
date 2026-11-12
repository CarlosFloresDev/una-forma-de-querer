using UFQ.Core.Narrative;
using UFQ.UseCases.Narrative;
using UnityEngine;
using UnityEngine.Events;
using VContainer;

namespace UFQ.Presentation.Interaction
{
    /// <summary>
    /// Un gesto de Abril que brilla en dorado y se puede «Guardar» (señales S01–S17).
    /// Solo traduce la interacción del jugador al caso de uso; el brillo, sonido y
    /// partículas se enganchan desde el inspector con los UnityEvent.
    /// </summary>
    public sealed class SignalInteractable : MonoBehaviour
    {
        [Tooltip("Id del Anexo C del guion: S01 … S17")]
        [SerializeField] private string signalId = "S01";

        [SerializeField] private UnityEvent onAvailable = new UnityEvent();
        [SerializeField] private UnityEvent onSaved = new UnityEvent();

        private NarrativeService _narrative;

        public string SignalId => signalId;
        public bool IsSaved => _narrative != null && _narrative.State.HasSavedSignal(signalId);

        [Inject]
        public void Construct(NarrativeService narrative) => _narrative = narrative;

        /// <summary>Lo llama la escena cuando el gesto ocurre y aparece el brillo dorado.</summary>
        public void MakeAvailable()
        {
            if (!IsSaved) onAvailable.Invoke();
        }

        /// <summary>Lo llama la UI al pulsar «Guardar».</summary>
        public void Save()
        {
            if (_narrative.SaveSignal(signalId)) onSaved.Invoke();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!NarrativeCatalog.IsSignal(signalId))
            {
                Debug.LogWarning($"[UFQ] «{signalId}» no es una señal del guion (S01–S17).", this);
            }
        }
#endif
    }
}
