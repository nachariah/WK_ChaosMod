using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ChaosMod.UI
{
    public class ChaosUI : MonoBehaviour
    {
        public static ChaosUI instance;
        public TMP_FontAsset ticketingFont = null;

        private GameObject canvasObj;
        private RectTransform bgRect;
        private RectTransform fillRect;
        private RectTransform listRoot;

        private readonly List<EventEntry> entries = new List<EventEntry>();
        public static List<Toggle> EventToggles = new List<Toggle>();

        public static void ShowUI()
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ChaosTimer");
                instance = go.AddComponent<ChaosUI>();
                instance.CreateUI();
            }
        }
        private void CreateUI()
        {
            canvasObj = new GameObject();
            canvasObj.transform.parent = transform;
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            //BG
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.parent = canvasObj.transform;
            Image BG = bgObj.AddComponent<Image>();
            BG.color = new Color(0f, 0f, 0f, 0.3f);

            bgRect = BG.rectTransform;
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.right;
            bgRect.pivot = Vector2.right / 2;
            bgRect.sizeDelta = new Vector2(0, 4f);
            bgRect.anchoredPosition = new Vector2(0f, 0f);

            //Fill
            GameObject fillObj = new GameObject("Background");
            fillObj.transform.parent = bgObj.transform;
            Image fill = fillObj.AddComponent<Image>();
            fill.color = new Color(1f,1f,1f,0.6f);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = (int)Image.OriginHorizontal.Left;
            fill.fillAmount = 0f;

            fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.up;
            fillRect.pivot = Vector2.up/2;
            fillRect.sizeDelta = new Vector2(bgRect.rect.width, 0f);
            fillRect.anchoredPosition = Vector2.zero;

            //Entries
            GameObject listObj = new GameObject("EventList");
            listObj.transform.SetParent(canvasObj.transform, false);
            listRoot = listObj.AddComponent<RectTransform>();

            listRoot.anchorMin = new Vector2(1f, 0f);
            listRoot.anchorMax = new Vector2(1f, 0f);
            listRoot.pivot = new Vector2(1f, 0f);
            listRoot.sizeDelta = new Vector2(100f, 0f);
            listRoot.anchoredPosition = new Vector2(-4f, 25f);

            ticketingFont = FindTicketingFont();
        }
        public void SetTimer(float value)
        {
            if (fillRect == null || bgRect == null)
                return;

            value = Mathf.Clamp01(value);

            float fullWidth = bgRect.rect.width;
            fillRect.sizeDelta = new Vector2(fullWidth * value, 0f);
        }
        public void AddEntry(string name, float time)
        {
            if (listRoot == null) return;

            GameObject entryObj = new GameObject(name);
            entryObj.transform.SetParent(listRoot, false);

            EventEntry entry = entryObj.AddComponent<EventEntry>();
            entry.Wake(name, time, this);

            entries.Insert(0, entry);
            RepositionEntries();
        }
        public void RemoveEntry(EventEntry entry)
        {
            if (entries.Remove(entry))
                RepositionEntries();
        }
        private void RepositionEntries()
        {
            float y = ChaosSettings.loggerYOffset;

            for (int i = 0; i < entries.Count; i++)
            {
                entries[i].SetTargetPosition(new Vector2(0f, y));
                y += entries[i].Height + 6f;
            }
        }
        private static TMP_FontAsset FindTicketingFont()
        {
            foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (font.name == "Ticketing SDF")
                    return font;
            }

            Debug.LogWarning("[Chaos - UI] Ticketing SDF font not found");
            return null;
        }
        public static void CreateMainMenuText()
        {
            GameObject root = new GameObject("ChaosMainMenuUI");

            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            root.AddComponent<CanvasScaler>().uiScaleMode =CanvasScaler.ScaleMode.ScaleWithScreenSize;

            root.AddComponent<GraphicRaycaster>();

            TMP_FontAsset font = FindTicketingFont();

            // Version
            CreateText(root.transform,$"CHAOS MOD v{Plugin.pluginVersion}",font,14,TextAlignmentOptions.TopRight,new Vector2(1, 1),new Vector2(-5, -30),new Vector2(250, 30));

            // Button
            Button btn = CreateButton(root.transform,"Chaos Options",font, new Vector2(1, 0),new Vector2(-120, 10),new Vector2(80, 16));

            GameObject panel = CreateOptionsPanel(root.transform, font);
            panel.SetActive(false);

            btn.onClick.AddListener(() =>
            {
                panel.SetActive(!panel.activeSelf);
            });
        }
        private static GameObject CreateOptionsPanel(Transform parent, TMP_FontAsset font)
        {
            GameObject panel = new GameObject("ChaosOptionsPanel");
            panel.transform.SetParent(parent, false);

            Image bg = panel.AddComponent<Image>();
            bg.color = new Color(0, 0, 0, 0.75f);

            RectTransform rect = bg.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(1, 0);
            rect.sizeDelta = new Vector2(360, 420);
            rect.anchoredPosition = new Vector2(-10, 30);

            // Easy Mode Toggle
            CreateToggle(
                panel.transform,
                "Easy Mode",
                font,
                new Vector2(10, -12.5f),
                new Vector2(330, 20),
                ChaosSettings.easyMode,
                v =>
                {
                    ChaosSettings.easyMode = v;
                    ChaosSettings.Save();
                });

            //Volume Slider
            TMP_Text vLabel = CreateText(
                panel.transform,
                $"Chaos Volume: {Mathf.RoundToInt(ChaosSettings.chaosVolume * 100f)}%",
                font,
                11,
                TextAlignmentOptions.Left,
                new Vector2(0, 1),
                new Vector2(10, -35),
                new Vector2(240, 18));

            CreateSlider(
                panel.transform,
                new Vector2(10, -55),
                new Vector2(330, 8),
                0f,
                1f,
                ChaosSettings.chaosVolume,
                v =>
                {
                    ChaosSettings.chaosVolume = v;
                    ChaosSettings.Save();

                    vLabel.text = $"Chaos Volume: {Mathf.RoundToInt(v * 100f)}%";
                });

            // Logger Offset Slider
            TMP_Text yLabel = CreateText(
                panel.transform,
                $"Logger Vertical Offset: {ChaosSettings.loggerYOffset}px",
                font,
                11,
                TextAlignmentOptions.Left,
                new Vector2(0, 1),
                new Vector2(10, -70),
                new Vector2(200, 18));

            CreateSlider(
                panel.transform,
                new Vector2(10, -90),
                new Vector2(330, 8),
                0,      // minimum
                400,    // maximum
                ChaosSettings.loggerYOffset,
                v =>
                {
                    ChaosSettings.loggerYOffset = v;
                    yLabel.text = $"Logger Vertical Offset: {v}px";
                    ChaosSettings.Save();
                });

            // Enabled Events Label
            CreateText(
                panel.transform,
                "Enabled Events:",
                font,
                11,
                TextAlignmentOptions.Left,
                new Vector2(0, 1),
                new Vector2(10, -110),
                new Vector2(200, 18));

            // Scroll Rect
            RectTransform content;
            ScrollRect scroll = CreateScrollRect(
                panel.transform,
                new Vector2(10, 60),
                new Vector2(340, 230),
                out content);

            // Event Toggles
            EventToggles.Clear();

            CreateBlankToggle(content,"",Vector2.zero, new Vector2(320, 18));

            foreach (var key in ChaosSettings.eventEnabled.Keys)
            {
                string eventName = key;

                Toggle t = CreateToggle(
                    content,
                    eventName,
                    font,
                    Vector2.zero,
                    new Vector2(320, 15),
                    ChaosSettings.eventEnabled[eventName],
                    v =>
                    {
                        ChaosSettings.eventEnabled[eventName] = v;
                        ChaosSettings.Save();
                    });

                EventToggles.Add(t);
            }

            CreateBlankToggle(content, "", Vector2.zero, new Vector2(320, 18));

            // Enable / Disable All Buttons
            Button enableAll = CreateButton(
                panel.transform,
                "Enable All",
                font,
                new Vector2(0, 0),
                new Vector2(10, 10),
                new Vector2(150, 28));

            enableAll.onClick.AddListener(() =>
            {
                ChaosUIHelpers.SetAllToggles(true);
                ChaosSettings.Save();
            });

            Button disableAll = CreateButton(
                panel.transform,
                "Disable All",
                font,
                new Vector2(1, 0),
                new Vector2(-10, 10),
                new Vector2(150, 28));

            disableAll.onClick.AddListener(() =>
            {
                ChaosUIHelpers.SetAllToggles(false);
                ChaosSettings.Save();
            });

            return panel;
        }
        public static TMP_Text CreateText(Transform parent,string text,TMP_FontAsset font,float size,TextAlignmentOptions alignment,Vector2 anchor,Vector2 pos,Vector2 dimensions)
        {
            GameObject go = new GameObject("Text");
            go.transform.SetParent(parent, false);

            RectTransform r = go.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = anchor;
            r.anchoredPosition = pos;
            r.sizeDelta = dimensions;

            TMP_Text t = go.AddComponent<TextMeshProUGUI>();
            t.text = text;
            t.font = font;
            t.fontSize = size;
            t.alignment = alignment;
            t.color = Color.white;

            return t;
        }

        static Button CreateButton(Transform parent, string text, TMP_FontAsset font, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(text);
            go.transform.SetParent(parent, false);

            Image img = go.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.1f);

            Button btn = go.AddComponent<Button>();

            RectTransform r = img.rectTransform;
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = anchor;
            r.anchoredPosition = pos;
            r.sizeDelta = size;

            CreateText(go.transform, text, font, 12,
                TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f),
                Vector2.zero, size);

            return btn;
        }
        static void CreateLabel(Transform parent, string text, TMP_FontAsset font, ref float y)
        {
            CreateText(parent, text, font, 12,
                TextAlignmentOptions.Left,
                new Vector2(0, 1),
                new Vector2(10, y),
                new Vector2(300, 20));
            y -= 20;
        }
        public static Toggle CreateBlankToggle(Transform parent, string label, Vector2 pos, Vector2 size)
        {
            GameObject root = new GameObject(label + "_Toggle");
            root.transform.SetParent(parent, false);

            RectTransform r = root.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0, 1);
            r.pivot = new Vector2(0, 1);
            r.anchoredPosition = pos;
            r.sizeDelta = size;

            Toggle toggle = root.AddComponent<Toggle>();

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(root.transform, false);
            Image bg = bgObj.AddComponent<Image>();
            bg.color = new Color(1, 1, 1, 0f);

            RectTransform bgRT = bg.rectTransform;
            bgRT.anchorMin = bgRT.anchorMax = new Vector2(0, 0.5f);
            bgRT.pivot = new Vector2(0, 0.5f);
            bgRT.sizeDelta = new Vector2(14, 14);
            bgRT.anchoredPosition = new Vector2(4, 0);

            return toggle;
        }
        public static Toggle CreateToggle(Transform parent,string label,TMP_FontAsset font,Vector2 pos,Vector2 size,bool initialValue,Action<bool> onChanged)
        {
            GameObject root = new GameObject(label + "_Toggle");
            root.transform.SetParent(parent, false);

            RectTransform r = root.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0, 1);
            r.pivot = new Vector2(0, 1);
            r.anchoredPosition = pos;
            r.sizeDelta = size;

            Toggle toggle = root.AddComponent<Toggle>();

            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(root.transform, false);
            Image bg = bgObj.AddComponent<Image>();
            bg.color = new Color(1, 1, 1, 0.2f);

            RectTransform bgRT = bg.rectTransform;
            bgRT.anchorMin = bgRT.anchorMax = new Vector2(0, 0.5f);
            bgRT.pivot = new Vector2(0, 0.5f);
            bgRT.sizeDelta = new Vector2(14, 14);
            bgRT.anchoredPosition = new Vector2(4, 0);

            // Checkmark
            GameObject ckObj = new GameObject("Checkmark");
            ckObj.transform.SetParent(bgObj.transform, false);
            Image ck = ckObj.AddComponent<Image>();
            ck.color = Color.white;

            RectTransform ckRT = ck.rectTransform;
            ckRT.anchorMin = Vector2.zero;
            ckRT.anchorMax = Vector2.one;
            ckRT.offsetMin = ckRT.offsetMax = Vector2.zero;

            // Label
            CreateText(
                root.transform,
                label,
                font,
                10,
                TextAlignmentOptions.Left,
                new Vector2(0, 0.5f),
                new Vector2(24, 0),
                new Vector2(size.x - 24, size.y)
            );

            toggle.targetGraphic = bg;
            toggle.graphic = ck;
            toggle.isOn = initialValue;
            toggle.onValueChanged.AddListener(v => onChanged(v));

            return toggle;
        }
        public static ScrollRect CreateScrollRect(Transform parent,Vector2 pos,Vector2 size,out RectTransform content)
        {
            GameObject root = new GameObject("ScrollRect");
            root.transform.SetParent(parent, false);

            RectTransform r = root.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0, 0);
            r.pivot = new Vector2(0, 0);
            r.anchoredPosition = pos;
            r.sizeDelta = size;

            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(root.transform, false);
            RectTransform vpRT = viewport.AddComponent<RectTransform>();
            vpRT.anchorMin = Vector2.zero;
            vpRT.anchorMax = Vector2.one;
            vpRT.offsetMin = vpRT.offsetMax = Vector2.zero;
            viewport.AddComponent<RectMask2D>();

            // Content
            GameObject cont = new GameObject("Content");
            cont.transform.SetParent(viewport.transform, false);
            content = cont.AddComponent<RectTransform>();
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = cont.AddComponent<VerticalLayoutGroup>();
            layout.childControlHeight = true;
            layout.childForceExpandHeight = false;
            layout.spacing = 15;

            ContentSizeFitter fitter = cont.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            ScrollRect sr = root.AddComponent<ScrollRect>();
            sr.viewport = vpRT;
            sr.content = content;
            sr.horizontal = false;

            return sr;
        }

        public static Slider CreateSlider(Transform parent,Vector2 pos,Vector2 size,float min,float max,float value,Action<float> onChanged)
        {
            GameObject root = new GameObject("Slider");
            root.transform.SetParent(parent, false);

            RectTransform r = root.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0, 1);
            r.pivot = new Vector2(0, 1);
            r.anchoredPosition = pos;
            r.sizeDelta = size;

            Slider slider = root.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            slider.onValueChanged.AddListener(v => onChanged(v));

            Image bg = root.AddComponent<Image>();
            bg.color = new Color(1, 1, 1, 0.15f);
            slider.targetGraphic = bg;

            // Fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(root.transform, false);
            Image fill = fillObj.AddComponent<Image>();
            fill.color = Color.white;

            RectTransform fillRT = fill.rectTransform;
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = new Vector2(0, 1);
            fillRT.offsetMin = fillRT.offsetMax = Vector2.zero;
            slider.fillRect = fillRT;

            // Handle
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(root.transform, false);
            Image handle = handleObj.AddComponent<Image>();
            handle.color = Color.white;

            RectTransform handleRT = handle.rectTransform;
            handleRT.sizeDelta = new Vector2(10, size.y);
            slider.handleRect = handleRT;

            return slider;
        }

    }
    static class ChaosUIHelpers
    {
        public static void SetAllToggles(bool value)
        {
            foreach (var t in ChaosUI.EventToggles)
            {
                t.SetIsOnWithoutNotify(value);
                t.onValueChanged.Invoke(value);
            }
        }
    }
    public class EventEntry : MonoBehaviour
    {
        private ChaosUI owner;

        private RectTransform rect;
        private RectTransform timerFill;

        private TextMeshProUGUI label;

        private bool awake = false;

        private float timeLeft = 20f;
        private float eventTimer = 0f;
        private float eventTimerMax = 0f;

        private Vector2 targetPos;
        private float height = 18f;

        public float Height => height;
        public void Wake(string name,float time, ChaosUI ownerUI)
        {
            owner = ownerUI;
            awake = true;

            eventTimerMax = time;
            eventTimer = time;

            CreateEntryUI(name);
        }
        private void CreateEntryUI(string name)
        {
            rect = gameObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.one;
            rect.anchorMax = Vector2.one;
            rect.pivot = Vector2.one;
            rect.sizeDelta = new Vector2(350f,height);
            rect.anchoredPosition = Vector2.zero;

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(transform, false);
            label = textObj.AddComponent<TextMeshProUGUI>();
            label.text = name;
            if (owner.ticketingFont != null)
                label.font = owner.ticketingFont;
            label.fontSize = 10;
            label.alignment = TextAlignmentOptions.MidlineRight;
            label.color = Color.white;

            RectTransform textRect = label.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6f, 2f);
            textRect.offsetMax = new Vector2(-12f, -2f);

            if (eventTimerMax > 0f)
            {
                GameObject barObj = new GameObject("Timer");
                barObj.transform.SetParent(transform, false);
                Image barBG = barObj.AddComponent<Image>();
                barBG.color = new Color(0f,0f,0f,0.6f);

                RectTransform barRect = barBG.rectTransform;
                barRect.anchorMin = Vector2.right;
                barRect.anchorMax = Vector2.one;
                barRect.pivot = new Vector2(1f, 0.5f);
                barRect.sizeDelta = new Vector2(8f, 0f);
                barRect.anchoredPosition = new Vector2(-2f, 0f);

                GameObject fillObj = new GameObject("Fill");
                fillObj.transform.SetParent(barObj.transform, false);
                Image fill = fillObj.AddComponent<Image>();
                fill.color = new Color(1f, 1f, 1f, 0.6f);

                timerFill = fill.rectTransform;
                timerFill.anchorMin = Vector2.zero;
                timerFill.anchorMax = Vector2.right;
                timerFill.pivot = Vector2.right / 2;
                timerFill.sizeDelta = new Vector2(0f, barRect.rect.height);
                timerFill.anchoredPosition = Vector2.zero;
            }
        }
        public void SetTargetPosition(Vector2 pos)
        {
            targetPos = pos;
        }

        void Update()
        {
            if (!awake) return;

            if (timeLeft > 0f)
                timeLeft -= Time.deltaTime;
            if (eventTimer > 0f)
                eventTimer -= Time.deltaTime;

            if (timerFill != null && eventTimerMax > 0f)
            {
                float t = Mathf.Clamp01(eventTimer / eventTimerMax);
                float fullHeight = rect.rect.height;
                timerFill.sizeDelta = new Vector2(0f, fullHeight * t);
            }

            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * 12f);

            if (timeLeft <= 0f && eventTimer <= 0f)
            {
                owner.RemoveEntry(this);
                Destroy(gameObject);
            }
        }
    }
}
