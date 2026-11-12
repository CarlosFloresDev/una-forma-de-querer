// =====================================================================
//  Variables y funciones compartidas por todos los capítulos.
//  Las variables las ESCRIBE el juego (InkNarrativeBridge.SyncToStory):
//  aquí solo se leen. Para cambiar el estado, llama a las funciones EXTERNAL.
//  Nombres = tabla 4.6 del guion.
// =====================================================================

VAR SENALES = 0
VAR LENTE = 0
VAR VALE = 0
VAR GUARDAR_C5 = 0
VAR RECUERDOS_PAPA = 0
VAR DECISION_C3 = "None"
VAR NICO_RESP = "None"
VAR HONESTO_VALE = false
VAR ESPERA_TOTAL = 0

EXTERNAL guardar_senal(id)
EXTERNAL notar_gesto(id)
EXTERNAL recuerdo_papa(id)
EXTERNAL guardar_c5(id)
EXTERNAL lente(momento, algoMas)
EXTERNAL decidir_c3(valor)
EXTERNAL respuesta_nico(valor)
EXTERNAL deseo_ano_nuevo(valor)
EXTERNAL honesto_vale(honesto)
EXTERNAL mensaje_final(valor)

// --- Versiones de respaldo para probar en Inky sin el juego ---------
=== function guardar_senal(id)
~ SENALES = SENALES + 1
=== function notar_gesto(id)
~ VALE = VALE + 1
=== function recuerdo_papa(id)
~ RECUERDOS_PAPA = RECUERDOS_PAPA + 1
=== function guardar_c5(id)
~ GUARDAR_C5 = GUARDAR_C5 + 1
=== function lente(momento, algoMas)
{ algoMas:
    ~ LENTE = LENTE + 1
}
=== function decidir_c3(valor)
~ DECISION_C3 = valor
=== function respuesta_nico(valor)
~ NICO_RESP = valor
=== function deseo_ano_nuevo(valor)
~ return
=== function honesto_vale(honesto)
~ HONESTO_VALE = honesto
=== function mensaje_final(valor)
~ return
