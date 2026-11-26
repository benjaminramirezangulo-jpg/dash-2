# dash-2 · DASH / FIGHT

**Encuentra la línea. Supera tu límite.**

Un juego de acción cenital para un jugador, construido alrededor de la velocidad, los dashes y la precisión. Seis pruebas compactas te obligan a leer el escenario, devolver proyectiles y encadenar movimientos sin perder el ritmo.

**Windows de 64 bits · Teclado y ratón · Unity 6000.6.7f1 · v0.3.0 / Third Cut · Pre-release**

[**Descargar y jugar**](https://github.com/benjaminramirezangulo-jpg/dash-2/releases/download/v0.3.0/dash-2-Windows.zip) · [Ver la versión publicada](https://github.com/benjaminramirezangulo-jpg/dash-2/releases/tag/v0.3.0) · [Reportar un problema](https://github.com/benjaminramirezangulo-jpg/dash-2/issues)

![Pantalla de inicio real de dash-2: tipografía blanca, fondo negro y personaje con bufanda.](Docs/media/dash-2-title.png)

## Contenido

- [La propuesta](#la-propuesta)
- [Quién está detrás](#quién-está-detrás)
- [Origen y motivación](#origen-y-motivación)
- [Qué queremos construir](#qué-queremos-construir)
- [Video, audio y material visual](#video-audio-y-material-visual)
- [Descarga y primeros pasos](#descarga-y-primeros-pasos)
- [Mecánicas y flow](#mecánicas-y-flow)
- [Las seis pruebas](#las-seis-pruebas)
- [Controles y opciones](#controles-y-opciones)
- [Estado, pruebas y próximos pasos](#estado-pruebas-y-próximos-pasos)
- [Plan, calendario y presupuesto](#plan-calendario-y-presupuesto)
- [Preguntas frecuentes](#preguntas-frecuentes)
- [Desarrollo y estructura](#desarrollo-y-estructura)
- [Créditos y referencias](#créditos-y-referencias)

## La propuesta

Una bufanda blanca atraviesa una sala negra. Las rutas son breves; el reto está en ejecutarlas con precisión. Moverte, disparar y usar el dash forman una misma secuencia: un corte bien colocado puede eliminar una amenaza, recuperar una carga y abrir la siguiente línea de avance.

El proyecto prioriza tres aspectos:

| Pilar | Cómo se refleja en el juego |
| --- | --- |
| **Velocidad** | Movimiento base de 20 unidades por segundo, dashes a 64 y reinicio inmediato del intento. |
| **Control** | Dirección del dash ajustable, movimiento de precisión, parry con ventana corta y colisiones que respetan las paredes. |
| **Lectura** | Blanco, negro y grises; siluetas distintas, señales de ataque y una cámara que acompaña al personaje. |

Está pensado para quien disfruta aprender una ruta, fallar, reconocer el error y volver a intentarlo. La campaña se compone de pruebas cortas que se pueden repetir para mejorar tiempos, rangos y dominio de las mecánicas.

## Quién está detrás

Esta iteración se publica y mantiene en [benjaminramirezangulo-jpg/dash-2](https://github.com/benjaminramirezangulo-jpg/dash-2). La presentación utiliza los responsables y el trabajo que se pueden identificar en el repositorio:

| Participación | Responsable o procedencia |
| --- | --- |
| Publicación y responsabilidad de esta edición | Cuenta [benjaminramirezangulo-jpg](https://github.com/benjaminramirezangulo-jpg). |
| Prototipo educativo de origen | [hiwhatsup67/dash](https://github.com/hiwhatsup67/dash), acreditado por separado. |
| Asistencia en esta iteración | Codex para programación, comprobaciones automatizadas y documentación. |
| Evaluación de control, balance y sonido | Próxima ronda de sesiones humanas propuesta en el calendario. |

### Trabajo que puedes revisar

La trayectoria demostrable de esta presentación es el propio desarrollo publicado: el [prototipo inicial](https://github.com/hiwhatsup67/dash), la [demo Third Cut](https://github.com/benjaminramirezangulo-jpg/dash-2/releases/tag/v0.3.0), el código de seis pruebas y sus [resultados de verificación](TestResults/runtime/runtime-tests.xml). Son ejemplos concretos para valorar el proyecto; la autoría del prototipo original conserva su crédito.

## Origen y motivación

El punto de partida fue **Dash and Fight**, un prototipo educativo para TILE22 de INACAP con una arena, movimiento, disparos y enemigos. `dash-2` conserva ese proyecto de Unity y su reproductor de Windows, y amplía la propuesta hacia pruebas de velocidad, precisión y repetición.

### Por qué nos importa

La motivación de diseño es que el movimiento resulte satisfactorio por sí mismo: reconocer una abertura, ajustar el giro y enlazar un dash con el siguiente. Queremos que una derrota ayude a entender la ruta y que un reintento permita aplicar lo aprendido enseguida.

La velocidad y el flow fueron la prioridad desde el inicio de esta iteración. El blanco y negro, la geometría sencilla y los trazos de movimiento apoyan esa intención. El trabajo pendiente se concentra en comprobar cómo se siente esa combinación en manos de personas y en pulir lo que todavía interrumpe el ritmo.

### Lo recorrido hasta ahora

| Fecha | Hito | Qué se puede comprobar |
| --- | --- | --- |
| 24 oct 2026 | Prototipo educativo jugable. | [Entrega del proyecto de origen](https://github.com/hiwhatsup67/dash/commit/93a5e959f1930f2b04763d561918e65b84f3ace0). |
| 19 nov 2026 | Publicación de Third Cut / v0.3.0. | Seis pruebas, demo de Windows, grabaciones y resultados de 95 comprobaciones de modelo y 115 de integración. |
| 25 nov 2026 | Presentación detallada en español. | README, capturas y GIF de Crosscut. |
| 26 nov 2026 | Historia, planificación y material de presentación ampliados. | Video con subtítulos en dos idiomas, muestra sonora, gráfico y presupuesto propuesto. |

## Qué queremos construir

La intención es convertir las seis pruebas actuales en un conjunto de retos que recompense aprender, ejecutar y mejorar. La siguiente etapa propuesta se centra en esas rutas, el control, el balance y la distribución de Windows.

| Experiencia buscada | Lo disponible | Siguiente comprobación |
| --- | --- | --- |
| Moverse con precisión a gran velocidad. | Dash dirigido, movimiento de precisión y reinicio inmediato. | Observar giros, lectura y respuesta con jugadores humanos. |
| Encontrar distintas formas de resolver una ruta. | Puertas direccionales, escudos, láseres, persecución y núcleo final. | Revisar dificultad y rango S, especialmente en Redline. |
| Aprender del intento anterior. | Práctica, tiempos parciales, ghosts y grabaciones. | Recoger ejemplos de errores claros y situaciones confusas. |
| Mantener un ritmo visual y sonoro legible. | Estilo monocromo, señales de ataque y mezcla sintetizada. | Investigar fotogramas largos y escuchar la mezcla en distintos dispositivos. |

El criterio para una próxima candidata incluye rutas completas verificables, problemas de progreso y guardado resueltos, y un informe de sesiones humanas. El calendario y los recursos para intentar ese trabajo se detallan más abajo como propuesta.

## Video, audio y material visual

### Presentación breve, con subtítulos

[**Ver video en español · 32 segundos**](https://raw.githubusercontent.com/benjaminramirezangulo-jpg/dash-2/main/Docs/media/dash-2-presentacion-es.mp4) · [**Watch in English · 32 seconds**](https://raw.githubusercontent.com/benjaminramirezangulo-jpg/dash-2/main/Docs/media/dash-2-presentation-en.mp4)

El video presenta el concepto, muestra Crosscut y Terminal Velocity, y termina con una invitación a probar la demo. Utiliza capturas reales y cartelas, sin necesidad de una presentación frente a cámara. Los subtítulos están integrados en la imagen; hay una edición en español y otra en inglés.

| Material | Formato y tamaño aproximado | Uso |
| --- | --- | --- |
| [Video ES](Docs/media/dash-2-presentacion-es.mp4) / [Video EN](Docs/media/dash-2-presentation-en.mp4) | MP4, 32 s, 1,43 MB por edición. | Mostrar la idea y el juego en movimiento. |
| [Subtítulos ES](Docs/media/dash-2-presentacion.es.srt) / [English subtitles](Docs/media/dash-2-presentation.en.srt) | SRT, menos de 1 KB cada uno. | Leer o reutilizar las leyendas fuera del video. |
| [Escuchar la partitura sintetizada](https://raw.githubusercontent.com/benjaminramirezangulo-jpg/dash-2/main/Docs/media/dash-2-musica.mp3) | MP3, 10 s, 0,12 MB. | Conocer una muestra musical del juego. |
| [GIF de Crosscut](Docs/media/dash-2-crosscut.gif) | GIF, 4,4 s, 4,87 MB. | Vista rápida dentro del README. |
| [Gráfico de rendimiento](Docs/media/dash-2-frame-times.png) | PNG, menos de 0,1 MB. | Consultar las mediciones, incluido el fotograma largo. |

Las imágenes, videos, sonido y GIF de esta presentación pesan individualmente **menos de 5 MB**. Los enlaces MP4 y MP3 abren el archivo o permiten descargarlo según el navegador. La procedencia, edición y límites del material están en [MEDIA.md](Docs/media/MEDIA.md).

**English overview.** `dash-2` is a monochrome top-down action game focused on speed, precision and repeatable short trials. A Windows development build is available now. The video uses ordinary-input automated replays; the proposed schedule and budget are planning estimates. An English-captioned video and a separate SRT file are linked above.

### El juego en movimiento

![Reproducción de Crosscut mostrando desplazamientos rápidos y cruces de puertas mediante dash.](Docs/media/dash-2-crosscut.gif)

*Secuencia real de Third Cut a velocidad normal, reproducida desde una grabación de entradas de Crosscut. La controla el verificador automático con movimientos, disparos y dashes normales; no modifica salud ni posiciones. El GIF se presenta a 15 fps y sin sonido; no representa la frecuencia de simulación del juego ni una partida humana.*

La versión actual se llama **Third Cut**. El repositorio es `dash-2`; el ejecutable conserva el nombre `DashAndFight.exe`.

## Descarga y primeros pasos

### Jugar en otro PC

1. Descarga [**dash-2-Windows.zip**](https://github.com/benjaminramirezangulo-jpg/dash-2/releases/download/v0.3.0/dash-2-Windows.zip).
2. Extrae **todo** el ZIP en una carpeta.
3. Entra en `dash-2-Windows/` y ejecuta **`DashAndFight.exe`**.
4. Consulta **How to Flow** en el menú de inicio y comienza con **Slipstream**.

Mantén junto al ejecutable las carpetas `DashAndFight_Data`, `MonoBleedingEdge` y los demás archivos distribuidos. **Para jugar no necesitas instalar Unity, compilar el proyecto ni iniciar sesión en un Editor.**

La distribución publicada es para **Windows de 64 bits**. Utiliza teclado y ratón, y su interfaz está en inglés. No se han establecido requisitos mínimos de CPU, GPU o memoria mediante pruebas en distintos equipos; las mediciones de rendimiento disponibles corresponden al equipo de desarrollo.

### Obtener el proyecto completo

Puedes usar **Code > Download ZIP** en este repositorio o descargar [dash-2-project.zip](https://github.com/benjaminramirezangulo-jpg/dash-2/releases/download/v0.3.0/dash-2-project.zip). Después de extraerlo, el juego está en `Executable/DashAndFight.exe` y el proyecto de Unity en la raíz.

Los archivos [SHA256SUMS.txt](https://github.com/benjaminramirezangulo-jpg/dash-2/releases/download/v0.3.0/SHA256SUMS.txt) y las [notas de la versión](https://github.com/benjaminramirezangulo-jpg/dash-2/releases/tag/v0.3.0) permiten identificar los paquetes publicados. Estos ZIP corresponden a v0.3.0; la documentación de la rama `main` puede recibir actualizaciones posteriores.

## Mecánicas y flow

### Movimiento y dash

El movimiento base alcanza **20 unidades/s** y el dash **64 unidades/s**. Cada dash dura **125 ms**, recorre aproximadamente **8 unidades** y admite correcciones de dirección limitadas. Mantener `Shift` reduce la velocidad para realizar ajustes precisos.

Dispones de dos cargas de dash. Eliminar a un enemigo mediante un dash devuelve una carga; las cargas también se recuperan con el tiempo. La elección de la trayectoria influye tanto en la supervivencia como en la posibilidad de continuar atacando.

El modelo funciona con un paso de simulación de **120 Hz** e incluye un búfer de entrada de **120 ms** para el dash. Esto describe las reglas de simulación, no una garantía de 120 fotogramas renderizados por segundo en cualquier equipo.

### Disparo, guardias y parry

Apunta con el ratón y mantén el botón izquierdo para disparar. Algunos enemigos bloquean ataques frontales: debes flanquearlos, aprovechar sus aperturas o devolver sus proyectiles.

`F` activa un **parry** con una ventana de **140 ms**. Los proyectiles cercanos y visibles pueden regresar hacia los enemigos; las paredes bloquean esa interacción. En The Spindle, el fuego devuelto es necesario para romper los escudos de los spinners.

### FLOW y overdrive

Las eliminaciones, los parries y el roce cercano con proyectiles mientras te mueves alimentan **FLOW**. Al alcanzar el umbral de **overdrive**, el movimiento normal sube a **24 unidades/s** y el disparo mejora. El sistema recompensa encadenar acciones y mantener el desplazamiento.

### Fallar, practicar y volver a entrar

- **Hard:** dos puntos de salud.
- **Nightmare:** un punto de salud, con récords separados.
- **`R`:** reinicia el intento.
- **Práctica:** permite trabajar desde puntos de control sin registrar marcas competitivas.
- **Ghosts y tiempos parciales:** ayudan a comparar una ruta con tus mejores intentos guardados.

## Las seis pruebas

Cada prueba tiene una condición de avance y una exigencia adicional de maestría para el rango S.

| Prueba | Objetivo para avanzar | Maestría del rango S |
| --- | --- | --- |
| **01 · Slipstream** | Despejar encuentros y activar sus relés. | Conseguir una cadena de cinco eliminaciones. |
| **02 · Crosscut** | Despejar cada sector y atravesar sus puertas con un dash en la dirección indicada. | Cortar las tres puertas direccionales. |
| **03 · The Spindle** | Devolver disparos para romper escudos y completar los encuentros. | Romper tres escudos mediante fuego devuelto. |
| **04 · Razorwire** | Atravesar las puertas de los láseres con dash; eliminar a todos los enemigos es opcional. | Cruzar dos líneas de láser mientras están activas. |
| **05 · Redline** | Llegar a los puntos de control y escapar de los perseguidores. | Contrarrestar a los tres rushers. |
| **06 · Terminal Velocity** | Completar la ruta y derrotar a un núcleo con tres fases. | Superar las tres fases del núcleo. |

![Selección de las seis pruebas con objetivos de maestría, tiempos y acceso al modo práctica.](Docs/media/dash-2-trials.png)

Para obtener **S** también debes terminar sin recibir daño, dentro del tiempo objetivo y con movimiento durante al menos el **78 %** del intento. Los tiempos objetivo son, respectivamente, **8, 12, 18, 12, 12 y 20 segundos**. Superar una prueba desbloquea la siguiente; el rango S es opcional.

**Pendiente de validación:** el buscador automático consiguió registros que cumplen S en cinco pruebas, pero no lo estableció para Redline después de 64 intentos. Eso no demuestra que sea imposible; su exigencia práctica todavía debe revisarse. El resultado está conservado en [mastery-search.csv](TestResults/mastery-search.csv).

## Controles y opciones

| Entrada | Acción |
| --- | --- |
| `W`, `A`, `S`, `D` | Moverse. |
| Ratón | Apuntar. |
| Botón izquierdo mantenido | Disparar. |
| `Espacio` o botón derecho | Dash; el movimiento permite ajustar su dirección. |
| `Shift` izquierdo | Movimiento de precisión. |
| `F` | Parry y devolución de proyectiles cercanos. |
| `R` | Reiniciar el intento. |
| `Esc` | Pausar, continuar o volver. |
| `C` en pausa o tras morir | Acceder a la práctica desde un punto de control. |
| `Tab` | Ampliar el mapa. |
| `Enter` | Continuar, reintentar o avanzar según la pantalla. |

En **Settings** puedes ajustar volumen, música, intensidad de la sacudida de cámara, destellos reducidos, ghosts, asistencia de apuntado y dificultad. El cambio de dificultad se aplica al siguiente intento.

### Progreso y guardado

El progreso de esta versión se guarda en `velocity-v3.json`, dentro del directorio persistente de Unity. En Windows:

```text
%USERPROFILE%\AppData\LocalLow\INACAP\Dash and Fight
```

Los ghosts y las grabaciones se guardan junto a ese archivo. Hard y Nightmare mantienen marcas separadas. El guardado valida sus datos y utiliza sustitución del archivo con copia de respaldo. Los registros de versiones anteriores permanecen separados porque cambiaron las reglas de las pruebas.

## Dirección visual y sonora

El estilo utiliza fondos negros, figuras blancas y grises, trazos de movimiento y algunas tramas. La bufanda y la silueta del personaje ayudan a seguirlo, mientras las formas de los enemigos y las señales de ataque distinguen amenazas sin depender del color.

La referencia al manga se limita al contraste y a los trazos: el diseño mantiene una presentación geométrica sencilla. La intención es que puedas reconocer una abertura mientras te desplazas con rapidez.

El sonido combina efectos sintetizados, una base rítmica y recursos conservados del prototipo. Incluye controles de volumen y música, separación de eventos y un limitador de la mezcla. Las pruebas verifican señal y niveles; la valoración subjetiva de la mezcla sigue pendiente.

## Estado, pruebas y próximos pasos

### Disponible en v0.3.0

- [x] Seis pruebas con rutas y objetivos diferenciados.
- [x] Movimiento rápido, dash dirigido, parry, FLOW y overdrive.
- [x] Enemigos con guardias, persecución y patrones de disparo distintos.
- [x] Núcleo final con tres fases y ventanas de contraataque.
- [x] Menú inicial, guía, pausa y ajustes.
- [x] Guardado, récords por dificultad, rangos y tiempos parciales.
- [x] Puntos de práctica, ghosts y reproducción de entradas.
- [x] Juego portátil de Windows y herramientas de comprobación.

### Evidencia de la versión publicada

Las verificaciones de v0.3.0 se realizaron el **19 de noviembre de 2026**. Los resultados están incluidos en el repositorio; son pruebas del comportamiento del software y no sustituyen una evaluación humana del juego.

| Verificación | Resultado registrado | Evidencia |
| --- | --- | --- |
| Modelo de juego | **95 comprobaciones aprobadas, 0 fallos.** | [model-tests.xml](TestResults/model-tests.xml) |
| Integración en el reproductor real de Unity | **115 comprobaciones aprobadas, 0 fallos.** | [runtime-tests.xml](TestResults/runtime/runtime-tests.xml) |
| Finalización de las pruebas | Las seis tienen grabaciones de entradas ordinarias que llegan a la victoria. | [flow-witness.csv](TestResults/flow-witness.csv) y archivos `stage-*.replay`. |
| Copia portátil distribuida | Volvió a aprobar las 115 comprobaciones de integración. | [Notas de v0.3.0](https://github.com/benjaminramirezangulo-jpg/dash-2/releases/tag/v0.3.0). |

La cobertura incluye velocidad y aceleración, normalización del movimiento, colisiones, puertas direccionales, escudos, láseres, progreso, práctica, guardado, entrada a distintas frecuencias, cámara, interfaz, monocromía, audio y rendimiento con muchos proyectiles.

En la comprobación de la copia portátil se midieron **2.279 fotogramas**, con una media de **4,27 ms**, un percentil 99 de **4,83 ms** y un pico de **67,35 ms**. El ensayo llegó a **376 proyectiles simultáneos**. Las pruebas de percentiles aprobaron, pero hubo un fotograma largo; no se afirma un ritmo de renderizado perfectamente uniforme. El escenario de estrés utiliza invulnerabilidad para sostener la carga; las grabaciones de finalización no la utilizan.

![Mediciones de la copia portátil: media de 4,27 ms, percentil 95 de 4,57 ms, percentil 99 de 4,83 ms y máximo de 67,35 ms.](Docs/media/dash-2-frame-times.png)

*Resumen de una prueba real, con [sus datos de origen](Docs/media/portable-performance.json). La línea de 16,67 ms es una referencia matemática para 60 fps. La barra máxima conserva el pico observado; la figura no describe todos los equipos ni una partida humana.*

El [informe de la versión](Docs/DASH-2-SNAPSHOT.md), la [rúbrica de calidad](Docs/QUALITY-RUBRIC.md) y la [revisión de Second Cut](Docs/SECOND-CUT-REVIEW.md) conservan el alcance y las limitaciones de las evaluaciones. Third Cut sigue siendo una **pre-release** y no tiene una nueva calificación global validada.

### Prioridades propuestas

| Prioridad | Trabajo pendiente | Qué permitiría comprobar |
| --- | --- | --- |
| Sensación de control | Realizar sesiones humanas con teclado y ratón y recoger observaciones por prueba. | Comodidad, lectura y aprendizaje de las rutas. |
| Maestría | Revisar Redline y su requisito de tres contraataques. | Que S sea exigente y alcanzable con una ruta práctica. |
| Rendimiento | Investigar los fotogramas largos en más equipos y sesiones. | Mayor uniformidad durante el movimiento rápido. |
| Audio | Escuchar y ajustar la mezcla con distintos dispositivos de salida. | Claridad de señales, balance y fatiga auditiva. |
| Compilación | Importar y compilar el proyecto completo en un Editor con licencia activada. | Reproducibilidad del conjunto de recursos y del ejecutable. |

Estas prioridades se organizan en el siguiente plan de trabajo propuesto.

## Plan, calendario y presupuesto

### Calendario propuesto de seis semanas

**Escenario de planificación, todavía sin compromiso de fechas:** inicio de referencia el **30 de noviembre de 2026**, con **15 horas semanales**, hasta **90 horas de trabajo**. La disponibilidad de participantes, equipo y Editor debe confirmarse antes de convertirlo en un calendario de entrega. Las fechas incluyen semanas de fin de año y deben revisarse con esa disponibilidad.

| Semana propuesta | Foco principal | Entregable para revisar |
| --- | --- | --- |
| **1 · 30 nov–6 dic 2026** | Primera ronda de tres sesiones humanas; registrar problemas de control, lectura y aprendizaje. | Informe inicial, incidencias reproducibles y lista priorizada de ajustes. |
| **2 · 7–13 dic 2026** | Ajustar rutas y dificultad; revisar los tres contraataques de Redline. | Una ruta práctica de maestría o una modificación explicada de sus requisitos, con grabación. |
| **3 · 14–20 dic 2026** | Investigar los fotogramas largos y repetir mediciones comparables. | Perfil de rendimiento, comparación de resultados y registro de los problemas que sigan abiertos. |
| **4 · 21–27 dic 2026** | Escuchar la mezcla y revisar la claridad de ataques, destellos y opciones visuales. | Lista de ajustes de sonido y legibilidad con comprobaciones posteriores. |
| **5 · 28 dic 2026–3 ene 2027** | Validar la importación y compilación completa con un Editor activado, y comprobar la copia portátil. | Procedimiento reproducible, paquete candidato e incidencias de compilación documentadas. |
| **6 · 4–10 ene 2027** | Segunda ronda de tres sesiones humanas, regresiones y documentación de resultados. | Informe final de la candidata, seis rutas de finalización y notas de versión. |

La revisión del **10 de enero de 2027 es un objetivo propuesto**, condicionado al cierre de los problemas que impidan avanzar, guardar o ejecutar el juego. Una dependencia sin resolver debe reflejarse en las incidencias y en una revisión del calendario.

### Qué se comprobaría antes de publicar otra candidata

- Poder terminar las seis pruebas mediante entradas normales.
- Volver a ejecutar las regresiones y la integración después de cambios en las reglas.
- Documentar las seis sesiones humanas propuestas y las decisiones tomadas a partir de ellas.
- Explicar la situación del rango S de Redline y los fotogramas largos.
- Identificar con claridad la versión de Unity, el procedimiento de compilación y los archivos distribuidos.

### Presupuesto propuesto

Los importes siguientes son un **ejemplo de planificación en pesos chilenos (CLP)** para el trabajo de estas seis semanas. No representan un gasto ya efectuado, una cotización comercial ni fondos recaudados. La tarifa de **$10.000 CLP por hora** y la asignación por sesión son supuestos del escenario, no precios de mercado verificados.

| Partida | Supuesto de cálculo | Estimación CLP |
| --- | --- | ---: |
| Control, movimiento y balance de rutas | 30 h × $10.000 | $300.000 |
| Pruebas, análisis de errores y validación | 24 h × $10.000 | $240.000 |
| Rendimiento y procedimiento de compilación | 18 h × $10.000 | $180.000 |
| Sonido y legibilidad visual | 12 h × $10.000 | $120.000 |
| Documentación y preparación de la candidata | 6 h × $10.000 | $60.000 |
| **Subtotal de trabajo** | **90 h** | **$900.000** |
| Apoyo a sesiones de prueba | 6 sesiones × $10.000 | $60.000 |
| **Base del escenario** | Trabajo y sesiones | **$960.000** |
| Reserva para imprevistos | 15 % de $960.000 | $144.000 |
| **Total estimado del escenario** | **Base + reserva** | **$1.104.000** |

El escenario supone el uso del equipo y las herramientas disponibles y no presupuesta nuevas compras. La disponibilidad y las condiciones del Editor, posibles necesidades de hardware y cualquier servicio adicional deben revisarse antes de fijar un costo real; si requieren un desembolso, la estimación debe actualizarse. El historial del repositorio no contiene una contabilidad que permita calcular el gasto acumulado del proyecto.

Cada partida tendría como respaldo un entregable del calendario: cambios de control, informes de sesiones, perfiles de rendimiento, ajustes de sonido o documentación de la candidata. El objetivo es poder contrastar recursos propuestos con trabajo verificable.

## Preguntas frecuentes

### ¿Hay algo que pueda probar hoy?

Sí. La [demo v0.3.0 para Windows](https://github.com/benjaminramirezangulo-jpg/dash-2/releases/tag/v0.3.0) ya está publicada e incluye las seis pruebas. El video, las capturas y las grabaciones muestran esa versión de desarrollo.

### ¿Qué hace especial al proyecto?

Su enfoque está en cómo se conectan el movimiento rápido, la lectura de una ruta, los dashes y los contraataques. La presentación monocroma busca que esas decisiones se puedan reconocer durante el desplazamiento.

### ¿Qué demuestra que el proyecto está preparado para seguir avanzando?

Hay una demo ejecutable, código, rutas registradas, informes de pruebas, límites documentados y un plan con entregables. La evaluación humana y los problemas abiertos siguen formando parte de lo que hay que validar.

### ¿Las fechas y el presupuesto ya están confirmados?

Son una propuesta con supuestos explícitos. La persona responsable de la publicación debe validar disponibilidad, recursos y costos antes de asumir compromisos de entrega.

### ¿Cómo puedo ayudar a mejorarlo?

Prueba una ruta y comparte una observación concreta: qué acción intentaste, dónde ocurrió y qué esperabas sentir o ver. El apartado de incidencias explica cómo dejar información útil.

## Desarrollo y estructura

### Abrir el proyecto

```powershell
git clone https://github.com/benjaminramirezangulo-jpg/dash-2.git
cd dash-2
```

Abre la carpeta con **Unity 6000.6.7f1** y utiliza la escena `Assets/Scenes/Game.unity`. El proyecto contiene el comando **Velocity > Build Windows Player**, que genera una distribución en `Builds/Windows/`.

**Alcance de la compilación validada:** el juego distribuido conserva el motor y los recursos serializados del reproductor Mono suministrado. Los scripts se compilan contra sus bibliotecas exactas, y la campaña, las mallas y la interfaz se crean durante la ejecución. La importación y compilación completa de recursos desde el Editor todavía no se han validado en este trabajo por falta de una licencia activada.

### Ejecutar las comprobaciones

Las herramientas usan **Roslyn 4.14.0**. Para ejecutarlas en otro equipo, indica la ubicación de su `csc.exe`:

```powershell
.\Tools\run-all.ps1 -Compiler 'D:\tools\roslyn\tasks\net472\csc.exe'
```

El compilador es una dependencia de desarrollo; no se distribuye como requisito para jugar. Si no se indica `-Compiler`, los scripts utilizan la ubicación original `D:\DashWork\roslyn\tasks\net472\csc.exe`.

La secuencia comprueba el modelo, busca grabaciones que completen las seis pruebas, reconstruye el ensamblado de gameplay y ejecuta la integración en Unity. Los fallos devuelven un código de salida distinto de cero y los informes se escriben en `TestResults/`. El comando recompila y reemplaza el ensamblado de `Executable/`, por lo que conviene ejecutarlo en una copia de desarrollo.

### Reproducir una ruta registrada

Desde la raíz del repositorio:

```powershell
$replay = (Resolve-Path .\TestResults\stage-6.replay).Path
& .\Executable\DashAndFight.exe --dash-replay $replay
```

Estas grabaciones almacenan entradas. No son guiones que coloquen al jugador en la salida o eliminen enemigos mediante cambios de estado. Los escenarios de integración que sí preparan estados de prueba están diferenciados de las grabaciones de finalización.

### Mapa del repositorio

```text
dash-2/
├── Assets/
│   ├── Editor/VelocityBuild.cs       # Compilación desde Unity
│   ├── Scenes/Game.unity            # Escena de arranque
│   ├── StreamingAssets/VelocityAudio/
│   └── scripts/Velocity/            # Reglas, vista, interfaz y persistencia
├── Docs/                           # Informes, rúbrica y material visual
├── Executable/                     # Distribución portátil de Windows
├── Packages/                       # Dependencias de Unity
├── ProjectSettings/                # Configuración del proyecto
├── TestResults/                    # Informes y grabaciones verificables
├── Tests/                          # Regresiones y controlador de verificación
├── Tools/                          # Scripts PowerShell de compilación y pruebas
└── README.md
```

| Archivo | Responsabilidad |
| --- | --- |
| `VelocityModel.cs` | Simulación, colisiones, enemigos, niveles y objetivos. |
| `VelocityGame.cs` | Entrada, menús, HUD, avance y práctica. |
| `VelocityView.cs` | Cámara, mallas, siluetas y efectos visuales. |
| `VelocitySound.cs` y `VelocityAudioCapture.cs` | Síntesis, mezcla y captura opcional del audio del juego. |
| `VelocityRecording.cs` y `VelocityReplayPlayer.cs` | Grabaciones, ghosts y reproducción. |
| `VelocitySave.cs` | Validación y persistencia de progreso y opciones. |
| `VelocityRuntimeTests.cs` | Comprobaciones dentro del reproductor de Unity. |

`GameManager.Awake` inicia esta campaña y desactiva las raíces de la escena anterior. Los componentes originales permanecen como referencia; las reglas actuales están en `Assets/scripts/Velocity/`.

## Reportar problemas y aportar observaciones

Utiliza [Issues](https://github.com/benjaminramirezangulo-jpg/dash-2/issues) e incluye la versión, la prueba, la dificultad, los pasos para reproducir el problema y lo que esperabas que ocurriera. Si afecta al rendimiento, añade tu equipo y resolución. Una grabación o captura ayuda a distinguir un error de una regla poco clara.

Para comentar la sensación del juego, resulta útil indicar el punto concreto: giro del dash, respuesta al soltar una tecla, lectura de un ataque, recuperación tras un error o repetición desde un punto de práctica.

## Créditos y referencias

- **Prototipo de origen:** [hiwhatsup67/dash](https://github.com/hiwhatsup67/dash), Dash and Fight, creado para el trabajo TILE22 de INACAP.
- **Repositorio de esta iteración:** [benjaminramirezangulo-jpg/dash-2](https://github.com/benjaminramirezangulo-jpg/dash-2).
- **Tecnología:** Unity, C# y Roslyn para el flujo de compilación de scripts documentado.
- **Material de presentación:** capturas y repeticiones reales de Third Cut, música sintetizada del juego, subtítulos ES/EN y un gráfico a partir de mediciones registradas. Procedencia y edición en [MEDIA.md](Docs/media/MEDIA.md).

La presentación toma como referencia la [guía de Kickstarter para contar un proyecto](https://www.kickstarter.com/help/handbook/your_story?lang=en): explicar la propuesta, mostrar material real, describir lo realizado y exponer los siguientes pasos. También se consultaron las presentaciones de [Hollow Knight](https://www.kickstarter.com/projects/11662585/hollow-knight) y [Crowsworn](https://www.kickstarter.com/projects/mongooserodeo/crowsworn-a-dark-and-mysterious-action-packed-metroidvania) como ejemplos de un concepto breve acompañado de material visual.

Presentación actualizada el **26 de noviembre de 2026**. La versión jugable y las comprobaciones citadas corresponden a **v0.3.0, publicada el 19 de noviembre de 2026**.
