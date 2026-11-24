using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>Captures real Play Mode frames after rendering has settled. Invoke without -quit.</summary>
[InitializeOnLoad]
public static class TapWingRuntimePreview
{
    const string Prefix = "TapWing.RuntimePreview.";
    const string Output = "../TapWing-Backups/Reports";
    static readonly string[] IntegerKeys = { MetricasPartida.ClaveMonedas, "RecordTapWing", "Vibracion" };
    static readonly string[] FloatKeys = { MetricasPartida.ClaveDistancia, "VolumenMusica", "VolumenEfectos" };
    static int step, afterFrame;
    static double next, deadline;
    static Camera camera;
    static RenderTexture target;
    static string errors = "", editorIssues = "", captures = "";

    static TapWingRuntimePreview()
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
        step = 0; afterFrame = 0;
        errors = editorIssues = captures = "";
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

    static void Wait(double seconds)
    {
        step++;
        next = EditorApplication.timeSinceStartup + seconds;
        afterFrame = Time.frameCount + 5;
    }

    static void PrepareCamera()
    {
        camera = Camera.main;
        if (camera == null) throw new Exception("Main camera missing.");
        target = new RenderTexture(1600, 900, 24);
        target.Create();
        camera.targetTexture = target;
        camera.aspect = 1600f / 900;
        foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            if (!canvas.isRootCanvas) continue;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
        }
        Arrange();
    }

    static void Arrange()
    {
        UnityEngine.Object.FindAnyObjectByType<CityParallax>()?.Arrange();
        foreach (var safe in UnityEngine.Object.FindObjectsByType<SafeAreaFitter>()) safe.Refresh();
        Canvas.ForceUpdateCanvases();
        foreach (var panel in UnityEngine.Object.FindObjectsByType<AdaptivePanel>()) panel.Refresh();
        Canvas.ForceUpdateCanvases();
    }

    static void Capture(string name)
    {
        Arrange();
        camera.Render();
        var previous = RenderTexture.active;
        var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        try
        {
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            image.Apply();
            Directory.CreateDirectory(Output);
            File.WriteAllBytes(Output + "/" + name + ".jpg", image.EncodeToJPG(92));
            captures += "CAPTURE " + name + ".jpg at runtime frame " + Time.frameCount + "\n";
        }
        finally
        {
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(image);
        }
    }

    static void Tick()
    {
        if (!EditorApplication.isPlaying || SessionState.GetBool(Prefix + "Finished", false)) return;
        try
        {
            if (EditorApplication.timeSinceStartup > deadline) throw new TimeoutException("Runtime preview exceeded 45 seconds.");
            if (EditorApplication.timeSinceStartup < next || Time.frameCount < afterFrame) return;
            var gm = GameManager.Instance;
            if (gm == null) throw new Exception("GameManager missing.");
            if (step == 0)
            {
                gm.Jugar();
                var body = gm.player.GetComponent<Rigidbody2D>();
                body.gravityScale = 0;
                body.linearVelocity = Vector2.zero;
                var generator = gm.generadorObstaculos.GetComponent<GeneradorObstaculos>();
                generator.enabled = false;
                var obstacle = UnityEngine.Object.Instantiate(generator.obstaculoPrefab,
                    new Vector3(3, -.7f, 0), Quaternion.identity);
                obstacle.GetComponent<Obstaculo>().velocidad = 0;
                var coin = UnityEngine.Object.Instantiate(generator.monedaPrefab,
                    new Vector3(2, 1.5f, 0), Quaternion.identity);
                coin.velocidad = 0;
                PrepareCamera();
                Wait(.6);
            }
            else if (step == 1)
            {
                if (!gm.PartidaActiva || !gm.player.activeSelf) throw new Exception("Gameplay ended before its capture.");
                Capture("runtime-gameplay");
                gm.MostrarMenu();
                Arrange();
                Wait(.7);
            }
            else
            {
                var menu = gm.UIManager.GetWindow(gm.menuID);
                if (!menu.CanvasRectTransform.gameObject.activeInHierarchy || menu.CanvasGroup.alpha < .99f)
                    throw new Exception("Menu transition did not finish before capture.");
                Capture("runtime-menu");
                Finish(errors.Length == 0 ? 0 : 1);
            }
        }
        catch (Exception exception) { errors += exception + "\n"; Finish(1); }
    }

    static void Finish(int code)
    {
        if (camera != null) camera.targetTexture = null;
        if (target != null)
        {
            if (RenderTexture.active == target) RenderTexture.active = null;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            target = null;
        }
        Directory.CreateDirectory(Output);
        File.WriteAllText(Output + "/runtime-preview.txt", captures + "\n" + errors + "\n" + editorIssues);
        Debug.Log(code == 0 ? "TAPWING_RUNTIME_PREVIEW_OK" : "TAPWING_RUNTIME_PREVIEW_FAILED");
        SessionState.SetInt(Prefix + "ExitCode", code);
        SessionState.SetBool(Prefix + "Finished", true);
        EditorApplication.ExitPlaymode();
    }
}
