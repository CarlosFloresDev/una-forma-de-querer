// =====================================================================
//  CAPÍTULO 1 — AREQUIPA, «El comienzo»   (guion, escenas 1.1 a 1.12)
//  Agosto a 31 de diciembre · Paleta: blanco sillar, azul cielo, fucsia buganvilla
//
//  Convenciones (ver Prologo.ink):
//    «PERSONAJE: texto»  → globo de texto (el nombre se separa en DialogueLine)
//    #accion  #voz_interior  #titulo  #celular:entrante|enviado|borrador
//    #senal:Sxx        → gesto de Abril que brilla; la UI muestra «Guardar»
//    #gesto:Vx         → gesto de Valeria; NO brilla, solo «Mirar»
//    #semilla:X        → pista para el Cap. 4 (solo informativa, para QA)
//    #escena:1.x  #musica:  #sfx:  #camara:  #juego:<id> (minijuego/acción jugable)
//
//  Guardar una señal o notar un gesto lo decide el JUGADOR en la presentación,
//  que llama a NarrativeService. Ink solo marca el momento con la etiqueta.
// =====================================================================
INCLUDE _variables.ink

-> c1_terminal

// ---------------------------------------------------------------- 1.1
=== c1_terminal ===
#escena:1.1 #musica:vals_ciudad_blanca #camara:terminal_llegada
Un bus interprovincial llega después de dieciséis horas desde Lima. El sol pega fuerte. Al fondo, el Misti con su gorrito de nieve. #accion
DON AURELIO: ¿Primera vez en Arequipa, joven?
MATEO: Vengo a estudiar.
DON AURELIO: Aquí el sol pega fuerte y el aire es delgadito. Todo cuesta un poco más… pero el cielo vale la pena.
#juego:tutorial_movimiento #semilla:panel_salidas
Una ciudad donde nadie me conoce. Eso era lo que quería. Creo. #voz_interior
-> c1_libreria

// ---------------------------------------------------------------- 1.2
=== c1_libreria ===
#escena:1.2 #camara:libreria_interior
Una casona de sillar. Estantes hasta el techo, olor a papel, una campanita en la puerta. Un gato negro, Tinta, duerme sobre una pila de diccionarios. #accion
DOÑA INÉS: Tú debes ser el joven de la beca. Rivas, ¿no? El limeño.
MATEO: Mateo. Sí, señora.
DOÑA INÉS: Nada de «señora», que me haces vieja. Inés nomás. Tres reglas: nada de música después de las once, el alquiler los días cinco… y el gato manda.
#juego:acariciar_tinta #semilla:A_canasta_separadores
DOÑA INÉS: Necesito tu fecha de nacimiento para el contrato.
MATEO: Catorce de julio.
Inés lo anota en el contrato. Luego sube a la cocina y lo escribe también en un calendario lleno de nombres. #accion #semilla:B_calendario
#sfx:campanita
ABRIL: ¡Abuela, la combi me dejó plantada otra vez!
ABRIL: ¡Tú eres el de la azotea! Bienvenido a Arequipa. Si necesitas algo, da dos golpes en el piso. Yo duermo justo debajo.
MATEO: Gracias. Igual no hace falta.
ABRIL: Siempre hace falta algo.
Abril le deja en las manos un vaso de chicha morada que era para ella y sube corriendo. #accion
-> c1_azotea

// ---------------------------------------------------------------- 1.3
=== c1_azotea ===
#escena:1.3 #camara:azotea_noche
#juego:desempacar #semilla:recuerdo_papa_1
Mateo abre el cuaderno en blanco. Le tiembla la mano. #accion
#juego:dibujo_ventana
Dos años sin dibujar. Papá, ¿esto cuenta? #voz_interior
#sfx:dos_golpes_piso
…Dos golpes. Si necesitas algo. #voz_interior
-> c1_aula

// ---------------------------------------------------------------- 1.4
=== c1_aula ===
#escena:1.4 #camara:aula_3b
ABRIL: Te ves cansada. ¿Dormiste? #para:SOFI #semilla:C_dormiste_sofi
ABRIL: Te ves cansado. ¿Dormiste? #senal:S01
MATEO: Un poco. La cama de la azotea suena.
ABRIL: Ah, sí. Es que antes era mía.
PROF. SALGADO: Proyecto del semestre: «El mapa de la Arequipa que no sale en los mapas». Rincones, oficios, historias. Grupos de cuatro. Hasta diciembre.
NICO: Abril, Sofi, yo… y nos falta uno.
ABRIL: Ya no.
NICO: Nico. Crecí en la casa de al lado de Abril. Te aviso, causa: si te adopta, ya no hay vuelta atrás. #semilla:N_advertencia
Dos filas atrás, Valeria dibuja en el margen de su cuaderno pequeño. #accion #gesto:V3
-> c1_tintero

// ---------------------------------------------------------------- 1.5
=== c1_tintero ===
#escena:1.5 #camara:tintero_barra
DON FERMÍN: Así que tú eres el recomendado de Inés. Aquí se chambea, joven. Valeria te enseña.
VALERIA: Hola. Estamos en la misma clase. No sé si me has visto.
MATEO: Eh… sí. Claro.
#juego:minijuego_cafe
Valeria clava una tarjeta en el tablón de cumpleaños: «MATEO — 14/07». #accion #semilla:O_tablon
#sfx:campanita
ABRIL: ¡Un chocolate para llevar! Y… ¡hola, barista nuevo!
Al irse, Abril deja una servilleta con una flor dibujada: «¡Ánimo, barista! 🌼». #accion #senal:S02
En el descanso, Valeria deja una bandeja con un café… y una galleta de más. No dice nada. #accion #gesto:V1
-> c1_montaje

