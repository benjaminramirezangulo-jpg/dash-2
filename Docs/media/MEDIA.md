# Material de presentación de dash-2

Actualizado el 26 de noviembre de 2026. El material corresponde a la versión jugable v0.3.0 / Third Cut. Las fechas de producción del material no significan que se haya publicado una nueva versión del juego.

| Archivo | Procedencia y edición |
| --- | --- |
| `dash-2-title.png` y `dash-2-trials.png` | Capturas reales del reproductor de Unity durante la verificación de la copia portátil del 19 de noviembre. |
| `dash-2-crosscut.gif` | Reproducción de `TestResults/stage-2.replay`, capturada el 25 de noviembre. Fragmento a velocidad normal, reducido a 680 píxeles de ancho y 15 fps, sin audio. |
| `dash-2-presentacion-es.mp4` | Presentación de 32 s con subtítulos españoles integrados. Combina el inicio, un fragmento de Crosscut, Terminal Velocity, la selección de pruebas y una cartela final. |
| `dash-2-presentation-en.mp4` | La misma presentación con subtítulos ingleses integrados. |
| Archivos `.srt` | Siete leyendas temporizadas, disponibles en español e inglés para su lectura o reutilización. |
| `dash-2-musica.mp3` | Muestra de diez segundos de la partitura sintetizada exportada por `VelocitySound.ExportMusic` en la verificación del reproductor. Se aplicaron fundidos de entrada y salida. |
| `dash-2-frame-times.png` | Gráfico generado con Matplotlib a partir de `portable-performance.json`. Incluye media, percentiles 95 y 99, máximo y referencia de 16,67 ms. |
| `portable-performance.json` | Copia sin cambios de las mediciones de la prueba de la distribución portátil. |

## Repeticiones, montaje y sonido

Las secuencias de juego utilizan las grabaciones de entradas de Crosscut y Terminal Velocity incluidas en el repositorio. El controlador automático mueve, apunta, dispara, usa parry y dash mediante entradas normales; no modifica la salud, los enemigos, las posiciones ni el avance de los objetivos. Son repeticiones automatizadas, no sesiones humanas.

El montaje conserva el ritmo normal de las secuencias elegidas y agrega imágenes estáticas de introducción, selección y cierre. Los videos se distribuyen a 24 fps; el modelo del juego trabaja a 120 Hz. El audio del video es un fragmento repetido de la partitura sintetizada del propio juego, con ajuste de nivel y fundidos; no se presenta como una captura sincronizada de efectos de esa partida. No hay narración de voz ni grabación de micrófono.

Los subtítulos se colocan en una franja añadida bajo la imagen del juego para conservar la visibilidad del HUD. Elige la edición ES o EN según el idioma deseado. Cada archivo multimedia del README pesa menos de 5 MB.

## Alcance de los datos

El gráfico resume una medición del 19 de noviembre de 2026: 2.279 fotogramas, pico de 376 proyectiles y un fotograma que superó 16 ms. El escenario de estrés utiliza invulnerabilidad para sostener la carga. Las grabaciones de finalización no la utilizan. Las cifras no son un benchmark universal de hardware ni una evaluación humana del control.

La comprobación técnica de formatos, subtítulos y niveles de señal no sustituye una revisión subjetiva de la mezcla. Esa revisión permanece en el plan propuesto.
