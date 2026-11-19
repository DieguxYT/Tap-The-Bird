using System.IO;
using System.Linq;
using UnityEngine;

public static partial class TapWingVisualBuild
{
    public static void InspectCity()
    {
        Open();
        foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include)) canvas.gameObject.SetActive(false);
        var city=UnityEngine.Object.FindAnyObjectByType<CityParallax>();
        var camera=Camera.main; camera.aspect=16f/9;
        city.Arrange();
        Directory.CreateDirectory("../TapWing-Backups/VisualReports");
        string report="";
        for(int layer=0;layer<4;layer++)
        {
            for(int l=0;l<3;l++) foreach(var tile in city.layers[l].tiles) tile.enabled=l==layer || layer==3;
            if (layer==3) {
                camera.Render();
            }
            report += $"Layer {layer}\n";
            var rt=new RenderTexture(1000,562,24); rt.Create(); camera.targetTexture=rt;
            camera.Render(); RenderTexture.active=rt;
            var tex=new Texture2D(1000,562,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1000,562),0,0);tex.Apply();
            File.WriteAllBytes($"../TapWing-Backups/VisualReports/layer-{layer}.jpg",tex.EncodeToJPG(90));
            camera.targetTexture=null;RenderTexture.active=null;rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
        }
        File.WriteAllText("../TapWing-Backups/VisualReports/layers.txt",report);
    }
}
