# TapWing: rediseño visual y preparación de entrega

## Alcance

Trabajo sobre Unity **6000.5.9f1** y la escena existente
`Assets/Scenes/PopupUI.unity`. Este documento continúa `FASES-1-2.md`:
las referencias a fases visuales pendientes de aquel informe son históricas.
Se conservan el jugador original, los controles de mantener para ascender y
soltar para descender, la puntuación, las monedas, la distancia, los récords,
los audios y los botones principales.

La dificultad mantiene generación cada 2 segundos y velocidad de 3 a 7,
con incrementos de 0.5 cada 5 puntos. Los obstáculos existentes son bloques
individuales; se conserva esa mecánica. No se ha sustituido por parejas de tuberías.

## Cambios implementados

- **Pájaro:** el Animator reproduce el aleteo original. `BirdVisual` inclina,
  colorea y escala únicamente el hijo gráfico; mantiene el cuerpo físico del
  jugador. La derrota dispone de una transición breve antes de mostrar sus resultados.
- **Ciudad:** `CityParallax` desplaza tres capas a velocidades distintas, con
  escalado uniforme y repetición alternada en espejo. El fondo se adapta a la
  cámara sin estirar el arte para rellenar diferentes proporciones de pantalla.
- **Obstáculos y monedas:** arte urbano y moneda dorada con estrella, brillo,
  giro simulado y flotación visual. El trigger permanece separado de la animación.
  Se conserva la protección contra doble recogida, la limpieza fuera de pantalla
  y el espacio de aproximación al generar monedas.
- **Interfaz:** Fredoka en los textos principales, tarjetas y botones nuevos
  sobre los controles existentes. HUD con puntos, monedas, distancia e icono de
  pausa. `SafeAreaFitter` y `AdaptivePanel` ajustan los márgenes y paneles.
- **Respuesta visual:** pulsación de botones, cambio de contadores y transiciones
  de ventanas mediante DOTween con tiempo independiente de `Time.timeScale`.
  Hay prefabs de partículas breves para recogida y derrota, con destrucción automática.
- **Entrada y ciclo de partida:** las pulsaciones sobre controles no activan el
  vuelo; pausa, menú y reinicio liberan la pulsación anterior. La aplicación pausa
  una partida activa al pasar a segundo plano. El objetivo de render es 60 FPS;
  esto no constituye una medición de rendimiento en Android.
- **Audio y ajustes:** conversión logarítmica del volumen con máximo de 0 dB,
  grupos de mezcla asignados antes de reproducir y preferencias conservadas.
  El control de vibración guarda su estado y se consulta al perder en Android.
- **Paquetes:** `manifest.json` solicita URP 17.5.0, la versión incluida con
  Unity 6000.5.9f1. La petición anterior de 17.6.0 bloqueaba la resolución de
  paquetes en esta instalación. No se editó manualmente `Library`.

## Recursos y procedencia

La animación original utilizada está en:

- `Assets/Assets/2DBirds_Pack3/Animations/Bird5/Bird5 Black.anim`.
- `Assets/Assets/2DBirds_Pack3/Spritesheets/Bird5/Bird 5 Black.png`.
- Controlador conectado: `Assets/Animations/TapWing/Bird.controller`.

Los recursos originales permanecen en el proyecto con sus condiciones de uso
existentes. No se atribuye una licencia nueva al paquete del pájaro.

Se generó arte nuevo mediante **ImageGen integrado** y se guardó en
`Assets/Art/City/`:

| Archivo | Uso |
| --- | --- |
| `SkylineFar.png` | Cielo azul, nubes y ciudad distante en tonos pastel. |
| `SkylineMid.png` | Edificios intermedios con transparencia sobre la silueta. |
| `SkylineNear.png` | Azoteas cálidas y vegetación en primer plano. |
| `UrbanObstacle.png` | Bloque industrial azul verdoso con señales amarillas. |
| `GoldCoin.png` | Moneda dorada con estrella y fondo transparente. |

`Assets/Art/UI/` contiene formas e iconos geométricos generados por código para
tarjetas, pausa, puntuación, distancia y partículas. `CoinBurst.prefab` y
`DefeatBurst.prefab` se preparan en `Assets/Prefabs/Effects/`.

