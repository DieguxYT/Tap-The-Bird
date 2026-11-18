using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Integration checks in Play Mode. Restores the user's persistent statistics.</summary>
[InitializeOnLoad]
public static class TapWingPhase2Checks
{
    const string Active = "TapWing.Checks.Active";
    const string Finished = "TapWing.Checks.Finished";
    static int step;
    static double next;
    static int initialTotal;
    static float finalDistance;
    static string errors = "";
    static string editorIssues = "";
    static string passed = "";

    static TapWingPhase2Checks()
    {
        if (!SessionState.GetBool(Active, false)) return;
        EditorApplication.playModeStateChanged += State;
        EditorApplication.update += Tick;
        Application.logMessageReceived += Log;
    }

    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/PopupUI.unity");
        TapWingPhase2Setup.Validate();
        foreach (string key in new[] { MetricasPartida.ClaveMonedas, MetricasPartida.ClaveDistancia })
        {
            SessionState.SetBool(key + ".exists", PlayerPrefs.HasKey(key));
            SessionState.SetInt(key + ".int", PlayerPrefs.GetInt(key));
            SessionState.SetFloat(key + ".float", PlayerPrefs.GetFloat(key));
        }
        SessionState.SetBool(Active, true);
        SessionState.SetBool(Finished, false);
        EditorApplication.playModeStateChanged -= State;
        EditorApplication.playModeStateChanged += State;
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Application.logMessageReceived -= Log;
        Application.logMessageReceived += Log;
        EditorApplication.EnterPlaymode();
    }

    static void State(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode) next = EditorApplication.timeSinceStartup + 1;
        if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(Finished, false)) return;
        Restore(MetricasPartida.ClaveMonedas, false);
        Restore(MetricasPartida.ClaveDistancia, true);
        PlayerPrefs.Save();
        SessionState.SetBool(Active, false);
        EditorApplication.Exit(SessionState.GetInt("TapWing.Checks.ExitCode", 1));
    }

    static void Restore(string key, bool floating)
    {
        if (!SessionState.GetBool(key + ".exists", false)) PlayerPrefs.DeleteKey(key);
        else if (floating) PlayerPrefs.SetFloat(key, SessionState.GetFloat(key + ".float", 0));
        else PlayerPrefs.SetInt(key, SessionState.GetInt(key + ".int", 0));
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

    static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        passed += "PASS " + name + "\n";
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.isPaused || SessionState.GetBool(Finished, false) ||
            EditorApplication.timeSinceStartup < next) return;
        try
        {
            var gm = GameManager.Instance;
            var m = MetricasPartida.Instance;
            var gen = m.generador;
            var player = gm.player.GetComponent<Player>();
            if (step == 0)
            {
                Check(!gm.PartidaActiva && Time.timeScale == 0, "Menu does not advance distance");
                gm.Jugar();
                player.enabled = false;
                var body = player.GetComponent<Rigidbody2D>();
                body.gravityScale = 0;
                body.linearVelocity = Vector2.zero;
                gen.enabled = false;
                initialTotal = m.TotalMonedas;
                Check(m.Monedas == 0 && m.Distancia == 0, "New run resets session metrics");
                m.Avanzar(gen.VelocidadActual, 2);
                Check(Mathf.Approximately(m.Distancia, 6), "3 units/s for 2 s gives 6 metres");
                gen.ActualizarPuntos(5);
                m.Avanzar(gen.VelocidadActual, 2);
                Check(Mathf.Approximately(m.Distancia, 13), "Distance uses increased difficulty speed");
                gm.Pausar();
                m.Avanzar(7, 10);
                Check(Mathf.Approximately(m.Distancia, 13), "Pause freezes distance");
                var coin = UnityEngine.Object.Instantiate(gen.monedaPrefab, player.transform.position, Quaternion.identity);
                Check(!coin.IntentarRecoger(player), "Paused coin cannot be collected");
                gm.Continuar();
                Check(coin.IntentarRecoger(player), "Coin can be collected during gameplay");
                Check(!coin.IntentarRecoger(player), "Coin cannot be awarded twice");
                Check(m.Monedas == 1 && m.TotalMonedas == initialTotal + 1, "Session and lifetime counters increment");
                Check(m.textoMonedas.text.Contains("1"), "HUD updates immediately");
                UnityEngine.Object.Instantiate(gen.monedaPrefab, player.transform.position, Quaternion.identity).velocidad = 0;
                step = 1;
                next = EditorApplication.timeSinceStartup + 0.5;
            }
            else if (step == 1)
            {
                Check(m.Monedas == 2, "Real Physics2D trigger collects the second coin");
                gen.probabilidadMoneda = 1;
                gen.SendMessage("GenerarObstaculo");
                Check(UnityEngine.Object.FindObjectsByType<Moneda>().Length > 0, "Generator creates collectible prefab");
                var offscreen = UnityEngine.Object.Instantiate(gen.monedaPrefab, new Vector3(-100, 0, 0), Quaternion.identity);
                offscreen.name = "OffscreenCheck";
                finalDistance = m.Distancia;
                step = 2;
                next = EditorApplication.timeSinceStartup + 0.2;
            }
            else if (step == 2)
            {
                Check(GameObject.Find("OffscreenCheck") == null, "Offscreen coins are destroyed");
                gm.Perder();
                finalDistance = m.Distancia;
                m.Avanzar(7, 5);
                Check(m.Distancia == finalDistance, "Defeat freezes distance");
                Check(m.textoResumenDerrota.text.Contains("Monedas de la partida: 2"), "Defeat displays final coins");
                Check(PlayerPrefs.GetInt(MetricasPartida.ClaveMonedas) == initialTotal + 2, "Lifetime coins saved");
                Check(PlayerPrefs.GetFloat(MetricasPartida.ClaveDistancia) >= finalDistance, "Distance record saved");
                gm.ReiniciarPartida();
                Check(m.Monedas == 0 && m.Distancia == 0, "Retry resets session metrics");
                Check(m.TotalMonedas == initialTotal + 2 && m.RecordDistancia >= finalDistance, "Retry preserves lifetime records");
                gm.MostrarMenu();
                step = 3;
                next = EditorApplication.timeSinceStartup + 0.3;
            }
            else
            {
                Check(UnityEngine.Object.FindObjectsByType<Moneda>().Length == 0, "Restart/menu removes all coins");
                Check(UnityEngine.Object.FindObjectsByType<Obstaculo>().Length == 0, "Restart/menu removes obstacles");
                Check(UnityEngine.Object.FindObjectsByType<MetricasPartida>().Length == 1, "Statistics manager is not duplicated");
                Check(errors.Length == 0, "No runtime error or exception during checks");
                Finish(0);
            }
        }
        catch (Exception e) { errors += e + "\n"; Finish(1); }
    }

    static void Finish(int code)
    {
        Directory.CreateDirectory("../TapWing-Backups/Reports");
        File.WriteAllText("../TapWing-Backups/Reports/phase2-tests.txt", passed + "\n" + errors + "\n" + editorIssues);
        Debug.Log(code == 0 ? "TAPWING_PHASE2_TESTS_OK" : "TAPWING_PHASE2_TESTS_FAILED");
        SessionState.SetInt("TapWing.Checks.ExitCode", code);
        SessionState.SetBool(Finished, true);
        EditorApplication.ExitPlaymode();
    }
}
