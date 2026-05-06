// using UnityEngine;

// public class HexTileView : MonoBehaviour
// {
//     public HexCase data;
//     private MapManager mapManager;
//     private SpriteRenderer sr;

//     private static Sprite hexSprite;

//     // =========================================================
//     //  INIT
//     // =========================================================

//     public void Init(HexCase data, MapManager mapManager)
//     {
//         this.data = data;
//         this.mapManager = mapManager;

//         sr = GetComponent<SpriteRenderer>();
//         if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

//         sr.sprite = GetHexSprite();
//         SetupHexCollider();
//         RefreshVisual();
//     }

//     // =========================================================
//     //  COLLIDER HEXAGONAL
//     // =========================================================

//     private void SetupHexCollider()
//     {
//         var old = GetComponent<Collider2D>();
//         if (old != null) Destroy(old);

//         var col = gameObject.AddComponent<PolygonCollider2D>();
//         Vector2[] points = new Vector2[6];
//         for (int i = 0; i < 6; i++)
//         {
//             float angle = Mathf.PI / 6f + Mathf.PI / 3f * i; // pointy-top
//             points[i] = new Vector2(0.5f * Mathf.Cos(angle), 0.5f * Mathf.Sin(angle));
//         }
//         col.SetPath(0, points);
//     }

//     // =========================================================
//     //  SPRITE HEXAGONAL (généré une seule fois)
//     // =========================================================

//     private static Sprite GetHexSprite()
//     {
//         if (hexSprite != null) return hexSprite;

//         int size = 256;
//         Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

//         float cx = size * 0.5f;
//         float cy = size * 0.5f;
//         float fillR   = size * 0.47f;  // remplissage
//         float borderR = size * 0.50f;  // bordure (bord du sprite)

//         Vector2[] fillVerts   = HexVerts(cx, cy, fillR);
//         Vector2[] borderVerts = HexVerts(cx, cy, borderR);

//         Color borderColor = new Color(0.2f, 0.2f, 0.2f, 1f);

//         for (int py = 0; py < size; py++)
//         {
//             for (int px = 0; px < size; px++)
//             {
//                 Vector2 p = new Vector2(px + 0.5f, py + 0.5f);

//                 if (PointInHex(p, fillVerts))
//                     tex.SetPixel(px, py, Color.white);
//                 else if (PointInHex(p, borderVerts))
//                     tex.SetPixel(px, py, borderColor);
//                 else
//                     tex.SetPixel(px, py, Color.clear);
//             }
//         }

//         tex.Apply();
//         tex.filterMode = FilterMode.Bilinear;

//         hexSprite = Sprite.Create(tex, new Rect(0, 0, size, size),
//                                   new Vector2(0.5f, 0.5f), size);
//         return hexSprite;
//     }

//     private static Vector2[] HexVerts(float cx, float cy, float r)
//     {
//         Vector2[] v = new Vector2[6];
//         for (int i = 0; i < 6; i++)
//         {
//             float a = Mathf.PI / 6f + Mathf.PI / 3f * i; // pointy-top
//             v[i] = new Vector2(cx + r * Mathf.Cos(a), cy + r * Mathf.Sin(a));
//         }
//         return v;
//     }

//     private static bool PointInHex(Vector2 p, Vector2[] hex)
//     {
//         bool inside = false;
//         int j = 5;
//         for (int i = 0; i < 6; i++)
//         {
//             if ((hex[i].y > p.y) != (hex[j].y > p.y) &&
//                 p.x < (hex[j].x - hex[i].x) * (p.y - hex[i].y)
//                       / (hex[j].y - hex[i].y) + hex[i].x)
//                 inside = !inside;
//             j = i;
//         }
//         return inside;
//     }

//     // =========================================================
//     //  COULEURS
//     // =========================================================

//     public void RefreshVisual()
//     {
//         if (sr == null) return;
//         switch (data.type)
//         {
//             case CaseType.Home:    sr.color = new Color(0.45f, 0.85f, 0.45f); break;
//             case CaseType.Combat:  sr.color = new Color(0.90f, 0.45f, 0.45f); break;
//             case CaseType.Casino:  sr.color = new Color(0.95f, 0.65f, 0.20f); break;
//             case CaseType.Shop:    sr.color = new Color(0.40f, 0.70f, 0.95f); break;
//             case CaseType.Event:   sr.color = new Color(0.95f, 0.85f, 0.30f); break;
//             case CaseType.Boss:    sr.color = new Color(0.75f, 0.35f, 0.85f); break;
//         }
//     }

//     // =========================================================
//     //  ÉTAT VISUEL
//     // =========================================================

//     public void SetState(bool isCurrent, bool isClickable, bool isVisited)
//     {
//         if (sr == null) return;
//         RefreshVisual();
//         Color c = sr.color;

