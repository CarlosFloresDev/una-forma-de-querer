// =====================================================================
//  CAPÍTULO 3 — LIMA, «La duda»   (guion, escenas 3.1 a 3.11)
//  Fines de marzo a agosto (invierno limeño) · Paleta: gris garúa y ocre
//
//  Convenciones: ver Capitulo1.ink y Capitulo2.ink. Nuevo en este capítulo:
//    #lente_rota       la Lente ya no funciona (3.10): tiembla, pero no cambia nada
//    #juego:espera:<s> el «escribiendo…»: el jugador espera (suma ESPERA_TOTAL)
//    #juego:celular_pesa  minijuego del afiche: mirar el celular lo empeora
// =====================================================================
INCLUDE _variables.ink

-> c3_maleta

// ---------------------------------------------------------------- 3.1
=== c3_maleta ===
#escena:3.1 #musica:garua #camara:azotea_plano_general
Mateo empaca. La ballena de peluche está sobre la cama. Abril, sentada en el piso, dobla polos mal a propósito. #accion
ABRIL: Cinco meses en Lima. ¿Y quién me va a ayudar con la tesis?
MATEO: Es mi ciudad. Vuelvo donde mi mamá. Las prácticas son en un estudio de Barranco.
ABRIL: Allá todo es gris, ¿no? Dicen que no ven el sol en meses.
Abril sale y vuelve con un paraguas amarillo, del mismo color que su casaca. #accion
ABRIL: Toma. Para que te acuerdes de que aquí sí sale el sol. #senal:S12
* [Guardar la ballena en la maleta]
    Mateo mete la ballena entre las camisas. #accion
* [Dejar la ballena en la azotea]
    ~ marcar("BallenaEnAzotea")
    Mateo la deja sobre la almohada. #accion
- DOÑA INÉS: La casa se queda en silencio sin ti, hijito.
Tinta sigue a Mateo hasta la esquina y se sienta en la vereda de sillar a mirarlo irse. #accion
-> c3_bus

// ---------------------------------------------------------------- 3.2
=== c3_bus ===
#escena:3.2 #camara:bus_noche
Bus cama de dos pisos. Afuera, el desierto bajo las estrellas. Alguien se sienta al lado de Mateo. #accion
MATEO: ¿Vale? ¿Tú también vas a Lima?
VALERIA: Me tocó el Estudio Cobre. Aceptan a dos practicantes de la universidad.
MATEO: ¿No ibas a postular a prácticas aquí en Arequipa? Me lo dijiste en el Tintero.
VALERIA: Cambié de idea. #semilla:L_cambie_de_idea
DON AURELIO: Lima. Allá la garúa no se va; uno aprende a caminar con ella.
-> c3_carmen

// ---------------------------------------------------------------- 3.3
=== c3_carmen ===
#escena:3.3 #camara:departamento_carmen
CARMEN: ¡Estás más flaco! ¿No te dan de comer en Arequipa? ¿Tanto rocoto relleno y estás así?
Al fondo del pasillo, una puerta cerrada con un cartel antiguo: «TALLER — ANDRÉS». Mateo pasa de largo. #accion #semilla:recuerdo_papa_3
CARMEN: ¡Volviste a dibujar! «Señales». ¿Señales de qué?
MATEO: Es… de un proyecto.
CARMEN: Ya.
-> c3_estudio

// ---------------------------------------------------------------- 3.4
=== c3_estudio ===
#escena:3.4 #camara:estudio_cobre
ORTEGA: Afiche para la Feria del Libro de Lima. Dos semanas. Sorpréndanme.
ROCÍO: No le hagan caso. A Ortega solo lo sorprende el café frío. Y tú, Hortensia, no te mueras hoy, por favor.
#juego:celular_pesa
MATEO: ¿Qué tal todo por allá? 😊 #celular:enviado #juego:espera:12
ABRIL: jaja sí todo bien!! 😊 #celular:entrante
«Jaja, sí». ¿Sí a qué? Yo no le pregunté nada de «sí». #voz_interior
-> c3_septiembre

// ---------------------------------------------------------------- 3.5
=== c3_septiembre ===
#escena:3.5 #camara:barranco_puente
Cielo gris panza de burro. Mateo y Valeria cruzan cada mañana el Puente de los Suspiros. Mateo mira el celular. #accion
ABRIL: La casa está rara sin ti. La abuela pone dos tazas por costumbre 🥲 #celular:entrante #senal:S13
NICO: ¡Mira, causa! Abril me mandó alfajores de la abuela hasta Cusco. Con nota y todo: «Para mi vecino favorito». ¿A ti te llegaron? #celular:videollamada #semilla:M_alfajores_nico
MATEO: No… todavía no.
Al día siguiente llega una encomienda: una caja de alfajores con una nota. #accion
ABRIL: Para mi favorito de la azotea 🍪 ¡Que no te coma la garúa! #celular:nota #senal:S14
La mía dice «de la azotea». Es diferente. #voz_interior
-> c3_sopa

