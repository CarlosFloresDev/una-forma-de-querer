using System.Collections.Generic;
using UFQ.UseCases.Dialogue;
using UnityEngine;
using UnityEngine.UIElements;

namespace UFQ.Presentation.Dialogue
{
    /// <summary>
    /// Vista del diálogo con UI Toolkit (UI/Dialogue/Dialogo.uxml + .uss).
    /// Solo muestra; no decide nada de la historia.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class DialogueView : MonoBehaviour, IDialogueView
    {
        private const string Hidden = "dialogo--oculto";

        [Tooltip("Letras por segundo del efecto de escritura.")]
        [SerializeField] private float charactersPerSecond = 48f;

        private VisualElement _root;
        private Label _title, _inner, _name, _text, _action, _phoneSender, _phoneText, _continue;
        private VisualElement _bubble, _phone, _choices;
        private Button _save;

        private Label _typingLabel;
        private string _typingFull = string.Empty;
        private float _typingProgress;
        private int _picked = -1;

        public bool IsTyping => _typingLabel != null && _typingProgress < _typingFull.Length;

        private void OnEnable()
        {
            var doc = GetComponent<UIDocument>().rootVisualElement;
            _root = doc.Q("dialogo");
            _title = doc.Q<Label>("titulo");
            _inner = doc.Q<Label>("voz-interior");
            _bubble = doc.Q("globo");
            _name = doc.Q<Label>("nombre");
            _text = doc.Q<Label>("texto");
            _action = doc.Q<Label>("accion");
            _phone = doc.Q("celular");
            _phoneSender = doc.Q<Label>("celular-remitente");
            _phoneText = doc.Q<Label>("celular-texto");
            _save = doc.Q<Button>("guardar");
            _choices = doc.Q("opciones");
            _continue = doc.Q<Label>("continuar");
        }

        public void Open() => _root?.RemoveFromClassList(Hidden);

        public void Close()
        {
            _root?.AddToClassList(Hidden);
            HideAll();
        }

        public void ShowLine(DialogueLine line, bool offerSave)
        {
            HideAll();
            switch (line.Kind)
            {
                case LineKind.Speech:
                    Show(_bubble);
                    _name.text = line.Speaker;
                    StartTyping(_text, line.Text);
                    break;
                case LineKind.InnerVoice:
                    Show(_inner);
                    StartTyping(_inner, line.Text);
                    break;
                case LineKind.Phone:
                    Show(_phone);
                    _phoneSender.text = line.Speaker.Length > 0 ? line.Speaker : "MATEO";
                    line.TryGetTag("celular", out var mode);
                    _phone.EnableInClassList("celular--enviado", mode == "enviado" || mode == "borrador");
                    StartTyping(_phoneText, line.Text);
                    break;
                case LineKind.Title:
                    Show(_title);
                    _title.text = line.Text;
                    break;
                default:
                    Show(_action);
                    StartTyping(_action, line.Text);
                    break;
            }

            _save.RemoveFromClassList("guardar--guardado");
            _save.text = "Guardar  ·  E";
            if (offerSave) Show(_save);
            Show(_continue);
        }

        public void MarkSaved()
        {
            _save.AddToClassList("guardar--guardado");
            _save.text = "Guardado en el cuaderno";
        }

        public void CompleteTyping()
        {
            if (_typingLabel == null) return;
            _typingProgress = _typingFull.Length;
            _typingLabel.text = _typingFull;
        }

        public void ShowChoices(IReadOnlyList<DialogueChoice> choices)
        {
            HideAll();
            _picked = -1;
            _choices.Clear();
            Show(_choices);
            Button first = null;
            foreach (var choice in choices)
            {
                var index = choice.Index;
                var button = new Button(() => _picked = index) { text = choice.Text };
                button.AddToClassList("opcion");
                _choices.Add(button);
                if (first == null) first = button;
            }

            first?.Focus();
        }

        public int TakePickedChoice()
        {
            var picked = _picked;
            _picked = -1;
            return picked;
        }

        private void Update()
        {
            if (!IsTyping) return;
            _typingProgress = Mathf.Min(_typingFull.Length, _typingProgress + charactersPerSecond * Time.unscaledDeltaTime);
            _typingLabel.text = _typingFull.Substring(0, Mathf.FloorToInt(_typingProgress));
        }

        private void StartTyping(Label label, string text)
        {
            _typingLabel = label;
            _typingFull = text ?? string.Empty;
            _typingProgress = 0;
            label.text = string.Empty;
        }

        private void HideAll()
        {
            foreach (var element in new VisualElement[] { _title, _inner, _bubble, _action, _phone, _save, _choices, _continue })
            {
                if (element != null) element.style.display = DisplayStyle.None;
            }
        }

        private static void Show(VisualElement element)
        {
            if (element != null) element.style.display = DisplayStyle.Flex;
        }
    }
}
