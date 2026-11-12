using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UFQ.Core.Narrative;
using UFQ.UseCases.Events;
using UFQ.UseCases.Narrative;
using UFQ.UseCases.Persistence;

namespace UFQ.Tests.EditMode
{
    /// <summary>Casos de uso: transacciones, carga robusta y eventos.</summary>
    public class NarrativeServiceTests
    {
        [Test]
        public void FailedSave_RollsBackState_AndAllowsRetry()
        {
            var bus = new EventBus();
            var repo = new FakeRepository();
            var service = new NarrativeService(bus, repo);
            var events = new List<SignalSaved>();
            bus.Subscribe<SignalSaved>(events.Add);

            repo.FailNextSave = true;
            Assert.Throws<IOException>(() => service.SaveSignal("S01"));
            Assert.That(service.State.Signals, Is.Zero, "El estado vuelve atrás si no se pudo guardar");
            Assert.That(events, Is.Empty, "No se publica un evento de algo que no quedó guardado");

            Assert.That(service.SaveSignal("S01"), Is.True, "Se puede reintentar");
            Assert.That(events.Count, Is.EqualTo(1));
        }

        [Test]
        public void FailedDecision_RollsBack()
        {
            var repo = new FakeRepository();
            var service = new NarrativeService(new EventBus(), repo);
            repo.FailNextSave = true;
            Assert.Throws<IOException>(() => service.DecideC3(DecisionC3.Confess));
            Assert.That(service.State.DecisionC3, Is.EqualTo(DecisionC3.None));
            service.DecideC3(DecisionC3.HangUp);
            Assert.That(service.State.DecisionC3, Is.EqualTo(DecisionC3.HangUp));
        }

        [Test]
        public void InvalidSave_IsQuarantined_AndGameStartsFresh()
        {
            var repo = new FakeRepository { Stored = new NarrativeSnapshot { savedSignals = new List<string> { "S99" } } };
            var service = new NarrativeService(new EventBus(), repo);

            Assert.That(service.State.Signals, Is.Zero);
            Assert.That(service.LoadIssue, Does.Contain("S99"));
            Assert.That(repo.QuarantineReasons.Count, Is.EqualTo(1));
        }

        [Test]
        public void FutureVersionSave_IsQuarantined()
        {
            var repo = new FakeRepository { Stored = new NarrativeSnapshot { version = NarrativeSnapshot.CurrentVersion + 1 } };
            var service = new NarrativeService(new EventBus(), repo);
            Assert.That(service.LoadIssue, Is.Not.Null);
            Assert.That(repo.QuarantineReasons.Count, Is.EqualTo(1));
        }

        [Test]
        public void NullCollectionsInSave_AreTreatedAsEmpty()
        {
            var repo = new FakeRepository { Stored = new NarrativeSnapshot { savedSignals = null, notedGestures = null } };
            var service = new NarrativeService(new EventBus(), repo);
            Assert.That(service.LoadIssue, Is.Null);
            Assert.That(service.State.Signals, Is.Zero);
        }

        [Test]
        public void UndefinedEnumValues_AreRejected()
        {
            var s = new NarrativeState();
            Assert.Throws<NarrativeRuleException>(() => s.DecideC3((DecisionC3)99));
            Assert.Throws<NarrativeRuleException>(() => s.DecideFinalMessage((FinalMessage)7));
            Assert.Throws<NarrativeRuleException>(() => s.AdvanceTo((Chapter)99));
            Assert.Throws<NarrativeRuleException>(() => s.AddWaitTime(double.NaN));

            var repo = new FakeRepository { Stored = new NarrativeSnapshot { chapter = 99 } };
            Assert.That(new NarrativeService(new EventBus(), repo).LoadIssue, Is.Not.Null);
        }

        [Test]
        public void RewindToChapter4_IsAUseCase_ThatPersistsAndPublishes()
        {
            var bus = new EventBus();
            var repo = new FakeRepository();
            var service = new NarrativeService(bus, repo);
            service.StartChapter(Chapter.Chapter5AprenderAMirar);
            service.SaveInChapter5("C5_Chalina");

            Chapter? started = null;
            bus.Subscribe<ChapterStarted>(e => started = e.Chapter);
            service.RewindToChapter4();

            Assert.That(started, Is.EqualTo(Chapter.Chapter4OtraMirada));
            Assert.That(repo.Stored.chapter, Is.EqualTo((int)Chapter.Chapter4OtraMirada));
            Assert.That(repo.Stored.chapter5Saves, Is.Empty);
        }

        private sealed class FakeRepository : INarrativeRepository
        {
            public NarrativeSnapshot Stored;
            public bool FailNextSave;
            public readonly List<string> QuarantineReasons = new List<string>();

            public bool TryLoad(out NarrativeSnapshot snapshot)
            {
                snapshot = Stored;
                return Stored != null;
            }

            public void Save(NarrativeSnapshot snapshot)
            {
                if (FailNextSave)
                {
                    FailNextSave = false;
                    throw new IOException("Disco lleno (simulado)");
                }

                Stored = snapshot;
            }

            public void Delete() => Stored = null;

            public void Quarantine(string reason)
            {
                QuarantineReasons.Add(reason);
                Stored = null;
            }
        }
    }
}
