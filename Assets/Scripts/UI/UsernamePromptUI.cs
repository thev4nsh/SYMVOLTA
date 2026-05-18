using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SYMVOLTA.Core;
using SYMVOLTA.Profile;

namespace SYMVOLTA.UI
{
    public class UsernamePromptUI : MonoBehaviour
    {
        private CanvasGroup _group;
        private TMP_InputField _input;
        private TextMeshProUGUI _statusText;
        private bool _submitting;

        private async void Start()
        {
            if (ProfileManager.Instance?.CurrentProfile == null) return;
            if (!ProfileManager.Instance.CurrentProfile.isFirstLaunch) return;

            BuildOverlay();
            _input.text = ProfileManager.Instance.CurrentProfile.username;
            Show();
            await System.Threading.Tasks.Task.CompletedTask;
        }

        private void BuildOverlay()
        {
            GameObject overlay = new GameObject("UsernamePromptOverlay", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
            overlay.transform.SetParent(transform, false);
            RectTransform rt = overlay.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            Image bg = overlay.GetComponent<Image>();
            bg.color = new Color(0f, 0f, 0f, 0.92f);
            bg.raycastTarget = true;
            _group = overlay.GetComponent<CanvasGroup>();

            GameObject panel = new GameObject("UsernamePromptPanel", typeof(RectTransform), typeof(Image), typeof(GlassPanel));
            panel.transform.SetParent(overlay.transform, false);
            RectTransform prt = panel.GetComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(820, 520);

            CreateLabel(panel.transform, "CHOOSE USERNAME", 54, Constants.COLOR_NEON_WHITE, new Vector2(0, 170), new Vector2(700, 80));
            CreateLabel(panel.transform, "Names are global and can be edited later.", 28, Constants.COLOR_TEXT_DIM, new Vector2(0, 110), new Vector2(700, 50));

            GameObject inputObj = new GameObject("UsernameInput", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            inputObj.transform.SetParent(panel.transform, false);
            RectTransform irt = inputObj.GetComponent<RectTransform>();
            irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
            irt.anchoredPosition = new Vector2(0, 20);
            irt.sizeDelta = new Vector2(640, 78);
            inputObj.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.11f, 1f);
            _input = inputObj.GetComponent<TMP_InputField>();
            _input.characterLimit = 20;

            TextMeshProUGUI inputText = CreateLabel(inputObj.transform, "", 34, Constants.COLOR_NEON_WHITE, Vector2.zero, new Vector2(590, 64));
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
            inputText.rectTransform.anchoredPosition = new Vector2(16, 0);
            _input.textComponent = inputText;

            _statusText = CreateLabel(panel.transform, "", 26, Constants.COLOR_WARNING_RED, new Vector2(0, -56), new Vector2(700, 46));

            GameObject confirm = CreateButton(panel.transform, "CONFIRM", new Vector2(0, -165), new Vector2(360, 82));
            confirm.GetComponent<Button>().onClick.AddListener(Submit);
        }

        private async void Submit()
        {
            if (_submitting) return;
            string username = _input.text.Trim();
            if (username.Length < 3 || username.Length > 20)
            {
                _statusText.text = "Use 3-20 characters.";
                return;
            }

            _submitting = true;
            _statusText.color = Constants.COLOR_TEXT_DIM;
            _statusText.text = "Checking name...";

            bool updated = await ProfileManager.Instance.TryUpdateUsernameAsync(username, true);
            if (updated)
            {
                ProfileManager.Instance.MarkFirstLaunchComplete();
                Hide();
                Destroy(_group.gameObject);
            }
            else
            {
                _statusText.color = Constants.COLOR_WARNING_RED;
                _statusText.text = "Name unavailable. Try another.";
            }

            _submitting = false;
        }

        private void Show()
        {
            _group.alpha = 1f;
            _group.blocksRaycasts = true;
            _group.interactable = true;
        }

        private void Hide()
        {
            _group.alpha = 0f;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }

        private TextMeshProUGUI CreateLabel(Transform parent, string text, int size, Color color, Vector2 pos, Vector2 sizeDelta)
        {
            GameObject obj = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            obj.transform.SetParent(parent, false);
            RectTransform rt = obj.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            TextMeshProUGUI label = obj.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private GameObject CreateButton(Transform parent, string text, Vector2 pos, Vector2 sizeDelta)
        {
            GameObject button = new GameObject("ConfirmUsernameButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(NeonButton));
            button.transform.SetParent(parent, false);
            RectTransform rt = button.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = sizeDelta;
            button.GetComponent<Image>().color = new Color(0.04f, 0.12f, 0.16f, 0.95f);
            CreateLabel(button.transform, text, 34, Constants.COLOR_CYAN, Vector2.zero, sizeDelta);
            return button;
        }
    }
}
