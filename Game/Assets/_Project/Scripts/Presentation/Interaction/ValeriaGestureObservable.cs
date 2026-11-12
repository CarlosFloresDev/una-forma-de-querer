using UFQ.Core.Narrative;
using UFQ.UseCases.Narrative;
using UnityEngine;
using VContainer;

namespace UFQ.Presentation.Interaction
{
    /// <summary>
    /// Un gesto de Valeria (V1–V7). A propósito NO brilla ni avisa: solo cuenta si el
    /// jugador usa «Mirar» sobre él o elige la respuesta atenta (sección 4.3 del guion).
    /// </summary>
    public sealed class ValeriaGestureObservable : MonoBehaviour, ISceneInjectable
    {
        [Tooltip("Id del Anexo C del guion: V1 … V7")]
        [SerializeField] private string gestureId = "V1";

        [Tooltip("Pensamiento de Mateo al mirarlo, p. ej. «Una galleta de más. Seguro sobró».")]
        [TextArea] [SerializeField] private string innerThought;

        private NarrativeService _narrative;

        public string InnerThought => innerThought;

        [Inject]
        public void Construct(NarrativeService narrative) => _narrative = narrative;

        /// <summary>El jugador pulsó «Mirar» sobre este objeto.</summary>
        public void Look() => _narrative.NoticeGesture(gestureId);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!NarrativeCatalog.IsValeriaGesture(gestureId))
            {
                Debug.LogWarning($"[UFQ] «{gestureId}» no es un gesto de Valeria (V1–V7).", this);
            }
        }
#endif
    }
}
