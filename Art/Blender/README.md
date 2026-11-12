# Kit del cuarto de la azotea de Mateo

17 props para diorama pictórico en Arequipa. Mallas en metros, proporciones ligeramente redondeadas, materiales planos de la paleta del proyecto y ningún mapa fotográfico. Requiere Blender 5.2.3 y Python con Pillow para la hoja de revisión (Pillow ya disponible en este entorno).

Desde la raíz del proyecto, regenerar fuentes, FBX, miniaturas y verificaciones con un comando de PowerShell:

```powershell
python ./Art/Blender/scripts/regenerate.py --blender 'D:\Blender\blender.exe' --temp-root 'D:\DevTools\Temp' --output .
```

Los scripts Python no contienen rutas absolutas. `--output` recibe la raíz del proyecto. `scripts/regenerate.py` configura TEMP/TMP con el argumento recibido y comprueba el código de salida de cada paso. No ejecuta Unity ni modifica scripts del juego, pruebas, narrativa o configuración. No hace commits.

- `scripts/generate_azotea.py`: primitivas parametrizadas, uniones booleanas exactas para quitar caras internas, limpieza, normales hacia fuera, triangulación explícita, validación de mallas cerradas y exportación individual.
- `scripts/render_previews.py`: render Eevee de todos los `.blend`, cámara ortográfica ajustada a cada silueta, tres luces de área, fondo neutro y PNG RGBA de 512 × 512. `--generate` ejecuta primero el generador.
- `scripts/verify_exports.py`: reimporta los 17 FBX y contrasta tris, dimensiones, materiales, escala, pivote, aristas manifold y dimensiones de los PNG.
- `scripts/contact_sheet.py`: reúne las miniaturas con sus nombres; usa Pillow.
- `Azotea/SM_*.blend`: fuentes con materiales y geometría final. Cambiar parámetros del generador para conservar cambios al regenerar.
- `Azotea/previews/SM_*.png`: una miniatura por modelo; `contact_sheet.png` reúne las 17.
- `Azotea/geometry_report.json`: tris, dimensiones, materiales y rutas por prop.
- `Azotea/verification_report.json`: resultados de la reimportación de cada FBX.
- `../../Game/Assets/_Project/Art/Props/Azotea/SM_*.fbx`: exportaciones para Unity (ruta relativa a la raíz: `Game/Assets/_Project/Art/Props/Azotea/`).

La lista exacta de modelos y tris está en `Azotea/INVENTARIO.md`.

## Encaje y ejes

Blender usa Z vertical. Los FBX usan `axis_forward='-Z'`, `axis_up='Y'`, `apply_unit_scale=True` y `apply_scale_options='FBX_SCALE_ALL'`. La reimportación devuelve las dimensiones originales en metros. Objetos con escala 1 y origen en el centro X/Y de la caja, en su mínimo Z; todos los objetos de vidrio comparten ese origen. En Unity corresponde a base centrada y Y vertical. El frente de muebles verticales mira hacia -Y en Blender.

Los tres muros miden 2 × 0,18 × 2,6 m; se colocan cada 2 m. Se dejan bordes rectos en los módulos estructurales para mantener las juntas exactas. Vano de ventana: 1,20 m de ancho, desde 1,00 hasta 2,20 m. Colocar `SM_Ventana` a 1,00 m sobre la base del muro; su peana sobresale lateralmente 4 cm y hacia la fachada. Vano de puerta: 1,02 × 2,10 m desde la base; `SM_Puerta` usa la misma altura y anchura. Sus componentes visuales están consolidados: la hoja de la puerta y el cajón no están preparados como piezas animables independientes.

Piso: 2 × 2 m, espesor 0,12 m con pivote debajo. Apoyar los muros sobre la cota superior del piso. Techo: huella exacta 2 × 2 m, chapa cerrada de 6 mm, 20 ondas de 10 cm, relieve de 36 mm y pendiente del 10% hacia +Y de Blender. Colocar módulos contiguos a lo ancho; para continuar en el sentido de la pendiente, subir 0,20 m por módulo. El apoyo superior del cuarto y el cierre de los extremos de la pendiente se resuelven en el montaje de la escena.

## Materiales e integración

`M_Sillar #EDE6DA`, `M_Madera #8A5A3C`, `M_Tela #6F7C8C`, `M_Lana #B5523B`, `M_Metal #3B3A3F`, `M_Papel #F3EAD3`, `M_Dorado #D9A441`; `M_Vidrio #A7C8CD` es el tinte elegido para vidrio. Los colores hex se convierten de sRGB a lineal en Blender. El vidrio de la ventana es geometría separada; también la pantalla del celular usa vidrio. Los FBX no incluyen cámaras, luces o suelo de las miniaturas.

En Unity URP, remapear los slots `M_` a los materiales toon/pintados del juego. Configurar `M_Vidrio` con el shader de vidrio/transparencia que use el proyecto: la transmisión de Principled no se convierte automáticamente a un shader URP. La comprobación realizada es geométrica y visual en Blender; no se ha abierto Unity ni se han creado materiales `.mat`.

## Revisión realizada

Se inspeccionaron visualmente las 17 miniaturas, incluida la hoja conjunta: siluetas completas, sin recortes, vanos abiertos y paleta coherente. La unión del dintel de la puerta produjo inicialmente dos aristas compartidas por cuatro caras; se corrigió el solape y la validación final exige cero aristas no manifold. Se mejoró el celular con esquinas redondeadas en planta independientes de su espesor.

En este entorno restringido Blender avisa que no puede crear su subdirectorio temporal en `D:\DevTools\Temp` y usa esa misma carpeta como alternativa; TEMP/TMP permanecen en D:. Se desactivaron las miniaturas internas automáticas de `.blend` porque su caché no era escribible. Los PNG solicitados se renderizan explícitamente en `Azotea/previews/`. También puede aparecer un aviso de deprecación de `use_nodes` para Blender 6; en 5.2.3 funciona. No hay errores geométricos pendientes si `verification_report.json` contiene 17 resultados correctos.

