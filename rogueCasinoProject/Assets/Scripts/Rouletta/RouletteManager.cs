using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// RouletteManager — Roulette complète générée par code.
/// Mises multiples, gains réels, feedback visuel, intégration Economy.
///
/// Cablage Inspector minimal requis :
///   - parentZero   → ConteneurZero
///   - grilleParent → GrilleNumeros
///   - zonesParent  → ZonesParis
///   - resultatText → TexteResultat
///   - boutonNumeroPrefab → ModelBouton  (optionnel)
///   - spriteTapis  → Assets/Images/Rouletta/rouletteTapis  (optionnel — active le mode image)
///   - spriteRoue   → Assets/Images/Rouletta/rouletteMain   (optionnel — déco)
/// </summary>
public class RouletteManager : MonoBehaviour
{
    // =========================================================
    //  INSPECTOR
    // =========================================================
    [Header("Tapis — Grille des numéros")]
    public Transform parentZero;
    public Transform grilleParent;
    public Transform colonnesParent;     // parent de Btn_Col1/2/3 (optionnel)

    [Header("Panel zones extérieures")]
    public Transform zonesParent;        // ZonesParis — rempli par code

    [Header("Root du popup (auto-détecté si null)")]
    public Transform rouletteRoot;       // Parent du popup "Choisir un nombre"

    [Header("Panels UI (null = créé auto dans rouletteRoot)")]
    public Transform jetonPanel;
    public Transform infoPanel;

    [Header("Prefab")]
    public GameObject boutonNumeroPrefab;

    [Header("UI existante")]
    public Text resultatText;
    public Text infoMiseText;

    [Header("Images pixel-art (optionnel — Assets/Images/Rouletta/)")]
    public Sprite spriteRoue;            // rouletteMain.jpg  → déco uniquement
    public Sprite spriteTapis;           // rouletteTapis.jpg → remplace boutons colorés par overlays

    [Header("Debug (si Economy absent)")]
    public int debugStartCoins = 1000;

    [Header("Debug overlays — calibration positions")]
    [Tooltip("Cocher en Play pour voir les rectangles des zones cliquables")]
    public bool debugOverlays = false;

    // =========================================================
    //  CONSTANTES ROULETTE
    // =========================================================
    private static readonly int[] ROUGES = {
        1,3,5,7,9,12,14,16,18,19,21,23,25,27,30,32,34,36
    };
    private static readonly int[] VALEURS_JETONS = { 1, 5, 10, 25, 50, 100 };

    private static readonly Color C_VERT_ZERO  = new Color(0.00f, 0.50f, 0.12f);
    private static readonly Color C_ROUGE       = new Color(0.75f, 0.08f, 0.08f);
    private static readonly Color C_NOIR        = new Color(0.12f, 0.12f, 0.12f);
    private static readonly Color C_VERT_ZONE   = new Color(0.08f, 0.42f, 0.12f);
    private static readonly Color C_BLEU_ZONE   = new Color(0.10f, 0.18f, 0.50f);

    // =========================================================
    //  MODÈLE INTERNE
    // =========================================================
    private class BetZone
    {
        public string  id;
        public int[]   numeros;
        public int     multiplicateur;   // gain NET / unité (plein=35, ext=1, dz/col=2)
        public int     montant;
        public Image   image;
        public Color   couleurBase;
        public Text    overlayMontant;
        public bool    transparent;      // true = mode image (overlay transparent)

        public BetZone(string id, int[] nums, int mult, Image img, Color col, bool transp = false)
        {
            this.id = id; this.numeros = nums; this.multiplicateur = mult;
            this.montant = 0; this.image = img; this.couleurBase = col;
            this.transparent = transp;
        }

        public bool Couvre(int numero) =>
            System.Array.IndexOf(numeros, numero) >= 0;
    }

    // =========================================================
    //  ÉTAT
    // =========================================================
    private int    jetonChoisi      = 10;
    private bool   spinEnCours      = false;
    private int    debugCoins;
    private Button spinBtn;
    private Button annulerBtn;
    private Text   balanceTxt;
    private Text   miseTxt;
    private Text   jetonTxt;
    private Image[] jetonImages;

    private bool      modeImage      = false;
    private Transform tapisContainer = null;  // conteneur image+overlays
    private Text      zoneInfoTxt    = null;  // texte "Zone survolée"

    private readonly Dictionary<string, BetZone> zones = new Dictionary<string, BetZone>();
    private Font defaultFont;

    // =========================================================
    //  DÉMARRAGE
    // =========================================================
    void Start()
    {
        debugCoins = debugStartCoins;

        defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (defaultFont == null)
            defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        if (zonesParent == null)
        {
            var zp = GameObject.Find("ZonesParis");
            if (zp != null) zonesParent = zp.transform;
        }

        if (rouletteRoot == null && zonesParent != null)
            rouletteRoot = zonesParent.parent;
        if (rouletteRoot == null && grilleParent != null && grilleParent.parent != null)
            rouletteRoot = grilleParent.parent.parent;
        if (rouletteRoot == null)
        {
            Debug.LogWarning("[RouletteManager] rouletteRoot non trouvé — assign-le dans l'Inspector.");
            rouletteRoot = FindObjectOfType<Canvas>()?.transform;
        }

        if (spriteTapis != null)
        {
            // ── Mode image : on peuple seulement la dict, pas de visuels colorés ──
            modeImage = true;
            PopulerZonesNombresDict();
            PopulerZonesColonnesDict();
            PopulerZonesExtDansDict();
            // Masquer les éventuels panels colorés encore présents
            if (parentZero    != null) parentZero.gameObject.SetActive(false);
            if (grilleParent  != null) grilleParent.gameObject.SetActive(false);
            if (zonesParent   != null) zonesParent.gameObject.SetActive(false);
            if (colonnesParent!= null) colonnesParent.gameObject.SetActive(false);
            // Tapis image + overlays transparents
            SetupTapisAvecImage();
            if (spriteRoue != null) SetupRoueImage();

            // ── Diagnostic : vérifie que le conteneur est bien positionné ──
            if (tapisContainer != null)
            {
                var tcRT = tapisContainer as RectTransform;
                if (tcRT != null)
                    Debug.Log($"[Roulette] TapisContainer OK — anchorMin={tcRT.anchorMin} anchorMax={tcRT.anchorMax} offsetMax={tcRT.offsetMax}");
                else
                    Debug.LogError("[Roulette] TapisContainer N'EST PAS un RectTransform !");

                var ov0 = tapisContainer.Find("Ov_0");
                if (ov0 != null)
                {
                    var ov0RT = ov0 as RectTransform ?? ov0.GetComponent<RectTransform>();
                    if (ov0RT != null)
                        Debug.Log($"[Roulette] Ov_0 OK — anchorMin={ov0RT.anchorMin} anchorMax={ov0RT.anchorMax} rect={ov0RT.rect}");
                    else
                        Debug.LogError("[Roulette] Ov_0 n'a pas de RectTransform !");
                }
                else
                    Debug.LogWarning("[Roulette] Ov_0 introuvable dans TapisContainer");
            }
            else
                Debug.LogError("[Roulette] tapisContainer est NULL après SetupTapisAvecImage !");
        }
        else
        {
            // ── Mode fallback coloré (si pas d'image assignée) ──
            GenererNombres();
            SetupColonnes();
            GenererZonesExterieures();
        }

        GenererJetons();
        GenererControles();
        MettreAJourUI();
    }

