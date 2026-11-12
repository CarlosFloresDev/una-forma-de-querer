// =====================================================================
//  CAPÍTULO 2 — MOLLENDO, «La ilusión»   (guion, escenas 2.1 a 2.9)
//  Febrero (verano y carnavales) · Paleta: turquesa y naranja
//
//  Convenciones: ver Capitulo1.ink. Nuevo en este capítulo:
//    #lente:<momento>   la frase de Abril TIEMBLA (doble lectura). Justo después
//                       viene la elección «amistad / algo más» → lente(momento, bool)
//    #reescrita         la versión «algo más» de la frase (letra más cálida)
//    #solo_boton:<txt>  la elección solo tiene ese botón (p. ej. «No pensar en eso»)
// =====================================================================
INCLUDE _variables.ink

-> c2_bus

// ---------------------------------------------------------------- 2.1
=== c2_bus ===
#escena:2.1 #musica:brisa_de_mollendo #camara:bus_ventana
Un bus soleado baja de la sierra al desierto y luego al mar. Sofi duerme con la boca abierta. Nico lee una novela; su separador tiene dibujada una guitarra. #accion #semilla:G_separador_nico
DON AURELIO: Mollendo. Allá el verano dura más de lo que debería.
Abril, junto a la ventana, se queda dormida poco a poco. Su cabeza cae sobre el hombro de Mateo. #accion #senal:S08
#sfx:vibracion
VALERIA: ¿Ya llegaron? Pásenla bonito 🙂 Aquí el Tintero está vacío sin ti… digo, sin ustedes jaja #celular:entrante
* [«¡Gracias, Vale! Te traigo una conchita 🐚»]
    ~ notar_gesto("V4")
    MATEO: ¡Gracias, Vale! Te traigo una conchita 🐚 #celular:enviado
    VALERIA: trato hecho 🐚🐚🐚 #celular:entrante
    ~ promesa_conchita = true // TODO Cap. 4: pasar al dominio para que 4.1 sepa si hay conchita
* [Dejar en visto]
    El chat se queda con el doble check azul. #accion
- -> c2_casa

// ---------------------------------------------------------------- 2.2
=== c2_casa ===
#escena:2.2 #camara:casa_benavides
Casa blanca con ventanas azules, buganvillas y un perro dormido en la puerta. #accion
MARTA: ¡Abril, hijita! ¿Y estos dos?
ABRIL: A Nico ya lo conoces. Y este es Mateo. Es de los buenos. #semilla:P_de_los_buenos
#juego:explorar_mollendo #semilla:recuerdo_papa_2
-> c2_playa

// ---------------------------------------------------------------- 2.3
=== c2_playa ===
#escena:2.3 #camara:primera_playa
#juego:voley_playa
ABRIL: ¡Foto! Ponte serio… no, mejor ríete.
ABRIL: Esta es mi foto favorita del viaje. #senal:S09
Al fondo, Abril se toma otra foto con Sofi y otra con Nico. Su risa suena igual. #accion #semilla:Q_fotos_favoritas
{ promesa_conchita:
    En la orilla brilla una conchita rosada. Mateo la guarda en el bolsillo para Valeria. #accion #juego:recoger_conchita
}
-> c2_nico

// ---------------------------------------------------------------- 2.4
=== c2_nico ===
#escena:2.4 #camara:playa_atardecer
Abril y Sofi corren hacia el agua. Nico se sienta junto a Mateo con dos botellas de Kola Escocesa. #accion
NICO: Oe.
MATEO: ¿Qué?
NICO: Ten cuidado, ¿ya?
MATEO: ¿Con qué?
NICO: Con Abril. La conozco desde que teníamos seis años. Ella quiere con todo… pero a todos. A la señora del quiosco, a los perros de la calle, a mí. Así es ella, pues.
* [«¿Y tú qué sabes de lo que yo siento?»]
    ~ respuesta_nico("Anger")
    NICO: Ya, ya. Solo te lo digo porque me caes bien.
* [«No sé de qué me hablas.»]
    ~ respuesta_nico("Denial")
    NICO: Claro. Yo tampoco.
* [«…Ya lo sé.»]
    ~ respuesta_nico("Accept")
    NICO: Entonces listo. Ya lo sabes.
- Abril los llama desde el agua. Mateo corre hacia ella. Nico se queda sentado, solo, con la botella. #accion #camara:nico_solo
-> c2_feria

// ---------------------------------------------------------------- 2.5
=== c2_feria ===
#escena:2.5 #musica:feria_carnaval #camara:feria
Luces de colores, olor a canchita y algodón de azúcar. #accion
#juego:tiro_al_blanco_abril
Abril gana tres peluches: un pulpo, una ballena y una estrella de mar. #accion
ABRIL: Para que te acuerdes de este día. #senal:S10
Al borde de la imagen, Abril le da la estrella a Sofi y se agacha ante un niño que llora. #accion
LALO: ¿Para mí?
ABRIL: Para que te acuerdes de este día. #semilla:I_misma_frase_lalo
-> c2_rueda

