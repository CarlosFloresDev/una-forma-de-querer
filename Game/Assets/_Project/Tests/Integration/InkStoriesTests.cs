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
        public void Story_CompilesWithoutErrors(string name)
        {
            var file = Load(name);
            Assert.That(file.hasErrors, Is.False, $"{name}.ink tiene errores de compilación");
            Assert.That(file.hasUnhandledCompileErrors, Is.False);
            Assert.That(string.IsNullOrEmpty(file.storyJson), Is.False, $"{name}.ink no generó JSON");
        }

        [TestCase("Prologo")]
        [TestCase("Capitulo1")]
        public void Story_CanBePlayedToTheEnd_AlwaysChoosingFirstOption(string name)
        {
            var lines = Play(name, out var narrative);
            Assert.That(lines, Is.Not.Empty);
            Assert.That(narrative.State, Is.Not.Null);
        }

        [TestCase("Prologo")]
        [TestCase("Capitulo1")]
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
        public void Chapter1_ChoicesReachTheDomain()
        {
            Play("Capitulo1", out var narrative);
            // Primera opción de 1.10: «Gracias, Vale» → nota el gesto V2.
            Assert.That(narrative.State.HasNotedGesture("V2"), Is.True);
            // Primera opción de 1.11: «Que esto dure».
            Assert.That(narrative.State.NewYearWish, Is.EqualTo(NewYearWish.ThatThisLasts));
        }

        // ------------------------------------------------------------------ utilidades
        private static InkFile Load(string name)
        {
            var file = AssetDatabase.LoadAssetAtPath<InkFile>(InkFolder + name + ".ink");
            Assert.That(file, Is.Not.Null, $"No se encontró {name}.ink");
            return file;
        }

        private static List<DialogueLine> Play(string name, out NarrativeService narrative)
        {
            narrative = new NarrativeService(new EventBus(), new MemoryRepository());
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
