# Una forma de querer

Juego narrativo en 3D ambientado en **Arequipa, Perú**, sobre el miedo más callado de todos: confundir cariño con amor. La historia recorre Arequipa, Mollendo, Lima y el Valle del Colca. Lo desarrolla **Feka Studios**. Estado: **en desarrollo**.

Página del juego: https://feka-studios.chefsitturchey.workers.dev/una-forma-de-querer

## Tecnología

- **Unity 6.3 LTS** con URP y C# 9.
- **Clean Architecture** con inyección de dependencias (VContainer). Cada capa es un ensamblado (`.asmdef`), así que Unity no compila si una capa interna usa una externa.
- Guion escrito en **Ink**, con una herramienta propia (InkCheck) que valida los guiones sin abrir Unity.
- Pruebas NUnit de las reglas del guion (finales, variables y señales).

| Capa | Contenido |
|---|---|
| Core | Estado narrativo, catálogo de escenas y resolución de finales (sin Unity) |
| UseCases | Servicios narrativos, eventos y puerto de guardado |
| Infrastructure | Guardado en JSON y puente Ink ↔ dominio |
| Presentation | Interacción, UI, cámara y efectos |
| Bootstrap | Composición de dependencias |

Más detalle en [`Docs/ARQUITECTURA.md`](Docs/ARQUITECTURA.md).

## Abrir el proyecto

1. Instala Unity **6000.3.28f1** y Git LFS.
2. Clona el repositorio (`git lfs pull` para los recursos).
3. Abre la carpeta `Game` desde Unity Hub.
