using System.Collections.Generic;
using UnityEngine;

// Mohammad SLIM — Map & Progression
// Flat-top hex grid, horizontal : Home (gauche) → colonnes → Boss (droite)
// q = colonne (depth), r = lane (0..width-1)

public class MapManager : MonoBehaviour
{
    // --- Données map ---
    public Dictionary<(int, int), HexCase> map = new Dictionary<(int, int), HexCase>();
    public HexCase startCase;
    public HexCase bossCase;

    // --- Visuel ---
    public GameObject hexNodePrefab;
    [Tooltip("Circonrayon réel du prefab hex en unités Unity (ajuster pour que les hexagones soient jointifs)")]
    public float hexRadius = 0.5f;
    [Tooltip("1.0 = hexagones jointifs, >1.0 = espace entre eux")]
    public float spacing = 1.0f;
    private Dictionary<HexCase, HexTileView> views = new Dictionary<HexCase, HexTileView>();

    // --- Player ---
    public PlayerMapController player;

    // --- Scaling ennemis ---
    public float baseEnemyHP     = 50f;
    public float baseEnemyDamage = 10f;
    private int runCount = 0;

    public float GetScaledHP(HexCase hex)     => baseEnemyHP     * (1f + hex.q * 0.3f);
    public float GetScaledDamage(HexCase hex) => baseEnemyDamage * (1f + hex.q * 0.2f);

    // --- Paramètres map ---
    public int depth = 5;   // nombre de colonnes entre Home et Boss
    public int width = 3;   // nombre de lanes (haut/milieu/bas)

    // =========================================================
    //  UNITY LIFECYCLE
    // =========================================================

    void Start()
    {
        hexRadius = 0.5f;
        spacing = 1.0f;
        GenerateMap();
        RenderMap();
        if (player != null) player.InitAtStart(startCase);
        UpdateTilesVisuals();
        Debug.Log("Map générée !");
    }

    // =========================================================
    //  GÉNÉRATION PROCÉDURALE
    // =========================================================

public void GenerateMap()
{
    map.Clear();
    int centerLane = 1;
    int shopPlaced = 0;
    int maxShops   = Random.Range(2, 4); // 2 ou 3 shops par round

    // Home — depth 0
    startCase = new HexCase(0, centerLane, CaseType.Home, 0);
    map.Add((0, centerLane), startCase);

    // Forme proche du sujet
    Dictionary<int, int[]> shape = new Dictionary<int, int[]>
    {
        { 1, new int[] { 0, 1, 2 } },
        { 2, new int[] { 0, 1, 2 } },
        { 3, new int[] { 0, 1, 2 } },
        { 4, new int[] { 0, 1, 2 } },
        { 5, new int[] { 1, 2 } }
    };

    for (int d = 1; d <= depth; d++)
    {
        if (!shape.ContainsKey(d))
            continue;

        foreach (int lane in shape[d])
        {
            CaseType type = GetRandomCaseType(ref shopPlaced, maxShops);
            // Les lignes extérieures sont visuellement à x=(d-0.5)*W,
            // donc leur profondeur de progression = 2*d-1 (avant le centre 2*d).
            int hexDepth = (lane == centerLane) ? 2 * d : 2 * d - 1;
            HexCase newCase = new HexCase(d, lane, type, hexDepth);
            map.Add((d, lane), newCase);
        }
    }

    // Boss
    bossCase = new HexCase(depth + 1, centerLane, CaseType.Boss, 2 * (depth + 1));
    map.Add((depth + 1, centerLane), bossCase);

    // Connexions basées sur l'adjacence hexagonale réelle du layout staggered
    foreach (var kv in map)
    {
        HexCase c = kv.Value;

        if (c.r == centerLane)
        {
            // Ligne centrale : 6 voisins possibles
            TryConnect(c, c.q - 1, centerLane);  // centre gauche
            TryConnect(c, c.q + 1, centerLane);  // centre droite
            TryConnect(c, c.q, 0);                // même colonne haut
            TryConnect(c, c.q, 2);                // même colonne bas
            TryConnect(c, c.q + 1, 0);            // col suivante haut
            TryConnect(c, c.q + 1, 2);            // col suivante bas
        }
        else if (c.r == 0)
        {
            // Ligne du haut : 4 voisins possibles
            TryConnect(c, c.q - 1, 0);            // même ligne gauche
            TryConnect(c, c.q + 1, 0);            // même ligne droite
            TryConnect(c, c.q - 1, centerLane);   // centre col précédente
            TryConnect(c, c.q, centerLane);        // centre même colonne
        }
        else // c.r == 2
        {
            // Ligne du bas : 4 voisins possibles
            TryConnect(c, c.q - 1, 2);            // même ligne gauche
            TryConnect(c, c.q + 1, 2);            // même ligne droite
            TryConnect(c, c.q - 1, centerLane);   // centre col précédente
            TryConnect(c, c.q, centerLane);        // centre même colonne
        }
    }

}

private void TryConnect(HexCase c, int targetQ, int targetLane)
{
    if (map.TryGetValue((targetQ, targetLane), out HexCase target))
    {
        if (!c.neighbors.Contains(target))
            c.neighbors.Add(target);
        if (!target.neighbors.Contains(c))
            target.neighbors.Add(c);
    }
}

