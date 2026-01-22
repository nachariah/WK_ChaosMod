using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
            float y = 0f;
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
            // Canvas
            GameObject canvasObj = new GameObject("Canvas");
            canvasObj.transform.SetParent(new GameObject("ChaosVersion").transform, false);

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            canvasObj.AddComponent<CanvasScaler>().uiScaleMode =
                CanvasScaler.ScaleMode.ScaleWithScreenSize;

            // Text
            GameObject textObj = new GameObject("StatusText");
            textObj.transform.SetParent(canvasObj.transform, false);

            TMP_Text statusText = textObj.AddComponent<TextMeshProUGUI>();
            statusText.text = "CHAOS MOD v"+Plugin.pluginVersion;
            statusText.fontSize = 14;
            statusText.color = Color.white;
            statusText.alignment = TextAlignmentOptions.TopRight;

            TMP_FontAsset font = FindTicketingFont();
            if (font != null)
                statusText.font = font;

            RectTransform rect = statusText.rectTransform;
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);

            rect.anchoredPosition = new Vector2(-5f, -30f);
            rect.sizeDelta = new Vector2(250f, 30f);
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
