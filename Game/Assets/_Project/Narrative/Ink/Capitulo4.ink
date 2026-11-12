// =====================================================================
//  CAPÍTULO 4 — AREQUIPA, «La otra mirada»   (guion, escenas 4.1 a 4.7)
//  14 y 15 de agosto (aniversario de Arequipa) · Paleta: blanco papel, azul noche y crema
//
//  Convenciones: ver Capitulo1.ink a Capitulo3.ink. Nuevo en este capítulo:
//    #pagina:<id>        Mateo entra en una página del cuaderno (sueño, 4.4 / 4.5)
//    #de_senal:Sxx       la página es de esa señal: dorada si se guardó, a lápiz si no
//    #juego:mirar_alrededor  la cámara se abre y la luz se reparte entre todos
//    #pagina_crema       la página deja de brillar: «solo se vuelve normal»
//    #textura:carboncillo  página imaginada (4.4.10), no recordada
//    #controlar:VALERIA  el jugador pasa a controlar a Valeria (4.5), sin aviso
//    #brillo_valeria     el gesto brilla, pero en el cuaderno de ella
//    #ui:ocultar_medidor el Medidor de Ilusión desaparece para siempre
//    #sin_brillo         los gestos de Abril ya NO brillan ni ofrecen «Guardar»
//
//  Las páginas NO usan #senal: en el sueño no se guarda nada.
// =====================================================================
INCLUDE _variables.ink

-> c4_vereda

// ---------------------------------------------------------------- 4.1
=== c4_vereda ===
#escena:4.1 #musica:vals_ciudad_blanca_noche #camara:tintero_vereda
Arequipa se prepara para su aniversario: banderas de la ciudad en los balcones, ambulantes, gente que camina hacia la serenata. Sopla el viento seco de agosto. #accion
A través del vidrio de El Tintero: Daniel toca guitarra en un rincón. En la mesa de adelante, Abril lo mira con la barbilla apoyada en las manos, igual que en el muelle de Mollendo. Sofi y Nico están con ella. #accion
#juego:puerta_cerrada
Hoy no. #voz_interior
Por la puerta de atrás sale Valeria con una bolsa de basura. Ve a Mateo en la vereda. #accion
VALERIA: ¿No entras? Hace frío.
MATEO: Estoy cansado del viaje.
Valeria mira hacia adentro, a Abril, y luego a Mateo. Lo entiende todo en un segundo. No dice nada. #accion
VALERIA: Descansa, Mateo.
{ marcado("ConchitaRecogida"): -> conchita ->}
-> c4_regreso

= conchita
* [Dar la conchita]
    ~ marcar("ConchitaEntregada")
    MATEO: Te la debía. Desde febrero.
    Valeria la mira mucho rato. #accion
    VALERIA: Te acordaste.
* [Despedirse]
    Mateo se despide con la mano. La conchita se queda en su bolsillo. #accion
- ->->

// ---------------------------------------------------------------- 4.2
=== c4_regreso ===
#escena:4.2 #camara:libreria_interior
#sfx:campanita
Tinta salta del mostrador y se enreda en las piernas de Mateo. Doña Inés lo abraza. #accion
DOÑA INÉS: ¡Hijito! Sin ti la azotea ni sonaba.
{ marcado("BallenaEnAzotea"):
    Sobre el mostrador, la ballena de peluche. #accion
    DOÑA INÉS: Te la cuidé.
}
Se oyen pasos en la escalera. Es Abril, que volvió un momento por una casaca antes de ir a la serenata. #accion
{
- DECISION_C3 == "Confess":
    Abril se detiene en el último escalón. Duda. Luego baja y lo abraza con cuidado, como si él fuera de vidrio. #accion
    ABRIL: ¿Podemos estar bien? Yo… quiero que estemos bien.
    MATEO: Estamos bien.
    Ninguno de los dos se lo cree del todo todavía. #accion
- DECISION_C3 == "HangUp":
    Abril baja despacio. Se queda a un metro. #accion
    ABRIL: Me tenías preocupada. No contestaste en semanas.
    MATEO: Fueron semanas raras.
    ABRIL: ¿Hice algo mal?
    MATEO: …Mañana hablamos. Estoy muerto.
- else:
    Abril baja corriendo y salta a abrazarlo. #accion
    ABRIL: ¡Volviste! Vamos a la serenata, ¡tienes que conocer a Daniel en serio! ¿Ya?
    MATEO: Hoy no. Estoy muerto. Mañana.
}
Abril sale a la serenata. Inés observa desde el sillón, por encima de los lentes, y no dice nada. Mateo sube a la azotea. #accion
-> c4_azotea

