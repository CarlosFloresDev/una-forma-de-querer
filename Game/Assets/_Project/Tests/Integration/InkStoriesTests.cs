using System.Collections.Generic;
using Ink.Runtime;
using Ink.UnityIntegration;
using NUnit.Framework;
using UFQ.Core.Narrative;
using UFQ.Infrastructure.Dialogue;
using UFQ.UseCases.Dialogue;
using UFQ.UseCases.Events;
using UFQ.UseCases.Narrative;
using UFQ.UseCases.Persistence;
using UnityEditor;

namespace UFQ.Tests.Integration
{
    /// <summary>
    /// Recorre los guiones Ink COMPILADOS reales a través del puente con el dominio.
    /// Detecta: errores de compilación, funciones EXTERNAL sin enlazar, variables no
    /// declaradas, rutas sin salida e ids de señales/gestos que no existen en el catálogo.
    /// </summary>
    public class InkStoriesTests
    {
        private const string InkFolder = "Assets/_Project/Narrative/Ink/";
        private const int MaxSteps = 5000;

        [TestCase("Prologo")]
        [TestCase("Capitulo1")]
        [TestCase("Capitulo2")]
        [TestCase("Capitulo3")]
        [TestCase("Capitulo4")]
        public void Story_CompilesWithoutErrors(string name)
        {
            var file = Load(name);
            Assert.That(file.hasErrors, Is.False, $"{name}.ink tiene errores de compilación");
            Assert.That(file.hasUnhandledCompileErrors, Is.False);
            Assert.That(string.IsNullOrEmpty(file.storyJson), Is.False, $"{name}.ink no generó JSON");
        }

        [TestCase("Prologo")]
        [TestCase("Capitulo1")]
        [TestCase("Capitulo2")]
        [TestCase("Capitulo3")]
        [TestCase("Capitulo4")]
        public void Story_CanBePlayedToTheEnd_AlwaysChoosingFirstOption(string name)
        {
            var lines = Play(name, out var narrative);
            Assert.That(lines, Is.Not.Empty);
            Assert.That(narrative.State, Is.Not.Null);
        }

        [TestCase("Prologo")]
        [TestCase("Capitulo1")]
        [TestCase("Capitulo2")]
        [TestCase("Capitulo3")]
        [TestCase("Capitulo4")]
        public void SignalAndGestureTags_UseIdsFromTheCatalog(string name)
        {
            foreach (var line in Play(name, out _))
            {
                if (line.SignalId != null)
                    Assert.That(NarrativeCatalog.IsSignal(line.SignalId), Is.True, $"Señal desconocida «{line.SignalId}» en {name}: {line.Text}");
                if (line.GestureId != null)
                    Assert.That(NarrativeCatalog.IsValeriaGesture(line.GestureId), Is.True, $"Gesto desconocido «{line.GestureId}» en {name}: {line.Text}");
            }
        }

        [Test]
        public void Chapter1_MarksAllSevenSignalsOfTheScript()
        {
            var signals = new HashSet<string>();
            foreach (var line in Play("Capitulo1", out _))
                if (line.SignalId != null) signals.Add(line.SignalId);

            Assert.That(signals, Is.EquivalentTo(new[] { "S01", "S02", "S03", "S04", "S05", "S06", "S07" }));
        }

        [Test]
        public void Chapter2_MarksItsFourSignals_AndLensMoments()
        {
            var signals = new HashSet<string>();
            var lens = new HashSet<string>();
            foreach (var line in Play("Capitulo2", out _))
            {
                if (line.SignalId != null) signals.Add(line.SignalId);
                if (line.TryGetTag("lente", out var moment)) lens.Add(moment);
            }

            Assert.That(signals, Is.EquivalentTo(new[] { "S08", "S09", "S10", "S11" }));
            Assert.That(lens, Is.EquivalentTo(new[] { "2.5b", "2.6" }));
        }

        [Test]
        public void Chapter2_FirstOptions_ReachTheDomain()
        {
            Play("Capitulo2", out var narrative);
            Assert.That(narrative.State.HasNotedGesture("V4"), Is.True, "Responder con la conchita nota V4");
            Assert.That(narrative.State.NicoResponse, Is.EqualTo(NicoResponse.Anger));
            Assert.That(narrative.State.Lens, Is.EqualTo(0), "Leer como amistad no suma LENTE");
            Assert.That(narrative.State.HasStoryMark("ConchitaPrometida"), Is.True);
            Assert.That(narrative.State.HasStoryMark("ConchitaRecogida"), Is.True, "2.3: recogerla para Valeria");
        }

        [Test]
        public void Chapter4_DoesNotOfferToSaveAnything()
        {
            foreach (var line in Play("Capitulo4", out _))
                Assert.That(line.SignalId, Is.Null, $"En el sueño no se guarda nada: {line.Text}");
        }

