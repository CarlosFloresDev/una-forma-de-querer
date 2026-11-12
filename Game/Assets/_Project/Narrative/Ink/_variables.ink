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
// Reproducen las reglas del dominio: cada id cuenta una sola vez y hay topes
// (17 señales, 7 gestos, 6 recuerdos, 4 lentes, 3 guardados en el Cap. 5).
VAR _ids = ""

=== function _contar_una_vez(id)
// Devuelve true solo la primera vez que se ve este id.
{ _ids ? ("|" + id + "|"):
    ~ return false
}
~ _ids = _ids + "|" + id + "|"
~ return true

=== function guardar_senal(id)
{ SENALES < 17 && _contar_una_vez(id):
    ~ SENALES = SENALES + 1
}
=== function notar_gesto(id)
{ VALE < 7 && _contar_una_vez(id):
    ~ VALE = VALE + 1
}
=== function recuerdo_papa(id)
{ RECUERDOS_PAPA < 6 && _contar_una_vez(id):
    ~ RECUERDOS_PAPA = RECUERDOS_PAPA + 1
}
=== function guardar_c5(id)
{ GUARDAR_C5 < 3 && _contar_una_vez(id):
    ~ GUARDAR_C5 = GUARDAR_C5 + 1
}
=== function lente(momento, algoMas)
{ algoMas && LENTE < 4 && _contar_una_vez("lente:" + momento):
    ~ LENTE = LENTE + 1
}
=== function decidir_c3(valor)
{ DECISION_C3 == "None":
    ~ DECISION_C3 = valor
}
=== function respuesta_nico(valor)
~ NICO_RESP = valor
=== function deseo_ano_nuevo(valor)
~ return
=== function honesto_vale(honesto)
~ HONESTO_VALE = honesto
=== function mensaje_final(valor)
~ return