// ---------------------------------------------------------------- 3.6
=== c3_sopa ===
#escena:3.6 #camara:cuarto_mateo_lima
Mateo tiene gripe. Carmen está tres días en una feria textil en Trujillo. Tocan la puerta. #accion
VALERIA: Ortega dijo que estabas enfermo. Es caldo de gallina. Mi abuela dice que cura todo, hasta el corazón roto.
Mateo toma la sopa. Mira el celular cada pocos segundos. #accion
VALERIA: ¿Esperas un mensaje?
MATEO: No. Sí. No importa.
VALERIA: Sé lo que es esperar.
* [«Vale… gracias. De verdad. No tenías por qué.»]
    ~ notar_gesto("V5")
    Valeria sonríe y se queda diez minutos más a hablar de un libro. #accion
* [«Mm. Gracias.» (sin levantar la vista del celular)]
    Valeria asiente. #accion
- #sfx:vibracion
ABRIL: Me dijo Nico que estás malito 🤒 ¿Ya estás mejor? Cuídate mucho 💛 #celular:entrante #senal:S15
Mateo sonríe a la pantalla. Valeria recoge el recipiente vacío. #accion #camara:cuarto_mateo_general
-> c3_paraguas

// ---------------------------------------------------------------- 3.7
=== c3_paraguas ===
#escena:3.7 #camara:paradero_garua
A la salida del estudio cae una garúa tan espesa que moja de a poquitos. Mateo lleva el paraguas amarillo en el morral. #accion
#juego:paraguas_no_se_abre
No quiero que se malogre. #voz_interior
Un paraguas azul marino aparece sobre su cabeza. Es Valeria, que se moja el hombro para cubrirlo. #accion
VALERIA: En Lima nadie usa paraguas, y tú que tienes uno no lo abres. Eres rarísimo, Mateo.
* [«Te acompaño hasta tu pensión.»]
    ~ notar_gesto("V6")
    Caminan juntos por la Bajada de Baños. Valeria extraña el sol, el rocoto, el Misti. #accion
* [«Gracias, aquí ya me voy.»]
    Mateo corre bajo la garúa, cuidando el paraguas amarillo dentro del morral. #accion
- -> c3_cumple

// ---------------------------------------------------------------- 3.8
=== c3_cumple ===
#escena:3.8 #camara:departamento_carmen #semilla:O_tablon
Torta de chocolate con un «23» de velas chuecas. Carmen, Rocío, Valeria. El celular de Mateo, boca arriba junto al plato. #accion
VALERIA: Para que no vuelvas a dejar de dibujar.
Un estuche de lápices de grafito y una tarjeta dibujada a mano: el Tintero visto desde la calle, y Mateo detrás de la barra, chiquito. #accion #gesto:V7
MATEO: Qué bonito. Gracias, Vale.
Velas. «Happy birthday» a la peruana. Pasan las horas. Son las 23:48. #accion
#sfx:videollamada
ABRIL: ¡Feliz cumpleaños! ¡Perdón, perdón, casi se me pasa! ¡Te quiero mucho, sonso! #senal:S16 #lente:3.8 #semilla:B_alarma
* [Leerlo como amistad]
    ~ lente("3.8", false)
* [Leerlo como algo más]
    ~ lente("3.8", true)
    ABRIL: ¡Te quiero! #reescrita
- Mientras Mateo ríe en el balcón, Valeria deja la tarjeta bien puesta sobre los lápices y se despide de Carmen en voz baja. #accion #camara:mesa_vacia
La llamada dura cuatro minutos. Cuando Mateo vuelve, Valeria ya no está. #accion
* [Escribirle: «Perdón por desaparecer. Gracias por los lápices, de verdad. Los voy a usar»]
    ~ notar_gesto("V7")
    VALERIA: Feliz cumple, Mateo 🙂 #celular:entrante
* [No escribir]
    El celular queda boca abajo. #accion
- -> c3_ramon

