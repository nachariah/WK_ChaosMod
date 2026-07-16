using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DarkMachine.UI;
using System.ComponentModel.Design.Serialization;
using UnityEngine.SceneManagement;
using ChaosMod.Events;
using System.Runtime.Remoting.Contexts;
using System.Collections;

namespace ChaosMod.UI
{
    public class ChaosUI : MonoBehaviour
    {
        public static ChaosUI instance;
        public TMP_FontAsset ticketingFont = null;

        private static GameObject chaosSettingsPage;
        private static List<Transform> contentColumns = new List<Transform>();

        private static GameObject toggleTemplate;
        private static GameObject sliderTemplate;
        private static GameObject textTemplate;
        private static GameObject dropdownTemplate;
        private static GameObject buttonTemplate;

        public static Dictionary<string,string> nameTruncates = new Dictionary<string,string>();

        private GameObject canvasObj;
        private RectTransform bgRect;
        private RectTransform fillRect;
        private RectTransform listRoot;

        private static GameObject root = null;

        private readonly List<EventEntry> entries = new List<EventEntry>();
        public static List<Toggle> EventToggles = new List<Toggle>();

        private static Dictionary<int, string> difficultyDict = null;

        //Ingame
        public static void ShowUI()
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ChaosTimer");
                instance = go.AddComponent<ChaosUI>();
                instance.CreateUI();

