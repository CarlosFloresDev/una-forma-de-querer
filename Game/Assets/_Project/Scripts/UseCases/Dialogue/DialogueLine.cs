using System;
using System.Collections.Generic;

namespace UFQ.UseCases.Dialogue
{
    public enum LineKind
    {
        /// <summary>Un personaje habla: globo de texto sobre él.</summary>
        Speech,
        /// <summary>Descripción de lo que pasa (#accion).</summary>
        Action,
        /// <summary>Pensamiento de Mateo, manuscrito sobre la pantalla (#voz_interior).</summary>
        InnerVoice,
        /// <summary>Mensaje en el celular (#celular:…).</summary>
        Phone,
        /// <summary>Texto grande a pantalla completa (#titulo).</summary>
        Title,
        /// <summary>Línea vacía que solo lleva etiquetas (cámara, música, minijuego).</summary>
        Cue,
    }

    /// <summary>Una etiqueta de Ink «#clave:valor» (el valor puede venir vacío).</summary>
    public readonly struct DialogueTag
    {
        public DialogueTag(string key, string value)
        {
            Key = key;
            Value = value ?? string.Empty;
        }

        public string Key { get; }
        public string Value { get; }

        public static DialogueTag Parse(string raw)
        {
            var text = (raw ?? string.Empty).Trim().TrimStart('#').Trim();
            var colon = text.IndexOf(':');
            return colon < 0
                ? new DialogueTag(text.ToLowerInvariant(), string.Empty)
                : new DialogueTag(text.Substring(0, colon).Trim().ToLowerInvariant(), text.Substring(colon + 1).Trim());
        }

        public override string ToString() => Value.Length == 0 ? Key : $"{Key}:{Value}";
    }

    /// <summary>
    /// Una línea del guion ya interpretada. Convención de los .ink:
    /// «NOMBRE: texto» es un personaje hablando; las etiquetas dicen el resto.
    /// </summary>
    public sealed class DialogueLine
    {
        private DialogueLine(LineKind kind, string speaker, string text, IReadOnlyList<DialogueTag> tags)
        {
            Kind = kind;
            Speaker = speaker;
            Text = text;
            Tags = tags;
        }

        public LineKind Kind { get; }

        /// <summary>Nombre del personaje en mayúsculas (vacío si no habla nadie).</summary>
        public string Speaker { get; }

        public string Text { get; }
        public IReadOnlyList<DialogueTag> Tags { get; }

        public bool HasTag(string key) => TryGetTag(key, out _);

        public bool TryGetTag(string key, out string value)
        {
            foreach (var tag in Tags)
            {
                if (string.Equals(tag.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    value = tag.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>Id de la señal dorada de esta línea (S01–S17), o null.</summary>
        public string SignalId => TryGetTag("senal", out var v) ? v : null;

        /// <summary>Id del gesto de Valeria de esta línea (V1–V7), o null.</summary>
        public string GestureId => TryGetTag("gesto", out var v) ? v : null;

        public static DialogueLine Parse(string rawText, IEnumerable<string> rawTags)
        {
            var tags = new List<DialogueTag>();
            if (rawTags != null)
            {
                foreach (var raw in rawTags)
                {
                    if (!string.IsNullOrWhiteSpace(raw)) tags.Add(DialogueTag.Parse(raw));
                }
            }

            var text = (rawText ?? string.Empty).Trim();
            var speaker = string.Empty;
            if (TrySplitSpeaker(text, out var name, out var rest))
            {
                speaker = name;
                text = rest;
            }

            var probe = new DialogueLine(LineKind.Cue, speaker, text, tags);
            LineKind kind;
            if (probe.HasTag("celular")) kind = LineKind.Phone;
            else if (probe.HasTag("voz_interior")) kind = LineKind.InnerVoice;
            else if (probe.HasTag("titulo")) kind = LineKind.Title;
            else if (probe.HasTag("accion")) kind = LineKind.Action;
            else if (speaker.Length > 0) kind = LineKind.Speech;
            else if (text.Length == 0) kind = LineKind.Cue;
            else kind = LineKind.Action;

            return new DialogueLine(kind, speaker, text, tags);
        }

        /// <summary>
        /// «DOÑA INÉS: hola» → («DOÑA INÉS», «hola»). El nombre debe ir en mayúsculas
        /// (letras, espacios y puntos), para no confundir frases normales con dos puntos.
        /// </summary>
        private static bool TrySplitSpeaker(string text, out string name, out string rest)
        {
            name = null;
            rest = null;
            var colon = text.IndexOf(':');
            if (colon <= 0 || colon > 24) return false;

            var candidate = text.Substring(0, colon).Trim();
            var hasLetter = false;
            foreach (var c in candidate)
            {
                if (char.IsLetter(c))
                {
                    if (!char.IsUpper(c)) return false;
                    hasLetter = true;
                }
                else if (c != ' ' && c != '.')
                {
                    return false;
                }
            }

            if (!hasLetter) return false;
            name = candidate;
            rest = text.Substring(colon + 1).Trim();
            return true;
        }
    }

    public readonly struct DialogueChoice
    {
        public DialogueChoice(int index, string text, IReadOnlyList<DialogueTag> tags)
        {
            Index = index;
            Text = text;
            Tags = tags;
        }

        public int Index { get; }
        public string Text { get; }
        public IReadOnlyList<DialogueTag> Tags { get; }
    }

    /// <summary>
    /// Puerto: una historia que se puede recorrer línea a línea. Infrastructure lo
    /// implementa con Ink; las pruebas, con una lista en memoria.
    /// </summary>
    public interface IDialogueStory
    {
        bool CanContinue { get; }
        bool IsFinished { get; }
        DialogueLine Continue();
        IReadOnlyList<DialogueChoice> CurrentChoices { get; }
        void Choose(int index);
        void JumpTo(string knotPath);
    }
}