// ---------------------------------------------------------------- 4.3
=== c4_azotea ===
#escena:4.3 #musica:silencio_viento_serenata_lejana #camara:azotea_plano_general
Un cuarto pequeño de azotea. Por la ventana, el Misti bajo la luna. Cada tanto, un fuego artificial lejano tiñe el cuarto de color. #accion
Abril, tengo que decirte algo que llevo meses #celular:borrador
+ [BORRAR] #solo_boton:BORRAR
- Abril, ¿te acuerdas del castillo? #celular:borrador
+ [BORRAR] #solo_boton:BORRAR
- Abril, yo #celular:borrador
#esperar:5
¿Cómo íbamos a terminar… si nunca fuimos nada? #voz_interior
#sfx:vibracion
ABRIL: ¿Sigues despierto? 😊 #celular:entrante
Mateo deja el celular boca abajo sobre la frazada. Esta vez la escena sigue. #accion
* [Mirar la casaca]
    ~ recuerdo_papa("M4_BoletoChivay")
    En el bolsillo interior de la casaca hay un boleto de bus antiguo: «Arequipa–Chivay», a nombre de Andrés. #accion
    No sabía que lo llevaba conmigo. #voz_interior
* [Abrir el cuaderno]
- Mateo abre el cuaderno SEÑALES y pasa las páginas doradas una por una. #accion #juego:paginas_doradas
{ SENALES == 0:
    Ni siquiera lo apunté. Y aun así me lo creí. #voz_interior
- else:
    {veces(SENALES)} creí que significaba algo. #voz_interior
}
Un último fuego artificial lejano. Mateo se queda dormido sobre el cuaderno con la luz prendida. Las páginas se mueven solas. #accion
#camara:entrar_en_dibujo
-> c4_sueno

// ---------------------------------------------------------------- 4.4
=== c4_sueno ===
#escena:4.4 #musica:la_otra_mirada #camara:cuaderno_general
Un espacio blanco infinito, como sillar recién cortado. Páginas de cuaderno gigantes flotan como islas. Mateo camina entre ellas, descalzo, con la casaca de su padre. #accion
-> pagina_1

= pagina_1
#pagina:4.4.1 #de_senal:S01
Aula 3B, a lápiz. La luz dorada cae solo sobre Mateo, como un foco. #accion
+ [Mirar alrededor]
- #juego:mirar_alrededor
Abril entra al aula y le dice a Sofi, en grande y legible: #accion
ABRIL: Te ves cansada. ¿Dormiste?
Luego se sienta junto a Mateo y le dice lo mismo. #accion #pagina_crema
-> pagina_2

= pagina_2
#pagina:4.4.2 #de_senal:S06
El mostrador de la librería, gigante. La canasta «Hechos por Abril». #accion
+ [Mirar alrededor]
- #juego:mirar_alrededor
Cuatro separadores especiales en fila: un castillo para Mateo, una guitarra para Nico, un sol para Sofi y una taza de mate para la abuela. #accion #pagina_crema
-> pagina_3

= pagina_3
#pagina:4.4.3 #de_senal:S03
La biblioteca. Un sitio guardado. #accion
+ [Mirar alrededor]
- #juego:mirar_alrededor
Tres sillas guardadas con tres prendas: la casaca, la chalina y la bolsa de Abril. #accion #pagina_crema
-> pagina_4

= pagina_4
#pagina:4.4.4 #de_senal:S04
El celular de Abril, del tamaño de una puerta. 2:07 de la madrugada. #accion
+ [Mirar alrededor]
- #juego:mirar_alrededor
A Mateo: «este eres tú jajaja». A Nico: «este eres tú jajaja». A Sofi: «esta eres tú jajaja». #accion
A la abuela: «esta eres tú abuela jajaja (es un gato)». #accion #pagina_crema
-> pagina_5

= pagina_5
#pagina:4.4.5 #de_senal:S10
La feria de Mollendo, de papel. #accion
+ [Mirar alrededor]
- #juego:mirar_alrededor
Tres peluches: la ballena en manos de Mateo, la estrella en las de Sofi y el pulpo en las de Lalo. Sobre los tres, el mismo globo de texto: #accion
ABRIL: Para que te acuerdes de este día.
#pagina_crema
-> pagina_6

= pagina_6
#pagina:4.4.6 #de_senal:S09
Una foto: «Esta es mi foto favorita del viaje». #accion
+ [Mirar alrededor]
- #juego:mirar_alrededor
La publicación de Abril: un collage de doce fotos del viaje. Todas con la misma etiqueta: «favorita 💛». #accion #pagina_crema
-> pagina_7

