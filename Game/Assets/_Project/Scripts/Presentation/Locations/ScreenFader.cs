using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UFQ.Presentation.Locations
{
    /// <summary>Fundido a negro de pantalla completa (UI Toolkit), con espera asíncrona.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class ScreenFader : MonoBehaviour
    {
        [SerializeField] private Color color = new Color(0.04f, 0.035f, 0.06f, 1f);

        private VisualElement _veil;
        private float _alpha = 1f;

        private void OnEnable()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            root.pickingMode = PickingMode.Ignore;
            _veil = new VisualElement { name = "fundido", pickingMode = PickingMode.Ignore };
            _veil.style.position = Position.Absolute;
            _veil.style.left = 0;
            _veil.style.right = 0;
            _veil.style.top = 0;
            _veil.style.bottom = 0;
            _veil.style.backgroundColor = color;
            root.Add(_veil);
            Apply();
        }

        public UniTask FadeOutAsync(float seconds, CancellationToken token) => FadeAsync(1f, seconds, token);
        public UniTask FadeInAsync(float seconds, CancellationToken token) => FadeAsync(0f, seconds, token);

        private async UniTask FadeAsync(float target, float seconds, CancellationToken token)
        {
            var start = _alpha;
            var elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                _alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, elapsed / seconds));
                Apply();
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            _alpha = target;
            Apply();
        }

        private void Apply()
        {
            if (_veil == null) return;
            _veil.style.opacity = _alpha;
            _veil.style.display = _alpha <= 0.001f ? DisplayStyle.None : DisplayStyle.Flex;
        }
    }
}
