using System;
using Ink.Runtime;
using UFQ.Core.Narrative;
using UFQ.UseCases.Narrative;

namespace UFQ.Infrastructure.Dialogue
{
    /// <summary>
    /// Conecta los guiones .ink con el dominio. Ink decide QUÉ se dice; el dominio decide
    /// QUÉ significa. Ink nunca guarda estado propio: llama a funciones externas que pasan
    /// por NarrativeService, y lee las variables que este puente sincroniza.
    ///
    /// Las variables y funciones deben estar declaradas en Narrative/Ink/_variables.ink.
    /// </summary>
    public sealed class InkNarrativeBridge : IDisposable
    {
        private readonly Story _story;
        private readonly NarrativeService _narrative;

        public InkNarrativeBridge(Story story, NarrativeService narrative)
        {
            _story = story;
            _narrative = narrative;
            Bind();
            SyncToStory();
        }

        public Story Story => _story;

        /// <summary>Copia el estado del dominio a las variables de Ink (antes de cada bloque).</summary>
        public void SyncToStory()
        {
            var s = _narrative.State;
            var v = _story.variablesState;
            v["SENALES"] = s.Signals;
            v["LENTE"] = s.Lens;
            v["VALE"] = s.Vale;
            v["GUARDAR_C5"] = s.SavesInChapter5;
            v["RECUERDOS_PAPA"] = s.DadMemoriesFound;
            v["DECISION_C3"] = s.DecisionC3.ToString();
            v["NICO_RESP"] = s.NicoResponse.ToString();
            v["HONESTO_VALE"] = s.HonestWithValeria;
            v["ESPERA_TOTAL"] = (int)s.WaitSecondsTotal;
        }

        private void Bind()
        {
            _story.BindExternalFunction("guardar_senal", (string id) => Mutate(() => _narrative.SaveSignal(id)));
            _story.BindExternalFunction("notar_gesto", (string id) => Mutate(() => _narrative.NoticeGesture(id)));
            _story.BindExternalFunction("recuerdo_papa", (string id) => Mutate(() => _narrative.FindDadMemory(id)));
            _story.BindExternalFunction("guardar_c5", (string id) => Mutate(() => _narrative.SaveInChapter5(id)));
            _story.BindExternalFunction("lente", (string momento, bool algoMas) =>
                Mutate(() => _narrative.ApplyLens(momento, algoMas ? LensReading.SomethingMore : LensReading.Friendship)));
            _story.BindExternalFunction("decidir_c3", (string valor) =>
                Mutate(() => _narrative.DecideC3(Parse<DecisionC3>(valor))));
            _story.BindExternalFunction("respuesta_nico", (string valor) =>
                Mutate(() => _narrative.SetNicoResponse(Parse<NicoResponse>(valor))));
            _story.BindExternalFunction("deseo_ano_nuevo", (string valor) =>
                Mutate(() => _narrative.SetNewYearWish(Parse<NewYearWish>(valor))));
            _story.BindExternalFunction("honesto_vale", (bool honesto) =>
                Mutate(() => _narrative.DecideHonestyWithValeria(honesto)));
            _story.BindExternalFunction("mensaje_final", (string valor) =>
                Mutate(() => _narrative.DecideFinalMessage(Parse<FinalMessage>(valor))));
        }

        private void Mutate(Action change)
        {
            change();
            SyncToStory();
        }


        private static T Parse<T>(string value) where T : struct, Enum
        {
            // Solo nombres de miembros: "99" o "3" no valen aunque Enum.TryParse los acepte.
            if (!string.IsNullOrWhiteSpace(value) && !char.IsDigit(value.Trim()[0]) && value.Trim()[0] != '-'
                && Enum.TryParse(value, true, out T result) && Enum.IsDefined(typeof(T), result))
            {
                return result;
            }

            throw new NarrativeRuleException($"Valor de Ink inválido para {typeof(T).Name}: «{value}»");
        }

        public void Dispose()
        {
            foreach (var name in new[]
                     {
                         "guardar_senal", "notar_gesto", "recuerdo_papa", "guardar_c5", "lente", "decidir_c3",
                         "respuesta_nico", "deseo_ano_nuevo", "honesto_vale", "mensaje_final",
                     })
            {
                _story.UnbindExternalFunction(name);
            }
        }
    }
}
