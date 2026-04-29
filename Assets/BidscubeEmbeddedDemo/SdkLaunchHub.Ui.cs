using BidscubeSDK;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public partial class SdkLaunchHub
{
    /// <summary>Same padding, spacing, and alignment as the Direct SDK panel.</summary>
    static void ApplyDirectPanelPageLayout(VerticalLayoutGroup v)
    {
        v.padding = new RectOffset(24, 24, 28, 24);
        v.spacing = 14f;
        v.childAlignment = TextAnchor.UpperCenter;
        v.childControlWidth = true;
        v.childForceExpandWidth = true;
        v.childControlHeight = true;
        v.childForceExpandHeight = false;
    }

    static void DisableSdksForMenu()
    {
        BidscubeSDK.BidscubeSDK.ClearAdViewsParentTransform();
        BidscubeSDK.BidscubeSDK.Cleanup();
        BidscubeSDK.BidscubeSDK.SetInitializationEnabled(false);
    }

    static void StretchFull(RectTransform r)
    {
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.localScale = Vector3.one;
    }

    static GameObject BuildWhiteContentBlock(Transform backdrop, string name)
    {
        var inner = Panel(backdrop, name, Color.white);
        var rt = inner.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(16f, 16f);
        rt.offsetMax = new Vector2(-16f, -16f);
        return inner;
    }

    static void AddSpacer(Transform parent, float h)
    {
        var spacer = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
        spacer.transform.SetParent(parent, false);
        spacer.GetComponent<LayoutElement>().preferredHeight = h;
    }

    static GameObject Panel(Transform parent, string name, Color bg)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        StretchFull(rt);
        var img = go.GetComponent<Image>();
        img.color = bg;
        img.raycastTarget = true;
        return go;
    }

    static GameObject AddTmpTitle(Transform parent, string text, float fontSize, FontStyles style, TextAlignmentOptions align)
    {
        var go = new GameObject("Title (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.color = Color.black;
        tmp.alignment = align;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 44f;
        le.flexibleWidth = 1f;
        return go;
    }

    static void AddDockClearButton(Transform parent, UnityAction onClick)
    {
        var go = new GameObject("Clear slot (Button)", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var le = go.GetComponent<LayoutElement>();
        le.preferredWidth = 44f;
        le.preferredHeight = 36f;
        le.minWidth = 40f;
        var img = go.GetComponent<Image>();
        SetFlatUiGraphicNoBuiltinSprite(img, new Color(0.92f, 0.92f, 0.93f, 1f));
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var labelGo = new GameObject("Label (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        StretchFull(labelGo.GetComponent<RectTransform>());
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = "✕";
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = LauncherBodyText;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    static void AddTmpBody(Transform parent, string text, float fontSize, Color color, float preferredHeight = 72f)
    {
        var go = new GameObject("Description (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var tmp = go.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = FontStyles.Normal;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.TopJustified;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = preferredHeight;
        le.flexibleWidth = 1f;
    }

    static void AddTmpMaxFieldCaption(Transform parent, string text)
    {
        var go = new GameObject("MAX field caption (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var le = go.GetComponent<LayoutElement>();
        le.preferredHeight = 22f;
        le.flexibleWidth = 1f;
        var tmp = go.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = text;
        tmp.fontSize = 15f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = LauncherBodyText;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        tmp.raycastTarget = false;
    }

    static TMP_InputField CreateFlatTmpInput(Transform parent, string placeholderHint, string initialText, float preferredHeight)
    {
        var root = new GameObject("TMP_InputField", typeof(RectTransform), typeof(Image), typeof(TMP_InputField), typeof(LayoutElement));
        root.transform.SetParent(parent, false);
        var le = root.GetComponent<LayoutElement>();
        le.preferredHeight = preferredHeight;
        le.minHeight = 40f;
        le.flexibleWidth = 1f;

        var img = root.GetComponent<Image>();
        SetFlatUiGraphicNoBuiltinSprite(img, new Color(0.96f, 0.96f, 0.97f, 1f));

        var inputField = root.GetComponent<TMP_InputField>();
        inputField.lineType = TMP_InputField.LineType.SingleLine;

        var textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textArea.transform.SetParent(root.transform, false);
        var textAreaRt = textArea.GetComponent<RectTransform>();
        textAreaRt.anchorMin = Vector2.zero;
        textAreaRt.anchorMax = Vector2.one;
        textAreaRt.offsetMin = new Vector2(10, 6);
        textAreaRt.offsetMax = new Vector2(-10, -7);

        var childPlaceholder = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
        childPlaceholder.transform.SetParent(textArea.transform, false);
        var childText = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        childText.transform.SetParent(textArea.transform, false);

        for (var i = 0; i < textArea.transform.childCount; i++)
        {
            var ch = textArea.transform.GetChild(i);
            var r = ch.GetComponent<RectTransform>();
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.sizeDelta = Vector2.zero;
            r.offsetMin = Vector2.zero;
            r.offsetMax = Vector2.zero;
        }

        var text = childText.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(text);
        text.text = initialText ?? "";
        text.fontSize = 16f;
        text.color = LauncherBodyText;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.extraPadding = true;

        var placeholder = childPlaceholder.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(placeholder);
        placeholder.text = placeholderHint;
        placeholder.fontSize = 16f;
        placeholder.fontStyle = FontStyles.Italic;
        placeholder.textWrappingMode = TextWrappingModes.NoWrap;
        placeholder.extraPadding = true;
        var phc = LauncherBodyText;
        phc.a *= 0.45f;
        placeholder.color = phc;
        placeholder.raycastTarget = false;
        var ign = placeholder.gameObject.AddComponent<LayoutElement>();
        ign.ignoreLayout = true;

        inputField.textViewport = textAreaRt;
        inputField.textComponent = text;
        inputField.placeholder = placeholder;
        inputField.text = initialText ?? "";

        return inputField;
    }

    static void WireMaxPrefsOnEndEdit(TMP_InputField field, string prefsKey)
    {
        if (field == null)
            return;
        field.onEndEdit.AddListener(_ =>
        {
            PlayerPrefs.SetString(prefsKey, field.text != null ? field.text.Trim() : "");
            PlayerPrefs.Save();
        });
    }

    static void ApplyDefaultTmpFont(TextMeshProUGUI tmp)
    {
        if (TMP_Settings.defaultFontAsset != null)
            tmp.font = TMP_Settings.defaultFontAsset;
    }

    static void AddSdkStylePrimaryButton(Transform parent, string label, UnityAction onClick)
    {
        var go = new GameObject(label + " (Button)", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        StyleSdkPrimaryButtonGraphic(img);
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        var colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f);
        colors.pressedColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 0.5019608f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.onClick.AddListener(onClick);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 80f;
        le.minHeight = 56f;

        var labelGo = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        StretchFull(labelGo.GetComponent<RectTransform>());
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = label;
        tmp.fontSize = 24f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 12f;
        tmp.fontSizeMax = 30f;
        tmp.raycastTarget = false;
    }

    static void AddSdkStyleSecondaryButton(Transform parent, string label, UnityAction onClick)
    {
        var go = new GameObject(label + " (Button)", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        SetFlatUiGraphicNoBuiltinSprite(img, Color.white);

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        var colors = btn.colors;
        colors.normalColor = new Color(0f, 0.45882353f, 0.9607843f, 1f);
        colors.highlightedColor = new Color(0.9607843f, 0.9607843f, 0.9607843f, 1f);
        colors.pressedColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 1f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.78431374f, 0.78431374f, 0.78431374f, 0.5019608f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;
        btn.colors = colors;
        btn.navigation = new Navigation { mode = Navigation.Mode.None };
        btn.onClick.AddListener(onClick);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 56f;

        var labelGo = new GameObject("Text (TMP)", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelGo.transform.SetParent(go.transform, false);
        StretchFull(labelGo.GetComponent<RectTransform>());
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        ApplyDefaultTmpFont(tmp);
        tmp.text = label;
        tmp.fontSize = 22f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
    }

    static void StyleSdkPrimaryButtonGraphic(Image img)
    {
        SetFlatUiGraphicNoBuiltinSprite(img, SdkPrimaryBlue);
    }

    static void SetFlatUiGraphicNoBuiltinSprite(Image img, Color color)
    {
        img.sprite = null;
        img.type = Image.Type.Simple;
        img.color = color;
        img.raycastTarget = true;
    }
}
