using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class TapWingPhase2Preview
{
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/PopupUI.unity");
        var gm = Object.FindAnyObjectByType<GameManager>();
        var metrics = Object.FindAnyObjectByType<MetricasPartida>();
        Directory.CreateDirectory("../TapWing-Backups/Reports");
        foreach (var size in new[] { new Vector2Int(1600, 900), new Vector2Int(900, 1600), new Vector2Int(1200, 900) })
        {
            foreach (var window in gm.UIManager.UIWindows) window.Hide(true);
            gm.gameplayUI.SetActive(true);
            metrics.ActualizarTextos();
            Capture("hud", size);
            gm.gameplayUI.SetActive(false);
            gm.UIManager.GetWindow("DerrotaUI").Show(true);
            Capture("derrota", size);
            gm.UIManager.GetWindow("DerrotaUI").Hide(true);
            gm.UIManager.GetWindow("MenuUI").Show(true);
            Capture("menu", size);
        }
        TapWingPhaseAudit.Run();
        Debug.Log("TAPWING_PHASE2_PREVIEW_OK");
    }

    static void Capture(string name, Vector2Int size)
    {
        var camera = Camera.main;
        var target = new RenderTexture(size.x, size.y, 24);
        target.Create();
        camera.targetTexture = target;
        camera.aspect = (float)size.x / size.y;
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            if (!canvas.isRootCanvas) continue;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
        }
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active = target;
        var image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
        image.Apply();
        File.WriteAllBytes($"../TapWing-Backups/Reports/{name}-{size.x}x{size.y}.png", image.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(image);
        target.Release();
        Object.DestroyImmediate(target);
    }
}
