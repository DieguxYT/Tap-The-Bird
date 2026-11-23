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
        var gen = gm.generadorObstaculos.GetComponent<GeneradorObstaculos>();
        var obstacle = (GameObject)PrefabUtility.InstantiatePrefab(gen.obstaculoPrefab);
        var coin = (GameObject)PrefabUtility.InstantiatePrefab(gen.monedaPrefab.gameObject);
        foreach (var size in new[] { new Vector2Int(1600, 900), new Vector2Int(900, 1600), new Vector2Int(1200, 900), new Vector2Int(2000,900), new Vector2Int(1440,900) })
        {
            foreach (var window in gm.UIManager.UIWindows) window.Hide(true);
            gm.gameplayUI.SetActive(true);
            gm.player.SetActive(true);
            obstacle.SetActive(true); coin.SetActive(true);
            float halfWidth = 5f * size.x / size.y;
            obstacle.transform.position = new Vector3(halfWidth*.65f,-.7f,0);
            coin.transform.position = new Vector3(halfWidth*.5f,1.5f,0);
            metrics.ActualizarTextos();
            Capture("hud", size);
            gm.gameplayUI.SetActive(false);
            gm.player.SetActive(false); obstacle.SetActive(false); coin.SetActive(false);
            foreach (var id in new[]{"MenuUI","DerrotaUI","PausaUI","SettingsUI","CreditosUI"})
            {
                gm.UIManager.GetWindow(id).Show(true);
                Capture(id, size);
                gm.UIManager.GetWindow(id).Hide(true);
            }
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
        Object.FindAnyObjectByType<CityParallax>()?.Arrange();
        foreach (var canvas in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            if (!canvas.isRootCanvas) continue;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 1;
        }
        Canvas.ForceUpdateCanvases();
        foreach (var panel in Object.FindObjectsByType<AdaptivePanel>()) panel.Refresh();
        foreach (var hud in Object.FindObjectsByType<AdaptiveHUD>()) hud.Refresh();
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active = target;
        var image = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0);
        image.Apply();
        File.WriteAllBytes($"../TapWing-Backups/Reports/{name}-{size.x}x{size.y}.png", image.EncodeToPNG());
        File.WriteAllBytes($"../TapWing-Backups/Reports/{name}-{size.x}x{size.y}.jpg", image.EncodeToJPG(85));
        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(image);
        target.Release();
        Object.DestroyImmediate(target);
    }
}
