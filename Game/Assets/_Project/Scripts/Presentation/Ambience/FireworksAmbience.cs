using UnityEngine;

namespace UFQ.Presentation.Ambience
{
    /// <summary>
    /// Fuegos artificiales lejanos de la serenata del aniversario de Arequipa (P.1, 4.3):
    /// estallan cada tanto en el cielo y una luz de color «tiñe el cuarto» un instante.
    /// Solo ambiente: no afecta a la historia.
    /// </summary>
    public sealed class FireworksAmbience : MonoBehaviour
    {
        [SerializeField] private ParticleSystem burst;
        [SerializeField] private Light roomTint;
        [SerializeField] private Vector2 interval = new Vector2(3.5f, 8f);
        [SerializeField] private Vector3 areaCenter = new Vector3(-10f, 18f, 70f);
        [SerializeField] private Vector3 areaSize = new Vector3(60f, 12f, 20f);
        [SerializeField] private float tintIntensity = 0.9f;

        // Colores cálidos y festivos, sin saturar el dorado (reservado a las señales).
        private static readonly Color[] Palette =
        {
            new Color(0.95f, 0.45f, 0.55f),
            new Color(0.55f, 0.75f, 1.0f),
            new Color(0.75f, 0.55f, 0.95f),
            new Color(1.0f, 0.62f, 0.35f),
            new Color(0.6f, 0.95f, 0.75f),
        };

        private float _next;
        private float _tint;

        private void OnEnable()
        {
            _next = Time.time + Random.Range(1f, interval.y);
            if (roomTint != null) roomTint.intensity = 0f;
        }

        private void Update()
        {
            if (Time.time >= _next)
            {
                Launch();
                _next = Time.time + Random.Range(interval.x, interval.y);
            }

            if (roomTint == null) return;
            _tint = Mathf.MoveTowards(_tint, 0f, Time.deltaTime * 0.7f);
            roomTint.intensity = tintIntensity * _tint * _tint;
        }

        private void Launch()
        {
            var color = Palette[Random.Range(0, Palette.Length)];
            if (burst != null)
            {
                var position = areaCenter + Vector3.Scale(new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f), areaSize);
                var emit = new ParticleSystem.EmitParams
                {
                    position = position,
                    startColor = color,
                    applyShapeToPosition = true,
                };
                burst.Emit(emit, Random.Range(70, 120));
            }

            if (roomTint != null)
            {
                roomTint.color = color;
                _tint = 1f;
            }
        }
    }
}
