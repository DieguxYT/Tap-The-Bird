using System;
using System.IO;
using System.Linq;
using Dino.Utility.Audio;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Explicit, idempotent scene migration. Never runs automatically on import.</summary>
public static class TapWingPhase2Setup
{
    [MenuItem("TapWing/Fase 2/Configurar escena y recursos")]
    public static void Run()
    {
        if (Application.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PopupUI.unity");
        var gm = UnityEngine.Object.FindFirstObjectByType<GameManager>();
        var gen = UnityEngine.Object.FindFirstObjectByType<GeneradorObstaculos>();
        // The original scene referenced the obstacle prefab instead of the scene generator.
        gm.generadorObstaculos = gen.gameObject;
        EditorUtility.SetDirty(gm);
        var score = UnityEngine.Object.FindFirstObjectByType<Puntuacion>();
        var metrics = score.GetComponent<MetricasPartida>() ?? score.gameObject.AddComponent<MetricasPartida>();
        var texts = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TMP_Text>(true)).ToArray();

        foreach (var text in texts.Where(t => t.text.StartsWith("Combo", StringComparison.OrdinalIgnoreCase)))
        {
            if (text.transform.childCount != 0) throw new InvalidOperationException("Combo has children; inspect before removing.");
            UnityEngine.Object.DestroyImmediate(text.gameObject);
        }
        metrics.generador = gen;
        metrics.textoMonedas = texts.First(t => t != null && t.text.StartsWith("Monedas:"));
        metrics.textoDistancia = texts.First(t => t != null && t.text.StartsWith("Distancia:"));

        // Keep the existing HUD graphics; only give its three live counters predictable slots.
        var hud = gm.gameplayUI.GetComponentInChildren<Canvas>(true).transform;
        Position(score.textoPuntos, hud, new Vector2(0.02f, 0.88f), new Vector2(0.26f, 0.98f), 26);
        Position(metrics.textoMonedas, hud, new Vector2(0.27f, 0.88f), new Vector2(0.51f, 0.98f), 26);
        Position(metrics.textoDistancia, hud, new Vector2(0.52f, 0.88f), new Vector2(0.81f, 0.98f), 26);
        var pause = gm.gameplayUI.GetComponentsInChildren<Button>(true).First();
        foreach (var responsive in pause.GetComponents<ResponsiveElement>()) UnityEngine.Object.DestroyImmediate(responsive);
        RectTransform pauseRect = (RectTransform)pause.transform;
        pauseRect.SetParent(hud, false);
        pauseRect.anchorMin = new Vector2(0.84f, 0.88f);
        pauseRect.anchorMax = new Vector2(0.98f, 0.98f);
        pauseRect.offsetMin = pauseRect.offsetMax = Vector2.zero;

        var menu = gm.UIManager.UIWindows.First(w => w.Id == "MenuUI");
        metrics.textoTotalMonedas = GetLabel(menu.CanvasGroup.transform, "TotalMonedas", score.textoPuntos);
        Position(metrics.textoTotalMonedas, menu.CanvasGroup.transform, new Vector2(0.15f, 0.02f), new Vector2(0.85f, 0.09f), 32);

        var defeat = gm.UIManager.UIWindows.First(w => w.Id == "DerrotaUI");
        var defeatTexts = defeat.GetComponentsInChildren<TMP_Text>(true);
        var title = defeatTexts.First(t => t.name == "Derrota_Text");
        var textRoot = (RectTransform)title.transform.parent;
        var content = (RectTransform)textRoot.parent;
        var panel = (RectTransform)content.parent;
        Stretch(content, Vector2.zero, Vector2.one);
        Stretch(textRoot, Vector2.zero, Vector2.one);
        Position(title, textRoot, new Vector2(0.08f, 0.80f), new Vector2(0.92f, 0.95f), 70);
        var description = defeatTexts.First(t => t.name == "Descripcion");
        description.text = "¡Inténtalo de nuevo!";
        Position(description, textRoot, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.80f), 30);
        Position(score.textoPuntosDerrota, textRoot, new Vector2(0.05f, 0.60f), new Vector2(0.48f, 0.70f), 34);
        Position(score.textoRecord, textRoot, new Vector2(0.52f, 0.60f), new Vector2(0.95f, 0.70f), 34);
        metrics.textoResumenDerrota = GetLabel(defeat.CanvasGroup.transform, "ResumenRecorrido", score.textoPuntos);
        Position(metrics.textoResumenDerrota, panel, new Vector2(0.08f, 0.36f), new Vector2(0.92f, 0.58f), 32);
        metrics.textoResumenDerrota.color = description.color;
        foreach (var button in defeat.GetComponentsInChildren<Button>(true))
        {
            Stretch((RectTransform)button.transform.parent, Vector2.zero, Vector2.one);
            bool retry = button.name.Contains("Reintentar");
            Stretch((RectTransform)button.transform, new Vector2(0.2f, retry ? 0.21f : 0.07f), new Vector2(0.8f, retry ? 0.32f : 0.18f));
            var label = button.GetComponentInChildren<TMP_Text>(true);
            Position(label, button.transform, new Vector2(0.04f, 0.04f), new Vector2(0.96f, 0.96f), 32);
        }