                if (root != null)
                    root.SetActive(false);
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
        public EventEntry AddEntry(string name, float time, bool doubleEvent)
        {
            if (listRoot == null) return null;

            GameObject entryObj = new GameObject(name);
            entryObj.transform.SetParent(listRoot, false);

            EventEntry entry = entryObj.AddComponent<EventEntry>();
            entry.Wake(name, time, this, doubleEvent);

            entries.Insert(0, entry);
            RepositionEntries();

            return entry;
        }
        public void RemoveEntry(EventEntry entry)
        {
            if (entries.Remove(entry))
                RepositionEntries();
        }
        public void RemoveAllEntries()
        {
            foreach (EventEntry entry in entries)
            {
                Destroy(entry.gameObject);
            }
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
        private static void LoadNameTruncates()
        {
            nameTruncates["You are playing in IRON KNUCKLE mode. No perks for you!"] = "IRON KNUCKLE MODE";
            nameTruncates["Will you be my buddy?"] = "Be my buddy?";
            nameTruncates["I'll take that, it's mine now"] = "I'll Take That";
            nameTruncates["Give up, you're surrounded"] = "You're Surrounded";
        }
        private static TMP_FontAsset FindTicketingFont()
        {
            foreach (var font in Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (font.name == "Ticketing SDF")
                    return font;
            }

            Debug.LogWarning("[Chaos - FindTicketingFont] Ticketing SDF font not found");
            return null;
        }
        public static void SetEndScreens()
        {
            GameObject winTipObj = GameObject.Find("GameManager/Canvas/Score Screen/ScorePanel_Standard_Win(Clone)/Score Screen Root/Tip");
            GameObject loseTipObj = GameObject.Find("GameManager/Canvas/Score Screen/ScorePanel_Standard_Death(Clone)/Score Screen Root/Tip");

            string tipText = "Chaos Mod";

            if (difficultyDict == null)
            {
                difficultyDict = new Dictionary<int, string>();
                difficultyDict.Add(0, "Easy");
                difficultyDict.Add(1, "Normal");
                difficultyDict.Add(2, "Hard");
                difficultyDict.Add(3, "IMPOSSIBLE");
            }

            if (ChaosSettings.customTimer)
                tipText += " | " + ChaosSettings.customTimerValue.ToString("0.##") + "s Timer";
            else if (difficultyDict.TryGetValue(ChaosSettings.difficulty, out string diffTxt))
                tipText += " | " + diffTxt;


            if (EventManager.Events.Count == 0)
                EventManager.FillList();

            List<Events.Event> valid = EventManager.Events.Where(e => ChaosSettings.eventEnabled.TryGetValue(e.name, out bool on) && on).ToList();

            if (EventManager.Events.Count != valid.Count)
            {
                tipText += " | " + valid.Count + " Event";
                if (valid.Count != 1)
                    tipText += "s";
            }

            if (winTipObj != null)
            {
                TextMeshProUGUI text = winTipObj.GetComponent<TextMeshProUGUI>();
                text.text = tipText;
            }

            if (loseTipObj != null)
            {
                TextMeshProUGUI text = loseTipObj.GetComponent<TextMeshProUGUI>();
                text.text = tipText;
                text.fontSize = 24;
                text.fontSizeMax = 24;
            }
        }
        public void FlashScreen(Color color, float duration = 0.15f)
        {
            if (canvasObj == null)
                return;

            GameObject flashObj = new GameObject("ScreenFlash");
            flashObj.transform.SetParent(canvasObj.transform, false);

            Image flash = flashObj.AddComponent<Image>();
            flash.color = color;

            RectTransform rect = flash.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            StartCoroutine(FlashRoutine(flash, duration));
        }

        private IEnumerator FlashRoutine(Image flash, float duration)
        {
            Color start = flash.color;

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;

                Color c = start;
                c.a = Mathf.Lerp(start.a, 0f, t / duration);
                flash.color = c;

                yield return null;
            }

            Destroy(flash.gameObject);
        }
        //Main Menu
        public static void LoadMenuMenu()
        {
            if (root == null)
            {
                root = new GameObject("ChaosMainMenuUI");
                DontDestroyOnLoad(root);

                Canvas canvas = root.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1;

                root.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                root.AddComponent<GraphicRaycaster>();

                TMP_FontAsset font = FindTicketingFont();

                // Version
                CreateText(root.transform, $"CHAOS MOD v{Plugin.pluginVersion}", font, 14, TextAlignmentOptions.TopRight, new Vector2(1, 1), new Vector2(-5, -30), new Vector2(250, 30));

                LoadNameTruncates();
            }

            root.SetActive(true);

            // Button
            GameObject supportMenu = GameObject.Find("Canvas - Main Menu/Main Menu/Support Menu/");
            GameObject buttonObj = Instantiate(supportMenu.transform.Find("Update Info").gameObject);
            buttonObj.transform.parent = supportMenu.transform;
            buttonObj.transform.SetAsFirstSibling();
            buttonObj.transform.localScale = Vector3.one;

            buttonObj.GetComponent<Image>().color = new Color(0.5f,0.5f,1f,1f);

            TextMeshProUGUI text = buttonObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            text.text = "Chaos Settings";
            text.color = new Color(1f, 1f, 1f, 0.25f);

            ColorBlock newcolors = new ColorBlock();
            newcolors.normalColor = new Color(1f, 1f, 1f, 0.75f);
            newcolors.highlightedColor = new Color(0.2406f, 0.2406f, 1f, 0.9f);
            newcolors.pressedColor = new Color(0.0991f, 0.0991f, 1f, 1f);
            newcolors.selectedColor = new Color(0f, 0f, 1f, 1f);
            newcolors.disabledColor = new Color(0.7843f, 0.7843f, 0.7843f, 0.502f);
            newcolors.colorMultiplier = 1f;
            newcolors.fadeDuration = 0.1f;

            Button button = buttonObj.GetComponent<Button>();
            button.colors = newcolors;

            if (root.transform.Find("Chaos Settings") == null)
            {
                CreateOptionsPanel(root.transform);
            }

            GameObject page = root.transform.Find("Chaos Settings").gameObject;
            page.SetActive(true);

            UI_MenuScreen uiScreen = page.GetComponent<UI_MenuScreen>();

            button.onClick.AddListener(() =>
            {
                uiScreen.Open();
            });
        }
        private static GameObject CreateOptionsPanel(Transform root)
        {
            Transform original = GameObject.Find("Canvas - Screens/Screens/Canvas - Screen - Settings/Settings Menu/SettingsParent/Settings Pane").transform;

            GameObject page = Instantiate(original.gameObject, root);

            page.SetActive(false);
            page.name = "Chaos Settings";
            page.transform.localScale = Vector3.one * 0.37f;

            chaosSettingsPage = page;

            PreparePage(page);

            return page;
        }
        private static void PreparePage(GameObject page)
        {
            Transform tab = page.transform.Find("Video Settings").Find("Main Panel").Find("Tab - Video");

            contentColumns.Add(tab.Find("Column - Video"));
            contentColumns.Add(tab.Find("Column - 2 Other"));
            contentColumns.Add(tab.Find("Column - 3"));

            page.transform.Find("Overview Titles").Find("Title Text").GetComponent<TextMeshProUGUI>().text = "CHAOS SETTINGS";

            toggleTemplate = contentColumns[0].Find("Vsync Toggle").gameObject;
            sliderTemplate = contentColumns[0].Find("SliderAsset - FOV").gameObject;
            textTemplate = contentColumns[0].Find("Video Settings").gameObject;
            dropdownTemplate = contentColumns[0].Find("Screen Resolution").gameObject;
            buttonTemplate = page.transform.Find("Video Settings").Find("Controls Page Tab Selector").Find("Video").gameObject;

            page.transform.Find("Tab Selection Hor").gameObject.SetActive(false);
            page.transform.Find("Video Settings").Find("Controls Page Tab Selector").gameObject.SetActive(false);
            page.transform.Find("Video Settings").Find("Tab Title").gameObject.SetActive(false);
            Destroy(page.transform.Find("Video Settings").Find("Main Panel").Find("Tab - Audio").gameObject);

            foreach (Transform child in contentColumns[0])
                Destroy(child.gameObject);
            foreach (Transform child in contentColumns[1])
                Destroy(child.gameObject);
            foreach (Transform child in contentColumns[2])
                Destroy(child.gameObject);

            UI_LerpOpen uilerp = page.GetComponent<UI_LerpOpen>();
            Destroy(uilerp);
            page.transform.localPosition = Vector3.zero;
            uilerp = page.AddComponent<UI_LerpOpen>();
            uilerp.startHidden = true;
            uilerp.startPosition = new Vector3(3000, 0, 0);
            uilerp.affectScale = false;
            uilerp.affectPosition = true;
            uilerp.hideOpposite = true;
            uilerp.positionLerpTime = 0.6f;
            uilerp.easeOut = DG.Tweening.Ease.InCubic;
            uilerp.easeIn = DG.Tweening.Ease.OutCubic;

            FieldInfo screenMenu = typeof(UI_MenuScreen).GetField("menu", BindingFlags.NonPublic | BindingFlags.Instance);

            UI_MenuScreen uiScreen = page.AddComponent<UI_MenuScreen>();
            uiScreen.openEvent = new UnityEngine.Events.UnityEvent();
            uiScreen.closeEvent = new UnityEngine.Events.UnityEvent();
            uiScreen.openEvent.AddListener(uilerp.Show);
            uiScreen.closeEvent.AddListener(uilerp.Hide);

            screenMenu.SetValue(uiScreen, GameObject.Find("Canvas - Main Menu/Main Menu").GetComponent<UI_Menu>());

            page.transform.Find("Save And Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                uiScreen.CloseScreen();
            });

