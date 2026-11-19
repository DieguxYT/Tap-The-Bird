using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class TapWingVisualBuild
{
    static Sprite ImportSprite(string path, float ppu = 100, Vector4 border = default)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = ppu;
        importer.spriteBorder = border;
        importer.spritePivot = new Vector2(0.5f, 0.5f);
        var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.alphaIsTransparency = true; importer.mipmapEnabled = false;
        importer.maxTextureSize = 2048;
        importer.textureCompression = TextureImporterCompression.CompressedHQ;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    public static void City()
    {
        var gm = Open();
        const string materialPath = Art + "/CityUnlit.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            AssetDatabase.CreateAsset(material, materialPath);
        }
        // Disable only the old full-screen landscape art, not menus or gameplay containers.
        foreach (var image in UnityEngine.Object.FindObjectsByType<Image>(FindObjectsInactive.Include))
        {
            if (image.sprite == null) continue;
            string path = AssetDatabase.GetAssetPath(image.sprite);
            if (path.EndsWith("Horizontal_BG.png") || path.EndsWith("Vertical_BG.png"))
                image.enabled = false;
        }
        var go = GameObject.Find("CityParallax");
        if (go == null) go = new GameObject("CityParallax");
        var parallax = Ensure<CityParallax>(go);
        parallax.view = Camera.main;
        parallax.generator = gm.generadorObstaculos.GetComponent<GeneradorObstaculos>();
        parallax.layers = new CityParallax.Layer[3];
        string[] names = { "SkylineFar", "SkylineMid", "SkylineNear" };
        float[] speeds = { 0.035f, 0.12f, 0.26f };
        for (int l = 0; l < 3; l++)
        {
            var layer = new CityParallax.Layer { sprite = ImportSprite(Art + "/" + names[l] + ".png"), speed = speeds[l], tiles = new SpriteRenderer[7] };
            for (int i = 0; i < 7; i++)
            {
                string name = names[l] + "_" + i;
                var child = go.transform.Find(name);
                if (child == null) { child = new GameObject(name).transform; child.SetParent(go.transform); }
                var sr = Ensure<SpriteRenderer>(child.gameObject);
                sr.sharedMaterial = material;
                sr.sortingOrder = -100 + l * 10;
                layer.tiles[i] = sr;
            }
            parallax.layers[l] = layer;
        }
        parallax.Arrange();
        foreach (float aspect in new[] { 16f/9, 20f/9, 16f/10, 4f/3, 9f/16 })
        {
            parallax.view.aspect = aspect;
            parallax.Arrange(1234);
            foreach (var layer in parallax.layers)
            {
                float left = float.MaxValue, right = float.MinValue;
                foreach (var tile in layer.tiles) { left = Mathf.Min(left, tile.bounds.min.x); right = Mathf.Max(right, tile.bounds.max.x); }
                if (left > -parallax.view.orthographicSize * aspect || right < parallax.view.orthographicSize * aspect)
                    throw new System.Exception("Parallax coverage failed at " + aspect);
            }
        }
        parallax.view.ResetAspect();
        parallax.Arrange();
        Save("CITY_COVERAGE_5_ASPECTS");
    }
}