= pagina_7
#pagina:4.4.7 #de_senal:S11 #camara:cuaderno_castillo
Mateo y Abril sentados en el muro del castillo, de papel, sobre un mar de papel. Esta vez la frase no se desvanece: se queda quieta, enorme, escrita sobre el cielo. #accion
ABRIL: El cariño que no se da se malogra. Así que yo lo doy todo. A todo el mundo. No sé hacerlo de otra manera.
Junto a la frase, a lápiz: «Me lo da todo. A mí». #accion
+ [Mirar]
- #juego:mirar_alrededor
La frase de Mateo se tacha sola. #accion
Debajo del castillo aparece la fogata: Abril abraza a Nico. El mismo globo de texto: #accion
ABRIL: Me alegra tanto que estés aquí, sonso.
{ LENTE >= 2: -> banderines ->}
#pagina_crema
-> pagina_8

= banderines
Colgadas como banderines de papel, las frases que Mateo reescribió: «Contigo TODO es fácil», «Me alegra tanto que estés conmigo», «Te quiero». #accion
+ [Mirar los banderines]
- Al mirarlas, la tinta cálida se escurre. Debajo queda el texto original. #accion
Ella nunca dijo eso. Lo escribí yo. #voz_interior
->->

= pagina_8
#pagina:4.4.8 #de_senal:S16
El calendario de la cocina, gigante, lleno de nombres con la letra de Inés. #accion
+ [Mirar alrededor]
- #juego:mirar_alrededor
«Mateo (el de la azotea) — 14/07», entre otros cuarenta. Al lado, el celular de Abril con una alarma: «23:45 — Cumple Mateo (el de la azotea) 🎂». #accion
Abajo, en fila, doce cajas de alfajores idénticas: «Para mi vecino favorito», «Para mi favorita del mundo», «Para mi favorito de la azotea», «Para el cartero favorito»… #accion #pagina_crema
-> pagina_9

= pagina_9
#pagina:4.4.9
El Mirador de Yanahuara, de papel. Las cenizas de la lata se levantan y vuelven a formar el papelito de Abril. #accion
+ [Mirar alrededor]
- #juego:mirar_alrededor
El papelito se despliega solo. Por fin se lee su deseo: «Que la abuela viva muchos años y que todos mis amigos sean felices. Todos». #accion
Todos. Yo estoy en ese «todos». #voz_interior #pagina_crema
-> opcionales

= opcionales
Más lejos flotan otras páginas, más chicas. Se pueden visitar… o no. #accion
+ {not servilleta} [La servilleta] -> servilleta
+ {not orgullosa} [«Orgullosa de ti»] -> orgullosa
+ {not hombro} [El hombro] -> hombro
+ {not paraguas} [El paraguas] -> paraguas
+ {not casa_rara} [La calle rara] -> casa_rara
+ [Seguir hacia la última página] -> pagina_10

= servilleta
#pagina:4.4.9b_servilleta #de_senal:S02 #juego:mirar_alrededor
El corcho de la trastienda del Tintero, lleno de servilletas con flores dibujadas por Abril para cada mozo. También para Don Fermín: «¡Ánimo, jefe gruñón! 🌼». #accion #pagina_crema
-> opcionales

= orgullosa
#pagina:4.4.9b_orgullosa #de_senal:S05 #juego:mirar_alrededor
Tres mensajes idénticos la noche de la presentación: a Mateo, a Nico y a Sofi. «Estoy orgullosa de ti, de verdad». #accion #pagina_crema
-> opcionales

= hombro
#pagina:4.4.9b_hombro #de_senal:S08 #juego:mirar_alrededor
Una foto de una excursión del colegio: Abril niña, dormida en el hombro de Sofi en un bus. #accion
SOFI: Se duerme en cualquier bus. #pagina_crema
-> opcionales

= paraguas
#pagina:4.4.9b_paraguas #de_senal:S12 #juego:mirar_alrededor
Abril le regala a Nico una casaca impermeable «para el frío de Cusco», antes de sus prácticas. #accion #pagina_crema
-> opcionales

= casa_rara
#pagina:4.4.9b_calle_rara #de_senal:S13 #juego:mirar_alrededor
Abril le escribe a Nico cuando él se va a Cusco: «La calle está rara sin ti». #accion #pagina_crema
-> opcionales

