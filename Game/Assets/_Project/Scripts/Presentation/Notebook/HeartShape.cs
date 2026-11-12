using UnityEngine;
using UnityEngine.UIElements;

namespace UFQ.Presentation.Notebook
{
    /// <summary>
    /// Corazón dibujado con vectores (Painter2D), relleno u solo contorno «a lápiz».
    /// Se adapta al tamaño del elemento, así se ve nítido en cualquier resolución.
    /// </summary>
    public sealed class HeartShape : VisualElement
    {
        private Color _color = Color.white;
        private bool _filled;
        private float _lineWidth = 2.5f;

        public HeartShape()
        {
            pickingMode = PickingMode.Ignore;
            generateVisualContent += Draw;
        }

        public Color Color
        {
            get => _color;
            set { _color = value; MarkDirtyRepaint(); }
        }

        public bool Filled
        {
            get => _filled;
            set { _filled = value; MarkDirtyRepaint(); }
        }

        public float LineWidth
        {
            get => _lineWidth;
            set { _lineWidth = value; MarkDirtyRepaint(); }
        }

        private void Draw(MeshGenerationContext context)
        {
            var r = contentRect;
            if (r.width <= 1 || r.height <= 1) return;

            // Margen para que el trazo no se corte en los bordes.
            var pad = _lineWidth;
            var w = r.width - pad * 2;
            var h = r.height - pad * 2;
            Vector2 P(float x, float y) => new Vector2(pad + x * w, pad + y * h);

            var p = context.painter2D;
            p.lineJoin = LineJoin.Round;
            p.lineCap = LineCap.Round;
            p.BeginPath();
            p.MoveTo(P(0.5f, 0.28f));
            p.BezierCurveTo(P(0.5f, 0.12f), P(0.32f, 0.0f), P(0.18f, 0.04f));
            p.BezierCurveTo(P(0.0f, 0.1f), P(-0.04f, 0.38f), P(0.1f, 0.56f));
            p.BezierCurveTo(P(0.22f, 0.72f), P(0.4f, 0.84f), P(0.5f, 1.0f));
            p.BezierCurveTo(P(0.6f, 0.84f), P(0.78f, 0.72f), P(0.9f, 0.56f));
            p.BezierCurveTo(P(1.04f, 0.38f), P(1.0f, 0.1f), P(0.82f, 0.04f));
            p.BezierCurveTo(P(0.68f, 0.0f), P(0.5f, 0.12f), P(0.5f, 0.28f));
            p.ClosePath();

            if (_filled)
            {
                p.fillColor = _color;
                p.Fill();
            }
            else
            {
                p.strokeColor = _color;
                p.lineWidth = _lineWidth;
                p.Stroke();
            }
        }
    }
}
