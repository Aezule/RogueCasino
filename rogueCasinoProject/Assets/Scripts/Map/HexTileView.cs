using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class HexTileView : MonoBehaviour
{
    public HexCase data;
    private MapManager mapManager;
    private SpriteRenderer sr;
    private GameObject iconObject;
    private SpriteRenderer iconSr;

    [Header("Map Icons")]
    [SerializeField] private Sprite homeIcon;
    [SerializeField] private Sprite bossIcon;
    [SerializeField] private Sprite combatIcon;
    [SerializeField] private Sprite shopIcon;

    private static Sprite hexSprite;

    private void Reset()
    {
        EnsureIconsAssigned();
    }

    private void OnValidate()
    {
        EnsureIconsAssigned();
    }

    private void EnsureIconsAssigned()
    {
#if UNITY_EDITOR
        if (homeIcon == null) homeIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MapIcons/icon_home.png");
        if (bossIcon == null) bossIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MapIcons/icon_boss.png");
        if (combatIcon == null) combatIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MapIcons/icon_combat.png");
        if (shopIcon == null) shopIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/MapIcons/icon_shop.png");
#endif
    }

    public void Init(HexCase data, MapManager mapManager)
    {
        this.data = data;
        this.mapManager = mapManager;

        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        sr.sprite = GetHexSprite();
        SetupHexCollider();

        CreateIcon();
        RefreshVisual();
    }

    private void CreateIcon()
    {
        Transform old = transform.Find("Icon");
        if (old != null) DestroyImmediate(old.gameObject);
        iconObject = null;
        iconSr = null;

        Sprite icon = GetIconForType(data.type);
        if (icon == null) return;

        iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(transform, false);

        iconSr = iconObject.AddComponent<SpriteRenderer>();
        iconSr.sprite = icon;
        iconSr.sortingOrder = 1;

        float targetSize = 0.85f;
        float spriteWorldWidth = icon.rect.width / icon.pixelsPerUnit;
        float s = spriteWorldWidth > 0f ? targetSize / spriteWorldWidth : targetSize;
        iconObject.transform.localScale = Vector3.one * s;

        Vector2 boundsCenter = icon.bounds.center;
        iconObject.transform.localPosition = new Vector3(-boundsCenter.x * s, -boundsCenter.y * s, -0.1f);
    }

    private Sprite GetIconForType(CaseType type)
    {
        Sprite sprite = type switch
        {
            CaseType.Home => homeIcon,
            CaseType.Boss => bossIcon,
            CaseType.Combat => combatIcon,
            CaseType.Shop => shopIcon,
            _ => null
        };

        if (sprite == null)
            Debug.LogWarning($"Icône non assignée pour le type {type} sur {name}.");

        return sprite;
    }

    private void SetupHexCollider()
    {
        var old = GetComponent<Collider2D>();
        if (old != null) Destroy(old);

        var col = gameObject.AddComponent<PolygonCollider2D>();
        Vector2[] points = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.PI / 6f + Mathf.PI / 3f * i;
            points[i] = new Vector2(0.5f * Mathf.Cos(angle), 0.5f * Mathf.Sin(angle));
        }
        col.SetPath(0, points);
    }

    private static Sprite GetHexSprite()
    {
        if (hexSprite != null) return hexSprite;

        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        float cx = size * 0.5f;
        float cy = size * 0.5f;
        float fillR = size * 0.47f;
        float borderR = size * 0.50f;

        Vector2[] fillVerts = HexVerts(cx, cy, fillR);
        Vector2[] borderVerts = HexVerts(cx, cy, borderR);

        Color borderColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        for (int py = 0; py < size; py++)
        {
            for (int px = 0; px < size; px++)
            {
                Vector2 p = new Vector2(px + 0.5f, py + 0.5f);

                if (PointInHex(p, fillVerts))
                    tex.SetPixel(px, py, Color.white);
                else if (PointInHex(p, borderVerts))
                    tex.SetPixel(px, py, borderColor);
                else
                    tex.SetPixel(px, py, Color.clear);
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        hexSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return hexSprite;
    }

    private static Vector2[] HexVerts(float cx, float cy, float r)
    {
        Vector2[] v = new Vector2[6];
        for (int i = 0; i < 6; i++)
        {
            float a = Mathf.PI / 6f + Mathf.PI / 3f * i;
            v[i] = new Vector2(cx + r * Mathf.Cos(a), cy + r * Mathf.Sin(a));
        }
        return v;
    }

    private static bool PointInHex(Vector2 p, Vector2[] hex)
    {
        bool inside = false;
        int j = 5;
        for (int i = 0; i < 6; i++)
        {
            if ((hex[i].y > p.y) != (hex[j].y > p.y) &&
                p.x < (hex[j].x - hex[i].x) * (p.y - hex[i].y) / (hex[j].y - hex[i].y) + hex[i].x)
                inside = !inside;
            j = i;
        }
        return inside;
    }

    public void RefreshVisual()
    {
        if (sr == null) return;

        switch (data.type)
        {
            case CaseType.Home:
                sr.color = new Color(0.45f, 0.85f, 0.45f);
                break;
            case CaseType.Combat:
                sr.color = new Color(0.90f, 0.45f, 0.45f);
                break;
            case CaseType.Shop:
                sr.color = new Color(0.40f, 0.70f, 0.95f);
                break;
            case CaseType.Boss:
                sr.color = new Color(0.75f, 0.35f, 0.85f);
                break;
        }
    }

    public void SetState(bool isCurrent, bool isClickable, bool isVisited)
    {
        if (sr == null) return;

        RefreshVisual();
        Color c = sr.color;

        if (isCurrent)
        {
            transform.localScale = Vector3.one * 1.15f;
            sr.color = new Color(c.r, c.g, c.b, 1f);
        }
        else if (isClickable)
        {
            transform.localScale = Vector3.one;
            sr.color = new Color(c.r, c.g, c.b, 1f);
        }
        else if (isVisited)
        {
            transform.localScale = Vector3.one;
            sr.color = new Color(c.r, c.g, c.b, 0.5f);
        }
        else
        {
            transform.localScale = Vector3.one;
            sr.color = new Color(c.r, c.g, c.b, 0.25f);
        }

        if (iconSr != null)
        {
            float alpha = sr.color.a;
            iconSr.color = new Color(1f, 1f, 1f, alpha);
        }
    }
}
