using UnityEditor;
using UnityEngine;

public static partial class TapWingVisualBuild
{
    public static void Props()
    {
        Open();
        var obstacleSprite = ImportSprite(Art + "/UrbanObstacle.png");
        var coinSprite = ImportSprite(Art + "/GoldCoin.png");
        string path = "Assets/Prefabs/Obstaculos/Obstaculo.prefab";
        var obstacle = PrefabUtility.LoadPrefabContents(path);
        var old = obstacle.GetComponent<SpriteRenderer>(); old.enabled = false;
        var child = obstacle.transform.Find("UrbanArt");
        if (child == null) { child = new GameObject("UrbanArt").transform; child.SetParent(obstacle.transform, false); }
        var art = Ensure<SpriteRenderer>(child.gameObject);
        art.sprite = obstacleSprite; art.sortingOrder = 10;
        art.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Art + "/CityUnlit.mat");
        child.localScale = new Vector3(1f/obstacleSprite.bounds.size.x, 1f/obstacleSprite.bounds.size.y, 1);
        // Slightly inset corners avoid collision in the transparent bevel; overall footprint is unchanged.
        obstacle.GetComponent<BoxCollider2D>().size = new Vector2(0.94f, 0.96f);
        PrefabUtility.SaveAsPrefabAsset(obstacle, path);
        PrefabUtility.UnloadPrefabContents(obstacle);

        path = "Assets/Prefabs/Collectibles/Moneda.prefab";
        var coin = PrefabUtility.LoadPrefabContents(path);
        coin.GetComponent<SpriteRenderer>().enabled = false;
        child = coin.transform.Find("CoinArt");
        if (child == null) { child = new GameObject("CoinArt").transform; child.SetParent(coin.transform, false); }
        art = Ensure<SpriteRenderer>(child.gameObject);
        art.sprite = coinSprite; art.sortingOrder = 12;
        art.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(Art + "/CityUnlit.mat");
        child.localScale = Vector3.one * (0.8f/coinSprite.bounds.size.x);
        coin.GetComponent<Moneda>().artwork = art;
        Ensure<CoinVisual>(coin).artwork = art;
        PrefabUtility.SaveAsPrefabAsset(coin, path);
        PrefabUtility.UnloadPrefabContents(coin);
        Save("URBAN_PROPS");
    }
}
