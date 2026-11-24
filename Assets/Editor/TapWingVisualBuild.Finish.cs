using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static partial class TapWingVisualBuild
{
    public static void FinalRuntimeCheck()
    {
        PolishLayout();
        TapWingRuntimePreview.Run();
    }

    public static void FinalizeAndPreview()
    {
        FinishVisuals();
        TapWingAndroidPreparation.Run();
        TapWingPhase2Preview.Run();
        TapWingAndroidPreparation.CompilePlayer();
    }

    public static void PolishLayout()
    {
        var gm=Open();
        var settings=gm.UIManager.GetWindow("SettingsUI");
        foreach(var slider in settings.GetComponentsInChildren<Slider>(true))
        {
            slider.colors=ColorBlock.defaultColorBlock;
            slider.transition=Selectable.Transition.ColorTint;
            if(slider.targetGraphic!=null) slider.targetGraphic.color=Gold;
            if(slider.handleRect!=null)
            {
                var handle=slider.handleRect;
                var track=(RectTransform)handle.parent;
                track.anchorMin=new Vector2(0,.5f); track.anchorMax=new Vector2(1,.5f);
                track.sizeDelta=new Vector2(-64,64); track.anchoredPosition=Vector2.zero;
                handle.anchorMin=new Vector2(handle.anchorMin.x,0);
                handle.anchorMax=new Vector2(handle.anchorMax.x,1);
                handle.sizeDelta=new Vector2(64,0);
            }
        }
        var canvas=gm.gameplayUI.GetComponentInChildren<Canvas>(true);
        Ensure<AdaptiveHUD>(canvas.transform.Find("SafeHUD/Counters").gameObject).Refresh();
        Save("LAYOUT_POLISH");
        TapWingAndroidPreparation.Run();
        TapWingPhase2Preview.Run();
        TapWingAndroidPreparation.CompilePlayer();
    }

    static ParticleSystem MakeBurst(string name, Color color, Material material, int count)
    {
        var go = new GameObject(name);
        var ps = go.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = ps.main;
        main.duration = .6f; main.loop = false; main.playOnAwake = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(.25f,.55f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(.6f,2.8f);
        main.startSize = new ParticleSystem.MinMaxCurve(.06f,.16f);
        main.startColor = color; main.gravityModifier = .15f;
        main.maxParticles = 24; main.useUnscaledTime = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.stopAction = ParticleSystemStopAction.Destroy;
        var emission = ps.emission; emission.rateOverTime = 0;
        emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)count)});
        var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = .15f;
        var fade = ps.colorOverLifetime; fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(color,1)},
            new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)});
        fade.color = gradient;
        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material; renderer.sortingOrder = 25;
        string path = "Assets/Prefabs/Effects/"+name+".prefab";
        PrefabUtility.SaveAsPrefabAsset(go,path);
        UnityEngine.Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent<ParticleSystem>();
    }

    public static void FinishVisuals()
    {
        var gm = Open();
        Directory.CreateDirectory("Assets/Prefabs/Effects");
        var spark = DrawIcon("Spark", (x,y) => new Color(1,1,1,
            Mathf.Clamp01((45-Vector2.Distance(new Vector2(x,y),new Vector2(63.5f,63.5f)))/12)));
        const string path="Assets/Art/UI/Spark.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null) { mat=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit")); AssetDatabase.CreateAsset(mat,path); }
        mat.SetTexture("_BaseMap",spark.texture);
        mat.SetFloat("_Surface",1); mat.SetFloat("_Blend",0);
        mat.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite",0); mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.SetOverrideTag("RenderType","Transparent"); mat.renderQueue=3000;
        EditorUtility.SetDirty(mat);
        var pickup=MakeBurst("CoinBurst",new Color(1,.76f,.16f),mat,12);
        var defeat=MakeBurst("DefeatBurst",new Color(1,.63f,.38f),mat,18);
        gm.player.GetComponent<BirdVisual>().deathEffect=defeat;
        const string coinPath="Assets/Prefabs/Collectibles/Moneda.prefab";
        var coin=PrefabUtility.LoadPrefabContents(coinPath);
        coin.GetComponent<Moneda>().pickupEffect=pickup;
        PrefabUtility.SaveAsPrefabAsset(coin,coinPath); PrefabUtility.UnloadPrefabContents(coin);
        var metrics=UnityEngine.Object.FindAnyObjectByType<MetricasPartida>();
        metrics.textoTotalMonedas.gameObject.SetActive(true);
        metrics.textoResumenDerrota.gameObject.SetActive(true);
        // Preserve actual buttons/listeners; repair direct children that were hidden with legacy containers.
        foreach(var window in gm.UIManager.UIWindows)
        {
            foreach(var button in window.GetComponentsInChildren<Button>(true))
                if(button.transform.parent.name=="Card") button.gameObject.SetActive(true);
            foreach(var slider in window.GetComponentsInChildren<Slider>(true))
                if(slider.transform.parent.name=="Card") slider.gameObject.SetActive(true);
        }
        metrics.ActualizarTextos();
        Save("PARTICLES_AND_FINAL_UI");
    }
}
