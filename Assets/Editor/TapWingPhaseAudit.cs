using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

public static class TapWingPhaseAudit
{
    public static void Run()
    {
        var scene = EditorSceneManager.OpenScene("Assets/Scenes/PopupUI.unity");
        var lines = scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<TMP_Text>(true))
            .Select(t => $"{t.name}: {t.text.Replace('\n', ' ')} | parent={t.transform.parent.name} | size={t.rectTransform.rect.size} | pos={t.rectTransform.anchoredPosition}").ToArray();
        Directory.CreateDirectory("../TapWing-Backups/Reports");
        File.WriteAllLines("../TapWing-Backups/Reports/scene-ui.txt", lines);
        Debug.Log("TAPWING_PHASE1_AUDIT_OK");
    }
}