= pagina_10
#pagina:4.4.10 #textura:carboncillo
Una página distinta: carboncillo, no lápiz. #accion
Esto no lo vi. Pero sé que fue así. #voz_interior
{
- DECISION_C3 == "Confess":
    Abril, en la cocina, llora en el regazo de Inés. #accion
    ABRIL: No quería hacerle daño, abuela. Te juro que no sabía.
    DOÑA INÉS: Ya sé, mi niña. Querer no es culpa de nadie. Ni tuya ni de él.
- DECISION_C3 == "HangUp":
    Abril, sentada en su cama, mira el celular: «¿Hice algo mal?», con el check sin leer. Lo vuelve a mirar. Lo vuelve a mirar. #accion
- else:
    Abril en el Tintero, contándole a Daniel con una sonrisa enorme: #accion
    ABRIL: Mateo se alegró un montón por mí. Es de los buenos.
}
Ella también espera mensajes. También mira el celular. #accion
Nunca me prometió nada. Nunca me dijo que me amaba. Fui yo quien guardó cada cosa como si fuera una promesa. #voz_interior
El cuaderno se cierra solo, con un golpe suave. #accion #ui:ocultar_medidor
-> c4_otro_cuaderno

// ---------------------------------------------------------------- 4.5
=== c4_otro_cuaderno ===
#escena:4.5 #musica:nota_escribiendo #camara:cuaderno_general
Cuando Mateo cree que el sueño terminó, cae del cielo blanco otro cuaderno, más pequeño. Tapa azul marino y una «V» en la portada. #accion
Este no es mío. #voz_interior
#controlar:VALERIA
-> galleta

= galleta
#pagina:4.5.1 #camara:valeria_espalda
El Tintero. Una bandeja con un café. #accion
+ [Poner una galleta de más]
- La galleta se ilumina en dorado. #accion #brillo_valeria
+ [Guardar]
- La guarda ella. #accion
{ VALE >= 5:
    Al margen, con su letra: «Hoy me dio las gracias de verdad». #accion
}
-> margen

= margen
#pagina:4.5.2
El aula 3B. El margen de un cuaderno pequeño. #accion
+ [Dibujar]
- Un chico de casaca grande, de espaldas. #accion #brillo_valeria
-> postulacion

= postulacion
#pagina:4.5.3
Una hoja de postulación a prácticas. «Arequipa», tachado con fuerza. Encima: «Lima — Estudio Cobre». #accion
En el margen, con letra pequeña: «Él dijo Barranco». #accion #semilla:L_pago
-> sopa

= sopa
#pagina:4.5.4 #camara:valeria_garua
La garúa de Lima. Un recipiente envuelto en una toalla, contra el pecho. #accion
#juego:caminar_garua
Dos combis. El camino es largo. #accion
Valeria toca la puerta. Mateo abre… y mira el celular. #accion
{ VALE >= 5:
    Al margen: «Me acompañó hasta la pensión». #accion
}
-> paraguas

= paraguas
#pagina:4.5.5
El paraguas azul, sobre Mateo. El hombro de ella se moja. #accion #brillo_valeria
En el morral de Mateo, un paraguas amarillo que no abre. #accion
-> cumpleanos

= cumpleanos
#pagina:4.5.6 #camara:valeria_mesa
Mateo sale corriendo al balcón. La cámara no lo sigue: se queda en la mesa. #accion
+ [Dejar la tarjeta]
- Valeria pone la tarjeta encima de los lápices, bien derechita. Se levanta. Se despide de Carmen. #accion
Al salir por la puerta, la voz de Mateo desde la cocina, escrita en el aire como un eco: #accion
MATEO: Es solo una amiga.
{ VALE >= 5:
    Al margen: «Me escribió después del cumple». #accion
}
-> bus

= bus
#pagina:4.5.7 #camara:valeria_bus
Amanece sobre el desierto. #accion
+ [Apoyar la cabeza en su hombro]
- Brilla en dorado. #accion #brillo_valeria
+ [Guardar]
- Última página. La portada por dentro, con el título que ella le puso: «COSAS QUE NO LE DIRÉ». #accion
#controlar:MATEO
Mateo cierra el cuaderno de Valeria con mucho cuidado y lo deja en el suelo blanco. El cuaderno se disuelve. #accion
Yo era su Abril. #voz_interior
#musica:nota_grave #camara:fundido_blanco
-> c4_amanecer