//         if (isCurrent)
//         {
//             transform.localScale = Vector3.one * 1.15f;
//             sr.color = new Color(c.r, c.g, c.b, 1f);
//         }
//         else if (isClickable)
//         {
//             transform.localScale = Vector3.one;
//             sr.color = new Color(c.r, c.g, c.b, 1f);
//         }
//         else if (isVisited)
//         {
//             transform.localScale = Vector3.one;
//             sr.color = new Color(c.r, c.g, c.b, 0.5f);
//         }
//         else
//         {
//             transform.localScale = Vector3.one;
//             sr.color = new Color(c.r, c.g, c.b, 0.25f);
//         }
//     }
// }


//new --------------------------



using UnityEngine;

public class HexTileView : MonoBehaviour
{
    public HexCase data;
    private MapManager mapManager;
    private SpriteRenderer sr;
    private GameObject iconObject;
    private SpriteRenderer iconSr;

    private static Sprite hexSprite;

    // =========================================================
    //  INIT
    // =========================================================

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

    // =========================================================
    //  ICÔNE
    // =========================================================

    private void CreateIcon()
    {
        // DestroyImmediate pour éviter les doublons (Destroy est différé)
        Transform old = transform.Find("Icon");
        if (old != null) DestroyImmediate(old.gameObject);
        iconObject = null;
        iconSr     = null;

        Sprite icon = GetIconForType(data.type);
        if (icon == null) return;

        iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(transform);

        iconSr              = iconObject.AddComponent<SpriteRenderer>();
        iconSr.sprite       = icon;
        iconSr.sortingOrder = 1;

        // Calcul du scale : l'icône remplit ~85% du hex (1 unité Unity)
        float targetSize       = 0.85f;
        float spriteWorldWidth = icon.rect.width / icon.pixelsPerUnit;
        float s = spriteWorldWidth > 0f ? targetSize / spriteWorldWidth : targetSize;
        iconObject.transform.localScale = Vector3.one * s;

        // Correction pivot : si le pivot n'est pas centré, on compense
        Vector2 boundsCenter = icon.bounds.center;
        iconObject.transform.localPosition = new Vector3(-boundsCenter.x * s, -boundsCenter.y * s, -0.1f);
    }

    private Sprite GetIconForType(CaseType type)
    {
        string path = type switch
        {
            CaseType.Home    => "Sprites/Icons/icon_home",
            CaseType.Boss    => "Sprites/Icons/icon_boss",
            CaseType.Combat  => "Sprites/Icons/icon_combat",
            CaseType.Event   => "Sprites/Icons/icon_combat",
            CaseType.Shop    => "Sprites/Icons/icon_shop",
            CaseType.Casino  => "Sprites/Icons/icon_shop",
            _                => null
        };

        if (path == null) return null;

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite == null)
            Debug.LogWarning($"Icône introuvable : {path} — vérifie que le fichier est dans Assets/Resources/Sprites/Icons/");

        return sprite;
    }

    // =========================================================
    //  COLLIDER HEXAGONAL
    // =========================================================

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

    // =========================================================
    //  SPRITE HEXAGONAL (généré une seule fois)
    // =========================================================

    private static Sprite GetHexSprite()
    {
        if (hexSprite != null) return hexSprite;

        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

        float cx = size * 0.5f;
        float cy = size * 0.5f;
        float fillR   = size * 0.47f;
        float borderR = size * 0.50f;

        Vector2[] fillVerts   = HexVerts(cx, cy, fillR);
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
                p.x < (hex[j].x - hex[i].x) * (p.y - hex[i].y)
                      / (hex[j].y - hex[i].y) + hex[i].x)
                inside = !inside;
            j = i;
        }
        return inside;
    }

    // =========================================================
    //  COULEURS
    // =========================================================

    public void RefreshVisual()
    {
        if (sr == null) return;
        switch (data.type)
        {
            case CaseType.Home:   sr.color = new Color(0.45f, 0.85f, 0.45f); break;
            case CaseType.Combat: sr.color = new Color(0.90f, 0.45f, 0.45f); break;
            case CaseType.Casino: sr.color = new Color(0.95f, 0.65f, 0.20f); break;
            case CaseType.Shop:   sr.color = new Color(0.40f, 0.70f, 0.95f); break;
            case CaseType.Event:  sr.color = new Color(0.95f, 0.85f, 0.30f); break;
            case CaseType.Boss:   sr.color = new Color(0.75f, 0.35f, 0.85f); break;
        }
    }

    // =========================================================
    //  ÉTAT VISUEL
    // =========================================================

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

        // L'icône suit la même opacité que le hex
        if (iconSr != null)
        {
            float alpha = sr.color.a;
            iconSr.color = new Color(1f, 1f, 1f, alpha);
        }
    }
}