        [TestCase(DecisionC3.Confess, "¿Podemos estar bien?")]
        [TestCase(DecisionC3.StaySilent, "¡Volviste!")]
        [TestCase(DecisionC3.HangUp, "¿Hice algo mal?")]
        public void Chapter4_BranchesOnTheChapter3Decision(DecisionC3 decision, string expected)
        {
            var lines = Play("Capitulo4", out _, n => n.DecideC3(decision));
            Assert.That(lines.Exists(l => l.Text.Contains(expected)), Is.True, $"Falta «{expected}» con {decision}");
        }

        [Test]
        public void Chapter4_UsesMarksFromEarlierChapters()
        {
            var lines = Play("Capitulo4", out var narrative, n =>
            {
                n.DecideC3(DecisionC3.Confess);
                n.Mark("ConchitaRecogida");
                n.Mark("BallenaEnAzotea");
            });

            Assert.That(narrative.State.HasStoryMark("ConchitaEntregada"), Is.True, "4.1: primera opción, dar la conchita");
            Assert.That(lines.Exists(l => l.Text.Contains("Te la cuidé")), Is.True, "4.2: la ballena que se quedó en la azotea");
            Assert.That(narrative.State.DadMemoryIds, Does.Contain("M4_BoletoChivay"), "4.3: mirar la casaca");
        }

        [Test]
        public void Chapter4_WithoutEarlierMarks_SkipsThoseMoments()
        {
            var lines = Play("Capitulo4", out var narrative);
            Assert.That(narrative.State.HasStoryMark("ConchitaEntregada"), Is.False);
            Assert.That(lines.Exists(l => l.Text.Contains("Te la cuidé")), Is.False);
        }

        [Test]
        public void Chapter3_MarksSignalsS12ToS17()
        {
            var signals = new HashSet<string>();
            foreach (var line in Play("Capitulo3", out _))
                if (line.SignalId != null) signals.Add(line.SignalId);
            Assert.That(signals, Is.EquivalentTo(new[] { "S12", "S13", "S14", "S15", "S16", "S17" }));
        }

        [Test]
        public void Chapter3_MainDecisionReachesTheDomain_AndBrokenLensDoesNotCount()
        {
            Play("Capitulo3", out var narrative);
            Assert.That(narrative.State.DecisionC3, Is.EqualTo(DecisionC3.Confess), "Primera opción de 3.10: confesar");
            Assert.That(narrative.State.HasNotedGesture("V5"), Is.True);
            Assert.That(narrative.State.HasNotedGesture("V6"), Is.True);
            Assert.That(narrative.State.HasNotedGesture("V7"), Is.True);
            Assert.That(narrative.State.LensMomentIds, Does.Not.Contain("3.10"), "En 3.10 la Lente ya no funciona");
        }

        [Test]
        public void Chapter1_ChoicesReachTheDomain()
        {
            Play("Capitulo1", out var narrative);
            // Primera opción de 1.10: «Gracias, Vale» → nota el gesto V2.
            Assert.That(narrative.State.HasNotedGesture("V2"), Is.True);
            // Primera opción de 1.11: «Que esto dure».
            Assert.That(narrative.State.NewYearWish, Is.EqualTo(NewYearWish.ThatThisLasts));
            Assert.That(narrative.State.HasStoryMark("FraseCartero"), Is.True, "1.9: mirar a Doña Inés");
        }

        // ------------------------------------------------------------------ utilidades
        private static InkFile Load(string name)
        {
            var file = AssetDatabase.LoadAssetAtPath<InkFile>(InkFolder + name + ".ink");
            Assert.That(file, Is.Not.Null, $"No se encontró {name}.ink");
            return file;
        }

        private static List<DialogueLine> Play(string name, out NarrativeService narrative, System.Action<NarrativeService> setup = null)
        {
            narrative = new NarrativeService(new EventBus(), new MemoryRepository());
            setup?.Invoke(narrative);
            var bridge = new InkNarrativeBridge(new Story(Load(name).storyJson), narrative);
            var dialogue = InkDialogueStory.Connected(bridge);
            var lines = new List<DialogueLine>();

            for (var step = 0; step < MaxSteps; step++)
            {
                while (dialogue.CanContinue) lines.Add(dialogue.Continue());
                if (dialogue.IsFinished) return lines;

                var choices = dialogue.CurrentChoices;
                Assert.That(choices.Count, Is.GreaterThan(0), $"{name}: sin líneas ni opciones (callejón sin salida)");
                dialogue.Choose(choices[0].Index);
            }

            Assert.Fail($"{name}: superó {MaxSteps} pasos (¿bucle infinito?)");
            return lines;
        }

        private sealed class MemoryRepository : INarrativeRepository
        {
            private NarrativeSnapshot _saved;
            public bool TryLoad(out NarrativeSnapshot snapshot) { snapshot = _saved; return _saved != null; }
            public void Save(NarrativeSnapshot snapshot) => _saved = snapshot;
            public void Delete() => _saved = null;
            public void Quarantine(string reason) => _saved = null;
        }
    }
}
