using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class TapWingVisualBuild
{
    static TMP_FontAsset face;
    static Sprite rounded, pauseIcon, starIcon, routeIcon;
    static readonly Color Ink = new Color(0.06f, 0.19f, 0.29f);
    static readonly Color Teal = new Color(0.035f, 0.40f, 0.48f);
    static readonly Color Gold = new Color(1f, 0.77f, 0.27f);

    static RectTransform Rect(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = 5; go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }
    static void Place(RectTransform r, Transform parent, float x0, float y0, float x1, float y1)
    {
        if (r.parent != parent) r.SetParent(parent, false);
        r.anchorMin = new Vector2(x0, y0); r.anchorMax = new Vector2(x1, y1);
        r.offsetMin = r.offsetMax = Vector2.zero;
        r.localScale = Vector3.one; r.localRotation = Quaternion.identity;
    }
    static Image Picture(string name, Transform parent, Sprite sprite, Color color)
    {
        var image = Rect(name, parent).gameObject.AddComponent<Image>();
        image.sprite = sprite; image.color = color; image.raycastTarget = false;
        if (sprite == rounded) image.type = Image.Type.Sliced;
        return image;
    }
    static void Style(TMP_Text text, float size, Color color)
    {
        text.font = face; text.fontSharedMaterial = face.material;
        text.fontStyle = FontStyles.Normal; text.color = color;
        text.enableAutoSizing = true; text.fontSizeMin = 18; text.fontSizeMax = size;
        text.alignment = TextAlignmentOptions.Center; text.raycastTarget = false;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.margin = new Vector4(8, 0, 8, 0);
    }
    static TMP_Text Label(string name, Transform parent, string value, float size)
    {
        var text = Rect(name, parent).gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value; Style(text, size, Ink); return text;
    }
    static void TextAt(TMP_Text text, Transform panel, string value, float y0, float y1, float size, float x0 = 0.08f, float x1 = 0.92f)
    {
        if (value != null) text.text = value;
        Place(text.rectTransform, panel, x0, y0, x1, y1); Style(text, size, Ink);
    }
    static void ButtonAt(Button button, Transform panel, string text, float y0, float y1, bool primary = false)
    {
        Place((RectTransform)button.transform, panel, 0.09f, y0, 0.91f, y1);
        var image = button.GetComponent<Image>(); image.sprite = rounded; image.type = Image.Type.Sliced;
        image.color = primary ? Gold : Teal; image.raycastTarget = true;
        button.targetGraphic = image; button.transition = Selectable.Transition.ColorTint;
        button.colors = ColorBlock.defaultColorBlock;
        var shadow = Ensure<Shadow>(button.gameObject); shadow.effectColor = new Color(0, 0.1f, 0.16f, 0.22f); shadow.effectDistance = new Vector2(0, -6);
        Ensure<ButtonFeedback>(button.gameObject);
        var label = button.GetComponentInChildren<TMP_Text>(true);
        if (label == null) label = Label("ButtonLabel", button.transform, text, 40);
        Place(label.rectTransform, button.transform, 0.035f, 0.06f, 0.965f, 0.94f);
        label.text = text; Style(label, 40, primary ? Ink : Color.white);
    }
    static Sprite DrawIcon(string name, Func<int, int, Color> pixel, int size = 128, bool sliced = false)
    {
        Directory.CreateDirectory("Assets/Art/UI");
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) texture.SetPixel(x, y, pixel(x, y));
        texture.Apply(); string path = "Assets/Art/UI/" + name + ".png";
        File.WriteAllBytes(path, texture.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(texture);
        return ImportSprite(path, 100, sliced ? new Vector4(32, 32, 32, 32) : Vector4.zero);
    }
    static void ResourcesUI()
    {
        rounded = DrawIcon("Rounded", (x, y) => {
            float dx = Mathf.Max(Mathf.Abs(x - 63.5f) - 34, 0), dy = Mathf.Max(Mathf.Abs(y - 63.5f) - 34, 0);
            float alpha = Mathf.Clamp01(29 - Mathf.Sqrt(dx * dx + dy * dy));
            float shade = Mathf.Lerp(0.91f, 1, y / 127f);
            return new Color(shade, shade, shade, alpha);
        }, 128, true);
        pauseIcon = DrawIcon("Pause", (x,y) => ((x >= 34 && x <= 51) || (x >= 76 && x <= 93)) && y >= 28 && y <= 99 ? Color.white : Color.clear);
        starIcon = DrawIcon("Star", (x,y) => {
            float dx = x-63.5f, dy = y-63.5f; float angle = Mathf.Atan2(dy, dx) - Mathf.PI/2;
            float sector = Mathf.Repeat(angle + Mathf.PI/5, Mathf.PI*2/5) - Mathf.PI/5;
            float radius = Mathf.Lerp(46, 21, Mathf.Abs(sector)/(Mathf.PI/5));
            return dx*dx+dy*dy < radius*radius ? Gold : Color.clear;
        });
        routeIcon = DrawIcon("Distance", (x,y) => {
            bool path = Mathf.Abs(x - (44 + y*0.32f)) < 8 && y>25 && y<103;
            bool dots = (x-44)*(x-44)+(y-25)*(y-25)<180 || (x-77)*(x-77)+(y-103)*(y-103)<180;
            return path || dots ? Teal : Color.clear;
        });
        const string fontPath = "Assets/Fonts/Fredoka/Fredoka-SemiBold SDF.asset";
        face = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if (face != null && (face.material == null || face.atlasTextures.Any(t => t == null)))
        {
            AssetDatabase.DeleteAsset(fontPath);
            face = null;
        }
        if (face == null)
        {
            AssetDatabase.ImportAsset("Assets/Fonts/Fredoka/Fredoka-SemiBold.ttf");
            var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Fredoka/Fredoka-SemiBold.ttf");
            face = TMP_FontAsset.CreateFontAsset(font);
            face.name = "Fredoka SemiBold SDF";
            AssetDatabase.CreateAsset(face, fontPath);
        }
        string glyphs = string.Concat(Enumerable.Range(32, 224).Select(n => (char)n));
        if (!face.TryAddCharacters(glyphs, out string missing) && !string.IsNullOrEmpty(missing))
            Debug.Log("Font excludes unused control characters: " + missing.Length);
        if (!face.HasCharacters("ÁÉÍÓÚÜÑáéíóúüñ¿¡0123456789")) throw new Exception("Spanish glyphs missing.");
        EditorUtility.SetDirty(face);
        if (!AssetDatabase.Contains(face.material)) AssetDatabase.AddObjectToAsset(face.material, face);
        foreach (var texture in face.atlasTextures)
            if (texture != null && !AssetDatabase.Contains(texture)) AssetDatabase.AddObjectToAsset(texture, face);
        EditorUtility.SetDirty(face.material);
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory("Assets/Resources/Licenses");
        File.Copy("Assets/Fonts/Fredoka/OFL.txt", "Assets/Resources/Licenses/Fredoka-OFL.txt", true);
    }

    public static void Interface()
    {
        var gm = Open(); ResourcesUI();
        var score = UnityEngine.Object.FindAnyObjectByType<Puntuacion>();
        var metrics = UnityEngine.Object.FindAnyObjectByType<MetricasPartida>();
        foreach (var window in gm.UIManager.UIWindows)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(window.gameObject))
                PrefabUtility.UnpackPrefabInstance(PrefabUtility.GetOutermostPrefabInstanceRoot(window.gameObject), PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            if (window.CanvasGroup.transform.Find("SafeLayout") != null) continue;
            foreach (var responsive in window.GetComponentsInChildren<ResponsiveElement>(true)) UnityEngine.Object.DestroyImmediate(responsive);
            var canvas = window.CanvasRectTransform.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = window.Id == "MenuUI" ? 20 : 30;
            var scaler = Ensure<CanvasScaler>(canvas.gameObject); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080,1920); scaler.matchWidthOrHeight = 0.5f;
            var group = (RectTransform)window.CanvasGroup.transform;
            Place(group, canvas.transform, 0,0,1,1);
            var oldChildren = group.Cast<Transform>().ToArray();
            var texts = window.GetComponentsInChildren<TMP_Text>(true).Where(t => t.GetComponentInParent<Button>() == null).ToArray();
            var buttons = window.GetComponentsInChildren<Button>(true);
            var sliders = window.GetComponentsInChildren<Slider>(true);
            var shade = Picture("BackdropShade", group, null, new Color(0.025f,0.12f,0.2f,window.Id == "MenuUI" ? 0.08f : 0.42f));
            Place(shade.rectTransform, group, 0,0,1,1); shade.raycastTarget = true;
            var safe = Rect("SafeLayout", group); Place(safe,group,0,0,1,1); safe.gameObject.AddComponent<SafeAreaFitter>();
            var panel = Picture("Card", safe, rounded, new Color(0.96f,0.99f,1,0.98f)).rectTransform;
            panel.anchorMin = panel.anchorMax = new Vector2(0.5f,0.5f);
            var adaptive = panel.gameObject.AddComponent<AdaptivePanel>();
            adaptive.preferredSize = new Vector2(740, window.Id == "MenuUI" ? 1000 : 940);
            var panelShadow = panel.gameObject.AddComponent<Shadow>(); panelShadow.effectColor = new Color(0,0.12f,0.2f,0.3f); panelShadow.effectDistance = new Vector2(0,-12);
            foreach (var t in texts) { t.transform.SetParent(panel, false); Style(t,32,Ink); }
            foreach (var b in buttons) b.transform.SetParent(panel,false);
            foreach (var slider in sliders) slider.transform.SetParent(panel,false);
            foreach (var child in oldChildren) child.gameObject.SetActive(false);
            TMP_Text Named(string name) => texts.First(t => t.name == name);
            Button Btn(string name) => buttons.First(b => b.name == name);
            switch (window.Id)
            {
                case "MenuUI":
                    TextAt(Named("Game_Text"),panel,"TapWing",.80f,.96f,112);
                    Named("Game_Text").color = Teal;
                    TextAt(Named("Instrucciones_Text"),panel,"Mantén para subir · Suelta para bajar",.71f,.80f,30);
                    TextAt(Named("Record_Text (1)"),panel,"MEJOR VUELO",.65f,.71f,24);
                    score.textoRecordMenu = texts.First(t => t.text == "0000");
                    TextAt(score.textoRecordMenu,panel,"0",.58f,.66f,46);
                    TextAt(metrics.textoTotalMonedas,panel,null,.51f,.58f,27);
                    ButtonAt(Btn("Button_Jugar"),panel,"¡A VOLAR!",.36f,.48f,true);
                    ButtonAt(Btn("Button_Configuracion"),panel,"Configuración",.22f,.32f);
                    ButtonAt(Btn("Button_Creditos"),panel,"Créditos",.08f,.18f);
                    break;
                case "PausaUI":
                    TextAt(Named("Pausa_Text"),panel,"En pausa",.80f,.95f,76);
                    TextAt(Named("Descripcion"),panel,"Tu vuelo te espera",.68f,.79f,32);
                    ButtonAt(Btn("Button_Continuar"),panel,"Continuar",.49f,.62f,true);
                    ButtonAt(Btn("Button_Reiniciar"),panel,"Reiniciar",.31f,.44f);
                    ButtonAt(Btn("Button_Menu"),panel,"Volver al menú",.13f,.26f);
                    break;
                case "DerrotaUI":
                    TextAt(Named("Derrota_Text"),panel,"Fin del vuelo",.83f,.95f,72);
                    TextAt(Named("Descripcion"),panel,"¡La ciudad te espera otra vez!",.74f,.83f,28);
                    TextAt(score.textoPuntosDerrota,panel,null,.63f,.73f,38,.05f,.49f);
                    TextAt(score.textoRecord,panel,null,.63f,.73f,38,.51f,.95f);
                    TextAt(metrics.textoResumenDerrota,panel,null,.34f,.60f,34);
                    metrics.textoResumenDerrota.lineSpacing = 16;
                    ButtonAt(Btn("Button_Reintentar"),panel,"Volver a volar",.18f,.30f,true);
                    ButtonAt(Btn("Button_Menu"),panel,"Menú principal",.04f,.15f);
                    break;
                case "SettingsUI":
                    foreach(var extra in texts.Where(t => t.name != "Configuracion_Text" && t.name != "Musica_Text" && t.name != "SFX_Text" && t.name != "Vibracion_Text")) extra.gameObject.SetActive(false);
                    TextAt(Named("Configuracion_Text"),panel,"Configuración",.83f,.96f,66);
                    TextAt(Named("Musica_Text"),panel,"Música",.69f,.77f,34);
                    TextAt(Named("SFX_Text"),panel,"Efectos de sonido",.48f,.56f,34);
                    TextAt(Named("Vibracion_Text"),panel,"Vibración",.27f,.35f,34);
                    var settings = new SerializedObject(window);
                    var music = (Slider)settings.FindProperty("_volumeSlider").objectReferenceValue;
                    var sfx = (Slider)settings.FindProperty("_sfxSlider").objectReferenceValue;
                    SliderAt(music,panel,.59f,.68f); SliderAt(sfx,panel,.38f,.47f);
                    foreach (var slider in sliders.Where(s=>s!=music && s!=sfx)) SliderAt(slider,panel,.18f,.27f);
                    ButtonAt(buttons[0],panel,"Volver",.04f,.15f);
                    break;
                case "CreditosUI":
                    TextAt(Named("Creditos_Text"),panel,"Créditos",.82f,.95f,76);
                    TextAt(Named("Game_Text"),panel,"TapWing",.69f,.81f,56);
                    TextAt(Named("Equipo_Text"),panel,null,.57f,.68f,32);
                    TextAt(Named("Equipo_Text (1)"),panel,null,.42f,.57f,40);
                    var license = Label("FontCredit",panel,"Tipografía: Fredoka\nSIL Open Font License 1.1",24);
                    TextAt(license,panel,null,.26f,.40f,24);
                    ButtonAt(buttons[0],panel,"Volver",.08f,.21f);
                    break;
                default:
                    for(int i=0;i<texts.Length;i++) TextAt(texts[i],panel,"¿Quieres salir?",.63f,.85f,48);
                    for(int i=0;i<buttons.Length;i++) ButtonAt(buttons[i],panel,i==0?"Sí":"No",.32f-i*.17f,.46f-i*.17f,i==0);
                    break;
            }
            adaptive.Refresh();
        }
        Hud(gm,score,metrics);
        score.ActualizarTextos(); metrics.ActualizarTextos();
        Save("FREDOKA_UI");
    }

    static void SliderAt(Slider slider, Transform panel, float y0,float y1)
    {
        Place((RectTransform)slider.transform,panel,.13f,y0,.87f,y1);
        foreach(var image in slider.GetComponentsInChildren<Image>(true))
        {
            image.sprite = rounded; image.type = Image.Type.Sliced;
            image.color = image == slider.targetGraphic ? Gold : (image.name.Contains("Fill") ? Teal : new Color(.77f,.87f,.9f));
        }
    }
    static void Hud(GameManager gm,Puntuacion score,MetricasPartida metrics)
    {
        var canvas = gm.gameplayUI.GetComponentInChildren<Canvas>(true);
        if(canvas.transform.Find("SafeHUD") != null) return;
        var scaler = Ensure<CanvasScaler>(canvas.gameObject); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080,1920); scaler.matchWidthOrHeight = .5f;
        canvas.sortingOrder = 10;
        var pause = canvas.GetComponentInChildren<Button>(true);
        foreach(var responsive in canvas.GetComponentsInChildren<ResponsiveElement>(true)) UnityEngine.Object.DestroyImmediate(responsive);
        var safe = Rect("SafeHUD",canvas.transform); Place(safe,canvas.transform,0,0,1,1); safe.gameObject.AddComponent<SafeAreaFitter>();
        var row = Rect("Counters",safe); row.anchorMin = new Vector2(0,1); row.anchorMax = Vector2.one;
        row.pivot = new Vector2(.5f,1); row.offsetMin = new Vector2(24,-132); row.offsetMax = new Vector2(-144,-24);
        TMP_Text[] values = {score.textoPuntos, metrics.textoMonedas, metrics.textoDistancia};
        string[] names = {"PUNTOS","MONEDAS","DISTANCIA"};
        Sprite[] icons = {starIcon,AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/GoldCoin.png"),routeIcon};
        for(int i=0;i<3;i++)
        {
            var card = Picture(names[i],row,rounded,new Color(.96f,.99f,1,.92f));
            Place(card.rectTransform,row,i/3f+.006f,0,(i+1)/3f-.006f,1);
            var icon = Picture("Icon",card.transform,icons[i],Color.white); icon.preserveAspect=true;
            Place(icon.rectTransform,card.transform,.04f,.22f,.27f,.78f);
            var caption=Label("Caption",card.transform,names[i],20); Place(caption.rectTransform,card.transform,.28f,.64f,.96f,.94f);
            Place(values[i].rectTransform,card.transform,.28f,.04f,.96f,.68f); Style(values[i],42,Ink);
            if(i<2) Ensure<ValuePulse>(values[i].gameObject);
        }
        var pauseRect=(RectTransform)pause.transform; pauseRect.SetParent(safe,false);
        pauseRect.anchorMin=pauseRect.anchorMax=Vector2.one; pauseRect.pivot=Vector2.one;
        pauseRect.anchoredPosition=new Vector2(-24,-24); pauseRect.sizeDelta=new Vector2(104,108);
        foreach(var t in pause.GetComponentsInChildren<TMP_Text>(true)) UnityEngine.Object.DestroyImmediate(t.gameObject);
        var pauseImage=pause.GetComponent<Image>(); pauseImage.sprite=rounded;pauseImage.type=Image.Type.Sliced;pauseImage.color=Teal;
        var pauseGraphic=Picture("PauseIcon",pause.transform,pauseIcon,Color.white);
        Place(pauseGraphic.rectTransform,pause.transform,.07f,.07f,.93f,.93f);
        Ensure<ButtonFeedback>(pause.gameObject);
        foreach(var image in canvas.GetComponentsInChildren<Image>(true))
            if(!image.transform.IsChildOf(safe)) image.enabled=false;
    }
}