    // =========================================================
    //  GÉNÉRATION — NUMÉROS 0-36
    // =========================================================
    void GenererNombres()
    {
        if (parentZero   != null) ViderPanel(parentZero);
        if (grilleParent != null) ViderPanel(grilleParent);

        if (parentZero != null)
            CreerBoutonNumero(0, parentZero, C_VERT_ZERO);

        if (grilleParent != null)
            for (int i = 1; i <= 36; i++)
            {
                Color c = System.Array.IndexOf(ROUGES, i) >= 0 ? C_ROUGE : C_NOIR;
                CreerBoutonNumero(i, grilleParent, c);
            }
    }

    void CreerBoutonNumero(int numero, Transform parent, Color couleur)
    {
        GameObject obj;
        if (boutonNumeroPrefab != null)
        {
            obj = Instantiate(boutonNumeroPrefab, parent);
        }
        else
        {
            obj = new GameObject("Btn_" + numero);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<CanvasRenderer>();
            var img0 = obj.AddComponent<Image>();
            var btn0 = obj.AddComponent<Button>(); btn0.targetGraphic = img0;
            var lbl = CreerTexteGO("Label", obj.transform, numero.ToString(),
                TextAnchor.MiddleCenter, 20, FontStyle.Bold, Color.white);
            StretchRect(lbl.GetComponent<RectTransform>());
        }
        obj.name = "Btn_" + numero;

        Image fond = obj.GetComponent<Image>();
        if (fond != null) fond.color = couleur;

        Text lbl2 = obj.GetComponentInChildren<Text>();
        if (lbl2 != null) { lbl2.text = numero.ToString(); lbl2.color = Color.white; }

        var ov = CreerTexteGO("Overlay", obj.transform, "",
            TextAnchor.MiddleCenter, 18, FontStyle.Bold, new Color(1f, 0.9f, 0.1f));
        var ovR = ov.GetComponent<RectTransform>();
        ovR.anchorMin = new Vector2(0f, 0f);
        ovR.anchorMax = new Vector2(1f, 0.45f);
        ovR.sizeDelta = Vector2.zero;
        ovR.anchoredPosition = Vector2.zero;

        Button btn = obj.GetComponent<Button>();
        BetZone zone = new BetZone(numero.ToString(), new int[] { numero }, 35, fond, couleur);
        zone.overlayMontant = ov.GetComponent<Text>();
        zones[numero.ToString()] = zone;

        int cap = numero;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => PlacerMise(cap.ToString()));
    }

    // =========================================================
    //  GÉNÉRATION — COLONNES (boutons existants réutilisés)
    // =========================================================
    void SetupColonnes()
    {
        string[] noms    = { "Btn_Col1", "Btn_Col2", "Btn_Col3" };
        string[] zoneIds = { "colonne1", "colonne2", "colonne3" };
        string[] labels  = { "2 à 1", "2 à 1", "2 à 1" };
        string[] sublbls = { "Colonne 1", "Colonne 2", "Colonne 3" };

        for (int k = 0; k < 3; k++)
        {
            Transform t = null;
            if (colonnesParent != null) t = colonnesParent.Find(noms[k]);
            if (t == null) t = GameObject.Find(noms[k])?.transform;
            if (t == null)
            {
                // Créer zone vide pour que la dict soit complète
                int[] nums = GenererColonne(k + 1);
                zones[zoneIds[k]] = new BetZone(zoneIds[k], nums, 2, null, C_VERT_ZONE);
                continue;
            }

            Button btn = t.GetComponent<Button>();
            if (btn == null) continue;
            btn.onClick.RemoveAllListeners();

            Image img = t.GetComponent<Image>();
            if (img != null)
            {
                img.color = C_VERT_ZONE;
                ColorBlock cb = btn.colors;
                cb.normalColor      = C_VERT_ZONE;
                cb.highlightedColor = Color.Lerp(C_VERT_ZONE, Color.white, 0.25f);
                cb.pressedColor     = Color.Lerp(C_VERT_ZONE, Color.black, 0.25f);
                btn.colors = cb;
            }

            Text lbl = t.GetComponentInChildren<Text>();
            if (lbl != null)
            {
                lbl.text      = labels[k] + "\n" + sublbls[k];
                lbl.color     = Color.white;
                lbl.fontSize  = 20;
                lbl.fontStyle = FontStyle.Bold;
                lbl.alignment = TextAnchor.MiddleCenter;
            }

            var ovGo = CreerTexteGO("Overlay", t, "",
                TextAnchor.MiddleCenter, 18, FontStyle.Bold, new Color(1f, 0.9f, 0.1f));
            var ovR = ovGo.GetComponent<RectTransform>();
            ovR.anchorMin = Vector2.zero;
            ovR.anchorMax = new Vector2(1f, 0.38f);
            ovR.sizeDelta = Vector2.zero;

            int[] numeros = GenererColonne(k + 1);
            BetZone zone = new BetZone(zoneIds[k], numeros, 2, img, C_VERT_ZONE);
            zone.overlayMontant = ovGo.GetComponent<Text>();
            zones[zoneIds[k]] = zone;

            string capturedId = zoneIds[k];
            btn.onClick.AddListener(() => PlacerMise(capturedId));
        }
    }

    // =========================================================
    //  GÉNÉRATION — ZONES EXTÉRIEURES (douzaines + simples)
    // =========================================================
    void GenererZonesExterieures()
    {
        if (zonesParent == null)
        {
            // Créer les zones dans la dict même sans parent visuel
            PopulerZonesExtDansDict();
            return;
        }
        ViderPanel(zonesParent);

        var oldGrid = zonesParent.GetComponent<GridLayoutGroup>();
        if (oldGrid != null) DestroyImmediate(oldGrid);
        var oldHlg = zonesParent.GetComponent<HorizontalLayoutGroup>();
        if (oldHlg != null) DestroyImmediate(oldHlg);

        var vlg = zonesParent.GetComponent<VerticalLayoutGroup>()
               ?? zonesParent.gameObject.AddComponent<VerticalLayoutGroup>();
        if (vlg == null)
        {
            Debug.LogError("[RouletteManager] Impossible d'ajouter VerticalLayoutGroup sur " + zonesParent.name);
            PopulerZonesExtDansDict();
            return;
        }
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = true;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = 5;
        vlg.padding = new RectOffset(5, 5, 5, 5);

        var douzRow = CreerRangeeHorizontale("Douzaines", zonesParent, 90f);
        CreerBoutonZone("douzaine1", "1ère Douzaine\n1  ▸  12",  C_VERT_ZONE, douzRow, GenererRange(1,  12), 2);
        CreerBoutonZone("douzaine2", "2ème Douzaine\n13 ▸ 24",  C_VERT_ZONE, douzRow, GenererRange(13, 24), 2);
        CreerBoutonZone("douzaine3", "3ème Douzaine\n25 ▸ 36",  C_VERT_ZONE, douzRow, GenererRange(25, 36), 2);

        var simpRow = CreerRangeeHorizontale("Simples", zonesParent, 90f);
        CreerBoutonZone("manque", "1 à 18",  C_BLEU_ZONE,                    simpRow, GenererRange(1, 18), 1);
        CreerBoutonZone("pair",   "Pair",    C_BLEU_ZONE,                    simpRow, GenererPair(),        1);
        CreerBoutonZone("rouge",  "Rouge",   C_ROUGE,                        simpRow, ROUGES,               1);
        CreerBoutonZone("noir",   "Noir",    new Color(0.15f, 0.15f, 0.15f), simpRow, GenererNoir(),        1);
        CreerBoutonZone("impair", "Impair",  C_BLEU_ZONE,                    simpRow, GenererImpair(),      1);
        CreerBoutonZone("passe",  "19 à 36", C_BLEU_ZONE,                    simpRow, GenererRange(19, 36), 1);
    }

    /// <summary>Peuple zones dict pour 0-36 sans créer de GameObjects.</summary>
    void PopulerZonesNombresDict()
    {
        // Zéro
        zones["0"] = new BetZone("0", new int[] { 0 }, 35, null, C_VERT_ZERO, true);
        // 1-36
        for (int i = 1; i <= 36; i++)
        {
            Color c = System.Array.IndexOf(ROUGES, i) >= 0 ? C_ROUGE : C_NOIR;
            zones[i.ToString()] = new BetZone(i.ToString(), new int[] { i }, 35, null, c, true);
        }
    }

    /// <summary>Peuple zones dict pour les 3 colonnes sans créer de GameObjects.</summary>
    void PopulerZonesColonnesDict()
    {
        zones["colonne1"] = new BetZone("colonne1", GenererColonne(1), 2, null, C_VERT_ZONE, true);
        zones["colonne2"] = new BetZone("colonne2", GenererColonne(2), 2, null, C_VERT_ZONE, true);
        zones["colonne3"] = new BetZone("colonne3", GenererColonne(3), 2, null, C_VERT_ZONE, true);
    }

    /// <summary>Enregistre les zones ext. dans la dict sans créer de GameObjects visuels.</summary>
    void PopulerZonesExtDansDict()
    {
        zones["douzaine1"] = new BetZone("douzaine1", GenererRange(1,  12), 2, null, C_VERT_ZONE);
        zones["douzaine2"] = new BetZone("douzaine2", GenererRange(13, 24), 2, null, C_VERT_ZONE);
        zones["douzaine3"] = new BetZone("douzaine3", GenererRange(25, 36), 2, null, C_VERT_ZONE);
        zones["manque"]    = new BetZone("manque",    GenererRange(1,  18), 1, null, C_BLEU_ZONE);
        zones["pair"]      = new BetZone("pair",      GenererPair(),        1, null, C_BLEU_ZONE);
        zones["rouge"]     = new BetZone("rouge",     ROUGES,               1, null, C_ROUGE);
        zones["noir"]      = new BetZone("noir",      GenererNoir(),        1, null, new Color(0.15f, 0.15f, 0.15f));
        zones["impair"]    = new BetZone("impair",    GenererImpair(),      1, null, C_BLEU_ZONE);
        zones["passe"]     = new BetZone("passe",     GenererRange(19, 36), 1, null, C_BLEU_ZONE);
    }

    Transform CreerRangeeHorizontale(string nom, Transform parent, float hauteur)
    {
        var go = new GameObject(nom);
        go.transform.SetParent(parent, false);
        go.AddComponent<CanvasRenderer>();
        var img = go.AddComponent<Image>(); img.color = new Color(0,0,0,0);
        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = true;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.spacing = 5;
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = hauteur;
        le.flexibleWidth   = 1f;
        return go.transform;
    }

    BetZone CreerBoutonZone(string id, string label, Color couleur, Transform parent,
                             int[] numeros, int multiplicateur)
    {
        var obj = new GameObject("Zone_" + id);
        obj.transform.SetParent(parent, false);
        obj.AddComponent<CanvasRenderer>();
        var img = obj.AddComponent<Image>(); img.color = couleur;
        var btn = obj.AddComponent<Button>(); btn.targetGraphic = img;
        var cb  = btn.colors;
        cb.normalColor      = couleur;
        cb.highlightedColor = Color.Lerp(couleur, Color.white, 0.25f);
        cb.pressedColor     = Color.Lerp(couleur, Color.black, 0.25f);
        cb.colorMultiplier  = 1f;
        btn.colors = cb;

        var lblGo = CreerTexteGO("Label", obj.transform, label,
            TextAnchor.MiddleCenter, 20, FontStyle.Bold, Color.white);
        var lblR = lblGo.GetComponent<RectTransform>();
        lblR.anchorMin = new Vector2(0f, 0.4f);
        lblR.anchorMax = Vector2.one;
        lblR.sizeDelta = Vector2.zero;

        var ovGo = CreerTexteGO("Overlay", obj.transform, "",
            TextAnchor.MiddleCenter, 18, FontStyle.Bold, new Color(1f, 0.9f, 0.1f));
        var ovR = ovGo.GetComponent<RectTransform>();
        ovR.anchorMin = Vector2.zero;
        ovR.anchorMax = new Vector2(1f, 0.42f);
        ovR.sizeDelta = Vector2.zero;

        var zone = new BetZone(id, numeros, multiplicateur, img, couleur);
        zone.overlayMontant = ovGo.GetComponent<Text>();
        zones[id] = zone;

        btn.onClick.AddListener(() => PlacerMise(id));
        return zone;
    }

    // =========================================================
    //  MODE IMAGE — SETUP PRINCIPAL
    // =========================================================
    /// <summary>
    /// Crée le conteneur image (spriteTapis comme fond) et y ajoute
    /// des boutons overlay transparents calés sur chaque zone du tapis.
    /// Les BetZones existantes sont mises à jour pour pointer vers les overlays.
    /// </summary>
    void SetupTapisAvecImage()
    {
        if (rouletteRoot == null)
        {
            Debug.LogWarning("[RouletteManager] rouletteRoot null — mode image impossible.");
            return;
        }

        // ── 1. Créer le conteneur avec RectTransform natif ────────
        // CRUCIAL : passer typeof(RectTransform) dans le constructeur.
        // Sans ça, le GO a un Transform ordinaire → les overlays enfants
        // ne peuvent pas s'ancrer correctement → tout tombe en bas-gauche.
        var go = new GameObject("TapisImageContainer", typeof(RectTransform));
        go.transform.SetParent(rouletteRoot, false);
        go.AddComponent<CanvasRenderer>();

        // ── 2. Configurer le RectTransform IMMÉDIATEMENT ──────────
        var rt = (RectTransform)go.transform;  // cast garanti (créé avec typeof(RectTransform))
        // Remplir tout le popup sauf les 140px du haut (BarreControle)
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(0f, 0f);
        rt.offsetMax = new Vector2(0f, -140f);

        // Sauvegarder la référence EN TANT QUE RectTransform (pas Transform)
        tapisContainer = rt;

        // ── 3. Image de fond (après le RT pour ne pas perturber sa config) ─
        var bgImg = go.AddComponent<Image>();
        bgImg.sprite         = spriteTapis;
        bgImg.color          = Color.white;
        bgImg.preserveAspect = false;
        bgImg.type           = Image.Type.Simple;
        bgImg.raycastTarget  = false;

        // ── 3. Overlays transparents ────────────────────────────
        CreerOverlaysNombres();
        CreerOverlaysZonesExt();

        // ── 4. Le conteneur doit être DERRIÈRE la BarreControle ─
        // (BarreControle est créée après, mais on s'assure que l'ordre est correct)
        go.transform.SetAsFirstSibling();
    }

    /// <summary>
    /// Affiche la roue (spriteRoue) en déco dans le coin haut-gauche,
    /// juste sous la BarreControle. Taille fixe 200×200, raycastTarget=false.
    /// </summary>
    void SetupRoueImage()
    {
        if (rouletteRoot == null || spriteRoue == null) return;

        var go = new GameObject("RoueDecor", typeof(RectTransform));
        go.transform.SetParent(rouletteRoot, false);
        go.AddComponent<CanvasRenderer>();

        var rt = (RectTransform)go.transform;
        rt.anchorMin        = new Vector2(0f, 1f);
        rt.anchorMax        = new Vector2(0f, 1f);
        rt.pivot            = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(8f, -148f);
        rt.sizeDelta        = new Vector2(200f, 200f);

        var img = go.AddComponent<Image>();
        img.sprite         = spriteRoue;
        img.color          = Color.white;
        img.preserveAspect = true;
        img.raycastTarget  = false;

        go.transform.SetAsFirstSibling();
    }

    // =========================================================
    //  MODE IMAGE — OVERLAYS NUMÉROS
    // =========================================================
    void CreerOverlaysNombres()
    {
        // ── Constantes de calibration ──────────────────────────────────────────
        // Modifier yTop et rh si les rangées débordent encore.
        // yTop  = bord haut de la rangée 3 (depuis le bas du container)
        // rh    = hauteur d'une rangée (3 rangées identiques)
        // → grille bas = yTop - 3*rh
        const float yTop = 0.900f;
        const float rh   = 0.175f;   // row height : 3 × 0.165 = 0.495 de hauteur totale

        float yR2min = yTop - rh;         // 0.695
        float yR1min = yTop - 2f * rh;    // 0.530
        float yR0min = yTop - 3f * rh;    // 0.365  ← bas de la grille

        float[] yMins = { yR0min, yR1min, yR2min };
        float[] yMaxs = { yR1min, yR2min, yTop    };

        // ── Cellule 0 — couvre exactement les 3 rangées ───────────────────────
        CreerOverlayZone("0", tapisContainer,
            new Vector2(0.07f, yR0min), new Vector2(0.14f, yTop));

        // ── Grille 1-36 : 12 colonnes × 3 rangées ────────────────────────────
        float xBase = 0.140f;
        float xStep = 0.060f;   // 12 × 0.060 → grille de 0.14 à 0.86

        for (int k = 0; k < 12; k++)
        {
            float xMin = xBase + k * xStep;
            float xMax = xMin + xStep;
            int[] rowNums = { 3*k+1, 3*k+2, 3*(k+1) };
            for (int row = 0; row < 3; row++)
            {
                string id = rowNums[row].ToString();
                CreerOverlayZone(id, tapisContainer,
                    new Vector2(xMin, yMins[row]), new Vector2(xMax, yMaxs[row]));
            }
        }

        // ── 2à1 Colonnes — même hauteurs que les rangées ──────────────────────
        CreerOverlayZone("colonne3", tapisContainer,
            new Vector2(0.860f, yR2min), new Vector2(0.968f, yTop));
        CreerOverlayZone("colonne2", tapisContainer,
            new Vector2(0.860f, yR1min), new Vector2(0.968f, yR2min));
        CreerOverlayZone("colonne1", tapisContainer,
            new Vector2(0.860f, yR0min), new Vector2(0.968f, yR1min));
    }

    // =========================================================
    //  MODE IMAGE — OVERLAYS ZONES EXTÉRIEURES
    // =========================================================
    void CreerOverlaysZonesExt()
    {
        // Constantes partagées avec CreerOverlaysNombres :
        // grille bas = yTop - 3*rh = 0.860 - 0.495 = 0.365
        const float yGridBottom = 0.365f;   // = yTop - 3*rh

        // ── Douzaines — juste sous la grille ──────────────────────────────────
        const float dzH = 0.14f;            // hauteur rangée douzaines
        float yDzTop = yGridBottom;         // 0.365
        float yDzBot = yDzTop - dzH;        // 0.225

        float dzW = (0.860f - 0.14f) / 3f; // 0.2400
        CreerOverlayZone("douzaine1", tapisContainer,
            new Vector2(0.14f,          yDzBot), new Vector2(0.14f + dzW,      yDzTop));
        CreerOverlayZone("douzaine2", tapisContainer,
            new Vector2(0.14f + dzW,    yDzBot), new Vector2(0.14f + dzW*2f,   yDzTop));
        CreerOverlayZone("douzaine3", tapisContainer,
            new Vector2(0.14f + dzW*2f, yDzBot), new Vector2(0.860f,           yDzTop));

        // ── Simples — juste sous les douzaines ────────────────────────────────
        const float sH = 0.15f;             // hauteur rangée simples
        float ySTop = yDzBot;               // 0.225
        float ySBot = ySTop - sH;           // 0.075

        float sx = 0.14f, sw = (0.860f - 0.14f) / 6f; // 0.120 par zone
        CreerOverlayZone("manque", tapisContainer,
            new Vector2(sx,          ySBot), new Vector2(sx + sw,      ySTop));
        CreerOverlayZone("pair",   tapisContainer,
            new Vector2(sx + sw,     ySBot), new Vector2(sx + sw*2f,   ySTop));
        CreerOverlayZone("rouge",  tapisContainer,
            new Vector2(sx + sw*2f,  ySBot), new Vector2(sx + sw*3f,   ySTop));
        CreerOverlayZone("noir",   tapisContainer,
            new Vector2(sx + sw*3f,  ySBot), new Vector2(sx + sw*4f,   ySTop));
        CreerOverlayZone("impair", tapisContainer,
            new Vector2(sx + sw*4f,  ySBot), new Vector2(sx + sw*5f,   ySTop));
        CreerOverlayZone("passe",  tapisContainer,
            new Vector2(sx + sw*5f,  ySBot), new Vector2(sx + sw*6f,   ySTop));
    }

    // =========================================================
    //  MODE IMAGE — HELPER OVERLAY TRANSPARENT
    // =========================================================
    /// <summary>
    /// Crée un bouton overlay transparent sur la zone id du tapis image.
    /// Met à jour zones[id].image et zones[id].overlayMontant.
    /// </summary>
    void CreerOverlayZone(string zoneId, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        // ── Créer avec RectTransform natif dès le départ ─────────
        // new GameObject() seul donne un Transform ordinaire : GetComponent<RectTransform>()
        // retourne null → les ancres ne sont jamais appliquées → tout tombe en (0,0).
        // En passant typeof(RectTransform), le GO a un vrai RectTransform dès sa naissance.
        var obj = new GameObject("Ov_" + zoneId, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        obj.AddComponent<CanvasRenderer>();

        // ── Positionner le RectTransform AVANT d'ajouter l'Image ─
        var rt = (RectTransform)obj.transform;  // cast sûr : le transform EST un RectTransform
        rt.anchorMin        = anchorMin;
        rt.anchorMax        = anchorMax;
        rt.sizeDelta        = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;

        // ── Image (debug = cyan visible, prod = transparent) ──────
        var img = obj.AddComponent<Image>();
        img.color         = debugOverlays
            ? new Color(0f, 0.8f, 1f, 0.35f)
            : new Color(1f, 1f,   1f, 0f);
        img.raycastTarget = true;

        // ── Button Transition.None ────────────────────────────────
        var btn = obj.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.transition    = Selectable.Transition.None;

        // ── Hover visuel via EventTrigger ─────────────────────
        // (ColorBlock inutilisable avec Transition.None — on gère soi-même)
        var et      = obj.AddComponent<EventTrigger>();
        string captId  = zoneId;   // capture correcte pour les lambdas
        Image  captImg = img;

        var onEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        onEnter.callback.AddListener((_) => {
            bool hasBet = zones.ContainsKey(captId) && zones[captId].montant > 0;
            captImg.color = hasBet
                ? new Color(1f, 0.85f, 0f, 0.55f)   // hover sur zone misée → jaune vif
                : new Color(1f, 0.90f, 0f, 0.28f);   // hover neutre → jaune doux
            if (zoneInfoTxt != null) zoneInfoTxt.text = NomZone(captId);
        });
        et.triggers.Add(onEnter);

        var onExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        onExit.callback.AddListener((_) => {
            bool hasBet = zones.ContainsKey(captId) && zones[captId].montant > 0;
            captImg.color = hasBet
                ? new Color(1f, 0.85f, 0f, 0.40f)   // reste jaune si mise placée
                : new Color(1f, 1f,    1f, 0f);       // retour transparent
            if (zoneInfoTxt != null) zoneInfoTxt.text = string.Empty;
        });
        et.triggers.Add(onExit);

        // ── Texte de mise ─────────────────────────────────────
        var ovGo = CreerTexteGO("OvMise", obj.transform, "",
            TextAnchor.LowerCenter, 18, FontStyle.Bold, new Color(1f, 0.95f, 0.1f));
        var ovR = ovGo.GetComponent<RectTransform>();
        ovR.anchorMin        = Vector2.zero;
        ovR.anchorMax        = new Vector2(1f, 0.42f);
        ovR.sizeDelta        = Vector2.zero;
        ovR.anchoredPosition = Vector2.zero;

        var sh = ovGo.AddComponent<Shadow>();
        sh.effectColor    = new Color(0f, 0f, 0f, 0.9f);
        sh.effectDistance = new Vector2(1f, -1f);

        // ── Mettre à jour la BetZone ──────────────────────────
        if (zones.ContainsKey(zoneId))
        {
            zones[zoneId].image          = img;
            zones[zoneId].overlayMontant = ovGo.GetComponent<Text>();
            zones[zoneId].couleurBase    = new Color(1f, 1f, 1f, 0f);
            zones[zoneId].transparent    = true;
        }
        else
        {
            Debug.LogWarning("[RouletteManager] Zone '" + zoneId + "' absente de la dict — zoneId=" + zoneId);
        }

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => PlacerMise(captId));
    }

    // =========================================================
    //  GÉNÉRATION — BARRE DE CONTRÔLE
    // =========================================================
    void GenererJetons() { /* no-op — intégré dans GenererControles */ }

    void GenererControles()
    {
        if (rouletteRoot == null) return;

        var barre = new GameObject("BarreControle");
        barre.transform.SetParent(rouletteRoot, false);
        barre.AddComponent<CanvasRenderer>();
        var barreImg = barre.AddComponent<Image>();
        barreImg.color = new Color(0.04f, 0.10f, 0.04f, 0.95f);

        var barreRT = barre.GetComponent<RectTransform>();
        barreRT.anchorMin        = new Vector2(0f, 1f);
        barreRT.anchorMax        = new Vector2(1f, 1f);
        barreRT.pivot            = new Vector2(0.5f, 1f);
        barreRT.anchoredPosition = Vector2.zero;
        barreRT.sizeDelta        = new Vector2(0f, 140f);

        var barreHLG = barre.AddComponent<HorizontalLayoutGroup>();
        barreHLG.childAlignment         = TextAnchor.MiddleCenter;
        barreHLG.spacing                = 14;
        barreHLG.padding                = new RectOffset(16, 16, 10, 10);
        barreHLG.childControlWidth      = true;
        barreHLG.childControlHeight     = true;
        barreHLG.childForceExpandWidth  = true;
        barreHLG.childForceExpandHeight = true;

        // La barre doit être au-dessus du tapis image
        barre.transform.SetAsLastSibling();

        // ── Section JETONS ──────────────────────────────────────
        var sectionJetons = CreerSection("SectionJetons", barre.transform, 2.5f);
        var jetonsHLG = sectionJetons.AddComponent<HorizontalLayoutGroup>();
        jetonsHLG.childAlignment         = TextAnchor.MiddleCenter;
        jetonsHLG.spacing                = 10;
        jetonsHLG.childControlWidth      = false;
        jetonsHLG.childControlHeight     = false;
        jetonsHLG.childForceExpandWidth  = false;
        jetonsHLG.childForceExpandHeight = false;

        jetonImages = new Image[VALEURS_JETONS.Length];
        for (int i = 0; i < VALEURS_JETONS.Length; i++)
        {
            int   valeur  = VALEURS_JETONS[i];
            Color couleur = CouleurJeton(valeur);
            int   idx     = i;

            var jeton = new GameObject("Jeton_" + valeur);
            jeton.transform.SetParent(sectionJetons.transform, false);
            jeton.AddComponent<CanvasRenderer>();
            var jImg = jeton.AddComponent<Image>(); jImg.color = couleur;
            jetonImages[i] = jImg;

            var jRT = jeton.GetComponent<RectTransform>();
            jRT.sizeDelta = new Vector2(110f, 110f);

            var jBtn = jeton.AddComponent<Button>(); jBtn.targetGraphic = jImg;
            var jCB  = jBtn.colors;
            jCB.normalColor      = couleur;
            jCB.highlightedColor = Color.Lerp(couleur, Color.white, 0.30f);
            jCB.pressedColor     = Color.Lerp(couleur, Color.black, 0.30f);
            jBtn.colors = jCB;

            var jTxt = CreerTexteGO("Val", jeton.transform, valeur.ToString(),
                TextAnchor.MiddleCenter, 34, FontStyle.Bold, Color.white);
            StretchRect(jTxt.GetComponent<RectTransform>());

            var outline = jeton.AddComponent<Outline>();
            outline.effectColor    = Color.yellow;
            outline.effectDistance = new Vector2(3, -3);
            outline.enabled        = false;

            jBtn.onClick.AddListener(() => SelectionnerJeton(valeur, idx));
        }

        // ── Section INFOS ──────────────────────────────────────
        var sectionInfo = CreerSection("SectionInfo", barre.transform, 1.5f);
        var infoVLG = sectionInfo.AddComponent<VerticalLayoutGroup>();
        infoVLG.childAlignment         = TextAnchor.MiddleCenter;
        infoVLG.childControlWidth      = true;
        infoVLG.childControlHeight     = false;
        infoVLG.childForceExpandWidth  = true;
        infoVLG.childForceExpandHeight = false;
        infoVLG.spacing = 4;

        var tSolde = CreerTexteGO("Solde", sectionInfo.transform, "Solde : ---",
            TextAnchor.MiddleCenter, 34, FontStyle.Bold, new Color(1f, 0.85f, 0.10f));
        tSolde.AddComponent<LayoutElement>().preferredHeight = 48f;
        balanceTxt = tSolde.GetComponent<Text>();

        var tMise = CreerTexteGO("Mise", sectionInfo.transform, "Mise : 0",
            TextAnchor.MiddleCenter, 26, FontStyle.Normal, new Color(0.85f, 0.85f, 0.85f));
        tMise.AddComponent<LayoutElement>().preferredHeight = 36f;
        miseTxt = tMise.GetComponent<Text>();

        var tJeton = CreerTexteGO("Jeton", sectionInfo.transform, "Jeton : 10",
            TextAnchor.MiddleCenter, 26, FontStyle.Normal, new Color(0.85f, 0.85f, 0.85f));
        tJeton.AddComponent<LayoutElement>().preferredHeight = 36f;
        jetonTxt = tJeton.GetComponent<Text>();

        // ── Ligne "zone survolée" ──────────────────────────────
        var tZone = CreerTexteGO("ZoneInfo", sectionInfo.transform, "",
            TextAnchor.MiddleCenter, 24, FontStyle.Italic, new Color(0.55f, 0.90f, 1.0f));
        tZone.AddComponent<LayoutElement>().preferredHeight = 32f;
        zoneInfoTxt = tZone.GetComponent<Text>();

        // ── Section BOUTONS Spin + Annuler ─────────────────────
        var sectionBtns = CreerSection("SectionBoutons", barre.transform, 1f);
        var btnsVLG = sectionBtns.AddComponent<VerticalLayoutGroup>();
        btnsVLG.childAlignment         = TextAnchor.MiddleCenter;
        btnsVLG.childControlWidth      = true;
        btnsVLG.childControlHeight     = false;
        btnsVLG.childForceExpandWidth  = true;
        btnsVLG.childForceExpandHeight = false;
        btnsVLG.spacing = 6;
        btnsVLG.padding = new RectOffset(6, 6, 4, 4);

        var spinGo = new GameObject("BtnLancer");
        spinGo.transform.SetParent(sectionBtns.transform, false);
        spinGo.AddComponent<CanvasRenderer>();
        var spinImg = spinGo.AddComponent<Image>(); spinImg.color = new Color(0.72f, 0.08f, 0.08f);
        spinBtn = spinGo.AddComponent<Button>(); spinBtn.targetGraphic = spinImg;
        var spinCB = spinBtn.colors;
        spinCB.normalColor      = new Color(0.72f, 0.08f, 0.08f);
        spinCB.highlightedColor = new Color(0.92f, 0.14f, 0.14f);
        spinCB.pressedColor     = new Color(0.42f, 0.04f, 0.04f);
        spinBtn.colors = spinCB;
        spinGo.AddComponent<LayoutElement>().preferredHeight = 70f;
        var spinTxt = CreerTexteGO("Label", spinGo.transform, "🎲  LANCER",
            TextAnchor.MiddleCenter, 30, FontStyle.Bold, Color.white);
        StretchRect(spinTxt.GetComponent<RectTransform>());
        spinBtn.onClick.AddListener(Spin);

        var annGo = new GameObject("BtnAnnuler");
        annGo.transform.SetParent(sectionBtns.transform, false);
        annGo.AddComponent<CanvasRenderer>();
        var annImg = annGo.AddComponent<Image>(); annImg.color = new Color(0.28f, 0.28f, 0.28f);
        annulerBtn = annGo.AddComponent<Button>(); annulerBtn.targetGraphic = annImg;
        var annCB = annulerBtn.colors;
        annCB.normalColor      = new Color(0.28f, 0.28f, 0.28f);
        annCB.highlightedColor = new Color(0.42f, 0.42f, 0.42f);
        annCB.pressedColor     = new Color(0.15f, 0.15f, 0.15f);
        annulerBtn.colors = annCB;
        annGo.AddComponent<LayoutElement>().preferredHeight = 48f;
        var annTxt = CreerTexteGO("Label", annGo.transform, "✕  Annuler",
            TextAnchor.MiddleCenter, 26, FontStyle.Bold, new Color(0.9f, 0.9f, 0.9f));
        StretchRect(annTxt.GetComponent<RectTransform>());
        annulerBtn.onClick.AddListener(AnnulerMises);

        if (infoMiseText != null)
            infoMiseText.gameObject.SetActive(false);

        if (resultatText != null)
        {
            resultatText.supportRichText = true;
            resultatText.text      = "Choisissez vos mises…";
            resultatText.fontSize  = 32;
            resultatText.fontStyle = FontStyle.Bold;
            resultatText.alignment = TextAnchor.MiddleCenter;
            resultatText.color     = Color.white;
        }

        int defIdx = System.Array.IndexOf(VALEURS_JETONS, jetonChoisi);
        if (defIdx < 0) defIdx = 2;
        SelectionnerJeton(VALEURS_JETONS[defIdx], defIdx);
    }

    GameObject CreerSection(string nom, Transform parent, float flexWeight)
    {
        var go = new GameObject(nom);
        go.transform.SetParent(parent, false);
        go.AddComponent<CanvasRenderer>();
        go.AddComponent<Image>().color = new Color(0,0,0,0);
        var le = go.AddComponent<LayoutElement>();
        le.flexibleWidth  = flexWeight;
        le.flexibleHeight = 1f;
        return go;
    }

    // =========================================================
    //  LOGIQUE — SÉLECTIONNER UN JETON
    // =========================================================
    void SelectionnerJeton(int valeur, int idx)
    {
        jetonChoisi = valeur;
        if (jetonImages == null) return;
        for (int i = 0; i < jetonImages.Length; i++)
        {
            if (jetonImages[i] == null) continue;
            var outline = jetonImages[i].GetComponent<Outline>();
            if (outline != null) outline.enabled = (i == idx);
            jetonImages[i].transform.localScale = (i == idx)
                ? new Vector3(1.18f, 1.18f, 1f) : Vector3.one;
        }
        MettreAJourUI();
    }

    // =========================================================
    //  LOGIQUE — PLACER UNE MISE
    // =========================================================
    void PlacerMise(string zoneId)
    {
        Debug.Log("[Roulette] PlacerMise appelé → zone=" + zoneId);
        if (spinEnCours) return;
        if (!zones.ContainsKey(zoneId)) { Debug.LogWarning("[Roulette] Zone introuvable: " + zoneId); return; }

        int solde       = GetSolde();
        int totalActuel = CalculerTotalMise();

        if (totalActuel + jetonChoisi > solde)
        {
            AfficherMessage("<color=#ff6666>⚠ Solde insuffisant !</color>");
            return;
        }

        zones[zoneId].montant += jetonChoisi;
        MettreAJourZoneVisuel(zoneId);
        MettreAJourUI();
    }

    // =========================================================
    //  LOGIQUE — LANCER LA ROULETTE
    // =========================================================
    public void Spin()
    {
        if (spinEnCours) return;
        int total = CalculerTotalMise();
        if (total <= 0)    { AfficherMessage("Placez une mise d'abord !"); return; }
        if (GetSolde() < total) { AfficherMessage("<color=#ff6666>⚠ Solde insuffisant !</color>"); return; }
        StartCoroutine(AnimerSpin(total));
    }

    IEnumerator AnimerSpin(int totalMise)
    {
        spinEnCours = true;
        if (spinBtn)    spinBtn.interactable    = false;
        if (annulerBtn) annulerBtn.interactable = false;

        DepenseCoins(totalMise);
        MettreAJourUI();

        float duree = 2.2f, elapsed = 0f;
        while (elapsed < duree)
        {
            int fake = Random.Range(0, 37);
            AfficherMessage($"🎰  . . . {fake} . . .");
            float pause = Mathf.Lerp(0.06f, 0.18f, elapsed / duree);
            elapsed += pause;
            yield return new WaitForSeconds(pause);
        }

        int  numero   = Random.Range(0, 37);
        bool estRouge = System.Array.IndexOf(ROUGES, numero) >= 0;
        string couleur = numero == 0 ? "Vert" : (estRouge ? "Rouge" : "Noir");

        int recuTotal = CalculerGains(numero);
        int gainNet   = recuTotal - totalMise;
        if (recuTotal > 0) GagneCoins(recuTotal);
        MettreAJourUI();

        string tagC  = numero == 0 ? "<color=#33ff55>" : estRouge ? "<color=#ff5555>" : "<color=#bbbbbb>";
        string ligne1 = $"{tagC}◉  {numero}  —  {couleur}</color>";
        string ligne2;
        if      (gainNet > 0)               ligne2 = $"<color=#ffdd00>✔  GAGNÉ  +{gainNet} 🪙</color>";
        else if (gainNet == 0 && recuTotal > 0) ligne2 = "<color=#aaaaaa>═  Mise récupérée</color>";
        else                                ligne2 = $"<color=#ff6666>✘  PERDU  -{totalMise} 🪙</color>";
        AfficherMessage(ligne1 + "\n" + ligne2);

        StartCoroutine(HighlightGagnant(numero.ToString()));

        yield return new WaitForSeconds(3.5f);
        ResetMises();

        spinEnCours = false;
        if (spinBtn)    spinBtn.interactable    = true;
        if (annulerBtn) annulerBtn.interactable = true;
        MettreAJourUI();
    }

    // =========================================================
    //  CALCULS
    // =========================================================
    int CalculerGains(int numeroGagnant)
    {
        int total = 0;
        foreach (var kv in zones)
        {
            if (kv.Value.montant <= 0) continue;
            if (kv.Value.Couvre(numeroGagnant))
                total += kv.Value.montant * (kv.Value.multiplicateur + 1);
        }
        return total;
    }

    int CalculerTotalMise()
    {
        int total = 0;
        foreach (var kv in zones) total += kv.Value.montant;
        return total;
    }

    // =========================================================
    //  FEEDBACK VISUEL
    // =========================================================
    void MettreAJourZoneVisuel(string zoneId)
    {
        if (!zones.ContainsKey(zoneId)) return;
        BetZone zone = zones[zoneId];

        // Texte de mise
        if (zone.overlayMontant != null)
            zone.overlayMontant.text = zone.montant > 0 ? $"{zone.montant} 🪙" : "";

        if (zone.image == null) return;

        if (zone.transparent)
        {
            // Transition.None est actif sur ces boutons — on écrit la couleur directement.
            // Pas besoin de sync ColorBlock (il n'est pas utilisé).
            zone.image.color = zone.montant > 0
                ? new Color(1f, 0.85f, 0f, 0.40f)   // jaune semi-opaque = mise placée
                : new Color(1f, 1f, 1f, 0f);          // transparent = pas de mise
        }
        else
        {
            // Mode coloré classique
            zone.image.color = zone.montant > 0
                ? Color.Lerp(zone.couleurBase, new Color(1f, 0.75f, 0f), 0.28f)
                : zone.couleurBase;
        }
    }

    IEnumerator HighlightGagnant(string zoneId)
    {
        if (!zones.ContainsKey(zoneId)) yield break;
        var zone = zones[zoneId];
        if (zone.image == null) yield break;

        Color original = zone.image.color;
        Color flash    = zone.transparent ? new Color(1f, 1f, 0f, 0.70f) : Color.yellow;

        for (int i = 0; i < 7; i++)
        {
            zone.image.color = flash;
            yield return new WaitForSeconds(0.14f);
            zone.image.color = original;
            yield return new WaitForSeconds(0.14f);
        }
    }

    void ResetMises()
    {
        foreach (var kv in zones)
        {
            kv.Value.montant = 0;
            MettreAJourZoneVisuel(kv.Key);
        }
    }

    public void AnnulerMises()
    {
        if (spinEnCours) return;
        ResetMises();
        AfficherMessage("Mises annulées.");
        MettreAJourUI();
    }

    void AfficherMessage(string msg)
    {
        if (resultatText == null) return;
        resultatText.supportRichText = true;
        resultatText.text = msg;
    }

    void MettreAJourUI()
    {
        int solde = GetSolde();
        int total = CalculerTotalMise();

        if (balanceTxt != null)
            balanceTxt.text = $"Solde : {solde} 🪙";
        if (miseTxt != null)
            miseTxt.text = $"Mise totale : {total}    |    Jeton : {jetonChoisi} 🪙";
        if (infoMiseText != null && infoMiseText.gameObject.activeInHierarchy)
            infoMiseText.text = $"Solde: {solde}  |  Mise: {total}  |  Jeton: {jetonChoisi}";
    }

    // =========================================================
    //  ECONOMY
    // =========================================================
    int  GetSolde()           => Economy.Instance != null ? Economy.Instance.coins : debugCoins;
    void DepenseCoins(int a)  { if (Economy.Instance != null) Economy.Instance.SpendCoins(a); else debugCoins = Mathf.Max(0, debugCoins - a); }
    void GagneCoins(int a)    { if (Economy.Instance != null) Economy.Instance.AddCoins(a);   else debugCoins += a; }

    // =========================================================
    //  COMPATIBILITÉ ANCIENNE API
    // =========================================================
    public void ChoisirNumero(int numero)   => PlacerMise(numero.ToString());
    public void FaireRouler()               => Spin();
    public void SelectionnerMise(int m)     { jetonChoisi = m; MettreAJourUI(); }
    public void AnnulerTout()               => AnnulerMises();

    public void ChoisirZone(string nomZone)
    {
        string id;
        switch (nomZone.ToLower())
        {
            case "rouge":     id = "rouge";     break;
            case "noir":      id = "noir";      break;
            case "pair":      id = "pair";      break;
            case "impair":    id = "impair";    break;
            case "1to18":     id = "manque";    break;
            case "19to36":    id = "passe";     break;
            case "douzaine1": id = "douzaine1"; break;
            case "douzaine2": id = "douzaine2"; break;
            case "douzaine3": id = "douzaine3"; break;
            case "colonne1":  id = "colonne1";  break;
            case "colonne2":  id = "colonne2";  break;
            case "colonne3":  id = "colonne3";  break;
            default:          id = nomZone.ToLower(); break;
        }
        PlacerMise(id);
    }

    // =========================================================
    //  NOM LISIBLE D'UNE ZONE
    // =========================================================
    string NomZone(string id)
    {
        switch (id)
        {
            case "0":         return "0  —  Vert";
            case "colonne1":  return "2 à 1  —  Colonne 1  (1, 4, 7…)";
            case "colonne2":  return "2 à 1  —  Colonne 2  (2, 5, 8…)";
            case "colonne3":  return "2 à 1  —  Colonne 3  (3, 6, 9…)";
            case "douzaine1": return "1ère Douzaine  (1 → 12)";
            case "douzaine2": return "2ème Douzaine  (13 → 24)";
            case "douzaine3": return "3ème Douzaine  (25 → 36)";
            case "manque":    return "Manque  (1 → 18)";
            case "passe":     return "Passé  (19 → 36)";
            case "pair":      return "Pair";
            case "impair":    return "Impair";
            case "rouge":     return "Rouge";
            case "noir":      return "Noir";
            default:
                // Numéro 1-36 : indique rouge/noir
                if (int.TryParse(id, out int n))
                {
                    bool rouge = System.Array.IndexOf(ROUGES, n) >= 0;
                    return $"Numéro {n}  —  {(rouge ? "Rouge" : "Noir")}";
                }
                return id;
        }
    }

    // =========================================================
    //  UTILITAIRES UI
    // =========================================================
    void ViderPanel(Transform p)
    {
        for (int i = p.childCount - 1; i >= 0; i--)
            Destroy(p.GetChild(i).gameObject);
    }

    GameObject CreerTexteGO(string nom, Transform parent, string texte,
        TextAnchor align, int taille, FontStyle style, Color couleur)
    {
        // typeof(RectTransform) dans le constructeur → RectTransform natif, pas Transform ordinaire
        var go = new GameObject(nom, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.AddComponent<CanvasRenderer>();
        var t = go.AddComponent<Text>();
        t.text      = texte;
        t.alignment = align;
        t.fontSize  = taille;
        t.fontStyle = style;
        t.color     = couleur;
        t.supportRichText = true;
        if (defaultFont != null) t.font = defaultFont;
        return go;
    }

    void StretchRect(RectTransform rt)
    {
        if (rt == null) return;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero; rt.anchoredPosition = Vector2.zero;
    }

    // =========================================================
    //  GÉNÉRATEURS DE ZONES NUMÉRIQUES
    // =========================================================
    static int[] GenererColonne(int col)
    {
        var l = new List<int>();
        for (int i = col; i <= 36; i += 3) l.Add(i);
        return l.ToArray();
    }

    static int[] GenererRange(int from, int to)
    {
        var a = new int[to - from + 1];
        for (int i = 0; i < a.Length; i++) a[i] = from + i;
        return a;
    }

    static int[] GenererPair()
    {
        var l = new List<int>();
        for (int i = 2; i <= 36; i += 2) l.Add(i);
        return l.ToArray();
    }

    static int[] GenererImpair()
    {
        var l = new List<int>();
        for (int i = 1; i <= 36; i += 2) l.Add(i);
        return l.ToArray();
    }

    static int[] GenererNoir()
    {
        var l = new List<int>();
        for (int i = 1; i <= 36; i++)
            if (System.Array.IndexOf(ROUGES, i) < 0) l.Add(i);
        return l.ToArray();
    }

    static Color CouleurJeton(int valeur)
    {
        switch (valeur)
        {
            case 1:   return new Color(0.85f, 0.85f, 0.85f);
            case 5:   return new Color(0.85f, 0.12f, 0.12f);
            case 10:  return new Color(0.10f, 0.38f, 0.85f);
            case 25:  return new Color(0.10f, 0.62f, 0.22f);
            case 50:  return new Color(0.85f, 0.44f, 0.05f);
            case 100: return new Color(0.48f, 0.10f, 0.70f);
            default:  return Color.gray;
        }
    }
}
