// =====================================================================
//  PRÓLOGO — «Escribiendo…»  (guion, escena P.1)
//  INT. CUARTO DE LA AZOTEA, LA PLUMA QUIETA, AREQUIPA — NOCHE (14 DE AGOSTO)
//
//  Etiquetas (#) que interpreta la capa de presentación:
//    #musica:  #sfx:  #camara:  #celular:borrador|entrante  #accion:  #voz_interior
//    #solo_boton:BORRAR   (la elección solo muestra ese botón)
// =====================================================================
INCLUDE _variables.ink

-> prologo

=== prologo ===
#musica:silencio_viento_serenata_lejana
#camara:azotea_plano_general
Un cuarto pequeño de azotea. Por la ventana, el Misti bajo la luna. Cada tanto, un fuego artificial lejano tiñe el cuarto de color. #accion
Sobre el escritorio, un cuaderno cerrado. En la portada, a mano: SEÑALES. #accion
-> borradores

= borradores
Abril, tengo que decirte algo que llevo meses #celular:borrador
+ [BORRAR] #solo_boton:BORRAR
- Abril, ¿te acuerdas del castillo? #celular:borrador
+ [BORRAR] #solo_boton:BORRAR
- Abril, yo #celular:borrador
#esperar:5
¿Cómo íbamos a terminar… si nunca fuimos nada? #voz_interior
-> mensaje_entrante

= mensaje_entrante
#sfx:vibracion
ABRIL: ¿Sigues despierto? 😊 #celular:entrante
Mateo deja el celular boca abajo sobre la frazada. La luz se escapa por los bordes. #accion
#celular:bloqueado
Abre el cuaderno. Primera página: el Terminal Terrestre de Arequipa bajo el sol, con una maleta. #accion
#camara:entrar_en_dibujo
UN AÑO ANTES #titulo
-> END
