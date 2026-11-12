using System.Collections.Generic;
using NUnit.Framework;
using UFQ.Core.Narrative;
using UFQ.UseCases.Events;
using UFQ.UseCases.Narrative;
using UFQ.UseCases.Persistence;

namespace UFQ.Tests.EditMode
{
    public class NarrativeStateTests
    {
        [Test]
        public void Catalog_MatchesScriptCounts()
        {
            Assert.That(NarrativeCatalog.Signals.Count, Is.EqualTo(17));
            Assert.That(NarrativeCatalog.ValeriaGestures.Count, Is.EqualTo(7));
            Assert.That(NarrativeCatalog.DadMemories.Count, Is.EqualTo(6));
            Assert.That(NarrativeCatalog.Chapter5Gestures.Count, Is.EqualTo(3));
        }

        [Test]
        public void SavingTheSameSignalTwice_CountsOnce()
        {
            var s = new NarrativeState();
            Assert.That(s.SaveSignal("S01"), Is.True);
            Assert.That(s.SaveSignal("S01"), Is.False);
            Assert.That(s.Signals, Is.EqualTo(1));
        }

        [Test]
        public void UnknownIds_AreRejected()
        {
            var s = new NarrativeState();
            Assert.Throws<NarrativeRuleException>(() => s.SaveSignal("S99"));
            Assert.Throws<NarrativeRuleException>(() => s.NoticeGesture("S01"));
            Assert.Throws<NarrativeRuleException>(() => s.FindDadMemory(null));
        }

        [Test]
        public void Lens_OnlyCountsSomethingMore_OncePerMoment_UpToFour()
        {
            var s = new NarrativeState();
            Assert.That(s.ApplyLens("2.5b", LensReading.Friendship), Is.False);
            Assert.That(s.ApplyLens("2.5b", LensReading.SomethingMore), Is.True);
            Assert.That(s.ApplyLens("2.5b", LensReading.SomethingMore), Is.False);
            s.ApplyLens("2.6", LensReading.SomethingMore);
            s.ApplyLens("3.8", LensReading.SomethingMore);
            s.ApplyLens("extra-1", LensReading.SomethingMore);
            Assert.That(s.ApplyLens("extra-2", LensReading.SomethingMore), Is.False);
            Assert.That(s.Lens, Is.EqualTo(NarrativeCatalog.MaxLens));
        }

        [Test]
        public void DecisionC3_CanOnlyBeTakenOnce()
        {
            var s = new NarrativeState();
            s.DecideC3(DecisionC3.Confess);
            Assert.Throws<NarrativeRuleException>(() => s.DecideC3(DecisionC3.HangUp));
        }

        [Test]
        public void Chapters_OnlyMoveForward()
        {
            var s = new NarrativeState();
            s.AdvanceTo(Chapter.Chapter3Lima);
            Assert.Throws<NarrativeRuleException>(() => s.AdvanceTo(Chapter.Chapter1Arequipa));
        }

        [Test]
        public void RewindToChapter4_ClearsOnlyChapter5Choices()
        {
            var s = new NarrativeState();
            s.SaveSignal("S10");
            s.DecideC3(DecisionC3.StaySilent);
            s.AdvanceTo(Chapter.Chapter5AprenderAMirar);
            s.SaveInChapter5("C5_Chalina");
            s.DecideHonestyWithValeria(true);
            s.DecideFinalMessage(FinalMessage.Sent);

            s.RewindToChapter4();

            Assert.That(s.CurrentChapter, Is.EqualTo(Chapter.Chapter4OtraMirada));
            Assert.That(s.SavesInChapter5, Is.Zero);
            Assert.That(s.FinalMessage, Is.EqualTo(FinalMessage.None));
            Assert.That(s.HonestWithValeriaDecided, Is.False);
            Assert.That(s.Signals, Is.EqualTo(1));
            Assert.That(s.DecisionC3, Is.EqualTo(DecisionC3.StaySilent));
        }

        [Test]
        public void Snapshot_RoundTripsEveryVariable()
        {
            var s = new NarrativeState();
            s.AdvanceTo(Chapter.Chapter5AprenderAMirar);
            s.SaveSignal("S03"); s.SaveSignal("S17");
            s.NoticeGesture("V1"); s.NoticeGesture("V7");
            s.FindDadMemory("M4_BoletoChivay");
            s.ApplyLens("2.6", LensReading.SomethingMore);
            s.SaveInChapter5("C5_QuesoHelado");
            s.DecideC3(DecisionC3.HangUp);
            s.SetNicoResponse(NicoResponse.Accept);
            s.SetNewYearWish(NewYearWish.DrawWithoutFear);
            s.DecideHonestyWithValeria(true);
            s.DecideFinalMessage(FinalMessage.Sent);
            s.AddWaitTime(42.5);

            var r = NarrativeSnapshot.From(s).ToState();

            Assert.That(r.CurrentChapter, Is.EqualTo(s.CurrentChapter));
            Assert.That(r.SavedSignalIds, Is.EquivalentTo(s.SavedSignalIds));
            Assert.That(r.NotedGestureIds, Is.EquivalentTo(s.NotedGestureIds));
            Assert.That(r.DadMemoryIds, Is.EquivalentTo(s.DadMemoryIds));
            Assert.That(r.LensMomentIds, Is.EquivalentTo(s.LensMomentIds));
            Assert.That(r.Chapter5SaveIds, Is.EquivalentTo(s.Chapter5SaveIds));
            Assert.That(r.DecisionC3, Is.EqualTo(DecisionC3.HangUp));
            Assert.That(r.NicoResponse, Is.EqualTo(NicoResponse.Accept));
            Assert.That(r.NewYearWish, Is.EqualTo(NewYearWish.DrawWithoutFear));
            Assert.That(r.HonestWithValeria, Is.True);
            Assert.That(r.FinalMessage, Is.EqualTo(FinalMessage.Sent));
            Assert.That(r.WaitSecondsTotal, Is.EqualTo(42.5));
        }

        [Test]
        public void Service_PersistsAndPublishes()
        {
            var bus = new EventBus();
            var repo = new InMemoryRepository();
            var service = new NarrativeService(bus, repo);
            var received = new List<SignalSaved>();
            using (bus.Subscribe<SignalSaved>(received.Add))
            {
                service.SaveSignal("S01");
                service.SaveSignal("S01");
            }

            service.SaveSignal("S02");

            Assert.That(received.Count, Is.EqualTo(1), "Duplicados y desuscritos no publican");
            Assert.That(repo.Saved.savedSignals, Is.EquivalentTo(new[] { "S01", "S02" }));
            Assert.That(new NarrativeService(bus, repo).State.Signals, Is.EqualTo(2), "Se recupera la partida");
        }

        private sealed class InMemoryRepository : INarrativeRepository
        {
            public NarrativeSnapshot Saved;

            public bool TryLoad(out NarrativeSnapshot snapshot)
            {
                snapshot = Saved;
                return Saved != null;
            }

            public void Save(NarrativeSnapshot snapshot) => Saved = snapshot;
            public void Delete() => Saved = null;
        }
    }
}