    private CaseType GetRandomCaseType(ref int shopPlaced, int maxShops)
    {
        int roll = Random.Range(0, 100);
        if (roll < 50) return CaseType.Combat;
        if (roll < 70)
        {
            if (shopPlaced < maxShops) { shopPlaced++; return CaseType.Shop; }
            return CaseType.Combat;
        }
        if (roll < 85)
        {
            if (shopPlaced < maxShops) { shopPlaced++; return CaseType.Shop; }
            return CaseType.Combat;
        }
        return CaseType.Combat;
    }

    // =========================================================
    //  RENDU VISUEL
    // =========================================================

    public void RenderMap()
    {
        foreach (var v in views.Values)
            if (v != null) Destroy(v.gameObject);
        views.Clear();

        if (hexNodePrefab == null)
        {
            Debug.LogWarning("hexNodePrefab non assigné !");
            return;
        }

        // Calculer positions monde
        var toSpawn = new List<(HexCase c, Vector3 pos)>();
        Vector3 sum = Vector3.zero;

        foreach (var kv in map)
        {
            HexCase c   = kv.Value;
            Vector3 pos = ColLaneToWorld(c.q, c.r);
            toSpawn.Add((c, pos));
            sum += pos;
        }

        // Centrer la map
        Vector3 center = sum / Mathf.Max(1, toSpawn.Count);

        foreach (var item in toSpawn)
        {
            GameObject go = Instantiate(hexNodePrefab, item.pos - center, Quaternion.identity);
            go.name = $"Hex_{item.c.q}_{item.c.r}_{item.c.type}";

            HexTileView view = go.GetComponent<HexTileView>();
            if (view != null)
            {
                view.Init(item.c, this);
                views[item.c] = view;
            }
        }
    }

    /// <summary>
    /// Tessellation flat-top hex grid.
    /// Stagger SYMÉTRIQUE : colonnes paires -rowDist/4, impaires +rowDist/4.
    /// → décalage total = rowDist/2 (tessellation correcte) mais la grille
    ///   est centrée autour de y=0 sans biais diagonal.
    /// </summary>
private Vector3 ColLaneToWorld(int col, int lane)
{
    // hexRadius = taille réelle du prefab (1.0 = tessellation parfaite si correct)
    // spacing   = facteur d'écart supplémentaire (1.0 = jointif, >1 = espace)
    float s = hexRadius * spacing;
    float W = Mathf.Sqrt(3f) * s;  // pas horizontal entre centres (pointy-top)

    float x, y;

    if (lane == 1) // ligne centrale : alignée horizontalement
    {
        x = col * W;
        y = 0f;
    }
    else           // lignes haut (0) et bas (2) : insérées entre deux colonnes centrales
    {
        x = (col - 0.5f) * W;
        y = (lane == 0) ? 1.5f * s : -1.5f * s;
    }

    return new Vector3(x, y, 0);
}

    // =========================================================
    //  VISUEL : toutes les tiles visibles, opacité variable
    // =========================================================

    public void UpdateTilesVisuals()
    {
        if (player == null || player.currentCase == null) return;

        foreach (var kv in views)
        {
            HexCase     c = kv.Key;
            HexTileView v = kv.Value;

            bool isCurrent   = (c == player.currentCase);
            bool isClickable = player.currentCase.neighbors.Contains(c)
                               && c.depth > player.currentCase.depth;
            bool isVisited   = c.isVisited;

            v.gameObject.SetActive(true);
            v.SetState(isCurrent, isClickable, isVisited);
        }
    }

    // =========================================================
    //  BOSS VAINCU → NOUVELLE MAP + SCALING
    // =========================================================

    public void OnBossDefeated()
    {
        runCount++;
        Debug.Log($"Boss vaincu ! Run {runCount}");

        baseEnemyHP     *= 1.2f;
        baseEnemyDamage *= 1.15f;

        GenerateMap();
        RenderMap();
        if (player != null) player.InitAtStart(startCase);
        UpdateTilesVisuals();

        Debug.Log($"Nouvelle map — HP={baseEnemyHP:F0} DMG={baseEnemyDamage:F0}");
    }

    // =========================================================
    //  INPUT
    // =========================================================

    public void OnTileClicked(HexCase clicked)
    {
        if (player == null) return;
        player.TryMoveTo(clicked);
        UpdateTilesVisuals();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(worldPos.x, worldPos.y), Vector2.zero);
        if (hit.collider == null) return;

        HexTileView view = hit.collider.GetComponent<HexTileView>();
        if (view != null) OnTileClicked(view.data);
    }
}
