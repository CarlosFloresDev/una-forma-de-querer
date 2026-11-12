using System;
using UFQ.Core.Narrative;
using UFQ.UseCases.Events;
using UFQ.UseCases.Narrative;
using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

namespace UFQ.Presentation.Notebook
{
    /// <summary>
    /// Medidor de Ilusión (sección 4.2 del guion): un corazón a lápiz en la esquina que
    /// se va coloreando de dorado, desde abajo, con cada señal guardada (SEÑALES/17).
    /// Con 9 o más late suavemente (2.9). Desde el Capítulo 4 desaparece para siempre.
    /// Se construye en código: no necesita UXML propio, solo el PanelSettings del juego.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class IllusionMeterView : MonoBehaviour, ISceneInjectable
    {
        private static readonly Color Pencil = new Color(0.93f, 0.89f, 0.82f, 0.85f);
        private static readonly Color Gold = new Color(0.85f, 0.64f, 0.25f, 1f);

        [SerializeField] private float size = 74f;
        [SerializeField] private int beatThreshold = 9;

        private NarrativeService _narrative;
        private IDisposable _onSaved;
        private IDisposable _onChapter;

        private VisualElement _meter;
        private VisualElement _fillMask;
        private HeartShape _fillHeart;
        private float _shownFraction;
        private float _targetFraction;
        private float _pulse;

        [Inject]
        public void Construct(NarrativeService narrative, IEventBus bus)
        {
            _narrative = narrative;
            _onSaved = bus.Subscribe<SignalSaved>(e => SetTarget(e.Total, pulse: true));
            _onChapter = bus.Subscribe<ChapterStarted>(_ => RefreshVisibility());
        }

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.pickingMode = PickingMode.Ignore;

            _meter = new VisualElement { name = "medidor-ilusion", pickingMode = PickingMode.Ignore };
            _meter.style.position = Position.Absolute;
            _meter.style.right = 36;
            _meter.style.bottom = 30;
            _meter.style.width = size;
            _meter.style.height = size * 0.92f;

            // Capa dorada recortada desde abajo: su altura es la fracción llena.
            _fillMask = new VisualElement { pickingMode = PickingMode.Ignore };
            _fillMask.style.position = Position.Absolute;
            _fillMask.style.left = 0;
            _fillMask.style.right = 0;
            _fillMask.style.bottom = 0;
            _fillMask.style.height = Length.Percent(0);
            _fillMask.style.overflow = Overflow.Hidden;

            _fillHeart = new HeartShape { Filled = true, Color = Gold };
            _fillHeart.style.position = Position.Absolute;
            _fillHeart.style.left = 0;
            _fillHeart.style.right = 0;
            _fillHeart.style.bottom = 0;
            _fillHeart.style.height = size * 0.92f;
            _fillMask.Add(_fillHeart);

            var outline = new HeartShape { Filled = false, Color = Pencil, LineWidth = 2.2f };
            outline.style.position = Position.Absolute;
            outline.style.left = 0;
            outline.style.right = 0;
            outline.style.top = 0;
            outline.style.bottom = 0;

            _meter.Add(_fillMask);
            _meter.Add(outline);
            root.Add(_meter);
        }

        private void Start()
        {
            if (_narrative == null) return;
            SetTarget(_narrative.State.Signals, pulse: false);
            _shownFraction = _targetFraction;
            RefreshVisibility();
        }

        private void SetTarget(int signals, bool pulse)
        {
            _targetFraction = Mathf.Clamp01(signals / (float)NarrativeCatalog.Signals.Count);
            if (pulse) _pulse = 1f;
        }

        private void RefreshVisibility()
        {
            if (_meter == null || _narrative == null) return;
            // El medidor desaparece «para siempre» al terminar el sueño del Capítulo 4.
            var visible = _narrative.State.CurrentChapter < Chapter.Chapter5AprenderAMirar;
            _meter.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Update()
        {
            if (_meter == null) return;

            _shownFraction = Mathf.MoveTowards(_shownFraction, _targetFraction, Time.unscaledDeltaTime * 0.6f);
            _fillMask.style.height = Length.Percent(_shownFraction * 100f);

            // Pulso al guardar + latido suave cuando la ilusión ya es grande.
            _pulse = Mathf.MoveTowards(_pulse, 0f, Time.unscaledDeltaTime * 2.5f);
            var beating = _narrative != null && _narrative.State.Signals >= beatThreshold;
            var beat = beating ? Mathf.Pow(Mathf.Max(0f, Mathf.Sin(Time.unscaledTime * 2.4f)), 8f) * 0.06f : 0f;
            var scale = 1f + _pulse * 0.18f + beat;
            _meter.style.scale = new Scale(new Vector3(scale, scale, 1f));
        }

        private void OnDestroy()
        {
            _onSaved?.Dispose();
            _onChapter?.Dispose();
        }
    }
}