La tipografía `Assets/Fonts/Fredoka/Fredoka-SemiBold.ttf` procede del
[repositorio Fredoka](https://github.com/hafontia-zz/Fredoka-One/tree/gh-pages/fonts/ttf).
Se distribuye bajo [SIL Open Font License 1.1](https://github.com/hafontia-zz/Fredoka-One/blob/gh-pages/OFL.txt).
La licencia se incluye en `Assets/Fonts/Fredoka/OFL.txt` y en
`Assets/Resources/Licenses/Fredoka-OFL.txt` para incorporarla a la aplicación.
El atlas TextMesh Pro está en `Assets/Fonts/Fredoka/Fredoka-SemiBold SDF.asset`.

## Estado de verificación final

**Verificación del 24 de noviembre de 2026:** 23 comprobaciones de métricas y
monedas y 39 de integración visual/jugable superadas. Se probaron la recogida
mediante Physics2D, el bloqueo de doble premio, la colisión real con un obstáculo,
los botones serializados, pausa, derrota, reinicio y guardado/restauración de
preferencias. No se detectaron errores de ejecución del juego en esos recorridos.

Los scripts del jugador compilados para Windows Standalone también pasaron
(`TAPWING_PLAYER_SCRIPTS_OK`); esto no sustituye una compilación Android. Unity
mantiene avisos de APIs antiguas en código existente. En una ejecución apareció
una excepción interna de `UnityEditor.Search.SearchDatabase`, separada y
registrada en el informe; no corresponde a un componente del juego.

Se generaron 30 capturas de HUD, menú, pausa, derrota, configuración y créditos
en 16:9, 20:9, 16:10, 4:3 y 9:16. La captura adicional en Play Mode espera varios
frames para evitar artefactos de la primera renderización síncrona del editor.
Se revisaron `runtime-gameplay.jpg` (frame 79) y `runtime-menu.jpg` (frame 122):
las tres capas de ciudad, el obstáculo urbano y la moneda se muestran correctamente.
Se corrigieron también las dimensiones de los controles de volumen y vibración.
`runtime-preview.txt` terminó sin errores de ejecución.

Las herramientas de configuración de `Assets/Editor/TapWingVisualBuild.*` son
migraciones explícitas. No hace falta ejecutarlas para abrir y jugar la escena
guardada. Para revisar el juego: abrir `PopupUI.unity`, entrar en Play y recorrer
menú, partida, pausa, configuración, créditos, derrota y reintento.

Las comprobaciones automatizadas cubren:

- `TapWingPhase2Checks.Run`: métricas, recogida real mediante Physics2D,
  bloqueo de doble recogida, persistencia, pausa, derrota, reinicio y limpieza.
- `TapWingVisualChecks.Run`: referencias de ventanas y fuente, aleteo real,
  conservación del cuerpo del jugador, transiciones con tiempo pausado,
  primera apertura de configuración, colisión física con obstáculo, derrota,
  reintento y generación posterior.

Ambos ejecutores entran en Play Mode, finalizan el proceso y restauran las
preferencias que sus comprobaciones pueden alterar. Ejecutar por separado, con
el proyecto cerrado en otro editor, **sin `-quit`**:

```powershell
$unityEditor = 'C:/Program Files/Unity/Hub/Editor/6000.5.9f1-x86_64/Editor/Unity.exe'
$projectPath = (Get-Location).Path
& $unityEditor -batchmode -projectPath $projectPath -executeMethod TapWingPhase2Checks.Run -logFile '../TapWing-Backups/phase2-final.log'
```

Esperar a que termine ese editor antes de ejecutar:

```powershell
& $unityEditor -batchmode -projectPath $projectPath -executeMethod TapWingVisualChecks.Run -logFile '../TapWing-Backups/visual-final.log'
```

Las rutas anteriores parten de la raíz del proyecto; adaptar únicamente la ruta
del ejecutable a la instalación local. Informes y capturas se guardan fuera del
repositorio, en `../TapWing-Backups/Reports/`: `phase2-tests.txt`,
`visual-tests.txt`, `android-preparation.txt` y las imágenes de las pantallas.
La herramienta de capturas es `TapWingPhase2Preview.Run` y admite `-quit`.
Para capturas en ejecución usar `TapWingRuntimePreview.Run` sin `-quit`.
Los problemas internos de índice `UnityEditor.Search.SearchDatabase`, si aparecen,
se registran separados de los errores del juego.

## Android

`TapWingAndroidPreparation.Run` prepara el identificador Android
**com.diego.tapwing**, conservando IL2CPP, ARM64 y la API mínima 26. Mantiene
el nombre interno del producto y la empresa para conservar la ubicación de
las preferencias del editor. Genera un
informe con la configuración leída del editor. No configura claves de firma.

La instalación disponible carece de **Android Build Support**. Por ello no se
ha validado una compilación APK/AAB ni el funcionamiento en un dispositivo real.
Tras instalar el módulo con SDK, NDK y OpenJDK, quedan por comprobar en teléfono:
instalación, controles táctiles, áreas seguras, suspensión y regreso, vibración,
audio, persistencia y rendimiento. La firma de distribución y los iconos Android
también deben revisarse antes de una publicación en tienda.

No se han ejecutado commits ni pushes como parte del rediseño.
