# TapWing: fases 1 y 2

## Alcance

Se conserva la escena PopupUI, los prefabs originales, los audios, los créditos,
el movimiento de mantener para subir y soltar para bajar y la curva de dificultad.
El rediseño artístico, parallax y pulido general quedan para las fases 3–6.

## Preparación

- Unity del proyecto: 6000.5.9f1.
- Respaldo previo completo de Assets, Packages y ProjectSettings:
  `C:/Users/diego/Downloads/TapWing-Backups/Fase1-20261118-033136.zip`.
- Estado inicial de Git limpio. No se realizan commits ni pushes.
- No hay Unity MCP disponible. Se usa el editor local por línea de comandos.
- La instalación `6000.5.9f1` carece de UnityPackageManager.exe;
  la instalación `6000.5.9f1-x86_64` sí contiene ese componente.

## Diagnóstico

La puntuación y récord usan Puntuacion; GameManager controla las pantallas y
Time.timeScale. GeneradorObstaculos crea un obstáculo cada 2 segundos con velocidad
3–7, incrementos de 0.5 por cada 5 puntos. Player utiliza Rigidbody2D y el evento
OnTapEvent de InputManager. Obstaculo detecta el rebase mediante bounds.

Combo, Monedas y Distancia eran textos sin sistemas de juego asociados.
La UI usa uGUI, TextMesh Pro, ventanas con DOTween y anclajes responsive.
Hay recursos de aves, fondos y UI importados; no se presume su licencia para
crear nuevos recursos redistribuibles. Se conservan sin ampliar su uso.
AudioManager usa un catálogo ScriptableObject y grupos de AudioMixer.
Android tiene perfil APK, IL2CPP y ARM64. Su validación final queda en fase 6.

## Decisiones de fase 2

Una unidad de avance virtual equivale a un metro. La distancia integra la velocidad
actual del generador durante la partida. Los obstáculos existentes conservan su
velocidad original: no se cambia silenciosamente la dificultad de esta fase.
Las monedas tienen velocidad propia al aparecer, como los obstáculos actuales.
No se implementan tienda, compras, combos ni cambios de física.

## Cambios implementados

- `MetricasPartida` integra distancia, mantiene monedas de partida y total histórico,
  y guarda récord de distancia. Las claves nuevas son `TapWing.MonedasTotal` y
  `TapWing.RecordDistancia`; el récord de puntuación anterior no se borra.
- `Moneda.prefab` tiene sprite original y CircleCollider2D Trigger. Aparece con
  probabilidad 0.7, a medio intervalo detrás del obstáculo, en el corredor central.
  Cada moneda solo se recoge una vez, reproduce sonido y se eleva/desvanece.
- Monedas fuera de pantalla se eliminan; menú y reinicio limpian las restantes.
- HUD: puntos, monedas, distancia y pausa. Se elimina únicamente el objeto de texto
  Combo, sin eliminar su contenedor compartido.
- Menú: total acumulado. Derrota: distancia, récord de distancia y monedas de partida,
  junto a los puntos, récord y botones existentes.
- Se corrige la referencia original de GameManager: apuntaba al prefab Obstaculo en
  vez del objeto GeneradorObstaculos de la escena.
- Se corrige el falso error de UIManager.HideWindow y se protege `using UnityEditor`
  con compilación condicional en uGUITools.

## Probar en Unity

1. Abrir el proyecto con Unity **6000.5.9f1**, preferentemente la instalación completa
   `6000.5.9f1-x86_64` de este equipo. Esperar a que termine de importar.
2. Abrir `Assets/Scenes/PopupUI.unity`. No hay que arrastrar componentes ni ejecutar
   el comando de configuración: la escena y las referencias ya están guardadas.
3. Entrar en Play y pulsar Jugar. Mantener el ratón o el dedo para ascender; soltar
   para descender. Deben verse solo puntos, monedas, distancia y pausa en el HUD.
4. Esperar varios obstáculos y recoger una moneda dorada. Debe sumar exactamente una,
   emitir un sonido y desaparecer con una animación. No todas las oleadas generan
   monedas. Para una prueba manual determinista, poner Probabilidad Moneda en 1
   durante Play Mode; ese cambio temporal no debe guardarse.
5. Pausar y esperar: la distancia y las monedas deben quedar inmóviles. Continuar.
6. Perder: comprobar puntos, récord, distancia, récord de distancia y monedas.
7. Reintentar: puntos, distancia y monedas de partida vuelven a cero. Los históricos
   permanecen. Volver al menú muestra las monedas acumuladas.
8. Salir de Play Mode y volver a entrar: comprobar el total histórico. En Android,
   repetir cerrando y abriendo la aplicación cuando se prepare el APK en fase 6.
9. Revisar Game View a 16:9, 16:10, 4:3 y vertical. El rediseño completo, safe area y
   corrección del estiramiento de los fondos originales quedan para fases 3 y 5.

## Verificaciones automatizadas

Los scripts de `Assets/Editor` solo se ejecutan explícitamente. `TapWingPhase2Checks.Run`
entra en Play Mode y comprueba recogida real con Physics2D, doble recogida, distancia
con cambio de velocidad, pausa, derrota, persistencia, reinicio y limpieza. Restaura
las dos preferencias nuevas al terminar para no alterar los datos personales.

Informes y capturas: `C:/Users/diego/Downloads/TapWing-Backups/Reports/`.
Resultado: 23 comprobaciones de integración aprobadas; compilación de scripts en
Unity 6000.5.9f1 sin errores. Capturas inspeccionadas a 1600×900, 900×1600 y 1200×900;
se corrigió la superposición inicial del resumen de derrota con sus botones.
Los errores internos de `UnityEditor.Search.SearchDatabase` se registran aparte de
los errores del juego. No se presenta una comprobación del editor como prueba de APK
ni de rendimiento de 60 FPS en teléfono.

## Recursos nuevos

`Assets/Art/TapWing/Moneda.png` es una figura geométrica creada localmente por el
script de configuración. `Assets/Audio/Moneda.wav` es un tono sintetizado localmente.
No contienen imágenes ni grabaciones de terceros. Los audios originales se conservan.

## Siguiente etapa

Las fases 3–6 quedan pendientes de la revisión del usuario: arte, parallax, aleteo,
partículas, animaciones generales, rediseño completo, entrada táctil sobre UI,
área segura, suspensión de Android y pruebas de compilación/dispositivo.