            BuildChaosSettings();
        }
        private static void BuildChaosSettings()
        {
            CloneText(contentColumns[0], "Timer");

            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            options.Add(new TMP_Dropdown.OptionData("Easy (20s)"));
            options.Add(new TMP_Dropdown.OptionData("Normal (10s)"));
            options.Add(new TMP_Dropdown.OptionData("Hard (5s)"));
            options.Add(new TMP_Dropdown.OptionData("IMPOSSIBLE (2s)"));

            TMP_Dropdown difficultydropdown = CloneDropdown(contentColumns[0], "Difficulty", options, ChaosSettings.difficulty,
                i =>
                {
                    ChaosSettings.difficulty = i;
                    ChaosSettings.Save();
                });

            CloneText(contentColumns[0], "");

            SubmitSlider timerslider = CloneSlider(contentColumns[1], "Timer Interval", ChaosSettings.customTimerValue, 0.1f, 120f,
                    b =>
                    {
                        ChaosSettings.customTimerValue = b;
                        ChaosSettings.Save();
                    });

            CloneToggle(contentColumns[0],"Custom Timer",ChaosSettings.customTimer,
                v =>
                {
                    ChaosSettings.customTimer = v;

                    timerslider.interactable = v;
                    difficultydropdown.interactable = !v;

                    ChaosSettings.Save();
                });

            timerslider.interactable = ChaosSettings.customTimer;
            difficultydropdown.interactable = !ChaosSettings.customTimer;
            timerslider.transform.parent.parent = contentColumns[0];

            CloneText(contentColumns[1], "UI");
            CloneSlider(contentColumns[1],"Event Logger Vertical Offset",ChaosSettings.loggerYOffset,0,400,
                v =>
                {
                    ChaosSettings.loggerYOffset = v;
                    ChaosSettings.Save();
                });

            CloneText(contentColumns[2], "Active Events");
            RectTransform content;
            ScrollRect scroll = CreateScrollRect(contentColumns[2], new Vector2(310, 230), out content);

            foreach (var pair in ChaosSettings.eventEnabled)
            {
                string eventName = pair.Key;

                Toggle toggle = CloneToggle(content,eventName,pair.Value,
                    b =>
                    {
                        ChaosSettings.eventEnabled[eventName] = b;
                        ChaosSettings.Save();
                    });

                EventToggles.Add(toggle);
            }
            Toggle blank = CloneToggle(content, "", false, null);
            blank.transform.GetChild(0).gameObject.SetActive(false);
            CloneButton(contentColumns[2], "TOGGLE ALL",
                () =>
                {
                    ChaosUIHelpers.SetAllToggles(!ChaosSettings.eventEnabled["Perk Overdose"]);
                });
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
        private static TextMeshProUGUI CloneText(Transform parent, string label)
        {
            GameObject textObj = Instantiate(textTemplate, parent);
            TextMeshProUGUI text = textObj.GetComponent<TextMeshProUGUI>();

            textObj.name = label;
            textObj.SetActive(true);

            text.text = label;

            text.enabled = true;
            textObj.GetComponent<LayoutElement>().enabled = true;

            return text;
        }
        private static Button CloneButton(Transform parent, string label, UnityEngine.Events.UnityAction callback)
        {
            GameObject buttonObj = Instantiate(buttonTemplate, parent);
            Button button = buttonObj.GetComponent<Button>();

            buttonObj.name = label;
            buttonObj.SetActive(true);

            button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = label;

            button.onClick = new Button.ButtonClickedEvent();

            if (callback != null)
                button.onClick.AddListener(callback);

            button.enabled = true;
            buttonObj.GetComponent<Image>().enabled = true;
            buttonObj.GetComponent<UI_AnimateOnSelect>().enabled = true;

            return button;
        }
        private static Toggle CloneToggle(Transform parent,string label, bool value, UnityEngine.Events.UnityAction<bool> callback)
        {
            GameObject toggleObj = Instantiate(toggleTemplate, parent);
            Toggle toggle = toggleObj.GetComponent<Toggle>();

            toggleObj.name = label;
            toggleObj.SetActive(true);

            TextMeshProUGUI text = toggle.GetComponentInChildren<TextMeshProUGUI>(true);

            if (text != null)
                if (nameTruncates.TryGetValue(label, out string v))
                    text.text = v;
                else
                    text.text = label;

            toggle.onValueChanged = new Toggle.ToggleEvent();

            toggle.SetIsOnWithoutNotify(value);

            if (callback != null)
                toggle.onValueChanged.AddListener(callback);

            toggle.enabled = true;
            toggleObj.transform.GetChild(0).GetComponent<Image>().enabled = true;

            return toggle;
        }
        public static ScrollRect CreateScrollRect(Transform parent,Vector2 size,out RectTransform content)
        {
            GameObject root = new GameObject("ScrollRect");
            root.transform.SetParent(parent, false);

            RectTransform r = root.AddComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(0, 0);
            r.pivot = new Vector2(0, 0);
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
            layout.spacing = 30;

            ContentSizeFitter fitter = cont.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Scrollbar
            GameObject scrollbarGO = new GameObject("Scrollbar");
            scrollbarGO.transform.SetParent(root.transform, false);

            RectTransform sbRT = scrollbarGO.AddComponent<RectTransform>();
            sbRT.anchorMin = new Vector2(1, 0);
            sbRT.anchorMax = new Vector2(1, 1);
            sbRT.pivot = new Vector2(1, 1);
            sbRT.sizeDelta = new Vector2(20, 0);
            sbRT.anchoredPosition = Vector2.zero;

            Image sbBackground = scrollbarGO.AddComponent<Image>();
            sbBackground.color = new Color(0, 0, 0, 0.25f);

            Scrollbar scrollbar = scrollbarGO.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            // Sliding Area
            GameObject slidingArea = new GameObject("Sliding Area");
            slidingArea.transform.SetParent(scrollbarGO.transform, false);

            RectTransform saRT = slidingArea.AddComponent<RectTransform>();
            saRT.anchorMin = Vector2.zero;
            saRT.anchorMax = Vector2.one;
            saRT.offsetMin = new Vector2(2, 2);
            saRT.offsetMax = new Vector2(-2, -2);

            // Handle
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(slidingArea.transform, false);

            RectTransform handleRT = handle.AddComponent<RectTransform>();
            handleRT.anchorMin = Vector2.zero;
            handleRT.anchorMax = Vector2.one;
            handleRT.offsetMin = Vector2.zero;
            handleRT.offsetMax = Vector2.zero;

            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;

            scrollbar.targetGraphic = handleImage;
            scrollbar.handleRect = handleRT;

            ScrollRect sr = root.AddComponent<ScrollRect>();
            sr.viewport = vpRT;
            sr.content = content;
            sr.horizontal = false;
            sr.scrollSensitivity = 25;

            sr.verticalScrollbar = scrollbar;
            sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            sr.verticalScrollbarSpacing = 0;

            return sr;
        }

        private static SubmitSlider CloneSlider(Transform parent,string label,float value,float min,float max,UnityEngine.Events.UnityAction<float> callback)
        {
            GameObject sliderObj = Instantiate(sliderTemplate, parent);
            SubmitSlider slider = sliderObj.transform.Find("Slider").GetComponent<SubmitSlider>();

            sliderObj.name = label;
            sliderObj.SetActive(true);

            slider.enabled = true;

            slider.minValue = min;
            slider.maxValue = max;

            slider.onValueChanged = new SubmitSlider.SliderEvent();

            slider.SetValueWithoutNotify(value);

            slider.onValueChanged.AddListener(callback);

            sliderObj.GetComponent<TextMeshProUGUI>().enabled = true;
            sliderObj.GetComponent<TextMeshProUGUI>().text = label;
            sliderObj.GetComponent<LayoutElement>().enabled = true;
            TextMeshProUGUI valueText = sliderObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            valueText.enabled = true;
            valueText.text = $"{value}";
            slider.onValueChanged.AddListener(v =>
            {
                valueText.text = v.ToString("0.##");
            });

            return slider;
        }
        private static TMP_Dropdown CloneDropdown(Transform parent,string label, List<TMP_Dropdown.OptionData> options, int value, UnityEngine.Events.UnityAction<int> callback)
        {
            GameObject dropdownObj = Instantiate(dropdownTemplate, parent);
            TMP_Dropdown dropdown = dropdownObj.transform.GetChild(0).GetComponent<TMP_Dropdown>();

            dropdownObj.name = label;
            dropdownObj.SetActive(true);

            dropdownObj.GetComponent<TextMeshProUGUI>().enabled = true;
            dropdownObj.GetComponent<TextMeshProUGUI>().text = label;

            dropdown.enabled = true;

            dropdown.ClearOptions();
            dropdown.AddOptions(options);
            
            dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();

            dropdown.SetValueWithoutNotify(value);

            dropdown.onValueChanged.AddListener(callback);

            dropdown.GetComponent<Image>().enabled = true;
            dropdown.GetComponent<CanvasGroup>().enabled = true;

            return dropdown;
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
        private bool eventCompleted = false;

        private float timeLeft = 20f;
        private float eventTimer = 0f;
        private float eventTimerMax = 0f;

        private Vector2 targetPos;
        private float height = 18f;

        public List<UnityEngine.Object> relatedObjects = new List<UnityEngine.Object>();

        public float Height => height;
        public void Wake(string name,float time, ChaosUI ownerUI, bool doubleEvent)
        {
            owner = ownerUI;
            awake = true;

            eventTimerMax = time;
            eventTimer = time;

            CreateEntryUI(name, doubleEvent);
        }
        private void CreateEntryUI(string name, bool tintText)
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
            if (tintText)
                label.color = Color.red;
            else
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
                eventCompleted = true;
                Destroy(gameObject);
            }
        }
        void OnDestroy()
        {
            if (eventCompleted) return;
            owner.RemoveEntry(this);
            foreach (GameObject obj in relatedObjects)
            {
                Destroy(obj);
            }
        }
    }
}
