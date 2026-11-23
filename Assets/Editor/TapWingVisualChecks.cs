using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Play Mode integration checks. Run without -quit; this runner exits and restores preferences.</summary>
[InitializeOnLoad]
public static class TapWingVisualChecks
{
    const string Prefix = "TapWing.VisualChecks.";
    static readonly string[] IntegerKeys = { MetricasPartida.ClaveMonedas, "RecordTapWing", "Vibracion" };
    static readonly string[] FloatKeys = { MetricasPartida.ClaveDistancia, "VolumenMusica", "VolumenEfectos" };
    static int step;
    static double next, deadline;
    static Sprite firstFrame;
    static GameObject pausedObstacle;
    static string pausedPoints;
    static float radius;
    static Vector3 rootScale;
    static string passed = "", errors = "", editorIssues = "";

    static TapWingVisualChecks()
    {
        if (SessionState.GetBool(Prefix + "Active", false)) Register();
    }

    static void Register()
    {
        EditorApplication.playModeStateChanged -= State;
        EditorApplication.playModeStateChanged += State;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Application.logMessageReceived -= Log;
        Application.logMessageReceived += Log;
    }

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/PopupUI.unity");
        foreach (var key in IntegerKeys.Concat(FloatKeys))
        {
            SessionState.SetBool(Prefix + key + ".exists", PlayerPrefs.HasKey(key));
            if (IntegerKeys.Contains(key)) SessionState.SetInt(Prefix + key, PlayerPrefs.GetInt(key));
            else SessionState.SetFloat(Prefix + key, PlayerPrefs.GetFloat(key));
        }
        step = 0; passed = errors = editorIssues = "";
        SessionState.SetBool(Prefix + "Active", true);
        SessionState.SetBool(Prefix + "Finished", false);
        Register();
        EditorApplication.EnterPlaymode();
    }

    static void State(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            next = EditorApplication.timeSinceStartup + 1;
            deadline = EditorApplication.timeSinceStartup + 45;
        }
        if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Prefix + "Finished", false)) return;
        foreach (var key in IntegerKeys.Concat(FloatKeys))
        {
            if (!SessionState.GetBool(Prefix + key + ".exists", false)) PlayerPrefs.DeleteKey(key);
            else if (IntegerKeys.Contains(key)) PlayerPrefs.SetInt(key, SessionState.GetInt(Prefix + key, 0));
            else PlayerPrefs.SetFloat(key, SessionState.GetFloat(Prefix + key, 0));
        }
        PlayerPrefs.Save();
        SessionState.SetBool(Prefix + "Active", false);
        EditorApplication.Exit(SessionState.GetInt(Prefix + "ExitCode", 1));
    }

    static void Log(string message, string stack, LogType type)
    {
        if (stack.Contains("UnityEditor.Search.SearchDatabase"))
        {
            editorIssues += "EDITOR SEARCH INDEX: " + message + "\n" + stack + "\n";
            return;
        }
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            errors += message + "\n" + stack + "\n";
    }

    static void Check(bool condition, string description)
    {
        if (!condition) throw new Exception(description);
        passed += "PASS " + description + "\n";
    }

    static bool Visible(UIWindow window)
    {
        return window != null && window.CanvasRectTransform.gameObject.activeInHierarchy &&
            window.CanvasGroup.alpha > .99f && window.CanvasGroup.interactable;
    }

    static void Press(UIWindow window, string name)
    {
        var button = window.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b => b.name == name);
        Check(button.onClick.GetPersistentEventCount() > 0, name + " retains its serialized action");
        button.onClick.Invoke();
    }

    static void Wait(double seconds)
    {
        step++;
        next = EditorApplication.timeSinceStartup + seconds;
    }

    static void FreezeFlight(GameManager gm)
    {
        var body = gm.player.GetComponent<Rigidbody2D>();
        body.gravityScale = 0;
        body.linearVelocity = Vector2.zero;
        gm.generadorObstaculos.GetComponent<GeneradorObstaculos>().enabled = false;
    }

    static void CheckReferences(GameManager gm)
    {
        var all = UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        Check(all.All(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) == 0),
            "Loaded scene has no missing scripts");
        Check(gm.UIManager.UIWindows.All(w => w != null && w.CanvasGroup != null && w.CanvasRectTransform != null),
            "All registered windows have Canvas and CanvasGroup references");
        foreach (var window in gm.UIManager.UIWindows)
        {
            var card = window.CanvasGroup.transform.Find("SafeLayout/Card");
            Check(card != null, window.Id + " uses the adaptive safe-area card");
            var texts = card.GetComponentsInChildren<TMP_Text>(true).Where(t => t.gameObject.activeSelf).ToArray();
            Check(texts.Length > 0 && texts.All(t => t.font != null && t.font.name.Contains("Fredoka") &&
                t.font.material != null && t.font.atlasTextures.All(a => a != null)),
                window.Id + " has complete Fredoka font references");
        }
        var bird = gm.player.GetComponent<BirdVisual>();
        Check(bird != null && bird.sprite != null && bird.animator != null && bird.animator.runtimeAnimatorController != null,
            "Original bird animation is connected");
        Check(bird.deathEffect != null && bird.deathEffect.main.useUnscaledTime,
            "Defeat particle prefab is assigned and works at timeScale zero");
        var coin = gm.generadorObstaculos.GetComponent<GeneradorObstaculos>().monedaPrefab;
        Check(coin != null && coin.artwork != null && coin.pickupEffect != null && coin.pickupEffect.main.useUnscaledTime,
            "Coin artwork and unscaled pickup particles are assigned");
        Check(UnityEngine.Object.FindObjectsByType<CityParallax>().Length == 1,
            "Exactly one city parallax controller exists");
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying || SessionState.GetBool(Prefix + "Finished", false)) return;
        if (EditorApplication.timeSinceStartup < next) return;
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Visual tests exceeded 45 seconds.");
            var gm = GameManager.Instance;
            CheckPresent(gm);
            var player = gm.player.GetComponent<Player>();
            var bird = gm.player.GetComponent<BirdVisual>();
            var gen = gm.generadorObstaculos.GetComponent<GeneradorObstaculos>();
            switch (step)
            {
                case 0:
                    Check(Visible(gm.UIManager.GetWindow(gm.menuID)) && !gm.PartidaActiva && Time.timeScale == 0,
                        "Startup menu stays visible after Start and unscaled animation");
                    CheckReferences(gm);
                    radius = player.GetComponent<CircleCollider2D>().radius;
                    rootScale = player.transform.localScale;
                    Press(gm.UIManager.GetWindow(gm.menuID), "Button_Jugar"); FreezeFlight(gm);
                    firstFrame = bird.sprite.sprite;
                    Wait(.3);
                    break;
                case 1:
                    Check(firstFrame != bird.sprite.sprite, "Bird Animator changes its original sprite during flight");
                    Check(Mathf.Approximately(radius, player.GetComponent<CircleCollider2D>().radius) &&
                        rootScale == player.transform.localScale && player.transform.localRotation == Quaternion.identity,
                        "Bird visuals preserve the root collider, scale and rotation");
                    gm.gameplayUI.GetComponentInChildren<UnityEngine.UI.Button>().onClick.Invoke();
                    pausedPoints = Puntuacion.Instance.textoPuntos.text;
                    pausedObstacle = UnityEngine.Object.Instantiate(gen.obstaculoPrefab,
                        player.transform.position + Vector3.left * 3, Quaternion.identity);
                    Wait(.7);
                    break;
                case 2:
                    Check(Time.timeScale == 0 && !gm.PartidaActiva && Visible(gm.UIManager.GetWindow(gm.pausaID)),
                        "Pause animation finishes while timeScale is zero");
                    Check(Puntuacion.Instance.textoPuntos.text == pausedPoints,
                        "A passed obstacle cannot award points while paused");
                    pausedObstacle.SetActive(false); UnityEngine.Object.Destroy(pausedObstacle);
                    Press(gm.UIManager.GetWindow(gm.pausaID), "Button_Continuar"); Wait(.65);
                    break;
                case 3:
                    Check(gm.PartidaActiva && Time.timeScale == 1 &&
                        !gm.UIManager.GetWindow(gm.pausaID).CanvasRectTransform.gameObject.activeInHierarchy,
                        "Continue resumes gameplay and hides pause");
                    gm.MostrarMenu(); Wait(.65);
                    break;
                case 4:
                    Check(Visible(gm.UIManager.GetWindow(gm.menuID)) && !gm.player.activeSelf,
                        "Return to menu hides the bird");
                    Press(gm.UIManager.GetWindow(gm.menuID), "Button_Configuracion"); Wait(.65);
                    break;
                case 5:
                    Check(Visible(gm.UIManager.GetWindow("SettingsUI")), "Settings opens correctly on first use");
                    gm.CerrarConfiguracion(); Wait(.65);
                    break;
                case 6:
                    Check(Visible(gm.UIManager.GetWindow(gm.menuID)) &&
                        !gm.UIManager.GetWindow("SettingsUI").CanvasRectTransform.gameObject.activeInHierarchy,
                        "Settings returns to menu without overlapping windows");
                    gm.Jugar(); FreezeFlight(gm);
                    Check(player.enabled && player.EstaVivo, "Player behaviour is enabled before physical collision");
                    var obstacle = UnityEngine.Object.Instantiate(gen.obstaculoPrefab,
                        player.transform.position + Vector3.right * .5f, Quaternion.identity);
                    obstacle.GetComponent<Obstaculo>().velocidad = 0;
                    Physics2D.SyncTransforms();
                    Wait(1.2);
                    break;
                case 7:
                    Check(!player.EstaVivo && !gm.PartidaActiva && Time.timeScale == 0,
                        "Real Physics2D collision with an urban obstacle defeats the bird");
                    Check(!gm.player.activeSelf && Visible(gm.UIManager.GetWindow(gm.derrotaID)),
                        "Defeat transition completes and hides the bird");
                    Press(gm.UIManager.GetWindow(gm.derrotaID), "Button_Reintentar"); FreezeFlight(gm);
                    Check(player.EstaVivo && gm.player.activeSelf && !bird.animator.GetBool("Dead"),
                        "Retry resets bird and Animator defeat state");
                    gen.tiempoGeneracion = 2f; gen.probabilidadMoneda = 1; gen.enabled = true;
                    Wait(2.3);
                    break;
                case 8:
                    Check(gm.PartidaActiva && UnityEngine.Object.FindObjectsByType<Obstaculo>().Length > 0 &&
                        UnityEngine.Object.FindObjectsByType<Moneda>().Length > 0,
                        "Retry resumes timed obstacle and coin generation");
                    gm.MostrarMenu(); Wait(.65);
                    break;
                default:
                    Check(UnityEngine.Object.FindObjectsByType<Obstaculo>().Length == 0 &&
                        UnityEngine.Object.FindObjectsByType<Moneda>().Length == 0,
                        "Menu clears gameplay obstacles and coins after retry");
                    Check(Visible(gm.UIManager.GetWindow(gm.menuID)) &&
                        !gm.UIManager.GetWindow(gm.derrotaID).CanvasRectTransform.gameObject.activeInHierarchy,
                        "No delayed defeat window reappears after retry");
                    Check(errors.Length == 0, "No runtime errors or exceptions during visual integration checks");
                    Finish(0);
                    break;
            }
        }
        catch (Exception exception) { errors += exception + "\n"; Finish(1); }
    }

    static void CheckPresent(GameManager gm)
    {
        if (gm == null || gm.player == null || gm.UIManager == null || gm.generadorObstaculos == null)
            throw new Exception("GameManager references missing.");
    }

    static void Finish(int code)
    {
        Directory.CreateDirectory("../TapWing-Backups/Reports");
        File.WriteAllText("../TapWing-Backups/Reports/visual-tests.txt", passed + "\n" + errors + "\n" + editorIssues);
        Debug.Log(code == 0 ? "TAPWING_VISUAL_TESTS_OK" : "TAPWING_VISUAL_TESTS_FAILED");
        SessionState.SetInt(Prefix + "ExitCode", code);
        SessionState.SetBool(Prefix + "Finished", true);
        EditorApplication.ExitPlaymode();
    }
}
