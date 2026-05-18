using UnityEngine;
using UnityEngine.UI;

namespace SYMVOLTA.UI
{
    [RequireComponent(typeof(Image))]
    public class GlassPanel : MonoBehaviour
    {
        [Header("Glass Settings")]
        [SerializeField] private Color glassColor = new Color(0.05f, 0.1f, 0.15f, 0.85f);
        [SerializeField] private float borderWidth = 2f;
        [SerializeField] private Color borderColor = new Color(0f, 0.8f, 1f, 0.5f);

        private Image _panelImage;
        private static Sprite _roundedSprite;

        private void Awake()
        {
            SetupGlass();
        }

        private void SetupGlass()
        {
            _panelImage = GetComponent<Image>();
            if (_roundedSprite == null)
                _roundedSprite = CreateRoundedSprite(256, 256, 40);

            _panelImage.sprite = _roundedSprite;
            _panelImage.type = Image.Type.Sliced;
            _panelImage.color = glassColor;

            Outline outline = gameObject.AddComponent<Outline>();
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(borderWidth, borderWidth);
        }

        private Sprite CreateRoundedSprite(int width, int height, int radius)
        {
            Color[] pixels = new Color[width * height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool inside = true;
                    if (x < radius && y < radius)
                        inside = Vector2.Distance(new Vector2(x, y), new Vector2(radius, radius)) <= radius;
                    else if (x > width - radius && y < radius)
                        inside = Vector2.Distance(new Vector2(x, y), new Vector2(width - radius, radius)) <= radius;
                    else if (x < radius && y > height - radius)
                        inside = Vector2.Distance(new Vector2(x, y), new Vector2(radius, height - radius)) <= radius;
                    else if (x > width - radius && y > height - radius)
                        inside = Vector2.Distance(new Vector2(x, y), new Vector2(width - radius, height - radius)) <= radius;

                    pixels[x + y * width] = inside ? Color.white : Color.clear;
                }
            }

            Texture2D tex = new Texture2D(width, height);
            tex.name = "SYMVOLTA_GlassPanelSprite";
            tex.hideFlags = HideFlags.HideAndDontSave;
            tex.SetPixels(pixels);
            tex.Apply();
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, Vector4.one * radius);
            sprite.name = "SYMVOLTA_GlassPanelRounded";
            sprite.hideFlags = HideFlags.HideAndDontSave;
            return sprite;
        }
    }
}
