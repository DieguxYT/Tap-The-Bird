using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class TapWingVisualBuild
{
    static T Ensure<T>(GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }
    public const string Art = "Assets/Art/City";
    static GameManager Open()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/PopupUI.unity");
        return UnityEngine.Object.FindAnyObjectByType<GameManager>();
    }
    static void Save(string phase)
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        foreach (var c in scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Component>(true)))
            if (c != null && PrefabUtility.IsPartOfPrefabInstance(c)) PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("TAPWING_" + phase + "_OK");
    }
    public static void Bird()
    {
        var gm = Open();
        Directory.CreateDirectory("Assets/Animations/TapWing");
        var root = gm.player;
        var original = root.GetComponent<SpriteRenderer>();
        var child = root.transform.Find("BirdArt");
        if (child == null) { child = new GameObject("BirdArt").transform; child.SetParent(root.transform, false); }
        var sr = Ensure<SpriteRenderer>(child.gameObject);
        sr.sprite = original.sprite; sr.sharedMaterial = original.sharedMaterial;
        sr.sortingOrder = original.sortingOrder; original.enabled = false;
        const string clipPath = "Assets/Assets/2DBirds_Pack3/Animations/Bird5/Bird5 Black.anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (clip == null || AnimationUtility.GetObjectReferenceCurveBindings(clip).Length == 0)
            throw new Exception("Original bird animation missing.");
        const string controllerPath = "Assets/Animations/TapWing/Bird.controller";
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);
            var machine = controller.layers[0].stateMachine;
            var flight = machine.AddState("Flight"); flight.motion = clip; machine.defaultState = flight;
            var deadClip = new AnimationClip { name = "DefeatPose" };
            AnimationUtility.SetObjectReferenceCurve(deadClip,
                new EditorCurveBinding { path = "", type = typeof(SpriteRenderer), propertyName = "m_Sprite" },
                new[] { new ObjectReferenceKeyframe { time = 0, value = original.sprite } });
            AssetDatabase.CreateAsset(deadClip, "Assets/Animations/TapWing/DefeatPose.anim");
            var dead = machine.AddState("Defeat"); dead.motion = deadClip;
            var transition = flight.AddTransition(dead); transition.hasExitTime = false; transition.duration = 0;
            transition.AddCondition(AnimatorConditionMode.If, 0, "Dead");
            var back = dead.AddTransition(flight); back.hasExitTime = false; back.duration = 0;
            back.AddCondition(AnimatorConditionMode.IfNot, 0, "Dead");
        }
        var animator = Ensure<Animator>(child.gameObject);
        animator.runtimeAnimatorController = controller;
        var visual = Ensure<BirdVisual>(root);
        visual.sprite = sr; visual.animator = animator;
        // Sample the original clip to prove it contains changing sprites, not a static frame.
        clip.SampleAnimation(child.gameObject, 0); var first = sr.sprite;
        clip.SampleAnimation(child.gameObject, 0.2f); var second = sr.sprite;
        if (first == second) throw new Exception("Bird animation did not change sprite.");
        clip.SampleAnimation(child.gameObject, 0);
        Save("BIRD_ANIMATION");
        Directory.CreateDirectory("../TapWing-Backups/VisualReports");
        File.WriteAllText("../TapWing-Backups/VisualReports/bird.txt",
            $"Original clip: {clipPath}\nSprite samples differ: {first.name} / {second.name}\n" +
            $"Collider unchanged: {root.GetComponent<CircleCollider2D>().radius}\n" +
            $"Gravity unchanged: {root.GetComponent<Rigidbody2D>().gravityScale}\n");
    }
}