// ---------------------------------------------------------------- 1.6
=== c1_montaje ===
#escena:1.6 #musica:vals_ciudad_blanca_pleno
Semanas del proyecto. El grupo recorre la ciudad buscando lugares que no salen en los mapas. #accion
-> c1_montaje_hub

= c1_montaje_hub
+ {not c1_mercado} [Ir al Mercado San Camilo] -> c1_mercado
+ {not c1_picanteria} [Ir a la picantería de Sachaca] -> c1_picanteria
+ {not c1_biblioteca} [Ir a la Biblioteca Regional] -> c1_biblioteca
* {c1_mercado and c1_picanteria and c1_biblioteca} [Seguir] -> c1_madrugada

= c1_mercado
#escena:1.6m #juego:explorar_san_camilo
RELOJERO: Cincuenta años arreglando el tiempo de los demás. El mío nunca lo arreglé.
Abril compra fruta para la señora del puesto de al lado, «que está sola». #accion
-> c1_montaje_hub

= c1_picanteria
#escena:1.6p #juego:explorar_sachaca
DOÑA LUCHA: Aquí nadie se va con hambre.
Abril le regala a Doña Lucha un dibujo de su picantería «para que lo cuelgue». #accion
-> c1_montaje_hub

= c1_biblioteca
#escena:1.6b #camara:biblioteca
ABRIL: Te guardé el sitio. #senal:S03
Al fondo, sobre dos sillas más, una chalina y una bolsa de Abril. Al minuto llegan Nico y Sofi. #accion #semilla:D_tres_sitios
-> c1_montaje_hub

// ---------------------------------------------------------------- 1.7
=== c1_madrugada ===
#escena:1.7 #camara:azotea_noche
ABRIL: [foto de un gato aplastado contra una ventana] este eres tú jajaja #celular:entrante #senal:S04
El chat del grupo «MAPA FANTASMA» tiene 3 mensajes nuevos. #accion #semilla:E_chat_grupo
MATEO: jajaja ¿cómo sabías? #celular:enviado #juego:espera:40
ABRIL: intuición 😌 ¡a dormir! #celular:entrante
-> c1_presentacion

// ---------------------------------------------------------------- 1.8
=== c1_presentacion ===
#escena:1.8 #camara:aula_maqueta
PROF. SALGADO: Sobresaliente. Hace años que nadie me enseñaba mi propia ciudad.
Abril abraza a los cuatro a la vez. #accion
ABRIL: Estoy orgullosa de ti, de verdad. El castillo de la maqueta lo dibujaste tú y es lo más bonito de todo ✨ #celular:entrante #senal:S05
-> c1_separador

// ---------------------------------------------------------------- 1.9
=== c1_separador ===
#escena:1.9 #camara:libreria_noche
ABRIL: ¡Ven! Te hice algo.
ABRIL: Este es especial. Para ti. Porque dibujaste el castillo. #senal:S06 #semilla:G_separadores
ABRIL: Oye… Sofi tiene casa en Mollendo. En febrero, para carnavales, vamos los cuatro. Vienes, ¿no? #senal:S07
DOÑA INÉS: Esta chiquilla invita hasta al cartero. #semilla:H_cartero
ABRIL: ¡Y el 31 todos a Yanahuara! A ver los fuegos. ¡Con algo amarillo, que da suerte!
-> c1_turno

// ---------------------------------------------------------------- 1.10
=== c1_turno ===
#escena:1.10 #camara:tintero_31
ABRIL: ¡¡Esta noche Yanahuara!! Nos vemos en el mirador a las 11. Vienes, ¿no? 💛 #celular:entrante
VALERIA: Anda. Yo cierro.
MATEO: ¿En serio? Te toca doble.
VALERIA: No tengo planes.
* [«Gracias, Vale. Te debo una, de verdad.»]
    ~ notar_gesto("V2")
    Valeria sonríe y baja la mirada. #accion
* [«¡Eres la mejor!» (salir corriendo)]
    Valeria se queda mirando la puerta. #accion #gesto:V2 #solo_si_mira_atras
- -> c1_yanahuara

// ---------------------------------------------------------------- 1.11
=== c1_yanahuara ===
#escena:1.11 #musica:ano_nuevo #camara:mirador_yanahuara
Arcos de sillar con versos grabados. Abajo, toda Arequipa encendida. El grupo escribe sus deseos en papelitos para quemarlos a medianoche. #accion
ABRIL: Pide algo bonito.
* [«Que esto dure.»]
    ~ deseo_ano_nuevo("ThatThisLasts")
* [«Volver a dibujar sin miedo.»]
    ~ deseo_ano_nuevo("DrawWithoutFear")
* [«Que ella…»]
    ~ deseo_ano_nuevo("ThatSheUnfinished")
- ABRIL: ¡Si lo lees, no se cumple! #semilla:F_deseo_oculto
#sfx:doce_campanadas #camara:fuegos_artificiales
Queman los papelitos en una lata. Abril abraza a Sofi, a Nico, y le da a Mateo un abrazo rápido y un codazo cariñoso. #accion
-> c1_titulo

// ---------------------------------------------------------------- 1.12
=== c1_titulo ===
#escena:1.12 #camara:azotea_madrugada
#juego:titulo_cuaderno
No sé cuándo empezó. Solo sé que empecé a esperar sus mensajes. #voz_interior
FIN DEL CAPÍTULO 1 #titulo
-> END