= c2_rueda
#escena:2.5b #camara:rueda_chicago
La canastilla de la rueda sube. El pueblo se ve pequeño y el mar, negro y brillante. #accion
ABRIL: Contigo todo es fácil, ¿sabes? Hablar, estar callada… #lente:2.5b
* [Leerlo como amistad]
    ~ lente("2.5b", false)
* [Leerlo como algo más]
    ~ lente("2.5b", true)
    ABRIL: Contigo TODO es fácil, ¿sabes? 💛💛 #reescrita
- ABRIL: Mañana subimos al castillo. Dicen que de noche se ve todo el mar.
-> c2_castillo

// ---------------------------------------------------------------- 2.6
=== c2_castillo ===
#escena:2.6 #camara:castillo_forga
El Castillo Forga, cerrado y con andamios de restauración. Suben por las rocas hasta el muro, sobre el mar. #accion
#juego:subir_rocas
Viento salado. Se sientan en el muro con las piernas colgando hacia el mar. #accion
ABRIL: ¿Por qué dejaste de dibujar? El primer día en clase dijiste que hacía mucho que no dibujabas.
MATEO: Mi papá era carpintero. Era del Colca, de Yanque. Pero dibujaba. Todo el rato, a todo el mundo. Murió hace dos años, en su taller de Lima. Infarto. #musica:tema_de_andres
MATEO: La última vez que hablamos le dije que dibujar no servía para nada. Desde entonces no podía. Agarraba el lápiz y…
Abril le toma la mano. La aprieta. No dice nada durante un rato. #accion #senal:S11
ABRIL: Me alegra tanto que estés aquí. #lente:2.6
* [Leerlo como amistad]
    ~ lente("2.6", false)
* [Leerlo como algo más]
    ~ lente("2.6", true)
    ABRIL: Me alegra tanto que estés conmigo. #reescrita
- ABRIL: Mi mamá se fue a trabajar a Italia cuando yo tenía ocho años. Me crio mi abuela.
ABRIL: ¿Sabes qué me dijo siempre? Que el cariño que no se da se malogra. Así que yo lo doy todo. A todo el mundo. No sé hacerlo de otra manera. #fugaz #semilla:J_lo_doy_todo
Me lo da todo. A mí. #voz_interior
-> c2_fogata

// ---------------------------------------------------------------- 2.7
=== c2_fogata ===
#escena:2.7 #camara:fogata
Mantas, choclos asados y la guitarra de Nico. #accion
SOFI: ¿Y por qué nunca hablas de tus papás?
NICO: Porque se divorciaron por WhatsApp. Literal. Con un audio.
Silencio incómodo. Nico se ríe, pero la risa se rompe. Abril se sienta a su lado y lo abraza fuerte, con los ojos cerrados. #accion
ABRIL: Me alegra tanto que estés aquí, sonso. #semilla:K_misma_frase_nico
Son como hermanos. #voz_interior
-> c2_muelle

// ---------------------------------------------------------------- 2.8
=== c2_muelle ===
#escena:2.8 #musica:vals_del_muelle #camara:muelle_lejos
Al final del viejo muelle, bajo un farol, un muchacho toca la guitarra. Abril está sentada frente a él, escuchando, con la barbilla en las rodillas. #accion
Mateo, que venía con dos raspadillas, se queda a veinte metros. #accion
DANIEL: En marzo me voy a Arequipa. Me aceptaron en el Conservatorio, el Luis Duncker.
ABRIL: ¡Yo vivo en Arequipa! Encima de una librería, en San Lázaro.
DANIEL: Entonces ya conozco a alguien allá. #semilla:R_conservatorio
+ [No pensar en eso] #solo_boton:No pensar en eso
- Mateo se da la vuelta y se come las dos raspadillas en la orilla, solo. #accion
-> c2_vuelta

// ---------------------------------------------------------------- 2.9
=== c2_vuelta ===
#escena:2.9 #camara:bus_vuelta
Abril enseña fotos del viaje y todos se ríen. Mateo dibuja la ballena de peluche en su cuaderno. #accion
DON AURELIO: ¿Y qué tal el verano, jóvenes?
SOFI: ¡Demasiado corto!
DON AURELIO: Siempre es así. Por eso se recuerda.
{
- SENALES >= 9:
    Ella sabía que yo estaba ahí, en el muelle. Me vio. ¿Me vio? #voz_interior
- SENALES < 4:
    Fue un buen verano. Solo eso. #voz_interior
- else:
    Ella sabía que yo estaba ahí, en el muelle. ¿Me vio? #voz_interior
}
FIN DEL CAPÍTULO 2 #titulo
-> END
