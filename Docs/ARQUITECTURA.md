# Arquitectura de «Una forma de querer»

Unity 6.3 LTS (6000.3.28f1) · URP · C# 9 · Clean Architecture con inyección de dependencias (VContainer).

## La regla principal

Las dependencias solo apuntan **hacia dentro**. El guion (las reglas de la historia) no sabe que existe Unity.

```
 Presentation ─┐
 Infrastructure┼──► UseCases ──► Core
 Bootstrap ────┘   (casos de uso)  (dominio: reglas del guion)
```

Cada capa es un ensamblado (`.asmdef`). Unity **no compila** si una capa interna intenta usar una externa, así que la regla no depende de la disciplina.

| Capa | Carpeta | Puede usar | Qué contiene |
|---|---|---|---|
| **Core** | `Scripts/Core` | nada (ni UnityEngine) | `NarrativeState` (variables de la tabla 4.6), `NarrativeCatalog` (ids S01–S17, V1–V7…), `EndingResolver` (Anexo A) |
| **UseCases** | `Scripts/UseCases` | Core | `NarrativeService` (casos de uso), `IEventBus` y eventos, `INarrativeRepository` (puerto de guardado), `NarrativeSnapshot` |
| **Infrastructure** | `Scripts/Infrastructure` | Core, UseCases, Ink | `JsonNarrativeRepository` (guardado atómico), `InkNarrativeBridge` (Ink ↔ dominio) |
| **Presentation** | `Scripts/Presentation` | Core, UseCases, VContainer | MonoBehaviours: `SignalInteractable`, `ValeriaGestureObservable`, UI, cámara, VFX |
| **Bootstrap** | `Scripts/Bootstrap` | todo | `GameLifetimeScope`: el único sitio que conecta implementaciones |
| **Editor** | `Scripts/Editor` | Core, UseCases | Herramientas del editor |
| **Tests** | `Tests/EditMode` | Core, UseCases | Pruebas NUnit de las reglas del guion |

## Flujo de una señal dorada

1. El jugador pulsa «Guardar» sobre un gesto de Abril → `SignalInteractable.Save()`.
2. → `NarrativeService.SaveSignal("S04")` valida el id con el dominio, guarda la partida y publica `SignalSaved`.
3. → La UI del Medidor de Ilusión, el audio y el VFX escuchan `SignalSaved` en el `IEventBus`. Nadie llama a nadie directamente.

Ink funciona igual: un `.ink` llama `~ guardar_senal("S04")`, que pasa por el mismo `NarrativeService`. **Ink nunca guarda estado propio**; lee las variables que sincroniza `InkNarrativeBridge`.

## Narrativa (Ink)

- `Assets/_Project/Narrative/Ink/_variables.ink`: variables y funciones `EXTERNAL`, con respaldos para probar en Inky.
- Un archivo `.ink` por capítulo (`Prologo.ink`, `Capitulo1.ink`…), con el mismo número de escena que el guion en los comentarios.
- Etiquetas (`#celular:entrante`, `#camara:…`, `#musica:…`, `#voz_interior`) que interpreta la presentación.
- El guion completo está en `Docs/Guion/`.

## Reglas para añadir código

1. ¿Es una regla de la historia (qué cuenta, qué final sale)? → **Core**, con prueba en `Tests/EditMode`.
2. ¿Es una acción del jugador que cambia el estado? → método en **NarrativeService**, que guarda y publica un evento.
3. ¿Habla con disco, red, Ink o la API de Unity? → **Infrastructure**, detrás de una interfaz definida en UseCases.
4. ¿Se ve o se oye? → **Presentation**. Escucha eventos; no decide reglas.
5. Registra la implementación en `GameLifetimeScope`.
6. Los ids de señales, gestos y recuerdos salen **solo** de `NarrativeCatalog`.

## Pruebas

- En Unity: **Window > General > Test Runner > EditMode > Run All**.
- Sin abrir Unity (Core y UseCases son C# puro): se pueden compilar con .NET usando C# 9, igual que Unity.

## Paquetes

| Paquete | Para qué |
|---|---|
| URP | Render estilizado (pintura, profundidad de campo, luz cálida) |
| Cinemachine | Cámaras cinematográficas semilibres |
| Input System | Teclado, mando |
| Timeline | Escenas cinemáticas |
| ProBuilder | Bocetos de niveles (blockout) antes del arte final |
| Splines | Recorridos de cámara y del bus |
| Localization | Textos en español (y futuros idiomas) |
| Ink (inkle) 2.0.0 | Diálogos ramificados |
| VContainer 1.19.0 | Inyección de dependencias |
| UniTask 2.5.11 | Async sin basura para transiciones y esperas |
| MCP for Unity 10.3.0 | Permite controlar el editor de forma automatizada |
