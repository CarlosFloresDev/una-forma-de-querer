using NUnit.Framework;
using UFQ.UseCases.Dialogue;

namespace UFQ.Tests.EditMode
{
    public class DialogueLineTests
    {
        [Test]
        public void Speech_SplitsSpeakerWithAccents()
        {
            var line = DialogueLine.Parse("DOÑA INÉS: Inés nomás. Tres reglas: nada de música.", null);
            Assert.That(line.Kind, Is.EqualTo(LineKind.Speech));
            Assert.That(line.Speaker, Is.EqualTo("DOÑA INÉS"));
            Assert.That(line.Text, Is.EqualTo("Inés nomás. Tres reglas: nada de música."));
        }

        [Test]
        public void AbbreviatedSpeaker_IsRecognized()
        {
            var line = DialogueLine.Parse("PROF. SALGADO: Sobresaliente.", null);
            Assert.That(line.Speaker, Is.EqualTo("PROF. SALGADO"));
        }

        [Test]
        public void NormalSentenceWithColon_IsNotASpeaker()
        {
            var line = DialogueLine.Parse("Valeria clava una tarjeta: «MATEO — 14/07».", new[] { "accion" });
            Assert.That(line.Speaker, Is.Empty);
            Assert.That(line.Kind, Is.EqualTo(LineKind.Action));
        }

        [Test]
        public void SignalAndGestureTags_AreExposed()
        {
            var signal = DialogueLine.Parse("ABRIL: Te guardé el sitio.", new[] { "senal:S03" });
            Assert.That(signal.SignalId, Is.EqualTo("S03"));
            Assert.That(signal.GestureId, Is.Null);

            var gesture = DialogueLine.Parse("Una galleta de más.", new[] { "accion", " #gesto: V1 " });
            Assert.That(gesture.GestureId, Is.EqualTo("V1"));
        }

        [TestCase("celular:entrante", LineKind.Phone)]
        [TestCase("voz_interior", LineKind.InnerVoice)]
        [TestCase("titulo", LineKind.Title)]
        [TestCase("accion", LineKind.Action)]
        public void KindTags_WinOverSpeaker(string tag, LineKind expected)
        {
            Assert.That(DialogueLine.Parse("ABRIL: ¿Sigues despierto? 😊", new[] { tag }).Kind, Is.EqualTo(expected));
        }

        [Test]
        public void EmptyText_IsACue()
        {
            var line = DialogueLine.Parse("", new[] { "camara:azotea", "musica:vals" });
            Assert.That(line.Kind, Is.EqualTo(LineKind.Cue));
            Assert.That(line.TryGetTag("CAMARA", out var cam), Is.True);
            Assert.That(cam, Is.EqualTo("azotea"));
        }

        [Test]
        public void TagWithoutValue_HasEmptyValue()
        {
            var tag = DialogueTag.Parse("#voz_interior");
            Assert.That(tag.Key, Is.EqualTo("voz_interior"));
            Assert.That(tag.Value, Is.Empty);
        }
    }
}
