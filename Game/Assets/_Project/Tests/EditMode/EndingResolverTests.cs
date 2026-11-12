using NUnit.Framework;
using UFQ.Core.Narrative;

namespace UFQ.Tests.EditMode
{
    /// <summary>Verifica el árbol de finales del Anexo A del guion.</summary>
    public class EndingResolverTests
    {
        private static NarrativeState Played(FinalMessage msg, int c5Saves = 0, int vale = 0, bool honest = false)
        {
            var s = new NarrativeState();
            for (var i = 0; i < vale; i++) s.NoticeGesture(NarrativeCatalog.ValeriaGestures[i]);
            for (var i = 0; i < c5Saves; i++) s.SaveInChapter5(NarrativeCatalog.Chapter5Gestures[i]);
            s.DecideHonestyWithValeria(honest);
            if (msg != FinalMessage.None) s.DecideFinalMessage(msg);
            return s;
        }

        [Test]
        public void ThreeSavesInChapter5_GivesInfiniteNotebook_EvenIfMessageWasSent()
        {
            var r = EndingResolver.Resolve(Played(FinalMessage.Sent, c5Saves: 3, vale: 7, honest: true));
            Assert.That(r.Ending, Is.EqualTo(Ending.C_CuadernoInfinito));
            Assert.That(r.IncludesSecretCodaD, Is.False);
        }

        [Test]
        public void ThreeSavesInChapter5_EndsWithoutMessageDecision()
        {
            // En el final C el Epílogo se corta en E.1: no llega a la escena 5.3 completa.
            var s = Played(FinalMessage.None, c5Saves: 3);
            Assert.That(EndingResolver.Resolve(s).Ending, Is.EqualTo(Ending.C_CuadernoInfinito));
            Assert.That(EndingResolver.EpilogueIsCutShort(s), Is.True);
        }

        [Test]
        public void DeletedMessage_GivesDistance()
        {
            Assert.That(EndingResolver.Resolve(Played(FinalMessage.Deleted, c5Saves: 2)).Ending,
                Is.EqualTo(Ending.B_Distancia));
        }

        [Test]
        public void SentMessage_GivesTrueEnding()
        {
            var r = EndingResolver.Resolve(Played(FinalMessage.Sent));
            Assert.That(r.Ending, Is.EqualTo(Ending.A_UnaFormaDeQuerer));
            Assert.That(r.IncludesSecretCodaD, Is.False);
        }

        [TestCase(5, true, true)]
        [TestCase(7, true, true)]
        [TestCase(4, true, false)]
        [TestCase(7, false, false)]
        public void SecretCodaD_NeedsHonestyAndFiveGestures(int vale, bool honest, bool expected)
        {
            var r = EndingResolver.Resolve(Played(FinalMessage.Sent, vale: vale, honest: honest));
            Assert.That(r.IncludesSecretCodaD, Is.EqualTo(expected));
        }

        [Test]
        public void MissingFinalMessage_IsAnError()
        {
            Assert.Throws<NarrativeRuleException>(() => EndingResolver.Resolve(Played(FinalMessage.None)));
        }

        [Test]
        public void FathersLastPage_NeedsAllSixMemories()
        {
            var s = new NarrativeState();
            foreach (var id in NarrativeCatalog.DadMemories)
            {
                Assert.That(EndingResolver.FathersLastPageUnlocked(s), Is.False);
                s.FindDadMemory(id);
            }

            Assert.That(EndingResolver.FathersLastPageUnlocked(s), Is.True);
        }
    }
}