// ---------------------------------------------------------------- 3.9
=== c3_ramon ===
#escena:3.9 #camara:cocina_carmen
CARMEN: La de los lentes se fue llorando. Calladita, pero llorando.
MATEO: ¿Valeria? No… es solo una amiga. #eco_valeria
CARMEN: ¿Te conté alguna vez lo de Ramón?
MATEO: ¿Quién es Ramón?
CARMEN: Antes de tu papá. Yo tenía diecinueve años. Ramón me acompañaba a mi casa, me guardaba sitio en el cine Colón, me decía «mi favorita»… a mí, a sus tres hermanas y a la señora de la panadería.
CARMEN: Lo quise tres años callada. Guardaba todo: un boleto de micro, una servilleta…
CARMEN: Un día entendí que estaba enamorada de un muchacho que yo misma había inventado con pedacitos de él. Ramón nunca me engañó. Era así nomás.
CARMEN: Y luego, en un matrimonio, un carpintero torpe que acababa de llegar del Colca me pisó bailando un huayno…
MATEO: Papá.
CARMEN: Tu papá. Me demoré dos años en mirarlo de verdad. Él me esperó sin pedirme nada.
{ ESPERA_TOTAL > 600:
    CARMEN: Te pasas el día mirando ese celular, hijo. Lo que esperas no está ahí adentro.
}
Pedacitos de él. #voz_interior
-> c3_llamada

// ---------------------------------------------------------------- 3.10
=== c3_llamada ===
#escena:3.10 #camara:cuarto_mateo_lima
28 de julio. Banderas rojiblancas en los balcones y garúa. Mateo dibuja con los lápices de Valeria. #accion
ABRIL: ¡¡Te tengo que contar algo increíble!!! ¿Te llamo esta noche? 🙈 #celular:entrante #senal:S17
Mateo se peina frente al espejo para una llamada de voz. Se ríe de sí mismo. #accion
ABRIL: ¿Te acuerdas del chico del muelle de Mollendo? ¿Daniel? ¡Está en Arequipa, en el Conservatorio! Toca los jueves en el Tintero y… Mateo… estamos saliendo. #musica:corte_seco
ABRIL: Eres la primera persona a la que se lo cuento. Bueno, después de Sofi y de la abuela. Porque eres mi mejor amigo. #lente:3.10 #lente_rota
* [Leerlo como amistad]
    -> rota
* [Leerlo como algo más]
    La frase tiembla… y se queda igual. #accion #lente_rota
    -> rota
= rota
ABRIL: ¿Mateo? ¿Sigues ahí?
* [CONFESAR: «Abril… yo siento algo por ti. Desde hace tiempo.»]
    ~ decidir_c3("Confess")
    -> confesar
* [CALLAR: «Me alegro mucho por ti. De verdad.»]
    ~ decidir_c3("StaySilent")
    -> callar
* [COLGAR: «Me están llamando del estudio. Te hablo luego.»]
    ~ decidir_c3("HangUp")
    -> colgar

= confesar
MATEO: Abril… yo siento algo por ti. Desde hace tiempo. Desde la biblioteca, creo. O antes.
Un silencio larguísimo. Se oye a Abril respirar. #accion
ABRIL: Mateo… yo no… no lo sabía. Perdóname si te hice creer…
MATEO: No. No me hiciste creer nada. Lo creí yo solito.
ABRIL: Te quiero muchísimo. Pero no así. Perdóname.
MATEO: No tienes que pedir perdón.
-> balcon

= callar
MATEO: Me alegro mucho por ti. De verdad.
ABRIL: ¿Sí? ¡Ay, sabía que te ibas a alegrar! Te va a caer chévere, ya vas a ver. Es de los buenos, como tú.
Hablan diez minutos más. Ella habla y él dice «qué bien» cuatro veces. Al colgar, parte sin querer uno de los lápices de Valeria. #accion
-> balcon

= colgar
MATEO: Me están llamando del estudio. Te hablo luego.
ABRIL: ¿A las once de la noche? ¿Y en feriado? ¿Estás bien?
ABRIL: ¿Hice algo mal? 😟 #celular:entrante
Mateo pone el celular boca abajo. No lo abre. #accion
-> balcon

= balcon
Mateo sale al balcón bajo la garúa, con el paraguas amarillo en la mano. #accion #camara:balcon_garua
* [Abrir el paraguas]
    Por primera vez, el amarillo se abre contra el gris de Lima. #accion
* [Dejar que la garúa lo moje]
    Mateo se moja. #accion
- #juego:corazon_agrietado
-> c3_vuelta

// ---------------------------------------------------------------- 3.11
=== c3_vuelta ===
#escena:3.11 #camara:bus_espejo
Fin de las prácticas. La garúa queda atrás y, al amanecer, aparecen el desierto y el cielo azul de Arequipa. #accion
Valeria se queda dormida. Poco a poco, su cabeza cae sobre el hombro de Mateo. #accion #juego:no_moverse:20
DON AURELIO: Curioso. El bus lleva a todos al mismo sitio… pero cada uno viaja otra cosa.
FIN DEL CAPÍTULO 3 #titulo
-> END