// ---------------------------------------------------------------- 4.6
=== c4_amanecer ===
#escena:4.6 #musica:vals_ciudad_blanca_noche #camara:azotea_amanecer
Mateo despierta con la mejilla sobre el cuaderno. La luz sigue prendida. Por la ventana, el Misti rosado con el primer sol. #accion
#sfx:dos_golpes_puerta
Es Abril, en pijama y con una casaca encima, con dos tazas de mate de anís y una bolsa de pan de tres puntas calientito. Tinta se cuela entre sus piernas. #accion
ABRIL: Vi tu luz prendida toda la noche. Te escribí. ¿No viste? ¡Feliz aniversario de Arequipa, limeño!
Abril le da la taza, le acomoda la frazada sobre los hombros y le saca una pelusa del pelo. #accion #sin_brillo
Nada brilla. Es cariño. Solo eso. Y es bastante. #voz_interior
{
- DECISION_C3 == "Confess":
    ABRIL: No quiero que dejes de hablarme. Pero si necesitas tiempo… lo entiendo.
- DECISION_C3 == "HangUp":
    ABRIL: Esa noche… ¿hice algo mal? Llevo semanas pensándolo.
- else:
    ABRIL: Daniel quiere conocerte en serio. Le hablo de ti todo el rato.
}
MATEO: No hiciste nada mal, Abril. Nunca.
Abril sonríe aliviada y baja. Mateo se queda con la taza caliente entre las manos. #accion
-> c4_ines

= c4_ines
#escena:4.6b #camara:libreria_manana
Mateo baja. Doña Inés está abriendo la tienda. Le sirve un té sin preguntar. #accion
DOÑA INÉS: Esta chiquilla invita hasta al cartero. Te lo dije en diciembre.
{ marcado("FraseCartero"):
    Lo dijo. Lo dijo delante de mí. #voz_interior
}
DOÑA INÉS: Mi nieta quiere como yo le enseñé: a todos, sin medida. Es un regalo, hijito. Pero hay que saber recibirlo sin pedirle que sea otra cosa.
MATEO: ¿Usted lo sabía?
DOÑA INÉS: Tengo setenta y ocho años y una librería llena de novelas. Claro que lo sabía. Pero eso tenías que verlo tú.
-> c4_daniel

= c4_daniel
#escena:4.6c #camara:libreria_vereda
Afuera, un chico de casaca larga barre la vereda de sillar, llena de serpentinas y restos de cohetes de la serenata. Es Daniel. Abril le dijo que la abuela no puede agacharse. #accion
DANIEL: Tú debes ser Mateo. Abril habla mucho de ti. Dice que eres de los buenos. #semilla:P_pago
* [«Ella también lo es. Cuídala.»]
    MATEO: Ella también lo es. Cuídala.
    DANIEL: Eso intento. Ella cuida a todo el mundo, así que alguien tiene que cuidarla a ella.
* [«Mucho gusto.» (y entrar)]
    MATEO: Mucho gusto.
    Daniel se queda con la mano extendida un segundo. Luego sigue barriendo, sin rencor. #accion
- Ni siquiera puedo odiarlo. #voz_interior
-> c4_llamada

// ---------------------------------------------------------------- 4.7
=== c4_llamada ===
#escena:4.7 #camara:azotea_tarde
#sfx:llamada_mama
CARMEN: Hijo. Voy a vender la casa de tu papá en Yanque. Nadie va desde el entierro. Antes de firmar… ¿podrías ir tú a verla? A ver si hay algo que quieras guardar.
Mateo mira la foto de su padre en el escritorio. Por la ventana, a lo lejos, los volcanes; detrás de ellos, el camino al Colca. #accion
MATEO: Voy. Pasado mañana. El diecisiete.
CARMEN: ¿Solo? ¿Allá arriba con ese frío?
MATEO: Necesito irme un tiempo, mamá. No por nada malo. Para aprender a mirar.
CARMEN: Como tu papá.
FIN DEL CAPÍTULO 4 #titulo
-> END

// ---------------------------------------------------------------- utilidades
=== function veces(n)
// «Diecisiete veces», «Doce veces»… (4.3: el número cambia según SEÑALES).
{ n:
- 1:
    ~ return "Una vez"
- 2:
    ~ return "Dos veces"
- 3:
    ~ return "Tres veces"
- 4:
    ~ return "Cuatro veces"
- 5:
    ~ return "Cinco veces"
- 6:
    ~ return "Seis veces"
- 7:
    ~ return "Siete veces"
- 8:
    ~ return "Ocho veces"
- 9:
    ~ return "Nueve veces"
- 10:
    ~ return "Diez veces"
- 11:
    ~ return "Once veces"
- 12:
    ~ return "Doce veces"
- 13:
    ~ return "Trece veces"
- 14:
    ~ return "Catorce veces"
- 15:
    ~ return "Quince veces"
- 16:
    ~ return "Dieciséis veces"
- else:
    ~ return "Diecisiete veces"
}