        gen.monedaPrefab = CreateCoin();
        CreateCoinSound();
        metrics.ActualizarTextos();
        PrefabUtility.RecordPrefabInstancePropertyModifications(gen);
        EditorUtility.SetDirty(metrics);
        foreach (var component in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Component>(true)))
            if (component != null && PrefabUtility.IsPartOfPrefabInstance(component))
                PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Validate();
        Debug.Log("TAPWING_PHASE2_SETUP_OK");
    }

    static TMP_Text GetLabel(Transform parent, string name, TMP_Text source)
    {
        var existing = parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
        if (existing != null) return existing.GetComponent<TMP_Text>();
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var text = go.GetComponent<TextMeshProUGUI>();
        text.font = source.font;
        text.color = source.color;
        text.fontSharedMaterial = source.fontSharedMaterial;
        text.text = "";
        return text;
    }

    static void Position(TMP_Text text, Transform parent, Vector2 min, Vector2 max, float size)
    {
        if (text.transform.parent != parent) text.rectTransform.SetParent(parent, false);
        text.rectTransform.anchorMin = min;
        text.rectTransform.anchorMax = max;
        text.rectTransform.offsetMin = text.rectTransform.offsetMax = Vector2.zero;
        text.rectTransform.localScale = Vector3.one;
        text.enableAutoSizing = true;
        text.fontSizeMin = 12;
        text.fontSizeMax = size;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
    }

    static void Stretch(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    static Moneda CreateCoin()
    {
        Directory.CreateDirectory("Assets/Art/TapWing");
        Directory.CreateDirectory("Assets/Prefabs/Collectibles");
        // Original geometric artwork, generated locally without third-party images.
        const int size = 128;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            float dx = x - 63.5f, dy = y - 63.5f;
            float r = Mathf.Sqrt(dx * dx + dy * dy);
            Color color = r > 59 ? Color.clear : r > 51 ? new Color(0.75f, 0.36f, 0.04f) :
                r > 46 ? new Color(1f, 0.94f, 0.48f) : new Color(1f, 0.72f + y / 700f, 0.16f);
            if (r < 40 && (Mathf.Abs(dx) < 6 && Mathf.Abs(dy) < 28)) color = new Color(1f, 0.97f, 0.66f);
            texture.SetPixel(x, y, color);
        }
        texture.Apply();
        const string path = "Assets/Art/TapWing/Moneda.png";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 160;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
        var coin = new GameObject("Moneda", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Moneda));
        coin.GetComponent<SpriteRenderer>().sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        coin.GetComponent<SpriteRenderer>().sortingOrder = 12;
        coin.GetComponent<CircleCollider2D>().isTrigger = true;
        coin.GetComponent<CircleCollider2D>().radius = 0.34f;
        var prefab = PrefabUtility.SaveAsPrefabAsset(coin, "Assets/Prefabs/Collectibles/Moneda.prefab");
        UnityEngine.Object.DestroyImmediate(coin);
        return prefab.GetComponent<Moneda>();
    }

    static void CreateCoinSound()
    {
        const string path = "Assets/Audio/Moneda.wav";
        const int rate = 22050, count = 5292;
        using (var writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
            writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate * 2);
            writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
            for (int i = 0; i < count; i++)
            {
                float t = (float)i / rate;
                float envelope = Mathf.Min(1, t * 150) * Mathf.Exp(-t * 18);
                float tone = Mathf.Sin(2 * Mathf.PI * 1200 * t) + 0.35f * Mathf.Sin(2 * Mathf.PI * 1800 * t);
                writer.Write((short)(tone * envelope * 8000));
            }
        }
        AssetDatabase.ImportAsset(path);
        var data = AssetDatabase.LoadAssetAtPath<AudioManagerData>("Assets/ScriptableObjects/AudioManagerData.asset");
        var sound = data.audioData.Find(a => a.name == "Moneda");
        if (sound == null) { sound = new AudioData { name = "Moneda" }; data.audioData.Add(sound); }
        sound.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        sound.audioType = Dino.Utility.Audio.AudioType.SFX;
        sound.volume = 0.65f;
        EditorUtility.SetDirty(data);
    }

    public static void Validate()
    {
        var metrics = UnityEngine.Object.FindFirstObjectByType<MetricasPartida>();
        if (metrics == null || metrics.generador == null || metrics.textoMonedas == null ||
            metrics.textoDistancia == null || metrics.textoTotalMonedas == null || metrics.textoResumenDerrota == null ||
            metrics.generador.monedaPrefab == null) throw new Exception("Missing phase 2 references.");
        var roots = metrics.gameObject.scene.GetRootGameObjects();
        if (roots.SelectMany(r => r.GetComponentsInChildren<TMP_Text>(true)).Any(t => t.text.StartsWith("Combo")))
            throw new Exception("Combo remains in scene.");
        foreach (var go in roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)))
            if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go.gameObject) != 0)
                throw new Exception("Missing script: " + go.name);
        if (!metrics.generador.monedaPrefab.GetComponent<CircleCollider2D>().isTrigger)
            throw new Exception("Coin collider is not a trigger.");
        Debug.Log("TAPWING_PHASE2_REFERENCES_OK");
    }
}
